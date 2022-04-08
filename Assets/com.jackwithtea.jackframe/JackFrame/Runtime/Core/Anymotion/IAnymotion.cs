using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Collections.Generic;

namespace AnymotionNS {
    public interface IAnymotion {
        AnymotionClip GetClip(int clipID);
        PlayableGraph GetGraph();
    }
}