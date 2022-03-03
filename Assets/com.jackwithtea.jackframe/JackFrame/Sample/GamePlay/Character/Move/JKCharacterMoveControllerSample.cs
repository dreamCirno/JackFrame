using System.Collections;
using UnityEngine;

namespace JackGamePlay.Sample {

    public class JKCharacterMoveControllerSample : MonoBehaviour {

        // CHARACTER
        JKCharacterGo go;

        // CAMERA

        // CONTROLLER

        Vector2 moveAxis = new Vector2();
        bool isInputing;

        void Awake() {
            isInputing = true;
            go = GetComponentInChildren<JKCharacterGo>();
            go.SetBody(go.transform.Find("Body"));
        }

        void Start() {
            StartCoroutine(FackInputIE());
        }

        void Update() {
            go.MoveInTopDown(moveAxis.normalized, 5.5f);
        }

        void OnDestroy() {
            StopCoroutine(nameof(FackInputIE));
        }

        IEnumerator FackInputIE() {
            WaitForSeconds waitForSeconds = new WaitForSeconds(1f);
            while (isInputing) {
                float xAxis = Random.Range(-1f, 1f);
                float yAxis = Random.Range(-1f, 1f);
                moveAxis.x = xAxis;
                moveAxis.y = yAxis;
                yield return waitForSeconds;
            }
        }

    }

}