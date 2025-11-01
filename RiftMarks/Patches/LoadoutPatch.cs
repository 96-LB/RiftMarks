using HarmonyLib;
using Shared.TrackSelection;
using UnityEngine;

namespace RiftMarks.Patches;


public class LoadoutState : State<LoadoutScreenManager, LoadoutState> {
    public MetadataState? Metadata => Instance._trackMetadata?.Pipe(MetadataState.Of);
    public RiftMarkList? CurrentMarkList => Metadata?.GetMarks(Instance._currentDifficulty);
    public SliderData? Slider => Instance._practiceBeatRangeSlider?.Pipe(SliderData.Of);
    
    public void Initialize() {
        Slider?.InitializeSliders();
        Sfx.SwitchMarkMode = Instance._confirmCustomSeedSfxEventRef;
        Sfx.MarkModeError = Instance._deselectCustomSeedSfxEventRef;
    }

    public void UpdateSlider() {
        if(Slider is not null) {
            Slider.CurrentMarkList = CurrentMarkList;
            Slider.MaxBeats = Mathf.CeilToInt(Instance._totalBeats);
            Slider.SetMarkMode(true);
            Slider.InitializePracticeBeatRange();
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
    }
}
