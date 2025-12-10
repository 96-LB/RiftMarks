using BepInEx;
using RiftOfTheNecroManager;

namespace RiftMarks;


[BepInPlugin("com.lalabuff.necrodancer.riftmarks", "RiftMarks", "0.1.0")]
public class Plugin : RiftPlugin {
    public override string AllowedVersions => "1.10.0";
}
