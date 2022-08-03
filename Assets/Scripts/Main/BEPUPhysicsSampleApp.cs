using System;
using UnityEngine;
using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUPhysicsSample {

    public class BEPUPhysicsSampleApp : MonoBehaviour {

        Fixed64 timeStep = Fixed64.EN3 * 1;

        BEPUSpace space;

        Box ground;
        GameObject groundGo;

        Capsule role;
        GameObject roleGo;

        void Awake() {

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            space = new BEPUSpace();
            space.ForceUpdater.Gravity = new FixedV3(0, 0, 0);

            // ==== GROUND ====
            ground = new Box(new FixedV3(0, 0, 0), 20, 1, 50, -1);
            ground.Gravity = FixedV3.Zero;
            groundGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            groundGo.transform.localScale = new Vector3(ground.Width.AsFloat(), ground.Height.AsFloat(), ground.Length.AsFloat());
            space.Add(ground);

            // ==== ROLE ====
            role = new Capsule(new FixedV3(0, 2, 0), new FixedV3(0, 3, 0), 2, 1);
            role.Gravity = new FixedV3(0, -20, 0);
            roleGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            roleGo.transform.localScale = new Vector3(role.Radius.AsFloat(), role.Length.AsFloat(), role.Radius.AsFloat());
            space.Add(role);

            // CollisionRules.AddRule(ground, role, CollisionRule.NoNarrowPhaseUpdate);

            var events = ground.CollisionInformation.Events;
            events.CreatingPair += ((me, other, pair) => {
                Debug.Log("Enter: ");
            });

            events.PairCreated += (me, other, pair) => {
                Debug.Log("Pair: ");
            };

            events.ContactCreated += (me, other, pair, contact) => {
                // Debug.Log("Contact: ");
            };

            events.InitialCollisionDetected += (me, other, pair) => {
                Debug.Log("Collision: ");
            };

            events.RemovingPair += ((me, other) => {
                // Debug.Log("Remove: ");
            });

            events.PairUpdating += (me, other, pair) => {
                // Debug.Log("Updating: ");
            };

            events.PairTouching += (me, other, pair) => {
                // Debug.Log("Touching: ");
            };

            events.PairUpdated += (me, other, pair) => {
                // Debug.Log("Updated: ");
            };

        }

        static Fixed64 TARGET_INTERVAL = 16 * Fixed64.EN3;

        void Update() {

            int times = Fixed64.Floor(TARGET_INTERVAL / timeStep).AsInt();
            for (int i = 0; i < times; i += 1) {
                space.Update(timeStep);
            }

            groundGo.transform.position = ground.Position.ToVector3();
            roleGo.transform.position = role.Position.ToVector3();

            // FixedV3 dir = FixedV3.Zero;
            // if (Input.GetKey(KeyCode.A)) {
            //     dir.X = -1;
            // } else if (Input.GetKey(KeyCode.D)) {
            //     dir.X = 1;
            // }

            // FixedV3 v = role.LinearVelocity;
            // v.X = 5 * dir.X;
            // role.LinearVelocity = v;

            // if (Input.GetKeyDown(KeyCode.W)) {
            //     FixedV3 velo = (FixedV3)role.Gravity;
            //     velo.Y = new Fixed64(-20);
            //     role.Gravity = velo;

            //     velo = role.LinearVelocity;
            //     velo.Y = 16;
            //     role.LinearVelocity = velo;
            // } else {
            //     FixedV3 velo = (FixedV3)role.Gravity;
            //     if (velo.Y < 0) {
            //         velo.Y -= new Fixed64(16) / 100;
            //     } else {
            //         velo.Y -= new Fixed64(24) / 100;
            //     }
            //     role.Gravity = velo;
            // }

            // role.AngularVelocity = FixedV3.Zero;

            if (Input.GetKeyUp(KeyCode.I)) {
                float x = UnityEngine.Random.Range(1f, 3f);
                float y = UnityEngine.Random.Range(1f, 5f);
                role.Position = new FixedV3(Mathf.FloorToInt(x), Mathf.FloorToInt(y), 0);
            }

        }

    }

}