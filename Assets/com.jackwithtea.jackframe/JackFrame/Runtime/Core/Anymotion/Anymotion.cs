using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace AnymotionNS {

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

        void Awake() {

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

        public AnymotionMixer AddMixer(int mixerID, int defaultClipID) {
            var mixer = new AnymotionMixer(this, mixerID, graph, defaultClipID);
            mixerList.Add(mixer);
            defalutLayerMixer.AddMixer(mixer);
            return mixer;
        }

        public void AddInputClip(int mixerID, int clipID) {
            var mixer = mixerList.Find(value => value.ID == mixerID);
            mixer.AddInputClip(clipID);
        }

        public void PlayClip(int mixerID, int clipID) {
            var mixer = mixerList.Find(value => value.ID == mixerID);
            mixer.PlayClip(clipID);
        }

        public void CrossfadeTo(int mixerID, int clipID, float duaration, int targetKeyFrame = 0) {
            var mixer = mixerList.Find(value => value.ID == mixerID);
            mixer.CrossfadeTo(clipID, duaration, targetKeyFrame);
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
    }

}