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
        public const string ModVersion = "1.6.3";
        private static Harmony _harmony = new(ModName);
        internal static readonly ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        public static ConfigEntry<Boolean> JoinButton;
        public static ConfigEntry<Boolean> JoinShop;
        private static ConfigEntry<Boolean> splashScreen;
        private static ConfigEntry<Boolean> moonPhase;
        public static ConfigEntry<Boolean> PlayGamePopup;
        public static ConfigEntry<Boolean> StartGamePopup;
        public static ConfigEntry<Boolean> LoadSavePopup;
        private static ConfigEntry<Boolean> PickRegion;
        public static ConfigEntry<Boolean> PasswordSkip;
        public static ConfigEntry<String> PasswordSet;

        private void Awake() {
            logger.LogInfo("[LateRepo] Initialisiere Patches …");

            JoinButton = Config.Bind("Late Join", "Invite Button", true, "If true, a button appears in the Escape menu that opens the Steam overlay for inviting players.");
            JoinShop = Config.Bind("Late Join", "Late Join Shop", true, "If it’s true, players can late join in the shop.");
            PickRegion = Config.Bind("Lobby Settings", "Auto Region", true, "If true, it automatically picks the best region, so it doesn’t ask you to choose a region anymore.");
            PasswordSkip = Config.Bind("Lobby Settings", "Passwort Skip", true, "If true, it automatically bypasses the password input screen.");
            PasswordSet = Config.Bind("Lobby Settings", "SetPasswort", "", "Enter the password you want for your private lobby. If it’s empty, no password will be set. This only works if Password Skip is true.");
            splashScreen = Config.Bind("Popup", "Splash Screen", true, "If true, the splash screen at the beginning is skipped.");
            moonPhase = Config.Bind("Popup", "Moon Phase Change", true, "If true,");
            PlayGamePopup = Config.Bind("Popup", "Play Game Popup", true, "If true, the pop-ups for “Host a Game?” and “Warning: Playing with random players” are automatically accepted when you click PUBLIC or PRIVATE GAME, so they don’t appear anymore.");
            LoadSavePopup = Config.Bind("Popup", "Load Save Popup", false, "If true, the pop-up for “Load Save?” is automatically accepted when you click LOAD SAVE, so it doesn’t appear anymore.");
            StartGamePopup = Config.Bind("Popup", "Start Game Popup", true, "If true, the pop-up for “Start Game” is automatically accepted when you click START GAME, so it doesn’t appear anymore.");

            LateJoinPatch.InitializeHooks();
            PatchAllStuff();

            logger.LogInfo("[LateRepo] erfolgreich geladen!");
        }

        private void PatchAllStuff() {
            if (splashScreen.Value) {
                _harmony.PatchAll(typeof(LevelGeneratorPatch));
            }

            if (PickRegion.Value) {
                _harmony.PatchAll(typeof(MenuPageRegionPatch));
            }
            if (StartGamePopup.Value || PlayGamePopup.Value || LoadSavePopup.Value) {
                _harmony.PatchAll(typeof(PopUpPatch));
            }
            if (PasswordSkip.Value || !string.IsNullOrEmpty(PasswordSet.Value)) {
                _harmony.PatchAll(typeof(MenuPagePasswordPatch));
            }

            _harmony.PatchAll(typeof(PlayerNameCheckerUpdatePatch));
            _harmony.PatchAll(typeof(MenuPageEscPatch));

            if (moonPhase.Value) {
                _harmony.PatchAll(typeof(MoonUIPatch));
            }
        }
    }
}
