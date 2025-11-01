using HarmonyLib;
using Shared.TrackSelection;
using UnityEngine;

namespace RiftMarks.Patches;


public class LoadoutState : State<LoadoutScreenManager, LoadoutState> {
    public MetadataState? Metadata => Instance._trackMetadata?.Pipe(MetadataState.Of);
    public RiftMarkList? CurrentMarkList => Metadata?.GetMarks(Instance._currentDifficulty);
    public SliderData? Slider => Instance._practiceBeatRangeSlider?.Pipe(SliderData.Of);
    
    public void Initialize() {
        if(Slider is not null) {
            Slider.InitializeSliders();
            Slider.OnInitializeRange -= Instance.InitializePracticeBeatRange;
            Slider.OnInitializeRange += Instance.InitializePracticeBeatRange;
        }

        Sfx.SwitchMarkMode = Instance._confirmCustomSeedSfxEventRef;
        Sfx.MarkModeError = Instance._deselectCustomSeedSfxEventRef;
    }

    public void UpdateSlider() {
        if(Slider is not null) {
            Slider.SetMarkMode(true);
            Slider.CurrentMarkList = CurrentMarkList;
        }
    }
}

[HarmonyPatch(typeof(LoadoutScreenManager))]
public static class LoadoutPatch {
    
    [HarmonyPatch(nameof(LoadoutScreenManager.ConfigureSelectableOptions))]
    [HarmonyPostfix]
    public static void ConfigureSelectableOptions(LoadoutScreenManager __instance) {
        var state = LoadoutState.Of(__instance);
        state.Initialize();
    }

    [HarmonyPatch(nameof(LoadoutScreenManager.ShowImpl))]
    [HarmonyPostfix]
    public static void ShowImpl(LoadoutScreenManager __instance) {
        var state = LoadoutState.Of(__instance);
        state.UpdateSlider();
        state.Slider?.InitializePracticeBeatRange();
    }

    [HarmonyPatch(nameof(LoadoutScreenManager.HandlePracticeBeatRangeChanged))]
    [HarmonyPrefix]
    public static void HandlePracticeBeatRangeChanged(LoadoutScreenManager __instance, ref int min, ref int max) {
        var state = LoadoutState.Of(__instance);
        if((!state.Slider?.UsingMarks ?? false) && state.CurrentMarkList is not null) {
            min = Mathf.Clamp(state.CurrentMarkList.GetBeat(min), 0, Mathf.CeilToInt(__instance._totalBeats));
            max = Mathf.Clamp(state.CurrentMarkList.GetBeat(max), min, Mathf.CeilToInt(__instance._totalBeats));
        }
    }
}
