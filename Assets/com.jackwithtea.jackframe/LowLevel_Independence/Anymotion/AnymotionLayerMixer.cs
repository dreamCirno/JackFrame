using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace JackFrame.Anymotions {

    public class AnymotionLayerMixer {

        AnimationLayerMixerPlayable layerMixer;

        SortedDictionary<int, int> mixerDic;

        public AnymotionLayerMixer(PlayableGraph graph) {
            this.mixerDic = new SortedDictionary<int, int>();
            this.layerMixer = AnimationLayerMixerPlayable.Create(graph);
        }

        public AnimationLayerMixerPlayable GetLayerMixerPlayable() {
            return layerMixer;
        }

        public void AddMixer(AnymotionMixer mixer, float weight) {
            if (mixerDic.ContainsKey(mixer.ID)) {
                return;
            }
            int inputIndex = layerMixer.AddInput(mixer.GetMixerPlayable(), 0, weight);
            mixerDic.Add(mixer.ID, inputIndex);
        }

        public void RemoveMixer(int mixerID) {
            bool hasIndex = mixerDic.TryGetValue(mixerID, out var index);
            if (hasIndex) {
                mixerDic.Remove(mixerID);
                layerMixer.DisconnectInput(index);
            }
        }

    }
}