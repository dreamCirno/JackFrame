using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JackFrame {
    
    public static class AnimationExtention {
        /// <summary>
        /// 获取状态机中某动画片段长度
        /// </summary>
        /// <param name="clipName">片段名</param>
        /// <returns></returns>
        public static float GetAnimationClipLength(this Animator animator, string clipName) {
            if (clipName == null) {
                return -1;
            }

            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; i++) {
                if (clips[i].name == clipName) {
                    return clips[i].length;
                }
            }

            return -1;
        }

        /// <summary>
        /// 判断当前播放的动画是不是指定动画
        /// </summary>
        /// <param name="clipName">片段名</param>
        /// <param name="layer">在哪一层</param>
        /// <returns>这个方法的检测有一定的延迟，不适用于需要精准的实时检测</returns>
        public static bool IsPlayAnim(this Animator animator, string clipName, int layer) {
            return animator.GetCurrentAnimatorStateInfo(layer).IsName(clipName);
        }
    }
}
