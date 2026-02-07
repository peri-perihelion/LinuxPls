using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using HarmonyLib;
using UnityEditor;
using UnityEngine.Networking;
using VRC.Core;
using VRC.SDK3.Editor.Builder;
using VRC.SDKBase.Editor;
using Debug = UnityEngine.Debug;

// Changes the default world testing behaviour to be compatible with Linux
namespace moth.LinuxPls.Worlds.Editor {
    [HarmonyPatch]
    public static class World {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(VRCWorldAssetExporter), "RunWorldTestDesktop", typeof(string))]
        public static bool RunWorldTestDesktop(string bundleFilePath) {
            string vrcInstallPath = SDKClientUtilities.GetSavedVRCInstallPath();
            if (vrcInstallPath == "" || !File.Exists(vrcInstallPath)) {
                Debug.LogError("VRChat Path is invalid! Please manually set it using the SDK.");
                return true;
            }

            string compatDataPath = Base.CompatDataPath();
            if (compatDataPath == "" || !File.Exists(vrcInstallPath)) {
                Debug.LogError("Failed to locate VRChat compatdata folder!");
                return false;
            }

            // carried over from the normal VRCWorldAssetExporter:RunWorldTestDesktop
            bundleFilePath = UnityWebRequest.EscapeURL(bundleFilePath).Replace("+", "%20");
            
            string arguments = 
                // encase in quotes, so we can support spaces in the directory path
                $"\"{vrcInstallPath}\" " +
                // random room id
                $"--url=create?roomId={VRC.Tools.GetRandomDigits(10)}" + 
                // "Z:/" is present because bundle filepath is relative to root, not the c drive
                $"&hidden=true&name=BuildAndRun&url=file:///Z:/{bundleFilePath} " +
                // mandatory launch arguments (debug gui is broken? it exists in the official method so its staying)
                "--enable-debug-gui --enable-sdk-log-levels --enable-udon-debug-logging" +
                (VRCSettings.ForceNoVR ? " --no-vr" : "") + 
                (VRCSettings.WatchWorlds ? " --watch-worlds" : "");

            // open with umu-run, as without it, proton will use fallback wine libraries and likely crash
            var processStartInfo = new ProcessStartInfo("umu-run", arguments) {
                EnvironmentVariables = { 
                    // set the wine prefix to the one steam uses, as otherwise you have to log into vrchat again
                    { "WINEPREFIX", $"{compatDataPath}/pfx" },
                    // gameid has to be set for umu-run to respect the wineprefix
                    { "GAMEID", "438100" }, 

                    // default to umu-latest when there is no custom proton path
                    { "PROTONPATH", EditorPrefs.GetString("LinuxPls_ProtonPath") == "" ? "UMU-Latest" : EditorPrefs.GetString("LinuxPls_ProtonPath") },
                    
                    // disable proton fixes, as none exist for VRChat anyway
                    { "PROTONFIXES_DISABLE", "1" }
                },

                // vrchat sets this, so we will too
                WorkingDirectory = Path.GetDirectoryName(vrcInstallPath) ?? "",

                // must be enabled to use EnvironmentVariables
                UseShellExecute = false
            };

            // log exactly what were about to run (but adjusted to set environment variables through the terminal)
            Debug.Log("Launching VRChat with the following parameters:\n" +
                    $"WINEPREFIX={compatDataPath}/pfx " + 
                    "GAMEID=438100 PROTONFIXES_DISABLE=1 " + 
                    $"PROTONPATH={(EditorPrefs.GetString("LinuxPls_ProtonPath") == "" ? "UMU-Latest" : EditorPrefs.GetString("LinuxPls_ProtonPath"))} " + 
                    $"umu-run {arguments}");

            for (var index = 0; index < VRCSettings.NumClients; ++index) {
                /* TODO:
                    sometimes this causes a black screen when opening multiple clients, with the following error:
                    "VRCNP: Failed to create server: System.IO.IOException: Pipe busy"

                    i suspect it has something to do with all of this being open on the same thread,
                    so if i can replicate this error again, then it may be worth looking into multithreaded process opening.

                    for now though, this is percisely how vrchat opens their processes, so we'll be doing the same.
                */
                Process.Start(processStartInfo);
                Thread.Sleep(EditorPrefs.GetInt("LinuxPls_ClientTimeout", 3000));
            }

            // were excluding the default call to VRC.AnalyticsSDK here
            // this shouldn't impede functionality, but could be relevant to someones use case, so im making note of it

            return false;
        }
    }
}
