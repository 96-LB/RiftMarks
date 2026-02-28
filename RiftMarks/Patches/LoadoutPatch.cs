using HarmonyLib;
using RiftOfTheNecroManager;
using Shared;
using Shared.TrackSelection;
using TicToc.Localization.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RiftMarks.Patches;


public class LoadoutState : State<LoadoutScreenManager, LoadoutState> {
    public bool HasInitialized { get; private set; } = false;
    
    public MetadataState? Metadata => Instance._trackMetadata?.Pipe(MetadataState.Of);
    public RiftMarkList? CurrentMarkList => Metadata?.GetMarks(Instance._currentDifficulty);
    public SliderData? Slider => Instance._practiceBeatRangeSlider?.Pipe(SliderData.Of);
    
    public void Initialize() {
        if(HasInitialized || Slider is null) {
            return;
        }
        HasInitialized = true;
        
        Instance.OnDifficultyChanged -= HandleDifficultyChanged;
        Instance.OnDifficultyChanged += HandleDifficultyChanged;
        
        Slider.InitializeSliders();
        
        var label = Object.Instantiate(Instance._practiceBeatRangeSlider._textLabel, Instance._practiceModeExtraOptionsObject.transform);
        Object.Destroy(label.GetComponent<BaseLocalizer>());
        Object.Destroy(label.GetComponent<ContentSizeFitter>());
        foreach(Transform child in label.transform) {
            Object.Destroy(child.gameObject);
        }
        
        label.alignment = TextAlignmentOptions.BaselineLeft;
        label.enableWordWrapping = false;
        label.fontStyle &= ~FontStyles.Bold & ~FontStyles.UpperCase;
        label.fontSize *= 0.5f;
        
        var transform = label.GetComponent<RectTransform>();
        transform.anchorMin = new(0, 0);
        transform.anchorMax = new(0, 0);
        transform.anchoredPosition = new(465, 50);
        
        Slider.SetLabel(label);
    }
    
    public void UpdateSlider() {
        if(Slider is not null) {
            Slider.CurrentMarkList = CurrentMarkList;
            Slider.MaxBeats = Mathf.CeilToInt(Instance._totalBeats);
            Slider.SetMarkMode(true);
            Slider.InitializePracticeBeatRange();
        }
    }
    
    public void HandleDifficultyChanged(Difficulty difficulty) {
        UpdateSlider();
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
    
    [HarmonyPatch(nameof(LoadoutScreenManager.InitializePracticeBeatRange))]
    [HarmonyPostfix]
    public static void InitializePracticeBeatRange(LoadoutScreenManager __instance) {
        var state = LoadoutState.Of(__instance);
        state.Slider?.InitializePracticeBeatRange();
    }
}
