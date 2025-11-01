using HarmonyLib;
using Shared;
using Shared.SceneLoading.Payloads;
using UnityEngine;

namespace RiftMarks.Patches;


public class PauseState : State<PauseScreen, PauseState> {
    public RhythmRiftScenePayload? Payload => Instance._currentScenePayload as RhythmRiftScenePayload;
    public MetadataState? Metadata => Payload?._trackMetadata?.Pipe(MetadataState.Of);
    public RiftMarkList? CurrentMarkList => Metadata?.GetMarks(Instance._currentDifficulty);
    public SliderData? Slider => Instance._practiceBeatRangeSlider?.Pipe(SliderData.Of);
    
    public void Initialize() {
        if(Slider is not null) {
            Slider.InitializeSliders();

            // TODO: this didn't work because of initialize practicebeat range
            Slider.Instance.SetCurrentValueMin(Mathf.FloorToInt(Instance._practiceStartBeat));
            Slider.Instance.SetCurrentValueMax(Mathf.CeilToInt(Instance._practiceEndBeat));
            
            Slider.CurrentMarkList = CurrentMarkList;
            Slider.MaxBeats = Mathf.CeilToInt(Instance._totalBeats);
            Slider.SetMarkMode(true); // TODO: this should only be true if mark mode was on when selecting practice range

            Slider.InitializePracticeBeatRange();
        }
    }
}

[HarmonyPatch(typeof(PauseScreen))]
public static class PausePatch {
    
    [HarmonyPatch(nameof(PauseScreen.OnEnable))]
    [HarmonyPostfix]
    public static void OnEnable(PauseScreen __instance) {
        var state = PauseState.Of(__instance);
        state.Initialize();
    }
}
