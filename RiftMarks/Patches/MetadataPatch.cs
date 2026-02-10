using HarmonyLib;
using Newtonsoft.Json;
using RiftOfTheNecroManager;
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
        RiftMarks.Clear();
        
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

    public void LoadRiftMarks(string basePath) {
        var json = $"{PluginData.Name}.json";
        var markPath = Path.Combine(basePath, json);
        if(FileUtils.IsFile(markPath)) {
            try {
                FileUtils.ReadString(markPath)?
                    .Pipe(JsonConvert.DeserializeObject<Dictionary<string, List<RiftMark>>>)
                    .Pipe(SetRiftMarks);
            } catch(JsonException e) {
                Log.Warning($"Failed to deserialize {json} for track at {basePath}: {e.Message}");
            }
        }
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
        state.LoadRiftMarks(basePath);
    }
}
