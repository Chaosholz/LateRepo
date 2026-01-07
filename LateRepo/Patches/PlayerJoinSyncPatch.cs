using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using UnityEngine;


namespace LateRepo.Patches {

    [HarmonyPatch(typeof(NetworkManager))]
    internal class PlayerJoinSyncPatch {

        // Variablen //
        private const string FieldLoadingDone = "loadingDone";
        private const string RpcALlPlayersSpawned = "AllPlayerSpawnedRPC";

        [HarmonyPatch(typeof(NetworkManager), "OnPlayerEnteredRoom")]
        [HarmonyPostfix]
        public static void OnPlayerEnteredRoomPatch(NetworkManager __instance, Player newPlayer) {
            try {
                if (!PhotonNetwork.IsMasterClient) {
                    return;
                }
                var loadingDOneField = AccessTools.Field(typeof(NetworkManager), FieldLoadingDone);
                if (loadingDOneField != null) {
                    loadingDOneField.SetValue(__instance, false);
                }
                __instance.StartCoroutine(ResendAllPlayerSpawnDelayed(__instance, 1.0f));

            } catch (Exception e) {
                Plugin.logger.LogError($"[LateJoinSync] Error in OnPlayerEnteredRoomPatch {e}");
            }
        }

        private static IEnumerator ResendAllPlayerSpawnDelayed(NetworkManager nm, float delaySeconds) {
            yield return new WaitForSeconds(delaySeconds);
            if (nm = null) {
                yield break;
            }
            if (!PhotonNetwork.IsMasterClient) {
                yield break ;
            }
            PhotonView pv = nm.GetComponent<PhotonView>();
            if (pv == null) {
                yield break;
            }

            pv.RPC(RpcALlPlayersSpawned, RpcTarget.AllBuffered, []);

            Plugin.logger.LogDebug("Resent AllPlayersSpawnedRPC");
        }
    }
}
