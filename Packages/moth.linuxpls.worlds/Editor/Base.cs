using System;
using System.IO;
using HarmonyLib;
using UnityEditor;
using VRC.Core;
using VRC.SDKBase.Editor;

namespace moth.LinuxPls.Worlds.Editor {
    [HarmonyPatch]
    public static class Base {
        public static string CompatDataPath() {
            // check if the vrcpath is set
            string vrcpath = SDKClientUtilities.GetSavedVRCInstallPath();
            if (vrcpath == "") {
                return vrcpath;
            }

            // verify that the vrcpath points to a valid directory
            DirectoryInfo dir = new FileInfo(vrcpath).Directory;
            if (dir == null) {
                return "";
            }

            // step backwards until we reach the steamapps folder
            while (!dir.Name.Contains("steamapps", StringComparison.OrdinalIgnoreCase)) {
                dir = dir.Parent;
                if (dir == null) {
                    return "";
                }
            }

            // return the steamapps folder we found with compatdata for VRChat
            return dir.FullName + "/compatdata/438100";
        }

        // fix the default installation directory function
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SDKClientUtilities), "LoadRegistryVRCInstallPath")]
        public static bool LocateVRChatInstallation(ref string __result) {
            __result = "";

            // find our home directory
            string home = Environment.GetEnvironmentVariable("HOME");
            if (home == "") {
                return false;
            }

            // find our steamapps directory
            DirectoryInfo dir = new FileInfo($"{home}/.local/share/Steam/steamapps").Directory;
            if (dir == null) {
                return false;
            }

            // parse the steam library file for VRChat's installation drive
            string path = "";
            foreach (string line in File.ReadLines($"{home}/.local/share/Steam/steamapps/libraryfolders.vdf")) {
                // parsing through a new path
                if (line.Contains("path")) {
                    // use the tabs to locate the paths position in the line, and strip the quotes around it
                    path = line.Split('\t')[4].Replace("\"", "");
                }
                // located vrchats path, quit parsing the file
                if (line.Contains("438100")) {
                    break;
                }
            }

            // append the rest of the filepath and save it
            __result = $"{path}/steamapps/common/VRChat/VRChat.exe";
            EditorPrefs.SetString("VRC_installedClientPath", __result);

            return false;
        }
        
        // Thanks Bartkk <3
        [HarmonyPrefix]
        [HarmonyPatch(typeof(VRC_SdkBuilder), "GetLocalLowPath")]
        public static bool GetLocalLowPathPrefix(ref string __result) {
            __result = CompatDataPath() + "/pfx/drive_c/users/steamuser/AppData/LocalLow/";
            return false;
        }
    }
}