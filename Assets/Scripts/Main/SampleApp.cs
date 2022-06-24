using UnityEngine;
using JackFrame;

namespace MySampleApp {

    public class SampleApp : MonoBehaviour {

#if JACK_FRAME_DEV
        [UnityEditor.MenuItem("JackFrame/RunSampleApp")]
#endif
        public static void Run() {

        }

        void Awake() {

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