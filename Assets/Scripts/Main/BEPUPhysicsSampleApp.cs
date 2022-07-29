using System;
using UnityEngine;
using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUPhysicsSample {

    public class BEPUPhysicsSampleApp : MonoBehaviour {

        Fixed64 timeStep = Fixed64.EN3 * 16;

        BEPUSpace space;

        Box ground;
        GameObject groundGo;

        Box role;
        GameObject roleGo;

        void Awake() {

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            space = new BEPUSpace();
            space.ForceUpdater.Gravity = new FixedV3(0, Fixed64.EN2 * -981, 0);

            // ==== GROUND ====
            ground = new Box(new FixedV3(0, 0, 0), 20, 1, 50, -1);
            ground.Gravity = FixedV3.Zero;
            groundGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            groundGo.transform.localScale = new Vector3(ground.Width.AsFloat(), ground.Height.AsFloat(), ground.Length.AsFloat());
            space.Add(ground);

            // ==== ROLE ====
            role = new Box(new FixedV3(0, 5, 0), 1, 2, 1, 1);
            roleGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roleGo.transform.localScale = new Vector3(role.Width.AsFloat(), role.Height.AsFloat(), role.Length.AsFloat());
            space.Add(role);

            CollisionRules.AddRule(ground, role, CollisionRule.NoNarrowPhaseUpdate);

            var events = ground.CollisionInformation.Events;
            events.CreatingPair += ((me, other, pair) => {
                Debug.Log("Enter: ");
            });

            events.ContactCreated += (me, other, pair, contact) => {
                Debug.Log("Contact: ");
            };

            events.InitialCollisionDetected += (me, other, pair) => {
                Debug.Log("Collision: ");
            };

            events.RemovingPair += ((me, other) => {
                Debug.Log("Remove: ");
            });

            events.PairUpdating += (me, other, pair) => {
                Debug.Log("Updating: ");
            };

            events.PairTouching += (me, other, pair) => {
                Debug.Log("Touching: ");
            };

            events.PairUpdated += (me, other, pair) => {
                Debug.Log("Updated: ");
            };

        }

        void Update() {

            space.Update(timeStep);

            groundGo.transform.position = ground.Position.ToVector3();
            roleGo.transform.position = role.Position.ToVector3();

        }

    }

}