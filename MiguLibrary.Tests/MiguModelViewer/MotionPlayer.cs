using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using MiguEngine.Skeletons;
using MiguLibrary.Motions;
using MiguModelViewer.Renderer;

namespace MiguModelViewer
{
    public class MotionPlayer
    {
        public Motion SourceMotion;
        public GLBone[] TargetSkeleton;

        public bool Loop;
        public int StartFrame { get; }

        private int mCurrentBoneFrameIndex;
        private int mCurrentLoopNumber;

        /// <summary>
        /// Initialize a new MotionPlayer class
        /// </summary>
        /// <param name="srcMot">Source motion</param>
        /// <param name="tgtSkel">Target skeleton to play the motion in</param>
        /// <param name="startFrame">Frame number where the MotionPlayer was created</param>
        public MotionPlayer(Motion srcMot, GLBone[] tgtSkel, int startFrame)
        {
            SourceMotion = srcMot;
            TargetSkeleton = tgtSkel;

            Loop = false;
            StartFrame = startFrame;

            mCurrentBoneFrameIndex = 0;
            mCurrentLoopNumber = 0;
        }

        /// <summary>
        /// Get an array of transform matrices (using Linear interpolation)
        /// </summary>
        /// <param name="currentFrame">Current frame number</param>
        /// <param name="parentMotionTransforms">Parent motion transform matrices. Pass null if none.</param>
        /// <returns>An array of Matrix4 containing bone transformation data</returns>
        public Matrix4[] GetMatricesLerp(int currentFrame, Matrix4[] parentMotionTransforms)
        {
            int frame = currentFrame - StartFrame;

            if (Loop)
            {
                int cframe = (int)(frame - (SourceMotion.Bones[0].Keyframes.Length * Config.Delta * mCurrentLoopNumber));

                if (cframe >= 0)
                    frame = cframe;

                if (frame > 0 && frame % ((SourceMotion.Bones[0].Keyframes.Length * Config.Delta) - 1) == 0)
                {
                    mCurrentBoneFrameIndex = 0;
                    mCurrentLoopNumber++;
                    frame = 0;
                }
            }

            // This should never return a decimal if you're using round FPS (30, 60, 120, etc)
            int currentKeyFrame = (int)(SourceMotion.Bones[0].Keyframes[mCurrentBoneFrameIndex].Frame * Config.Delta);

            int nextKeyFrame = -1;

            if (SourceMotion.Bones[0].Keyframes.Length > mCurrentBoneFrameIndex + 1)
                nextKeyFrame = (int)(SourceMotion.Bones[0].Keyframes[mCurrentBoneFrameIndex + 1].Frame * Config.Delta);

            // Calculate motion data
            Matrix4[] transforms;
            if (parentMotionTransforms != null)
                transforms = parentMotionTransforms;
            else
            {
                transforms = new Matrix4[TargetSkeleton.Length];
                // set all to Identity
                for (int i = 0; i < transforms.Length; i++)
                    transforms[i] = Matrix4.Identity;
            }

            if (nextKeyFrame == -1)
                return transforms;

            foreach (MotionBone bone in SourceMotion.Bones)
            {
                /*if (currentFrame >= (int)(bone.Keyframes.Last().Frame * Config.Delta))
                    break;*/

                int boneIndex = GetBoneIndex(bone.Name);

                if (boneIndex == -1)
                    continue;

                Vector4 curTrans = bone.Keyframes[mCurrentBoneFrameIndex].Position.ToGL();
                Vector4 nextTrans = bone.Keyframes[mCurrentBoneFrameIndex + 1].Position.ToGL();

                Vector4 localTrans = Utils.Lerp(curTrans, nextTrans, currentKeyFrame, nextKeyFrame, currentFrame);

                Vector4 _rot = bone.Keyframes[mCurrentBoneFrameIndex].Rotation.ToGL();
                Vector4 _nextRot = bone.Keyframes[mCurrentBoneFrameIndex + 1].Rotation.ToGL();

                float blendFactor = Utils.Lerp(0.0f, 1.0f, (float)currentKeyFrame, (float)nextKeyFrame, currentFrame);

                Quaternion localRot = Quaternion.Slerp(new Quaternion(_rot.X, _rot.Y, _rot.Z, _rot.W), new Quaternion(_nextRot.X, _nextRot.Y, _nextRot.Z, _nextRot.W), blendFactor);

                // Create translation matrix
                Matrix4 translation = Matrix4.CreateTranslation(localTrans.X, localTrans.Y, localTrans.Z);

                // Create the rotation matrix
                Matrix4 rotation = Matrix4.CreateFromQuaternion(localRot);

                Matrix4 transform = rotation * translation * TargetSkeleton[boneIndex].BindPose;

                if (TargetSkeleton[boneIndex].ParentId != -1)
                {
                    transform *= transforms[TargetSkeleton[boneIndex].ParentId];
                }

                transforms[boneIndex] = TargetSkeleton[boneIndex].InverseBindPose * transform;
            }

            if (currentFrame >= nextKeyFrame)
                mCurrentBoneFrameIndex++;

            return transforms;
        }

        private int GetBoneIndex(string name)
        {
            for (int i = 0; i < TargetSkeleton.Length; i++)
                if (TargetSkeleton[i].Name == name)
                    return i;

            return -1;
        }

        // MotionEntry needs to be ref because we need to modify CurrentBoneIndex here
        // if we passed MotionEntry as a normal argument it would only modify locally, that is,
        // as soon as the method finishes the changes are discarded
        public static Matrix4[] GetTransformsLerp(GLBone[] bones, ref MotionEntry mot, string[] boneNames, int frame, Matrix4[] parentMotionTransforms, bool loop = false)
        {
            if (loop)
            {
                int cframe = (int)(frame - (mot.Motion.Bones[0].Keyframes.Length * Config.Delta * mot.CurrentLoopNumber));

                if (cframe >= 0)
                    frame = cframe;

                if (frame > 0 && frame % ((mot.Motion.Bones[0].Keyframes.Length * Config.Delta) - 1) == 0)
                {
                    mot.CurrentBoneIndex = 0;
                    mot.CurrentLoopNumber++;
                    frame = 0;
                }
            }

            float currentFrame = (float)frame;

            // This should never return a decimal if you're using round FPS (30, 60, 120, etc)
            int currentKeyFrame = (int)(mot.Motion.Bones[0].Keyframes[mot.CurrentBoneIndex].Frame * Config.Delta);

            int nextKeyFrame = -1;

            if (mot.Motion.Bones[0].Keyframes.Length > mot.CurrentBoneIndex + 1)
                nextKeyFrame = (int)(mot.Motion.Bones[0].Keyframes[mot.CurrentBoneIndex + 1].Frame * Config.Delta);

            // Calculate motion data
            Matrix4[] transforms;
            if (parentMotionTransforms != null)
                transforms = parentMotionTransforms;
            else
            {
                transforms = new Matrix4[bones.Length];
                // set all to Identity
                for (int i = 0; i < transforms.Length; i++)
                    transforms[i] = Matrix4.Identity;
            }

            if (nextKeyFrame == -1)
                return transforms;

            foreach (MotionBone bone in mot.Motion.Bones)
            {
                /*if (currentFrame >= (int)(bone.Keyframes.Last().Frame * Config.Delta))
                    break;*/

                int boneIndex = Array.IndexOf(boneNames, bone.Name);

                if (boneIndex == -1)
                    continue;

                Vector4 curTrans = bone.Keyframes[mot.CurrentBoneIndex].Position.ToGL();
                Vector4 nextTrans = bone.Keyframes[mot.CurrentBoneIndex + 1].Position.ToGL();

                Vector4 localTrans = Utils.Lerp(curTrans, nextTrans, currentKeyFrame, nextKeyFrame, currentFrame);

                Vector4 _rot = bone.Keyframes[mot.CurrentBoneIndex].Rotation.ToGL();
                Vector4 _nextRot = bone.Keyframes[mot.CurrentBoneIndex + 1].Rotation.ToGL();

                float blendFactor = Utils.Lerp(0.0f, 1.0f, (float)currentKeyFrame, (float)nextKeyFrame, currentFrame);

                Quaternion localRot = Quaternion.Slerp(new Quaternion(_rot.X, _rot.Y, _rot.Z, _rot.W), new Quaternion(_nextRot.X, _nextRot.Y, _nextRot.Z, _nextRot.W), blendFactor);

                // Create translation matrix
                Matrix4 translation = Matrix4.CreateTranslation(localTrans.X, localTrans.Y, localTrans.Z);

                // Create the rotation matrix
                Matrix4 rotation = Matrix4.CreateFromQuaternion(localRot);

                Matrix4 transform = rotation * translation * bones[boneIndex].BindPose;

                if (bones[boneIndex].ParentId != -1)
                {
                    transform *= transforms[bones[boneIndex].ParentId];
                }

                transforms[boneIndex] = bones[boneIndex].InverseBindPose * transform;
            }

            if (currentFrame >= nextKeyFrame)
                mot.CurrentBoneIndex++;

            return transforms;
        }

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
                    continue;

                Vector4 localTrans = bone.Keyframes[currentFrame].Position.ToGL();

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
