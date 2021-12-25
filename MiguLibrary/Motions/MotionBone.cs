using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiguLibrary.Motions
{
    public class MotionBone
    {
        public string Name;
        public int Index;
        public MotionBoneType Type;
        public Keyframe[] Keyframes;
    }

    public enum MotionBoneType : int
    {
        Rot = 0x02,
        RotTrans = 0x03
    }
}
