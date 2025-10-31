using HarmonyLib;
using Newtonsoft.Json;
using Shared;
using Shared.TrackData;
using Shared.Utilities;
using System.Collections.Generic;
using System.IO;

namespace RiftMarks.Patches;


public class MetadataState : State<ITrackMetadata, MetadataState> {
    const string DEFAULT = "DEFAULT";
    public Dictionary<string, RiftMarkList> RiftMarks { get; } = [];
    
    public void SetRiftMarks(Dictionary<string, List<RiftMark>>? marks) {
        RiftMarks.Clear(); ;
        
        foreach(var (key, value) in marks ?? []) {
            RiftMarks[key.ToUpperInvariant()] = new RiftMarkList(value);
        }
    }

    public RiftMarkList? GetMarks(Difficulty difficulty) {
        var key = difficulty.ToString().ToUpperInvariant();
        if(RiftMarks.TryGetValue(key, out var marks)) {
            return marks;
        }
        if(RiftMarks.TryGetValue(DEFAULT, out marks)) {
            return marks;
        }
        return null;
    }
}

[HarmonyPatch(typeof(LocalTrackMetadata))]
public static class MetadataPatch {
    [HarmonyPatch(nameof(LocalTrackMetadata.FromPathImpl))]
    [HarmonyPostfix]
    public static void FromPathImpl(LocalTrackMetadata? __result, string basePath) {
        if(__result is null) {
            return;
        }

        var state = MetadataState.Of(__result);
        var markPath = Path.Combine(basePath, "RiftMarks.json");
        if(FileUtils.IsFile(markPath)) {
            FileUtils.ReadString(markPath)?
                .Pipe(JsonConvert.DeserializeObject<Dictionary<string, List<RiftMark>>>)
                .Pipe(state.SetRiftMarks);
        }
        Plugin.Log.LogFatal(JsonConvert.SerializeObject(state.RiftMarks));
        // TODO: don't let this prevent loading the track
    }
}
