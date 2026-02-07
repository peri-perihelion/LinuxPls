using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

// Requests the user to upvote the Linux support Canny on first install
namespace moth.LinuxPls.Base.Editor{
    [InitializeOnLoad]
    public static class CannyRequest{
        // allow the canny request to be re-enabled
        [MenuItem("LinuxPls/Re-enable Canny Request")]
        public static void LinuxPls_EnableCannyRequest() {
            EditorPrefs.SetBool("LinuxPls_CannyRequest", false);
        }
        
        static CannyRequest() {
            // dont show this popup if its been disabled, or if were not on Linux
            if (EditorPrefs.GetBool("LinuxPls_CannyRequest", false) || !RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                // "EditorPrefs.GetBool(name, false)" means the default value is False when the option doesnt exist
                return;
            }

            var result = EditorUtility.DisplayDialog(
                "LinuxPls - Your currently installed Linux compatibility package", // dialogue title
                "Please consider voting on the Canny for official Linux support.", // dialogue contents
                "Open Canny", "Don't show again" // dialogue option labels (first is True, second is False)
            );
            
            // if they clicked to open the canny, then open the canny
            if (result){
               Application.OpenURL("https://feedback.vrchat.com/sdk-bug-reports/p/add-proton-support-to-the-sdk-for-local-tests");
            }

            // prevent the popup from displaying again, irregardless of what the user choses
            EditorPrefs.SetBool("LinuxPls_CannyRequest", true);
        }
    }
}
