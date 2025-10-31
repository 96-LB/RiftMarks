using HarmonyLib;
using RhythmRift;
using Shared.RhythmEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace RiftMarks.Patches;



// various patches to reduce excessive logging and improve performance
[HarmonyPatch]
public static class BeatmapPatch {
    public static MethodInfo DebugLogMethod { get; } = AccessTools.Method(typeof(Debug), nameof(Debug.Log), [typeof(object)]);
    public static MethodInfo NoOpMethod { get; } = AccessTools.Method(typeof(BeatmapPatch), nameof(NoOp), [typeof(object)]);

    public static void NoOp(object data) {
        // replaces Debug.Log calls
    }

    [HarmonyPatch(typeof(RRBeatmapPlayer), nameof(RRBeatmapPlayer.ParseTrapSpawnData))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ParseTrapSpawnData(IEnumerable<CodeInstruction> instructions) {
        // the logging in this method is excessive and slows down gameplay significantly
        // there's no option to disable it so we replace Debug.Log calls with no-ops in the IL

        if(DebugLogMethod is null) {
            Plugin.Log.LogError("Could not find Debug.Log method for transpiler!");
        }

        foreach(var instruction in instructions) {
            if(DebugLogMethod is not null && instruction.Calls(DebugLogMethod)) {
                yield return new CodeInstruction(OpCodes.Call, NoOpMethod);
            } else {
                yield return instruction;
            }
        }
    }

    [HarmonyPatch(typeof(BeatmapPlayer), nameof(BeatmapPlayer.ProcessAudioBeatEvents))]
    [HarmonyPrefix]
    public static void ProcessAudioBeatEvents(BeatmapPlayer __instance) {
        // TODO: only disable in practice mode on the first beat?
        // this logging slows things down a lot and is not useful
        __instance._shouldLogSkippedBeatmapEvents = false;
    }

    [HarmonyPatch(typeof(Beatmap), nameof(Beatmap.MarkEventProcessed))]
    [HarmonyPrefix]
    public static bool MarkEventProcessed() {
        // this function is wildly inefficient and doesn't actually do anything useful
        return false;
    }
}
