using HarmonyLib;
using Shared;
using Shared.SceneLoading.Payloads;
using TicToc.Localization.Components;
using UnityEngine;

namespace RiftMarks.Patches;


public class PauseState : State<PauseScreen, PauseState> {
    public bool HasInitialized { get; private set; }
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
        HasInitialized = true;

        Slider.InitializeSliders();

        OriginalStartBeat = Mathf.FloorToInt(Instance._practiceStartBeat);
        OriginalEndBeat = Mathf.CeilToInt(Instance._practiceEndBeat);
        
        Slider.CurrentMarkList = CurrentMarkList;
        Slider.MaxBeats = Mathf.CeilToInt(Instance._totalBeats);

        var markMode = SliderData.LastMarkMode;
        Slider.SetMarkMode(false);
        Slider.Instance.SetCurrentValueMin(OriginalStartBeat);
        Slider.Instance.SetCurrentValueMax(OriginalEndBeat);
        if(markMode) {
            Slider.ToggleMarkMode(playSfx: false);
        }

        Instance._hasChangedPracticeBeatRange = false;

        var title = Object.Instantiate(Instance._practiceModeSpeedCarousel._title, Instance._contentParent.transform);
        Object.Destroy(title.GetComponent<BaseLocalizer>());
        foreach(Transform child in title.transform) {
            Object.Destroy(child.gameObject);
        }

        title.GetComponent<RectTransform>().anchoredPosition = new(0, 290);
        
        Slider.SetLabel(title);
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
