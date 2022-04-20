using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MiguModelViewer.Animations
{
    public struct Vector3Keyframe
    {
        public int Frame;
        public InterpolationType Interpolation;
        public Vector3 Value;
    }
}
