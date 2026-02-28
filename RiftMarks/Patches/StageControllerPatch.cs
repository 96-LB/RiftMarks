using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RhythmRift;
using RiftOfTheNecroManager;
using Shared.SceneLoading.Payloads;

namespace RiftMarks.Patches;


[HarmonyPatch(typeof(RRStageController))]
public static class StageControllerPatch {
    
    public static MethodInfo SetPracticeModeBeatRangeMethod { get; } = AccessTools.Method(typeof(RhythmRiftScenePayload), nameof(RhythmRiftScenePayload.SetPracticeModeBeatRange));
    public static MethodInfo NoOpMethod { get; } = AccessTools.Method(typeof(StageControllerPatch), nameof(NoOp));
    
    public static void NoOp(float startBeat, float endBeat) {
        // replaces RhythmRiftScenePayload.SetPracticeModeBeatRange calls
    }
    
    [HarmonyPatch(nameof(RRStageController.ProcessPracticeModePayload))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ProcessPracticeModePayload(IEnumerable<CodeInstruction> instructions) {
        // destructively updating the payload seems to cause some issues
        
        if(SetPracticeModeBeatRangeMethod is null) {
            Log.Error($"Could not find {nameof(RhythmRiftScenePayload.SetPracticeModeBeatRange)} method for transpiler!");
        }
        
        foreach(var instruction in instructions) {
            if(SetPracticeModeBeatRangeMethod is not null && instruction.Calls(SetPracticeModeBeatRangeMethod)) {
                yield return new CodeInstruction(OpCodes.Call, NoOpMethod);
            } else {
                yield return instruction;
            }
        }
    }
}
