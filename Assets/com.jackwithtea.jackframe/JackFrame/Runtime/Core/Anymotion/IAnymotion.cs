using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Collections.Generic;

namespace JackFrame.Anymotions {
    public interface IAnymotion {
        AnymotionClip GetClip(int clipID);
        PlayableGraph GetGraph();
        AnimationLayerMixerPlayable GetAnimationLayerMixer();
    }
}