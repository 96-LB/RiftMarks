using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RiftMarks;

public class RiftMark {
    public int Beat { get; set; }
    public string Name { get; set; } = "";
}

public class RiftMarkList {
    public int MarkCount => _riftMarks.Count;
    public bool HasMarks => MarkCount > 0;

    private readonly List<RiftMark> _riftMarks = [];

    public RiftMarkList(IEnumerable<RiftMark> riftMarks) {
        _riftMarks.AddRange(riftMarks.OrderBy(r => r.Beat));
        if(_riftMarks.Count > 0 && _riftMarks[0].Beat != 0) {
            _riftMarks.Insert(0, new RiftMark { Beat = 0, Name = "Start" });
        }
    }

    public int GetBeat(int index) {
        if(index < 0) {
            return 0;
        }
        
        if(index >= _riftMarks.Count) {
            return int.MaxValue;
        }

        return _riftMarks[index].Beat;
    }
}
