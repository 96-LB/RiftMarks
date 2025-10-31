using BepInEx.Configuration;
using HarmonyLib;
using Shared.MenuOptions;
using Shared.TrackSelection;
using UnityEngine;

namespace RiftMarks.Patches;



public class LoadoutState : State<LoadoutScreenManager, LoadoutState> {
    public MetadataState? Metadata => Instance._trackMetadata?.Pipe(MetadataState.Of);
    public RiftMarkList? CurrentMarkList => Metadata?.GetMarks(Instance._currentDifficulty);
    public bool SelectionHasMarks => CurrentMarkList?.HasMarks ?? false;
    public int CurrentMarkCount => CurrentMarkList?.MarkCount ?? 0;
    public bool UsingMarks => SelectionHasMarks && MarkModeEnabled;

    public RangeSliderOptionController? Slider => Instance._practiceBeatRangeSlider;
    public SliderData? MinSlider => Instance._practiceBeatRangeSlider?.MinControlOption?.Pipe(SliderData.Of);
    public SliderData? MaxSlider => Instance._practiceBeatRangeSlider?.MaxControlOption?.Pipe(SliderData.Of);

    public bool MarkModeEnabled { get; private set; }
    public bool InitializedSliders { get; private set; }

    public void InitializeSliders() {
        if(InitializedSliders) {
            return;
        }
        InitializedSliders = true;

        MinSlider?.Pipe(x => x.OnModeSwitch += ToggleMarkMode);
        MaxSlider?.Pipe(x => x.OnModeSwitch += ToggleMarkMode);
    }

    public void SetMarkMode(bool enabled) {
        MarkModeEnabled = enabled;
        Instance.InitializePracticeBeatRange();
    }

    public void ToggleMarkMode() {
        var newMin = Slider?.CurrentValueMin ?? 0;
        var newMax = Slider?.CurrentValueMax ?? Slider?.SliderMax ?? Mathf.CeilToInt(Instance._totalBeats);
        SetMarkMode(!MarkModeEnabled);
        if(SelectionHasMarks) {
            if(MarkModeEnabled) {
                newMin = CurrentMarkList!.GetIndex(newMin);
                newMax = CurrentMarkList!.GetIndex(newMax) + 1;
            } else {
                newMin = CurrentMarkList!.GetBeat(newMin);
                newMax = CurrentMarkList!.GetBeat(newMax) - 1;
            }
            // TODO: play sound effect
        } else {
            // TODO: play error sound effect
        }
        Slider?.SetCurrentValueMin(newMin);
        Slider?.SetCurrentValueMax(newMax);
    }

    public void InitializePracticeBeatRange() {
        if(!UsingMarks || Slider is null) {
            return;
        }

        var max = CurrentMarkCount;
        Slider.SetSliderMinimumDifference(1);
        Slider.SetSliderBounds(0, max);
        Slider.SetCurrentValueMax(max);
        Slider.SetCurrentValueMin(0);
    }
}

[HarmonyPatch(typeof(LoadoutScreenManager))]
public static class LoadoutPatch {
    
    [HarmonyPatch(nameof(LoadoutScreenManager.ConfigureSelectableOptions))]
    [HarmonyPostfix]
    public static void ConfigureSelectableOptions(LoadoutScreenManager __instance) {
        var state = LoadoutState.Of(__instance);
        state.InitializeSliders();
    }

    [HarmonyPatch(nameof(LoadoutScreenManager.ShowImpl))]
    [HarmonyPostfix]
    public static void ShowImpl(LoadoutScreenManager __instance) {
        var state = LoadoutState.Of(__instance);
        state.SetMarkMode(true);
    }
    
    [HarmonyPatch(nameof(LoadoutScreenManager.InitializePracticeBeatRange))]
    [HarmonyPostfix]
    public static void InitializePracticeBeatRange(LoadoutScreenManager __instance) {
        var state = LoadoutState.Of(__instance);
        state.InitializePracticeBeatRange();
    }

    [HarmonyPatch(nameof(LoadoutScreenManager.HandlePracticeBeatRangeChanged))]
    [HarmonyPrefix]
    public static void HandlePracticeBeatRangeChanged(LoadoutScreenManager __instance, ref int min, ref int max) {
        var state = LoadoutState.Of(__instance);
        if(!state.UsingMarks) {
            return;
        }
        min = Mathf.Clamp(state.CurrentMarkList!.GetBeat(min), 0, Mathf.CeilToInt(__instance._totalBeats));
        max = Mathf.Clamp(state.CurrentMarkList!.GetBeat(max), min, Mathf.CeilToInt(__instance._totalBeats));
    }
}
