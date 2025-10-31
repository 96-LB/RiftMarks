using HarmonyLib;
using Shared.MenuOptions;
using System;
using System.Collections.Generic;

namespace RiftMarks.Patches;


public class SliderData : State<RangeSliderControlOption, SliderData> {
    public Dictionary<string, RiftMarkList> RiftMarks { get; } = [];

    public event Action? OnModeSwitch;

    public void SwitchMode() {
        OnModeSwitch?.Invoke();
    }
}


[HarmonyPatch(typeof(RangeSliderControlOption))]
public static class SliderPatch {
    [HarmonyPatch(nameof(RangeSliderControlOption.ReadAndHandleInputs))]
    [HarmonyPostfix]
    public static void ReadAndHandleInputs(RangeSliderControlOption __instance, RiftInputActions input) {
        var state = SliderData.Of(__instance);
        if(input.UI.CycleSort.WasPerformedThisFrame()) {
            state.SwitchMode();
        }
    }
}
