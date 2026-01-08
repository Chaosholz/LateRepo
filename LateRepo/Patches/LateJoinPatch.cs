using ExitGames.Client.Photon;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LateRepo.Patches {
    // === Late Join code ===
    internal static class LateJoinPatch {
        // --- Felder für Photon intern ---
        private static readonly FieldInfo removeFilterFieldInfo = AccessTools.Field(typeof(PhotonNetwork), "removeFilter");
        private static readonly FieldInfo keyByteSevenFieldInfo = AccessTools.Field(typeof(PhotonNetwork), "keyByteSeven");
        private static readonly FieldInfo serverCleanOptionsFieldInfo = AccessTools.Field(typeof(PhotonNetwork), "ServerCleanOptions");
        private static readonly MethodInfo raiseEventInternalMethodInfo = AccessTools.Method(typeof(PhotonNetwork), "RaiseEventInternal");

        // --- Hook-Objekte ---
        private static Hook changeLevelHook;
        private static Hook spawnHook;
        private static Hook levelGenHook;
        private static Hook startHook;

        // --- Variablen ---
        public static bool canJoin;

        // --- Initialisierung ---
        public static void InitializeHooks() {
            changeLevelHook = new Hook(
                AccessTools.Method(typeof(RunManager), "ChangeLevel"),
                typeof(LateJoinPatch).GetMethod(nameof(ChangeLevelHook))
            );

            spawnHook = new Hook(
                AccessTools.Method(typeof(PlayerAvatar), "Spawn"),
                typeof(LateJoinPatch).GetMethod(nameof(SpawnHook))
            );

            levelGenHook = new Hook(
                AccessTools.Method(typeof(LevelGenerator), "Start"),
                typeof(LateJoinPatch).GetMethod(nameof(LevelGeneratorHook))
            );

            startHook = new Hook(
                AccessTools.Method(typeof(PlayerAvatar), "Start"),
                typeof(LateJoinPatch).GetMethod(nameof(PlayerAvatarStartHook))
            );
        }

        // --- ChangeLevel ---
        public static void ChangeLevelHook(Action<RunManager, bool, bool, RunManager.ChangeLevelType> orig,
            RunManager self, bool _completedLevel, bool _levelFailed, RunManager.ChangeLevelType _changeLevelType) {
            if (_levelFailed || !PhotonNetwork.IsMasterClient) {
                orig.Invoke(self, _completedLevel, _levelFailed, _changeLevelType);
                return;
            }

            var runManagerPUN = AccessTools.Field(typeof(RunManager), "runManagerPUN").GetValue(self);
            var runManagerPhotonView = AccessTools.Field(typeof(RunManagerPUN), "photonView").GetValue(runManagerPUN) as PhotonView;
            PhotonNetwork.RemoveBufferedRPCs(runManagerPhotonView!.ViewID);

            foreach (var photonView in UnityEngine.Object.FindObjectsOfType<PhotonView>()) {
                if (photonView.gameObject.scene.buildIndex == -1)
                    continue;
                ClearPhotonCache(photonView);
            }

            orig.Invoke(self, _completedLevel, false, _changeLevelType);

            canJoin = SemiFunc.RunIsLobbyMenu() || SemiFunc.RunIsLobby() || (SemiFunc.RunIsShop() && Plugin.JoinShop.Value);

            if (canJoin) {
                SteamManager.instance.UnlockLobby(true);
                PhotonNetwork.CurrentRoom.IsOpen = true;
                Plugin.logger.LogInfo($"[LateRepo] Lobbystatus geändert: offen");
                if (!PopUpPatch.isPublicGame) {
                    return;
                } else {
                    Plugin.logger.LogInfo("Public");
                    PhotonNetwork.CurrentRoom.IsVisible = true;
                    GameManager.instance.SetConnectRandom(true);
                }
            } else {
                SteamManager.instance.LockLobby();
                PhotonNetwork.CurrentRoom.IsOpen = false;
                if (!PopUpPatch.isPublicGame) {
                    return;
                } else {
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                    GameManager.instance.SetConnectRandom(false);
                    Plugin.logger.LogInfo("nicht Public");
                }
            }
        }


        // --- PlayerAvatar Spawn ---
        public static void SpawnHook(Action<PlayerAvatar, Vector3, Quaternion> orig,
            PlayerAvatar self, Vector3 position, Quaternion rotation) {
            var spawnedField = AccessTools.Field(typeof(PlayerAvatar), "spawned");
            if ((bool)spawnedField.GetValue(self))
                return;
            orig.Invoke(self, position, rotation);
        }

        // --- LevelGenerator ---
        public static void LevelGeneratorHook(Action<LevelGenerator> orig, LevelGenerator self) {
            if (PhotonNetwork.IsMasterClient && SemiFunc.RunIsLobby()) {
                PhotonNetwork.RemoveBufferedRPCs(self.PhotonView.ViewID);
            }
            orig.Invoke(self);
        }

        // --- PlayerAvatar Start ---
        public static void PlayerAvatarStartHook(Action<PlayerAvatar> orig, PlayerAvatar self) {
            orig.Invoke(self);

            if (!PhotonNetwork.IsMasterClient || SemiFunc.RunIsLobby()) {
                return;
            }
            self.photonView.RPC("LoadingLevelAnimationCompletedRPC", RpcTarget.AllBuffered);
        }

        // --- Cache Cleanup ---
        private static void ClearPhotonCache(PhotonView photonView) {
            try {
                var removeFilter = removeFilterFieldInfo.GetValue(null) as ExitGames.Client.Photon.Hashtable;
                var keyByteSeven = keyByteSevenFieldInfo.GetValue(null);
                var serverCleanOptions = serverCleanOptionsFieldInfo.GetValue(null) as RaiseEventOptions;

                removeFilter![keyByteSeven] = photonView.InstantiationId;
                serverCleanOptions!.CachingOption = EventCaching.RemoveFromRoomCache;

                raiseEventInternalMethodInfo.Invoke(null, [(byte)202, removeFilter, serverCleanOptions, SendOptions.SendReliable]);
            } catch (Exception ex) {
                Plugin.logger.LogError($"[LateRepo] ClearPhotonCache Fehler: {ex.Message}");
            }
        }
    }

    // === Escape Invite Button ===
    [HarmonyPatch(typeof(MenuPageEsc))]
    internal static class MenuPageEscPatch {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void MenuPageStartPatch(MenuPageEsc __instance) {
            if (!Plugin.JoinButton.Value) return;
            if (!LateJoinPatch.canJoin) return;
            if (__instance == null) return;

            // MAIN MENU Button finden
            MenuButton mainMenuBtn = FindButtonByLabel(__instance.transform, "MAIN MENU");
            if (mainMenuBtn == null) {
                Plugin.logger.LogWarning("[LateRepo] MAIN MENU Button nicht gefunden.");
                return;
            }

            Transform parent = mainMenuBtn.transform.parent;
            if (parent == null) return;

            if (FindButtonByLabel(parent, "INVITE") != null) return;

            // Klonen
            GameObject inviteGO = UnityEngine.Object.Instantiate(mainMenuBtn.gameObject, parent);

            var popup = inviteGO.GetComponent<MenuButtonPopUp>();
            if (popup != null) UnityEngine.Object.Destroy(popup);

            var tmp = inviteGO.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null) {
                tmp.text = "INVITE";
                ButtonToTMPText(inviteGO, tmp, padX: 2f, padY: 2f);
            }

            // Optional
            var menuBtn = inviteGO.GetComponent<MenuButton>();
            menuBtn?.buttonTextString = "INVITE";

            var unityBtn = inviteGO.GetComponent<Button>();
            if (unityBtn != null) {
                unityBtn.onClick.RemoveAllListeners();
                unityBtn.onClick.AddListener(() => {
                    if (SteamManager.instance != null)
                        SteamManager.instance.OpenSteamOverlayToInvite();
                    else
                        Plugin.logger.LogWarning("[LateRepo] SteamManager.instance ist null.");
                });
            }

            inviteGO.transform.SetSiblingIndex(mainMenuBtn.transform.GetSiblingIndex() + 1);

            var vlg = parent.GetComponent<VerticalLayoutGroup>();
            var hlg = parent.GetComponent<HorizontalLayoutGroup>();

            if (vlg == null && hlg == null) {
                RectTransform mainRT = mainMenuBtn.GetComponent<RectTransform>();
                RectTransform newRT = inviteGO.GetComponent<RectTransform>();

                if (mainRT != null && newRT != null) {
                    float gap = 4f;
                    float yStep = mainRT.rect.height + gap;
                    newRT.anchoredPosition = mainRT.anchoredPosition + new Vector2(0f, -yStep);

                    LayoutRebuilder.ForceRebuildLayoutImmediate(newRT);
                }
            }
        }
        private static void ButtonToTMPText(GameObject buttonGO, TextMeshProUGUI tmp, float padX = 2f, float padY = 2f) {
            if (buttonGO == null || tmp == null) return;

            tmp.ForceMeshUpdate();

            Vector2 pref = tmp.GetPreferredValues(tmp.text);

            RectTransform btnRT = buttonGO.GetComponent<RectTransform>();
            if (btnRT == null) return;

            float targetW = pref.x + padX;
            float targetH = Mathf.Max(btnRT.sizeDelta.y, pref.y + padY);

            btnRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetW);
            btnRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetH);

            RectTransform textRT = tmp.GetComponent<RectTransform>();
            if (textRT != null) {
                textRT.anchorMin = new Vector2(0f, 0f);
                textRT.anchorMax = new Vector2(1f, 1f);
                textRT.offsetMin = Vector2.zero;
                textRT.offsetMax = Vector2.zero;
            }

            var le = buttonGO.GetComponent<LayoutElement>();
            if (le != null) {
                le.preferredWidth = targetW;
                le.preferredHeight = targetH;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(btnRT);
        }

        private static MenuButton FindButtonByLabel(Transform root, string label) {
            foreach (var btn in root.GetComponentsInChildren<MenuButton>(true)) {
                var tmp = btn.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp != null && string.Equals(tmp.text?.Trim(), label, StringComparison.OrdinalIgnoreCase))
                    return btn;
            }
            return null;
        }
    }
}

