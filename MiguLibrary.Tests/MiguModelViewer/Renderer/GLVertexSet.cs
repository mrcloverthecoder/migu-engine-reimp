using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sn = System.Numerics;
using OpenTK.Graphics.OpenGL4;
using MiguLibrary.Objects;

namespace MiguModelViewer.Renderer
{
    public class GLVertexSet : IDisposable
    {
        public int VertexArray;
        
        public int PositionBuffer, NormalBuffer, TexCoordBuffer, VertexColorBuffer, BoneIndicesBuffer, BoneWeightsBuffer;

        public int ElementBuffer;
        private int mElementLength;

        public GLMaterial Material;

        // This is used to select the bones to be sent to the render function by GLObjectData
        internal int[] mBoneIndices;
        internal float[] mBoneWeights;

        private bool mDisposed = false;

        public GLVertexSet(VertexSet vertSet, FaceSet faceSet, GLMaterial mat)
        {
            mBoneIndices = new int[vertSet.BoneInfluences.Length * 4];
            mBoneWeights = new float[vertSet.BoneInfluences.Length * 4];

            int pos = 0;
            Console.WriteLine($"BONE INFLUENCES LENGTH: {vertSet.BoneInfluences.Length} => {vertSet.Positions.Length}");
            //Console.WriteLine($"A: {vertSet.BoneInfluences.Length}");
            for(int i = 0; i < vertSet.BoneInfluences.Length; i++)
            {
                VertexBone influence = vertSet.BoneInfluences[i];
                

                /*try
                {
                    mBoneIndices[pos] = faceSet.BoneIndices[influence.Bone0];
                }
                catch
                {
                    mBoneIndices[pos] = influence.Bone0;
                }
                try
                {
                    mBoneIndices[pos + 1] = faceSet.BoneIndices[influence.Bone1];
                }
                catch
                {
                    mBoneIndices[pos + 1] = influence.Bone1;
                }
                try
                {
                    mBoneIndices[pos + 2] = faceSet.BoneIndices[influence.Bone2];
                }
                catch
                {
                    mBoneIndices[pos + 2] = influence.Bone2;
                }
                try
                {
                    mBoneIndices[pos + 3] = faceSet.BoneIndices[influence.Bone3];
                }
                catch
                {
                    mBoneIndices[pos + 3] = influence.Bone3;
                }*/

                if (faceSet.BoneIndices.Length > influence.Bone0)
                    mBoneIndices[pos] = faceSet.BoneIndices[influence.Bone0];
                else
                    mBoneIndices[pos] = 0;

                if (faceSet.BoneIndices.Length > influence.Bone1)
                    mBoneIndices[pos + 1] = faceSet.BoneIndices[influence.Bone1];
                else
                    mBoneIndices[pos + 1] = 0;

                if (faceSet.BoneIndices.Length > influence.Bone2)
                    mBoneIndices[pos + 2] = faceSet.BoneIndices[influence.Bone2];
                else
                    mBoneIndices[pos + 2] = 0;

                if (faceSet.BoneIndices.Length > influence.Bone3)
                    mBoneIndices[pos + 3] = faceSet.BoneIndices[influence.Bone3];
                else
                    mBoneIndices[pos + 3] = 0;





                //Console.WriteLine($"{influence.Bone1} {faceSet.BoneIndices.Length}");

                /*mBoneIndices[pos + 1] = 0;
                mBoneIndices[pos + 2] = 0;
                mBoneIndices[pos + 3] = 0;

                mBoneIndices[pos + 1] = faceSet.BoneIndices[influence.Bone1];
                mBoneIndices[pos + 2] = faceSet.BoneIndices[influence.Bone2];
                mBoneIndices[pos + 3] = faceSet.BoneIndices[influence.Bone2];*/

                mBoneWeights[pos] = influence.Weight0;
                mBoneWeights[pos + 1] = influence.Weight1;
                mBoneWeights[pos + 2] = influence.Weight2;
                mBoneWeights[pos + 3] = influence.Weight3;

                //Console.WriteLine($"WIGHT: {influence.Weight0} {influence.Weight1} {influence.Weight2} {influence.Weight3}");

                pos += 4;
            }
            
            /*
            Console.WriteLine("mBoneInfluences:");
            for(int i = 0; i < mBoneIndices.Length; i += 4)
            {
                Console.WriteLine($"     Influence #{i}: {mBoneIndices[i]} {mBoneIndices[i + 1]} {mBoneIndices[i + 2]} {mBoneIndices[i + 3]}");
            }*/

            VertexArray = GL.GenVertexArray();

            GL.BindVertexArray(VertexArray);

            PositionBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, PositionBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertSet.Positions.Length * 3 * sizeof(float), vertSet.Positions, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            NormalBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, NormalBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertSet.Normals.Length * 3 * sizeof(float), vertSet.Normals, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);

            TexCoordBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, TexCoordBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertSet.TextureCoordinates.Length * 2 * sizeof(float), vertSet.TextureCoordinates, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(2);

            VertexColorBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexColorBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertSet.Colors.Length * 4 * sizeof(float), vertSet.GetColorsAsVec4(), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(3);

            BoneIndicesBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, BoneIndicesBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, mBoneIndices.Length * sizeof(int), mBoneIndices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Int, false, 4 * sizeof(int), 0);
            GL.EnableVertexAttribArray(4);

            BoneWeightsBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, BoneWeightsBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, mBoneWeights.Length * sizeof(float), mBoneWeights, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(5);

            ElementBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, faceSet.FaceIndices.Length * sizeof(ushort), faceSet.FaceIndices, BufferUsageHint.StaticDraw);

            mElementLength = faceSet.FaceIndices.Length;

            Material = mat;
        }

        public void Render(GLShader shader)
        {
            shader.Use();

            GL.BindVertexArray(VertexArray);

            shader.Uniform("uProjection", Camera.GetProjectionMatrix());
            shader.Uniform("uView", Camera.GetViewMatrix());

            Material.Use(shader);

            GL.DrawElements(PrimitiveType.Triangles, mElementLength, DrawElementsType.UnsignedShort, 0);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!mDisposed)
            {
                GL.DeleteVertexArray(VertexArray);
                GL.DeleteBuffer(PositionBuffer);
                GL.DeleteBuffer(TexCoordBuffer);
                GL.DeleteBuffer(NormalBuffer);
                GL.DeleteBuffer(VertexColorBuffer);
                GL.DeleteBuffer(BoneIndicesBuffer);
                GL.DeleteBuffer(BoneWeightsBuffer);
                GL.DeleteBuffer(ElementBuffer);

                Material.DiffuseTexture?.Dispose();

                mDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
