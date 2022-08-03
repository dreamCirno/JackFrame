using System;
using UnityEngine;

namespace MySampleApp {

    public class SpanSampleApp : MonoBehaviour {

        void Awake() {
            Span<byte> src = System.Text.Encoding.UTF8.GetBytes("aa");
            Span<byte> dst = stackalloc byte[5] { 1, 2, 3, 4, 5 };
            var sli = dst.Slice(1, 3);
            src.CopyTo(sli);
            foreach (var b in dst) {
                Debug.Log(b.ToString());
            }
        }

    }

}