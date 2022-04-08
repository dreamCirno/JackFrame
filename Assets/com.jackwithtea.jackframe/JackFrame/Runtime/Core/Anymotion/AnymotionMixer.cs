using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace AnymotionNS {

    public class AnymotionMixer {

        public int ID { get; private set; }
        IAnymotion anymotion;
        AnimationMixerPlayable mixer;

        SortedDictionary<int, int> idToIndexDic;
        List<AnymotionTransitionInfo> blendingList;

        int currentClipID;
        int defaultClipID;

        AnimationClipPlayable beforeClip;
        AnimationClipPlayable nowClip;

        List<int> waitRemoveTempList;

        public AnymotionMixer(IAnymotion anymotion, int id, PlayableGraph graph, int defaultClipID) {

            this.idToIndexDic = new SortedDictionary<int, int>();
            this.blendingList = new List<AnymotionTransitionInfo>();
            this.waitRemoveTempList = new List<int>();

            this.ID = id;
            this.anymotion = anymotion;
            this.defaultClipID = defaultClipID;
            
            this.mixer = AnimationMixerPlayable.Create(graph);
            nowClip = AnimationClipPlayable.Create(graph, anymotion.GetClip(defaultClipID).Clip);
            beforeClip = nowClip;
            mixer.ConnectInput(0, beforeClip, 0);
            mixer.ConnectInput(1, nowClip, 0);
            mixer.SetInputWeight(1, 1);
            
            this.currentClipID = -1;

        }

        public AnimationMixerPlayable GetMixerPlayable() {
            return mixer;
        }

        public void Tick(float deltaTime) {
            ProcessCrossfade(deltaTime);
        }

        public void ProcessCrossfade(float deltaTime) {
            if (blendingList.Count == 0) {
                return;
            }

        }

        public int GetIndex(int clipID) {
            if (idToIndexDic.ContainsKey(clipID)) {
                return idToIndexDic[clipID];
            }
            return -1;
        }

        public void Enable() {
            mixer.Play();
        }

        public void Disable() {
            mixer.Pause();
        }

        int GetClipCount() {
            return idToIndexDic.Count;
        }

        public void AddInputClip(int clipID) {

            if (idToIndexDic.ContainsKey(clipID)) {
                return;
            }

            var clip = anymotion.GetClip(clipID);
            AnimationClipPlayable clipPa = AnimationClipPlayable.Create(anymotion.GetGraph(), clip.Clip);
            int inputIndex = mixer.AddInput(clipPa, 0, 0);
            idToIndexDic.Add(clip.ID, inputIndex);

        }

        public void PlayClip(int clipID) {

            if (currentClipID == clipID) {
                return;
            }

            mixer.DisconnectInput(0);
            mixer.DisconnectInput(1);

            if (beforeClip.IsValid()) {
                beforeClip.Destroy();
            }

            beforeClip = nowClip;
            var clip = anymotion.GetClip(clipID);
            nowClip = AnimationClipPlayable.Create(anymotion.GetGraph(), clip.Clip);

            mixer.ConnectInput(1, beforeClip, 0);
            mixer.ConnectInput(0, nowClip, 0);

            nowClip.SetSpeed(1);

            mixer.SetInputWeight(0, 1);
            mixer.SetInputWeight(1, 0);

            this.currentClipID = clipID;

        }

        public void CrossfadeTo(int clipID, float duaration, int targetKeyFrame = 0) {

            if (currentClipID == clipID) {
                return;
            }

            if (!idToIndexDic.ContainsKey(clipID)) {
                return;
            }

            // if transitionList contains clipID, do nothing
            int targetIndex = blendingList.FindIndex(value => value.ClipID == clipID);
            if (targetIndex != -1) {
                return;
            }

            AnymotionTransitionInfo info = new AnymotionTransitionInfo();
            info.Init(clipID, duaration, targetKeyFrame);
            blendingList.Add(info);

        }

        public void RemoveAllClips() {
            foreach (var kv in idToIndexDic) {
                int index = kv.Value;
                mixer.DisconnectInput(index);
            }
            idToIndexDic.Clear();
        }

        public void RemoveClip(int clipID) {
            if (idToIndexDic.ContainsKey(clipID)) {
                int index = idToIndexDic[clipID];
                mixer.DisconnectInput(index);
                idToIndexDic.Remove(clipID);
            }
        }

    }

}