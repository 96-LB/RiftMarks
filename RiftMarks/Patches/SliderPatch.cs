using HarmonyLib;
using RhythmRift;
using RiftOfTheNecroManager;
using Shared.Audio;
using Shared.MenuOptions;
using System;
using TMPro;
using UnityEngine;

namespace RiftMarks.Patches;


public class SliderData : State<RangeSliderOptionController, SliderData> {
    public static bool LastMarkMode { get; private set; }

    public RiftMarkList? CurrentMarkList { get; set; }
    public int MaxBeats { get; set; }
    public bool MarkModeEnabled { get; private set; }
    public TMP_Text? Label { get; private set; }

    public Color BeatModeFillColor { get; private set; } = Color.clear;
    public Color BeatModeBackgroundColor { get; private set; } = Color.clear;
    public Color MarkModeFillColor { get; private set; } = new(0.2f, 0.8f, 1.0f);
    public Color MarkModeBackgroundColor { get; private set; } = new(0.3f, 0.4f, 0.5f);
    
    public bool SelectionHasMarks => CurrentMarkList?.HasMarks ?? false;
    public int CurrentMarkCount => CurrentMarkList?.MarkCount ?? 0;
    public bool UsingMarks => SelectionHasMarks && MarkModeEnabled;

    public SliderOptionData? MinOption => Instance.MinControlOption?.Pipe(SliderOptionData.Of);
    public SliderOptionData? MaxOption => Instance.MaxControlOption?.Pipe(SliderOptionData.Of);


    public void InitializeSliders() {
        MinOption?.Pipe(x => {
            x.OnModeSwitch -= ToggleMarkMode;
            x.OnModeSwitch += ToggleMarkMode;
        });
        MaxOption?.Pipe(x => {
            x.OnModeSwitch -= ToggleMarkMode;
            x.OnModeSwitch += ToggleMarkMode;
        });
    }

    public void SetMarkMode(bool enabled) {
        LastMarkMode = MarkModeEnabled = enabled;
        InitializePracticeBeatRange();
    }

    public void ToggleMarkMode() {
        ToggleMarkMode(true);
    }

    public void ToggleMarkMode(bool playSfx) {
        var min = Instance.CurrentValueMin;
        var max = Instance.CurrentValueMax;
        SetMarkMode(!MarkModeEnabled);
        if(SelectionHasMarks) {
            if(MarkModeEnabled) {
                (min, max) = BeatToMarkRange(min, max);
            } else {
                (min, max) = MarkToBeatRange(min, max);
            }
        }
        if(playSfx) {
            var sfx = UsingMarks ? Sfx.SwitchMarkMode : Sfx.MarkModeError;
            AudioManager.Instance.PlayAudioEvent(sfx, shouldApplyLatency: false);
        }
        Instance.SetCurrentValueMin(min);
        Instance.SetCurrentValueMax(max);
    }

    public (int beatMin, int beatMax) MarkToBeatRange(int markMin, int markMax) {
        if(CurrentMarkList is null) {
            throw new InvalidOperationException("No current mark list available for conversion.");
        }
        var beatMin = CurrentMarkList.GetBeat(markMin);
        var beatMax = CurrentMarkList.GetBeat(markMax + 1) - 1;
        beatMin = Mathf.Clamp(beatMin, 0, MaxBeats);
        beatMax = Mathf.Clamp(beatMax, beatMin, MaxBeats);
        return (beatMin, beatMax);
    }

    public (int markMin, int markMax) BeatToMarkRange(int beatMin, int beatMax) {
        if(CurrentMarkList is null) {
            throw new InvalidOperationException("No current mark list available for conversion.");
        }
        var markMin = CurrentMarkList.GetIndex(beatMin);
        var markMax = CurrentMarkList.GetIndex(beatMax);
        return (markMin, markMax);
    }

    public void InitializePracticeBeatRange() {
        var diff = UsingMarks ? 0 : RRUtils.PracticeModeMinimumPracticeModeLength;
        Instance.SetSliderMinimumDifference(diff);
        
        var max = UsingMarks ? CurrentMarkCount : MaxBeats;
        var min = UsingMarks ? 1 : 0;
        Instance.SetSliderBounds(min, max);
        Instance.SetCurrentValueMin(min);
        Instance.SetCurrentValueMax(max);
        
        UpdateColors();
        UpdateLabel();
    }

    public void UpdateColors() {
        if(BeatModeFillColor == Color.clear) {
            BeatModeFillColor = Instance._selectedFillColor;
        }

        if(BeatModeBackgroundColor == Color.clear) {
            BeatModeBackgroundColor = Instance._selectedBackgroundColor;
        }

        Instance._selectedFillColor = UsingMarks ? MarkModeFillColor : BeatModeFillColor;
        Instance._selectedBackgroundColor = UsingMarks ? MarkModeBackgroundColor : BeatModeBackgroundColor;
        Instance.RefreshVisuals();
    }

    public void SetLabel(TMP_Text label) {
        Label = label;
        UpdateLabel();
    }

    public void UpdateLabel() {
        if(Label is null) {
            return;
        }

        if(!UsingMarks) {
            Label.SetText("");
            return;
        }

        var (minBeat, maxBeat) = MarkToBeatRange(Instance.CurrentValueMin, Instance.CurrentValueMax);
        var minText = CurrentMarkList!.GetName(Instance.CurrentValueMin) ?? $"Beat {minBeat}";
        var maxText = CurrentMarkList!.GetName(Instance.CurrentValueMax) ?? $"Beat {maxBeat}";
        var text = Instance._isMinControlSelected ? minText : Instance._isMaxControlSelected ? maxText : "";
        Label.SetText(text);
        Label.color = MarkModeFillColor;
    }
}


[HarmonyPatch(typeof(RangeSliderOptionController))]
public static class SliderPatch {
    [HarmonyPatch(nameof(RangeSliderOptionController.RaiseOnMinMaxChanged))]
    [HarmonyPrefix]
    public static void RaiseOnMinMaxChanged_Pre(RangeSliderOptionController __instance, ref Vector2Int? __state) {
        var state = SliderData.Of(__instance);
        if(state.UsingMarks) {
            __state = new(__instance._sliderValueMin, __instance._sliderValueMax);
            var (min, max) = state.MarkToBeatRange(__instance._sliderValueMin, __instance._sliderValueMax);
            __instance._sliderValueMin = min;
            __instance._sliderValueMax = max;
        }
    }

    [HarmonyPatch(nameof(RangeSliderOptionController.RaiseOnMinMaxChanged))]
    [HarmonyPostfix]
    public static void RaiseOnMinMaxChanged_Post(RangeSliderOptionController __instance, ref Vector2Int? __state) {
        var state = SliderData.Of(__instance);
        if(state.UsingMarks && __state.HasValue) {
            __instance._sliderValueMin = __state.Value.x;
            __instance._sliderValueMax = __state.Value.y;
        }
    }

    [HarmonyPatch(nameof(RangeSliderOptionController.UpdateVisualElements))]
    [HarmonyPostfix]
    public static void UpdateVisualElements(RangeSliderOptionController __instance) {
        var state = SliderData.Of(__instance);
        state.UpdateLabel();
    }
}
