using System;
using HarmonyLib;
using RhythmRift;
using RiftOfTheNecroManager;
using Shared;
using Shared.Analytics;
using Shared.Audio;
using Shared.SceneLoading;
using Shared.SceneLoading.Payloads;
using Shared.TrackData;
using Shared.TrackSelection;
using TicToc.Localization.Components;
using TMPro;
using UnityEngine;

namespace RiftMarks.Patches;


[HarmonyPatch(typeof(CustomTracksSelectionSceneController))]
public static class SelectionScenePatch {
    
    [HarmonyPatch(nameof(CustomTracksSelectionSceneController.HandlePracticeModeBeatRangeChanged))]
    [HarmonyPostfix]
    public static void HandlePracticeModeBeatRangeChanged(CustomTracksSelectionSceneController __instance) {
        Log.Message(__instance._practiceStartBeat);
        Log.Message(__instance._practiceEndBeat);
    }

    [HarmonyPatch(nameof(CustomTracksSelectionSceneController.GoToSelectedStage))]
    [HarmonyPrefix]
    public static bool GoToSelectedStage(CustomTracksSelectionSceneController __instance) {
        if (__instance._submittedTrackMetadata != null && __instance._submittedTrackDifficulty != null)
        {
            __instance.InputDisabled = true;
            if (__instance._startTrackSfx.Guid != Guid.Empty)
            {
                AudioManager.Instance.PlayAudioEvent(__instance._startTrackSfx, 0f, shouldCache: false, 0u, 0f, shouldApplyLatency: false);
            }

            string specialID = (__instance._isShopkeeperMode && !__instance._isPracticeMode) ? "Shopkeeper" : null;
            RRDynamicScenePayload rRDynamicScenePayload = RRDynamicScenePayload.FromMetadata(__instance._submittedTrackMetadata, __instance._submittedTrackDifficulty, TrackMetadataUtils.ResolveAudioChannel(__instance._submittedTrackMetadata, specialID));
            rRDynamicScenePayload.IsPracticeMode = __instance._isPracticeMode;
            rRDynamicScenePayload.SetPracticeModeBeatRange(__instance._practiceStartBeat, __instance._practiceEndBeat);
            rRDynamicScenePayload.SetPracticeModeSpeedAdjustment(__instance._practiceModeSpeedModifier);
            Log.Warning($"Practice Mode Start Beat: {rRDynamicScenePayload.PracticeModeStartBeat}, End Beat: {rRDynamicScenePayload.PracticeModeEndBeat}");
            SceneLoadData.SetCurrentScenePayload(rRDynamicScenePayload);
            SceneLoadData.SetReturnScenePayload(__instance.CreateReturnScenePayload(__instance._submittedTrackMetadata.LevelId));
            if (SceneLoadData.TryGetCurrentPayload(out var payload))
            {
                SceneLoadData.StageEntryType = RiftAnalyticsService.StageEntryType.StageSelectMenu;
                SceneLoadingController.Instance.GoToScene(payload.GetDestinationScene());
            }
        }
        return false;
    }
}



[HarmonyPatch(typeof(RRStageController))]
public static class StageControllerPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(RRStageController.ProcessPracticeModePayload))]
    public static bool ProcessPracticeModePayload(RRStageController __instance, RRDynamicScenePayload payload) {
        float practiceModeStartBeat = payload.PracticeModeStartBeat;
        float practiceModeEndBeat = payload.PracticeModeEndBeat;
        if (practiceModeStartBeat - __instance._microRiftMusicFadeInDurationInBeats < 0f)
        {
            __instance._practiceModeStartBeatNumber = 0f;
        }
        else if (practiceModeStartBeat - 8f < __instance._microRiftMusicFadeInDurationInBeats)
        {
            __instance._practiceModeStartBeatNumber = __instance._microRiftMusicFadeInDurationInBeats;
            __instance._practiceModeTotalStageBeats = __instance._practiceModeTotalStageBeats - 8f - 1f;
        }
        else if (practiceModeStartBeat - 8f > __instance._practiceModeTotalStageBeats - 1f)
        {
            __instance._practiceModeTotalStageBeats = __instance._practiceModeTotalStageBeats - 8f - 1f;
        }
        else
        {
            __instance._practiceModeStartBeatNumber = practiceModeStartBeat - 8f;
        }

        if (practiceModeEndBeat <= 0f || practiceModeEndBeat > __instance._practiceModeTotalStageBeats)
        {
            __instance._practiceModeEndBeatNumber = __instance._practiceModeTotalStageBeats;
        }
        else if (practiceModeEndBeat <= __instance._practiceModeStartBeatNumber)
        {
            __instance._practiceModeEndBeatNumber = __instance._practiceModeStartBeatNumber + 1f;
        }
        else if (__instance._practiceModeStartBeatNumber <= 0f && practiceModeEndBeat < 8f)
        {
            __instance._practiceModeEndBeatNumber = 8f;
        }
        else
        {
            __instance._practiceModeEndBeatNumber = practiceModeEndBeat;
        }
        
        float num = __instance._practiceModeStartBeatNumber;
        __instance._practiceModeStartBeatmapIndex = 0;
        __instance._practiceModeTotalBeatsSkippedBeforeStartBeatmap = 0f;
        for (int i = 0; i < __instance._beatmaps.Count; i++)
        {
            if (__instance._beatmaps[i].DurationInBeats > num)
            {
                __instance._practiceModeStartBeatmapIndex = i;
                break;
            }

            num -= __instance._beatmaps[i].DurationInBeats;
            __instance._practiceModeTotalBeatsSkippedBeforeStartBeatmap += __instance._beatmaps[i].DurationInBeats;
        }

        __instance._beatmapsIndex = __instance._practiceModeStartBeatmapIndex;
        __instance._practiceModeSpeedModifier = payload.PracticeModeSpeedModifier;
        if (__instance._practiceModeSpeedModifier != SpeedModifier.OneHundredPercent)
        {
            __instance.BeatmapPlayer.SetSongSpeedModifier(__instance._practiceModeSpeedModifier);
            __instance._enemyController.SongSpeedMultiplier = (float)__instance.BeatmapPlayer.SongSpeedMultiplier;
        }
        else if (__instance._practiceModeStartBeatNumber > 0f)
        {
            __instance.BeatmapPlayer.ShouldUse100SpeedSnapshot = true;
        }

        __instance._enemyController.SetStartBeatOffset(__instance._practiceModeStartBeatNumber);
        __instance._debugBeatmapIndex = 0;
        __instance._debugBeatmapPercentageStart = 0f;
        __instance._debugBeatmapStartTime = 0f;
        __instance._debugBeatToStartOn = 0f;
        
        return false;
    }
}
