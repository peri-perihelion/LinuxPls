using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

namespace moth.LinuxPls.Worlds.Editor {
    [HarmonyPatch]
    public static class UI {
        private static void OnProtonInstallPathGUI() {
            string protonPath = EditorPrefs.GetString("LinuxPls_ProtonPath", "");
            EditorGUILayout.LabelField("Custom Proton", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Leave unset to use UMU-Proton, the latest stable Proton with UMU patches");
            EditorGUILayout.LabelField("Proton Python File: ", protonPath);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("");
            
            if (GUILayout.Button("Edit")) {
                // find the users home directory
                string home = Environment.GetEnvironmentVariable("HOME");

                // open into the compatibility tools folder if they have no proton set
                string initPath = $"{home}/.local/share/Steam/compatibilitytools.d/";
                if (protonPath != "")
                    initPath = protonPath;

                protonPath = EditorUtility.OpenFilePanel("Choose Proton Python File", initPath, "");
                EditorPrefs.SetString("LinuxPls_ProtonPath", protonPath);
            }
            if (GUILayout.Button("Clear")) {
                EditorPrefs.SetString("LinuxPls_ProtonPath", "");
            }

            // display warnings at the end of the box
            EditorGUILayout.EndHorizontal();
            if (EditorGUILayout.LinkButton("If you're unsure which Proton version to use, its best to leave this blank")) {
               Application.OpenURL("https://github.com/Open-Wine-Components/umu-launcher/issues/123#issuecomment-2182675298");
            }
            EditorGUILayout.LabelField("Please be aware that only bug reports using UMU-Proton will be investigated!", EditorStyles.boldLabel);
            EditorGUILayout.Separator();

            /* TODO:
                every time a button is used, the following error is printed into the console:
                "EndLayoutGroup: BeginLayoutGroup must be called first."
                
                this also happens with the default VRChatSDK, so I'm unsure of where to proceed
            */
        }

        private static void OnProtonDelayGUI() {
            EditorGUILayout.LabelField("Proton Client Delay", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("The time between opening multiple clients");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("");

            int ms = EditorPrefs.GetInt("LinuxPls_ClientTimeout", 3000);
            ms = EditorGUILayout.IntField("Delay In Miliseconds:", ms);
            EditorPrefs.SetInt("LinuxPls_ClientTimeout", ms);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Consider increasing this if you encounter a black screen when opening multiple clients");
            EditorGUILayout.Separator();
        }
        
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(VRCSdkControlPanel), "ShowSettings")]
        // wait until the vrchat install path gui is about to be created, and insert our proton gui
        public static IEnumerable<CodeInstruction> ShowSettingsTranspiler(IEnumerable<CodeInstruction> instructions) {
            var codes = instructions.ToList();
            for (var i = 0; i < codes.Count; i++) {
                // skip codes that arent calling anything
                if (codes[i].opcode != OpCodes.Call) {
                    continue;
                }
                // insert into our target code and break the loop
                if (codes[i].operand.ToString().Contains("OnVRCInstallPathGUI")) {
                    codes.Insert(i, CodeInstruction.Call(typeof(UI), nameof(OnProtonInstallPathGUI)));
                    codes.Insert(i, CodeInstruction.Call(typeof(UI), nameof(OnProtonDelayGUI)));
                    break;
                }
            }

            return codes.AsEnumerable();
        }
    }
}