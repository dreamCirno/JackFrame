using System;
using UnityEngine;
using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUPhysicsSample {

    public class BEPUPhysicsSampleApp : MonoBehaviour {

        Fixed64 timeStep = Fixed64.EN3 * 16;

        BEPUSpace space;

        Box role;

        void Awake() {
            space = new BEPUSpace();
        }

        void Update() {
            space.Update(timeStep);
        }

    }

}