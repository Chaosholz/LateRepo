using HarmonyLib;
namespace LateRepo.Patches {

    [HarmonyPatch(typeof(PlayerNameChecker), "Update")]
    internal class PlayerNameCheckerUpdatePatch {
        public static class PlayerNamePatch {

            [HarmonyPrefix]
            private static bool SafeUpdate(PlayerNameChecker __instance) {
                if (__instance == null)
                    return false;

                var type = __instance.GetType();
                var targetField = type.GetField("target", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                var labelField = type.GetField("label", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

                var target = targetField?.GetValue(__instance);
                var label = labelField?.GetValue(__instance);

                if (target == null || label == null) {
                    return false;
                }

                return true;
            }
        }
    }
}
