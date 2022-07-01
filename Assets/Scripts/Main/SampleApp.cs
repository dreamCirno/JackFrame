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

            FixedTest();

            NormalTest();

        }

        void FixedTest() {

            Matrix4x4Fixed lhs = Matrix4x4Fixed.Multiply(Matrix4x4Fixed.ScaleByVector(new Vector3Fixed(1, 2, 1)), Matrix4x4Fixed.TranslateByVector(new Vector3Fixed(1, 1, 1)));
            Matrix4x4Fixed rhs = Matrix4x4Fixed.TranslateByVector(new Vector3Fixed(2, 3, 4));
            Matrix4x4Fixed mt = Matrix4x4Fixed.Multiply(lhs, rhs);
            Debug.Log(lhs);
            Debug.Log(mt);

        }

        void NormalTest() {

            Matrix4x4 lhs = Matrix4x4.Scale(new Vector3(1, 2, 1)) * Matrix4x4.Translate(new Vector3(1, 1, 1));
            Matrix4x4 rhs = Matrix4x4.Translate(new Vector3(2, 3, 4));
            Matrix4x4 mt = lhs * rhs;
            Debug.Log(lhs.ToString());
            Debug.Log(mt);

            Matrix4x4 rot = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 90));
            Debug.Log(rot);

        }

    }

}