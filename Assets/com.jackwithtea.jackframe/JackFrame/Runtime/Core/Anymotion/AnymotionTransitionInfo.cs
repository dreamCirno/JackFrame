using System;

namespace AnymotionNS {

    public class AnymotionTransitionInfo {

        public int ClipID { get; private set; }
        public float Duaration { get; private set; }
        public int TargetKeyFrame { get; private set; }

        public AnymotionTransitionInfo() {}

        public void Init(int clipID, float duaration, int targetKeyFrame) {
            this.ClipID = clipID;
            this.Duaration = duaration;
            this.TargetKeyFrame = targetKeyFrame;
        }
        
    }
}