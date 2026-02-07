using System.Runtime.InteropServices;
using HarmonyLib;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace moth.LinuxPls.Worlds.Editor {
    [InitializeOnLoad]
    public static class Patch {
        internal static Harmony _harmony;

        static Patch() {
            // check if this is being ran on linux 
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                Debug.LogWarning("Disabling Linux compatibility plugin: Host operating system is not Linux.");
                return;
            }
            
            // passed all checks, apply the patch
            _harmony = new Harmony("moth.LinuxPls.Worlds");
            _harmony.PatchAll();
        }
    }
}
