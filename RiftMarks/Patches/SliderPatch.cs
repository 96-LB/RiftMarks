using HarmonyLib;
using Shared.Audio;
using Shared.MenuOptions;
using System;
using UnityEngine;

namespace RiftMarks.Patches;


public class SliderData : State<RangeSliderOptionController, SliderData> {

    public RiftMarkList? CurrentMarkList { get; set; }
    public int MaxBeats { get; set; }
    public bool MarkModeEnabled { get; private set; }
    public bool InitializedSliders { get; private set; }

    public Color BeatModeFillColor { get; private set; } = Color.clear;
    public Color BeatModeBackgroundColor { get; private set; } = Color.clear;
    public Color MarkModeFillColor { get; private set; } = new(0.4f, 0.8f, 1.0f);
    public Color MarkModeBackgroundColor { get; private set; } = new(0.3f, 0.4f, 0.5f);

    public event Action? OnInitializeRange;

    public bool SelectionHasMarks => CurrentMarkList?.HasMarks ?? false;
    public int CurrentMarkCount => CurrentMarkList?.MarkCount ?? 0;
    public bool UsingMarks => SelectionHasMarks && MarkModeEnabled;

    public SliderOptionData? MinOption => Instance.MinControlOption?.Pipe(SliderOptionData.Of);
    public SliderOptionData? MaxOption => Instance.MaxControlOption?.Pipe(SliderOptionData.Of);


    public void InitializeSliders() {
        if(InitializedSliders) {
            return;
        }
        InitializedSliders = true;

        MinOption?.Pipe(x => x.OnModeSwitch += ToggleMarkMode);
        MaxOption?.Pipe(x => x.OnModeSwitch += ToggleMarkMode);
    }

    public void SetMarkMode(bool enabled) {
        MarkModeEnabled = enabled;
        InitializePracticeBeatRange();
    }

    public void ToggleMarkMode() {
        var newMin = Instance.CurrentValueMin;
        var newMax = Instance.CurrentValueMax;
        SetMarkMode(!MarkModeEnabled);
        if(SelectionHasMarks) {
            if(MarkModeEnabled) {
                newMin = CurrentMarkList!.GetIndex(newMin);
                newMax = CurrentMarkList!.GetIndex(newMax) + 1;
            } else {
                newMin = CurrentMarkList!.GetBeat(newMin);
                newMax = CurrentMarkList!.GetBeat(newMax) - 1;
            }
            AudioManager.Instance.PlayAudioEvent(Sfx.SwitchMarkMode, shouldApplyLatency: false);
        } else {
            AudioManager.Instance.PlayAudioEvent(Sfx.MarkModeError, shouldApplyLatency: false);
        }
        Instance.SetCurrentValueMin(newMin);
        Instance.SetCurrentValueMax(newMax);
    }

    public void InitializePracticeBeatRange() {
        OnInitializeRange?.Invoke();

        UpdateColors();
        if(!UsingMarks) {
            return;
        }

        var max = CurrentMarkCount;
        Instance.SetSliderMinimumDifference(1);
        Instance.SetSliderBounds(0, max);
        Instance.SetCurrentValueMax(max);
        Instance.SetCurrentValueMin(0);
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
}


[HarmonyPatch(typeof(RangeSliderOptionController))]
public static class SliderPatch {
    [HarmonyPatch(nameof(RangeSliderOptionController.RaiseOnMinMaxChanged))]
    [HarmonyPrefix]
    public static void RaiseOnMinMaxChanged_Pre(RangeSliderOptionController __instance, ref Vector2Int? __state) {
        var state = SliderData.Of(__instance);
        if(state.UsingMarks) {
            __state = new(__instance._sliderValueMin, __instance._sliderValueMax);
            __instance._sliderValueMin = Mathf.Clamp(state.CurrentMarkList!.GetBeat(__instance._sliderValueMin), 0, state.MaxBeats);
            __instance._sliderValueMax = Mathf.Clamp(state.CurrentMarkList.GetBeat(__instance._sliderValueMax), __instance._sliderValueMin, state.MaxBeats);
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
}
