using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace JackFrame.Anymotions {

    // 必须和 Animator 挂在同一层级
    // 必须和 Animator 挂在同一层级
    // 必须和 Animator 挂在同一层级
    [RequireComponent(typeof(Animator))]
    public class Anymotion : MonoBehaviour, IAnymotion {

        // BASE
        [SerializeField] string graphName = "AnymotionGraph";
        PlayableGraph graph;
        AnimationPlayableOutput output;
        AnymotionLayerMixer defalutLayerMixer;

        // STORAGE
        SortedDictionary<int, AnymotionClip> clipDic;

        // MIXER (FOR PLAYING ANIMATION CLIPS)
        List<AnymotionMixer> mixerList;

        public void Init() {

            this.clipDic = new SortedDictionary<int, AnymotionClip>();
            this.mixerList = new List<AnymotionMixer>();

            this.graph = PlayableGraph.Create(graphName);
            this.output = AnimationPlayableOutput.Create(graph, "output", GetComponent<Animator>());

            this.defalutLayerMixer = new AnymotionLayerMixer(graph);
            output.SetSourcePlayable(defalutLayerMixer.GetLayerMixerPlayable());

            graph.Play();

        }

        void OnDisable() {
            graph.Destroy();
        }

        public void Tick(float deltaTime) {
            mixerList.ForEach(value => value.Tick(deltaTime));
        }

        public void RegisterClip(int clipID, AnimationClip clip) {
            AnymotionClip anymotionClip = new AnymotionClip(clipID, clip);
            clipDic.Add(clipID, anymotionClip);
        }

        public AnymotionClip GetClip(int clipID) {
            clipDic.TryGetValue(clipID, out var clip);
            return clip;
        }

        public void AddMixer(int mixerID, float weight) {
            var mixer = new AnymotionMixer(this, mixerID);
            mixerList.Add(mixer);
            defalutLayerMixer.AddMixer(mixer, weight);
        }

        public void PlayClip(int mixerID, int clipID) {
            var mixer = mixerList.Find(value => value.ID == mixerID);
            mixer.PlayClip(clipID);
        }

        public void CrossfadeTo(int mixerID, int clipID, float duaration, int targetKeyFrame = 0) {
            var mixer = mixerList.Find(value => value.ID == mixerID);
            mixer.CrossfadeTo(clipID, duaration, targetKeyFrame);
        }

        public void SetInputWeight(int mixerID, int index, float weight) {
            var mixer = mixerList.Find(value => value.ID == mixerID);
            mixer.SetInputWeight(index, weight);
        }

        public AnymotionMixer GetMixer(int mixerID) {
            var mixer = mixerList.Find(value => value.ID == mixerID);
            return mixer;
        }

        public void RemoveMixer(int mixerID) {

        }

        PlayableGraph IAnymotion.GetGraph() {
            return graph;
        }

        AnimationLayerMixerPlayable IAnymotion.GetAnimationLayerMixer() {
            return defalutLayerMixer.GetLayerMixerPlayable();
        }
    }

}