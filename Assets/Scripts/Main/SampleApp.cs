using UnityEngine;
using JackFrame;
using JackFrame.DefiniteMath;

namespace MySampleApp {

    public class SampleApp : MonoBehaviour {

#if JACK_FRAME_DEV
        [UnityEditor.MenuItem("JackFrame/RunSampleApp")]
#endif
        public static void Run() {

        }

        void Awake() {

            Fixed64 fp = 3;

            var a = new Vector3Fixed(3, 4, 5);
            var b = new Vector3Fixed(8, 9, 10);
            Fixed64 d = Vector3Fixed.Dot(a, b);
            Debug.Log(d);
            Debug.Log(a * b);
            Debug.Log(a.x.ToString());
            Debug.Log(fp.ToString());

            float dot = Vector3.Dot(new Vector3(3, 4, 5), new Vector3(8, 9, 10));
            Debug.Log(dot);

            Matrix4x4 m = new Matrix4x4(
                new Vector4(1, 2, 3, 4),
                new Vector4(5, 6, 7, 8),
                new Vector4(9, 10, 11, 12),
                new Vector4(13, 14, 15, 16)
            );

            // Matrix4x4 mt = Matrix4x4.Translate(new Vector3(2, 3, 4));
            // Debug.Log(mt.ToString());

            Matrix4x4 mt2 = Matrix4x4.Transpose(m);
            Debug.Log(m.ToString());
            Debug.Log(mt2.ToString());

        }

        void Update() {
            if (Input.GetKeyUp(KeyCode.Space)) {
                Physics.Simulate(0.01f);
            }
        }

    }

}