using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiguModelViewer.Renderer;
using MiguLibrary.Objects;
using MiguLibrary.Motions;
using OpenTK;

namespace MiguModelViewer
{
    public abstract class Tests : IDisposable
    {
        internal bool mDisposed;
        public int CurrentFrame = 0;
        public string Identifier;

        public abstract int Get(GetAction action);

        public abstract void Switch();

        public abstract void Reload(string path, ReloadType type = ReloadType.Object);

        public abstract void Update();

        public abstract void Render();

        public abstract void Dispose();

        protected virtual void Dispose(bool disposing) { }
    }

    public class ObjectTest : Tests
    {
        private GLObjectData mObject;
        public string Identifier = "Object";

        public override void Reload(string path, ReloadType type = ReloadType.Object)
        {
            // Try to clear object from memory before creating a new one 
            mObject?.Dispose();

            mObject = new GLObjectData(ObjectData.FromFile(path), new GLShader(File.ReadAllText("Resource/Shader/OBJ_01.vert.shp"), File.ReadAllText("Resource/Shader/OBJ_01.frag.shp")), "OBJECTS/" + Path.GetDirectoryName(path).Replace("\\", "/").Split('/').Last());
        }

        public override int Get(GetAction action)
        {
            return 0;
        }

        public override void Switch()
        {
        }

        public override void Update()
        {
            /*Matrix4[] transforms = new Matrix4[mObject.Bones.Length];

            for(int i = 0; i < transforms.Length; i++)
            {
                transforms[i] = mObject.Bones[i].InverseBindPose;
            }

            mObject.Shader.Uniform("uBoneTransforms", transforms);*/
        }

        public override void Render()
        {
            mObject?.Render();
        }

        public override void Dispose()
        {
            if (!mDisposed)
            {
                mObject?.Dispose();

                mDisposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }

    public class MotionTest : Tests
    {
        private GLObjectData mObject;
        public Motion ActiveMotion;

        public string Identifier = "Motion";

        private bool mIsPlaying = false;

        public override void Reload(string path, ReloadType type = ReloadType.Motion)
        {
            if(type == ReloadType.Object)
            {
                // Try to clear object from memory before creating a new one 
                mObject?.Dispose();

                mObject = new GLObjectData(ObjectData.FromFile(path), new GLShader(File.ReadAllText("Resource/Shader/OBJ_01.vert.shp"), File.ReadAllText("Resource/Shader/OBJ_01.frag.shp")), "OBJECTS/" + Path.GetDirectoryName(path).Replace("\\", "/").Split('/').Last());
            }
            else if(type == ReloadType.Motion)
            {
                ActiveMotion = Motion.FromFile(path);
            }
        }

        public override void Switch()
        {
            if (!mIsPlaying)
                mIsPlaying = true;
            else
                mIsPlaying = false;
        }

        public override int Get(GetAction action)
        {
            if (action == GetAction.MotionLastFrame)
                return ActiveMotion.Bones[0].Keyframes.Length - 1;
            return 0;
        }

        public override void Update()
        {
            
            mObject.Shader.Uniform("uBoneTransforms", AnimationPlayer.GetTransforms(mObject.Bones, ActiveMotion.Bones, mObject.BoneInfluenceNames, CurrentFrame));

            if(mIsPlaying && CurrentFrame < ActiveMotion.Bones[0].Keyframes.Length)
                CurrentFrame++;
        }

        public override void Render()
        {
            mObject?.Render();
        }

        public override void Dispose()
        {
            if (!mDisposed)
            {
                mObject?.Dispose();
                ActiveMotion = null;

                mDisposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }

    public enum ReloadType
    {
        Object,
        Motion
    }

    public enum GetAction
    {
        MotionLastFrame
    }
}
