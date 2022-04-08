using System;
using UnityEngine;

namespace AnymotionNS {

    [Serializable]
    public class AnymotionClip {

        [SerializeField] int id;
        public int ID { get => id; private set => id = value; }

        [SerializeField] AnimationClip clip;
        public AnimationClip Clip { get => clip; private set => clip = value; }

        public AnymotionClip(int id, AnimationClip clip) {
            this.ID = id;
            this.Clip = clip;
        }

    }

}