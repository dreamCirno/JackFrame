using System;
using UnityEngine;

namespace JackFrame.Anymotions.Sample {

    public class AnymotionSample : MonoBehaviour {

        [SerializeField] GameObject role;
        [SerializeField] AnimationClip[] clips;

        Anymotion anymotion;
        int index;
        [SerializeField, Range(0, 1)] float weight;

        void Awake() {
            anymotion = role.GetComponent<Anymotion>();
            anymotion.Init();

            for (int i = 0; i < clips.Length; i += 1) {
                var clip = clips[i];
                anymotion.RegisterClip(i, clip);
            }

            anymotion.AddMixer(0, 1);

            anymotion.PlayClip(0, 0);
            index = 0;

        }

        void Update() {

            anymotion.Tick(Time.deltaTime);

            if (Input.GetKeyUp(KeyCode.S)) {
                index += 1;
                index = index % clips.Length;
                anymotion.PlayClip(0, index);
            }

            if (Input.GetKeyUp(KeyCode.W)) {
                index += 1;
                index = index % clips.Length;
                anymotion.CrossfadeTo(0, index, 0.2f);
            }

        }

    }

}