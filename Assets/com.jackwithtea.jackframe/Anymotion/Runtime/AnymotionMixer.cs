using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace JackFrame.Anymotions {

    public class AnymotionMixer {

        public int ID { get; private set; }
        IAnymotion anymotion;
        AnimationMixerPlayable mixer;

        int currentClipID;
        float duaration;
        bool isCrossfade;

        AnimationClipPlayable beforeClip;
        AnimationClipPlayable nowClip;

        public AnymotionMixer(IAnymotion anymotion, int id) {

            this.ID = id;
            this.anymotion = anymotion;

            this.mixer = AnimationMixerPlayable.Create(anymotion.GetGraph());
            mixer.AddInput(default(AnimationClipPlayable), 0, 0);
            mixer.AddInput(default(AnimationClipPlayable), 1, 0);

            this.currentClipID = -1;
            this.isCrossfade = false;

        }

        public AnimationMixerPlayable GetMixerPlayable() {
            return mixer;
        }

        public void Tick(float deltaTime) {
            ProcessCrossfade(deltaTime);
        }

        public void ProcessCrossfade(float deltaTime) {

            if (!isCrossfade) {
                return;
            }

            int inputCount = mixer.GetInputCount();
            if (inputCount <= 1) {
                return;
            }

            if (duaration == 0) {
                duaration = 1f;
            }
            float increasement = deltaTime / duaration;
            float nowWeight = mixer.GetInputWeight(0);
            nowWeight += increasement;
            if (nowWeight >= 1) {
                nowWeight = 1f;
                isCrossfade = false;
            }
            mixer.SetInputWeight(0, nowWeight);
            mixer.SetInputWeight(1, 1f - nowWeight);

        }

        public void Enable() {
            mixer.Play();
        }

        public void Disable() {
            mixer.Pause();
        }

        public void PlayClip(int clipID) {

            if (currentClipID == clipID) {
                return;
            }

            DisconnectAll();

            if (nowClip.IsValid()) {
                nowClip.Destroy();
            }

            nowClip = CreateClip(clipID);

            mixer.ConnectInput(0, nowClip, 0, 1);

            this.currentClipID = clipID;

        }

        AnimationClipPlayable CreateClip(int clipID) {
            var clip = anymotion.GetClip(clipID);
            var clipPa = AnimationClipPlayable.Create(anymotion.GetGraph(), clip.Clip);
            clipPa.SetTime(0);
            clipPa.SetSpeed(1);
            return clipPa;
        }

        void DisconnectAll() {
            int inputCount = mixer.GetInputCount();
            for (int i = 0; i < inputCount; i += 1) {
                mixer.DisconnectInput(i);
            }
        }

        public void CrossfadeTo(int clipID, float duaration, int targetKeyFrame = 0) {

            if (currentClipID == clipID) {
                return;
            }

            int inputCount = mixer.GetInputCount();
            if (inputCount == 0) {
                return;
            }

            float lastWeight = mixer.GetInputWeight(0);
            if (inputCount == 2) {
                // IF HAS TWO INPUTS
                // REMOVE 1
                if (beforeClip.IsValid()) {
                    beforeClip.Destroy();
                }
            }

            beforeClip = nowClip;

            DisconnectAll();

            // ONE INPUT
            // INSERT NOW INDEX = 0
            // SET BEFORE INDEX = 1
            nowClip = CreateClip(clipID);
            mixer.ConnectInput(0, nowClip, 0, 0);
            mixer.ConnectInput(1, beforeClip, 0, 0);

            this.duaration = duaration;
            this.isCrossfade = true;
            this.currentClipID = clipID;

        }

        public void SetInputWeight(int index, float weight) {
            var input = mixer.GetInput(index);
            if (!input.IsValid()) {
                return;
            }
            mixer.SetInputWeight(index, weight);
        }

    }

}