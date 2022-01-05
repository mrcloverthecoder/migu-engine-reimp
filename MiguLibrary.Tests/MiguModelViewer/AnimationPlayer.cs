using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using MiguModelViewer.Renderer;
using MiguLibrary.Motions;

namespace MiguModelViewer
{
    public class AnimationPlayer
    {

        public static Matrix4[] GetTransforms(GLBone[] bones, List<MotionBone> motBones, string[] boneNames, int currentFrame, Matrix4[] parentMotionTransforms = null)
        {
            Matrix4[] transforms;
            if (parentMotionTransforms != null)
                transforms = parentMotionTransforms;
            else
            {
                transforms = new Matrix4[bones.Length];
                // set all to 0
                for (int i = 0; i < transforms.Length; i++)
                    transforms[i] = Matrix4.Zero;
            }

            foreach(MotionBone bone in motBones)
            {
                if (currentFrame >= bone.Keyframes.Length)
                    break;

                if (currentFrame < bone.Keyframes[currentFrame].Frame)
                    break;

                int boneIndex = Array.IndexOf(boneNames, bone.Name);

                if (boneIndex == -1)
                    return transforms;

                Vector3 localTrans = bone.Keyframes[currentFrame].Position.ToGL();

                Vector4 _rot = bone.Keyframes[currentFrame].Rotation.ToGL();
                Quaternion localRot = new Quaternion(_rot.X, _rot.Y, _rot.Z, _rot.W);

                // Create translation matrix
                Matrix4 translation = Matrix4.CreateTranslation(localTrans.X, localTrans.Y, localTrans.Z);

                // Create the rotation matrix
                Matrix4 rotation = Matrix4.CreateFromQuaternion(localRot);

                //Matrix4 rotation = Matrix4.CreateFromQuaternion(new Quaternion(-1.0f, 0.0f, 0.0f, 0.0f));

                Matrix4 transform = rotation * translation * bones[boneIndex].BindPose;

                if (bones[boneIndex].ParentId != -1)
                {
                    transform *= transforms[bones[boneIndex].ParentId];
                }

                transforms[boneIndex] = bones[boneIndex].InverseBindPose * transform;
            }

            return transforms;
        }
    }
}
