using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace MiguEngine.Skeletons
{
    public class GLBone
    {
        public string Name;
        public Matrix4 InverseBindPose;
        public Matrix4 BindPose;
        public Vector3 Position;
        // For other purposes
        public Vector4 Rotation = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
        public short ParentId;

        public GLBone(string name, Matrix4 pose, Vector3 position, short parentId)
        {
            pose.Row3 = new Vector4(pose.Row3.Xyz * 0.08f, 1.0f);
            //pose.Row3.Z *= -1.0f;

            Name = name;
            InverseBindPose = Matrix4.Invert(pose);
            BindPose = pose;
            Position = position;
            ParentId = parentId;
        }
    }
}
