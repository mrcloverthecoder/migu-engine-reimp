using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiguModelViewer.Renderer;
using MiguEngine;
using MiguEngine.Objects;
using MiguLibrary.Objects;
using MiguLibrary.Motions;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace MiguModelViewer.Menus
{
    public class PushStart : Common.Menu
    {
        public PushStart() : base(true)
        {
            string motNumber = new Random().Next(1, 30).ToString("000");

            Load(
                new string[1] { "OPNING_GUMI/OBJ.BMD" },
                new string[2] { $"S{motNumber}.BMM", $"S{motNumber}_P1.BMM" },
                "",
                new string[0]
                );
        }

        /*public override void Load()
        {
            string path = $"OBJECTS/OPNING_GUMI";

            Gumi = new GLObjectData(ObjectData.FromFile($"{Config.DataPath}/{path}/OBJ.BMD"), GLShader.Default, path);

            



            Motion = new MotionEntry()
            {
                Type = MiguLibrary.Scene.MotionType.Body,
                Motion = MiguLibrary.Motions.Motion.FromFile($"{Config.DataPath}/MOTIONS/S{motNumber}.BMM")
            };

            PhysMotion = new MotionEntry()
            {
                Type = MiguLibrary.Scene.MotionType.Physics,
                Motion = MiguLibrary.Motions.Motion.FromFile($"{Config.DataPath}/MOTIONS/S{motNumber}_P1.BMM")
            };
        }*/

        public override void Render()
        {
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);

            Camera.Position = new Vector3(0.0f, 0.7f, 3.0f);

            ref GLObjectData gumi = ref Objects[0];

            Matrix4[] transforms = MotionPlayer.GetTransformsLerp(gumi.Bones, ref Motions[0], gumi.BoneInfluenceNames, CurrentFrame, null, true);

            transforms = MotionPlayer.GetTransformsLerp(gumi.Bones, ref Motions[1], gumi.BoneInfluenceNames, CurrentFrame, transforms, true);

            gumi.Shader.Uniform("uBoneTransforms", transforms);

            gumi.Model = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(-90.0f)) * Matrix4.CreateTranslation(0.5f, 0.0f, 0.0f) * Matrix4.CreateScale(1.0f, 1.0f, -1.0f);
            gumi.Draw(Camera);

            gumi.Model = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(90.0f)) * Matrix4.CreateTranslation(-0.5f, 0.0f, 0.0f) * Matrix4.CreateScale(1.0f, 1.0f, -1.0f);
            gumi.Draw(Camera);

            base.Update();
        }
    }
}
