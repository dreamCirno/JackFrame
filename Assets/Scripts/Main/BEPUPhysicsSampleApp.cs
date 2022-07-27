using System;
using UnityEngine;
using BEPUphysics;
using FixMath.NET;

namespace BEPUPhysicsSample {

    public class BEPUPhysicsSampleApp : MonoBehaviour {

        BEPUSpace space;

        void Awake() {
            space = new BEPUSpace();
        }

        void Update() {
            space.Update(Fixed64.EN3 * 16);
        }

    }

}