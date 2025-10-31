using HarmonyLib;
using RhythmRift;
using Shared.MenuOptions;
using Shared.TrackData;
using Shared.TrackSelection;
using System;
using UnityEngine;

namespace RiftMarks.Patches;



public class LoadoutState : State<LoadoutScreenManager, LoadoutState> {
    public MetadataState? Metadata => Instance._trackMetadata?.Pipe(MetadataState.Of);
    public RiftMarkList? CurrentMarkList => Metadata?.GetMarks(Instance._currentDifficulty);
    public bool SelectionHasMarks => CurrentMarkList?.HasMarks ?? false;
    public int CurrentMarkCount => CurrentMarkList?.MarkCount ?? 0;

}

[HarmonyPatch(typeof(LoadoutScreenManager))]
public static class LoadoutPatch {
    [HarmonyPatch(nameof(LoadoutScreenManager.InitializePracticeBeatRange))]
    [HarmonyPostfix]
    public static void InitializePracticeBeatRange(LoadoutScreenManager __instance) {
        var state = LoadoutState.Of(__instance);
        var slider = __instance._practiceBeatRangeSlider;
        if(!state.SelectionHasMarks || !slider) {
            return;
        }

        var max = state.CurrentMarkCount;
        slider.SetSliderMinimumDifference(1);
        slider.SetSliderBounds(0, max);
        slider.SetCurrentValueMax(max);
        slider.SetCurrentValueMin(0);
    }

    [HarmonyPatch(nameof(LoadoutScreenManager.HandlePracticeBeatRangeChanged))]
    [HarmonyPrefix]
    public static void HandlePracticeBeatRangeChanged(LoadoutScreenManager __instance, ref int min, ref int max) {
        var state = LoadoutState.Of(__instance);
        if(!state.SelectionHasMarks) {
            return;
        }
        min = Mathf.Clamp(state.CurrentMarkList!.GetBeat(min), 0, Mathf.CeilToInt(__instance._totalBeats));
        max = Mathf.Clamp(state.CurrentMarkList!.GetBeat(max), min, Mathf.CeilToInt(__instance._totalBeats));
    }
}
