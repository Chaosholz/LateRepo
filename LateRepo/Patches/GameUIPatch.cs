using HarmonyLib;
using UnityEngine;

// --- Auto Regionsauswahl ---

namespace LateRepo.Patches {
    [HarmonyPatch(typeof(MenuPageRegions))]
    internal static class MenuPageRegionPatch {
        [HarmonyPatch(typeof(MenuPageRegions), "Start")]
        [HarmonyPrefix]
        private static void MenuPageRegionStartPatch(MenuPageRegions __instance) {
            __instance.PickRegion("");
        }

        [HarmonyPatch(typeof(MenuPagePublicGameChoice), "ExitPage")]
        [HarmonyPrefix]
        private static bool PublicExitPagePatch() {
            MenuManager.instance.PageCloseAll();
            MenuManager.instance.PageOpen(MenuPageIndex.Main, false);
            return false;
        }
    }

    // --- PopUp Schließer ---

    [HarmonyPatch(typeof(MenuManager))]
    internal static class PopUpPatch {
        [HarmonyPatch(typeof(MenuManager), "PagePopUpTwoOptions")]
        [HarmonyPrefix]
        private static bool PagePopUpTwoOptionsPatch(
            MenuButtonPopUp menuButtonPopUp,
            string popUpHeader,
            Color popUpHeaderColor,
            string popUpText,
            string option1Text,
            string option2Text,
            bool richText) {
            // PRIVATE GAME PopUp
            if (Plugin.PlayGamePopup.Value) {
                if (IsPrivateGamePopup(popUpHeader, popUpText)) {
                    menuButtonPopUp.option1Event?.Invoke();
                    return false;
                }

                // PUBLIC GAME PopUp
                if (IsPublicGamePopup(popUpHeader, popUpText)) {
                    menuButtonPopUp.option1Event?.Invoke();
                    return false;
                }
            }
            if (Plugin.StartGamePopup.Value) {
                // START GAME PopUp
                if (IsStartGamePopup(popUpHeader, popUpText)) {
                    menuButtonPopUp.option1Event?.Invoke();
                    return false;
                }
            }
            if (Plugin.LoadSavePopup.Value) {
                // LOAD SAVE PopUp
                if (IsLoadSavePopup(popUpHeader, popUpText)) {
                    menuButtonPopUp.option1Event?.Invoke();
                    return false;
                }
            }

            return true;
        }
        private static bool IsPrivateGamePopup(string header, string body) {
            header = header.ToLower();
            body = body.ToLower();

            return header.Contains("host")
                || body.Contains("best computer")
                || body.Contains("are you this friend");
        }

        private static bool IsPublicGamePopup(string header, string body) {
            header = header.ToLower();
            body = body.ToLower();

            return header.Contains("warning")
                || body.Contains("random players")
                || body.Contains("mean people");
        }

        private static bool IsStartGamePopup(string header, string body) {
            header = header.ToLower();
            body = body.ToLower();

            return header.Contains("start")
                || body.Contains("everyone ready")
                || body.Contains("once the game");
        }

        private static bool IsLoadSavePopup(string header, string body) {
            header = header.ToLower();
            body = body.ToLower();

            return header.Contains("load")
                || body.Contains("load this save");
        }
    }

    // --- Auto Password ---

    [HarmonyPatch(typeof(MenuPagePassword))]
    internal static class MenuPagePasswordPatch {

        // --- Variablen ---
        private static bool passwordAlreadySet = false;

        [HarmonyPatch(typeof(MenuPagePassword), "Update")]
        [HarmonyPrefix]
        private static void PasswordSkipPatch(MenuPagePassword __instance) {
            if (Plugin.PasswordSkip.Value) {
                if (!string.IsNullOrEmpty(Plugin.PasswordSet.Value)) {
                    if (string.IsNullOrEmpty(Plugin.PasswordSet.Value) || passwordAlreadySet) {
                        return;
                    }

                    string autoPassword = Plugin.PasswordSet.Value.ToUpper().Replace("\n", "").Replace("\r", "").Replace(" ", "");

                    var passwordField = AccessTools.Field(typeof(MenuPagePassword), "password");
                    passwordField.SetValue(__instance, autoPassword);

                    passwordAlreadySet = true;

                    Plugin.logger.LogInfo("[LateRepo] Automatisches Passwort gesetzt: " + autoPassword);
                }
                __instance.ConfirmButton();
            }
        }

        [HarmonyPatch(typeof(MenuPagePassword), "Start")]
        [HarmonyPrefix]
        private static void ResetFlag() {
            passwordAlreadySet = false;
        }
    }
}