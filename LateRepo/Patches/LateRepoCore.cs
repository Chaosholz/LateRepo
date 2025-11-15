using ExitGames.Client.Photon;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Reflection;
using UnityEngine;

namespace LateRepo.Patches {
    internal static class LateRepoCore {
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


        // === Initialisierung ===
        public static void InitializeHooks() {
            changeLevelHook = new Hook(
                AccessTools.Method(typeof(RunManager), "ChangeLevel"),
                typeof(LateRepoCore).GetMethod(nameof(ChangeLevelHook))
            );

            spawnHook = new Hook(
                AccessTools.Method(typeof(PlayerAvatar), "Spawn"),
                typeof(LateRepoCore).GetMethod(nameof(SpawnHook))
            );

            levelGenHook = new Hook(
                AccessTools.Method(typeof(LevelGenerator), "Start"),
                typeof(LateRepoCore).GetMethod(nameof(LevelGeneratorHook))
            );

            startHook = new Hook(
                AccessTools.Method(typeof(PlayerAvatar), "Start"),
                typeof(LateRepoCore).GetMethod(nameof(PlayerAvatarStartHook))
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

            bool canJoin = SemiFunc.RunIsLobbyMenu() || SemiFunc.RunIsLobby();

            if (canJoin) {
                SteamManager.instance.UnlockLobby(true);
                Plugin.logger.LogInfo($"[LateRepo] Lobbystatus geändert: offen");
            } else {
                SteamManager.instance.LockLobby();
            }

            PhotonNetwork.CurrentRoom.IsOpen = canJoin;

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

                raiseEventInternalMethodInfo.Invoke(null, new object[] { (byte)202, removeFilter, serverCleanOptions, SendOptions.SendReliable });
            } catch (Exception ex) {
                Plugin.logger.LogWarning($"[LateRepo] ClearPhotonCache Fehler: {ex.Message}");
            }
        }
    }
}
