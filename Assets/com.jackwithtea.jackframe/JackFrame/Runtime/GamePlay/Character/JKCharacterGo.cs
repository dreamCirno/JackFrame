using System;
using UnityEngine;
using JackFrame;

namespace JackGamePlay {

    [RequireComponent(typeof(Rigidbody))]
    public class JKCharacterGo : MonoBehaviour {

        public Rigidbody Rigidbody { get; private set; }
        public Transform Body { get; private set; }

        protected virtual void Awake() {
            Rigidbody = GetComponent<Rigidbody>();
        }

        protected virtual void Start() {
            PLog.ForceAssert(Body != null, $"Body 不该为空, Body是角色的模型根节点; {name}则是刚体节点. 正确的层级关系是{name}/Body");
        }

        // ==== 初始化 ====
        public void SetBody(Transform body) {
            this.Body = body;
        }

        public virtual void MoveInTopDown(Vector2 moveAxis, float moveSpeed) {
            Rigidbody.MoveInTopDown(moveAxis, moveSpeed * JKGamePlaySetup.MeasurementUnit, transform, Body);
        }

    }

}