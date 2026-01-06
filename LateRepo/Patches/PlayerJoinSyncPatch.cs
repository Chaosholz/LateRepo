using HarmonyLib;
using Photon.Realtime;


namespace LateRepo.Patches {
    
    [HarmonyPatch(typeof(NetworkManager))]
    internal class PlayerJoinSyncPatch {
        [HarmonyPatch(typeof(NetworkManager), "OnPlayerEnteredRoom")]
        [HarmonyPrefix]
        public static void OnPlayerEnteredRoomPatch(Player newPlayer) {

        }
    }
}
