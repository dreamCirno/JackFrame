using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUPhysics1int;
using FixMath.NET;

namespace BEPUPhysics1int.CollisionShapes
{
    public struct EntityShapeVolumeDescription
    {
        public BEPUMatrix3x3 VolumeDistribution;
        public Fixed64 Volume;
    }
}
