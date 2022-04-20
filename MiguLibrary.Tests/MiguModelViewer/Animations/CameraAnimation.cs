using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MiguModelViewer.Animations
{
    public class CameraAnimation
    {
        public List<Vector3Keyframe> PositionKeyframes;
        public List<Vector3Keyframe> RotationKeyframes;

        public CameraAnimation()
        {
            PositionKeyframes = new List<Vector3Keyframe>();
            RotationKeyframes = new List<Vector3Keyframe>();
        }

        public void Write(string path)
        {
            StringBuilder sb = new StringBuilder();
            foreach(Vector3Keyframe k in PositionKeyframes)
            {
                sb.AppendLine($"0x00\t{k.Frame}\t{(int)k.Interpolation}\t{Utils.FloatToStringEx(k.Value.X)} {Utils.FloatToStringEx(k.Value.Y)} {Utils.FloatToStringEx(k.Value.Z)}");
            }

            foreach(Vector3Keyframe k in RotationKeyframes)
            {
                sb.AppendLine($"0x01\t{k.Frame}\t{(int)k.Interpolation}\t{Utils.FloatToStringEx(k.Value.X)} {Utils.FloatToStringEx(k.Value.Y)} {Utils.FloatToStringEx(k.Value.Z)}");
            }

            File.WriteAllText(path, sb.ToString());
        }
    }
}
