using HarmonyLib;
using Shared;
using Shared.SceneLoading.Payloads;
using UnityEngine;

namespace RiftMarks.Patches;


public class PauseState : State<PauseScreen, PauseState> {
    public bool HasInitialized { get; private set; } = false;
    public int OriginalStartBeat { get; private set; }
    public int OriginalEndBeat { get; private set; }
    public bool HasChangedPracticeRange => Mathf.FloorToInt(Instance._practiceStartBeat) != OriginalStartBeat
                                           || Mathf.CeilToInt(Instance._practiceEndBeat) != OriginalEndBeat;

    public RhythmRiftScenePayload? Payload => Instance._currentScenePayload as RhythmRiftScenePayload;
    public MetadataState? Metadata => Payload?._trackMetadata?.Pipe(MetadataState.Of);
    public RiftMarkList? CurrentMarkList => Metadata?.GetMarks(Instance._currentDifficulty);
    public SliderData? Slider => Instance._practiceBeatRangeSlider?.Pipe(SliderData.Of);


    public void Initialize() {
        if(HasInitialized || Slider is null) {
            return;
        }
        Slider.InitializeSliders();

        OriginalStartBeat = Mathf.FloorToInt(Instance._practiceStartBeat);
        OriginalEndBeat = Mathf.CeilToInt(Instance._practiceEndBeat);

        Slider.CurrentMarkList = CurrentMarkList;
        Slider.MaxBeats = Mathf.CeilToInt(Instance._totalBeats);

        Slider.SetMarkMode(false);
        Slider.Instance.SetCurrentValueMin(OriginalStartBeat);
        Slider.Instance.SetCurrentValueMax(OriginalEndBeat);
        Slider.ToggleMarkMode(playSfx: false); // TODO: this should only be true if mark mode was on when selecting practice range

        Instance._hasChangedPracticeBeatRange = false;

        HasInitialized = true;
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

    [HarmonyPatch(nameof(PauseScreen.HandlePracticeBeatRangeChanged))]
    [HarmonyPostfix]
    public static void HandlePracticeBeatRangeChanged(PauseScreen __instance) {
        var state = PauseState.Of(__instance);
        __instance._hasChangedPracticeBeatRange = state.HasChangedPracticeRange;
    }
}
