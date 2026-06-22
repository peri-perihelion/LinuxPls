using System.Runtime.InteropServices;
using HarmonyLib;
using UnityEditor;

using Debug = UnityEngine.Debug;
using MethodInfo = System.Reflection.MethodInfo;
using GraphicsDeviceType = UnityEngine.Rendering.GraphicsDeviceType;

namespace moth.LinuxPls.Worlds.Editor {
    [InitializeOnLoad]
    public static class Patch {
        internal static Harmony _harmony;

        static Patch() {
            _harmony = new Harmony("moth.LinuxPls.Worlds");

            // https://vrchat.canny.io/sdk-bug-reports/p/vrc-sdk-keeps-re-enabling-auto-graphics-for-linux-forcing-unity-to-open-in-openg
            FixVulkan();
            // applied regardless of platform, as otherwise sharing a project with windows machines is tedious

            // check if this is being ran on linux 
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                Debug.LogWarning("Disabling Linux compatibility plugin: Host operating system is not Linux.");
                return;
            }
            
            // passed checks, apply the rest of the patch
            _harmony.PatchAll();
        }
        private static void FixVulkan(){
            /*  
                Inside of EnvConfig, allowedGraphicsAPIs is set to null for Linux.

                This means that when SetDefaultGraphicsAPIs is called, it reverts Linux to defaults, and forces us onto OpenGL.

                Here, we undo that, by making sure Vulkan is selected first.
            */

            MethodInfo original = AccessTools.Method(typeof(VRC.Editor.EnvConfig), "SetDefaultGraphicsAPIs");
            MethodInfo postfix = AccessTools.Method(typeof(Patch), "SetLinuxGraphicsAPIs");
            _harmony.Patch(original, null, new HarmonyMethod(postfix));
        }
        public static void SetLinuxGraphicsAPIs(){
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneLinux64, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneLinux64, new[] {GraphicsDeviceType.Vulkan, GraphicsDeviceType.OpenGLCore});
        }
    }
}
