using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LateRepo.Patches;
using System;

namespace LateRepo {
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin {
        public const string ModGUID = "chaos.holz.laterepo";
        public const string ModName = "LateRepo";
        public const string ModVersion = "1.2.3";
        private static Harmony _harmony = new(ModName);
        internal static readonly ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        public static ConfigEntry<Boolean> PlayGamePopup;
        public static ConfigEntry<Boolean> StartGamePopup;
        public static ConfigEntry<Boolean> LoadSavePopup;
        private static ConfigEntry<Boolean> PickRegion;
        public static ConfigEntry<Boolean> PasswordSkip;
        public static ConfigEntry<String> PasswordSet;

        private void Awake() {
            logger.LogInfo("[LateRepo] Initialisiere Hooks …");

            PlayGamePopup = Config.Bind("Popup", "Play Game Popup", true, "Is it true the pop-ups for “Host a Game?” and “Warning: Playing with random players” get automatically accepted when you click PUBLIC or PRIVATE GAME, so they don’t appear anymore");
            LoadSavePopup = Config.Bind("Popup", "Load Save Popup", false, "Is it true the pop-ups for “Load Save?” get automatically accepted when you click LOAD SAVE, so they don’t appear anymore");
            StartGamePopup = Config.Bind("Popup", "Start Game Popup", true, "Is it true the pop-ups for “Start Game” get automatically accepted when you click START GAME, so they don’t appear anymore");
            PickRegion = Config.Bind("Lobby Settings", "Auto Region", true, "Is it true it automatically picks the best region, so it doesn’t ask you to choose a region anymore");
            PasswordSkip = Config.Bind("Lobby Settings", "Passwort Skip", true, "Is it true it automatically bypass the password input screen");
            PasswordSet = Config.Bind("Lobby Settings", "SetPasswort", "", "Enter the password you want for your private lobby. If it’s empty, no password will be set. This only works if Password Skip is true.");

            LateRepoCore.InitializeHooks();
            PatchAllStuff();

            logger.LogInfo("[LateRepo] erfolgreich geladen!");
        }

        private void PatchAllStuff() {
            _harmony.PatchAll(typeof(PlayerNameCheckerUpdatePatch));
            if (PickRegion.Value) {
                _harmony.PatchAll(typeof(MenuPageRegionPatch));
            }
            if (StartGamePopup.Value || PlayGamePopup.Value || LoadSavePopup.Value) {
                _harmony.PatchAll(typeof(PopUpPatch));
            }
            if (PasswordSkip.Value || !string.IsNullOrEmpty(PasswordSet.Value)) {
                _harmony.PatchAll(typeof(MenuPagePasswordPatch));
            }
        }
    }
}
