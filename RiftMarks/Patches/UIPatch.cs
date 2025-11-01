using HarmonyLib;
using RhythmRift;

namespace RiftMarks.Patches;



[HarmonyPatch(typeof(RRStageUIView))]
public static class UIPatch {
    [HarmonyPatch(nameof(RRStageUIView.UpdateScore))]
    [HarmonyPostfix]
    public static void UpdateScore(RRStageUIView __instance) {
        __instance._scoreText.overflowMode = TMPro.TextOverflowModes.Overflow;
        __instance._scoreText.enableWordWrapping = false;
    }
}
