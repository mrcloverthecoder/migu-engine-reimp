using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sn = System.Numerics;
using OpenTK.Graphics.OpenGL4;
using MiguLibrary.Objects;
using OpenTK;

namespace MiguEngine.Objects
{
    public class GLMesh : IDisposable
    {
        public int VertexArray;
        
        public int PositionBuffer, NormalBuffer, TexCoordBuffer, VertexColorBuffer, BoneIndicesBuffer, BoneWeightsBuffer;

        public float ZBias;

        public Sn.Vector3[] Positions;
        public uint[] Ids;

        public int ElementBuffer;
        private int mElementLength;

        public GLMaterial Material;

        // This is used to select the bones to be sent to the render function by GLObjectData
        internal float[] mBoneIndices;
        internal float[] mBoneWeights;

        private bool mDisposed = false;

        public GLMesh(VertexSet vertSet, FaceSet faceSet, GLMaterial mat, BufferUsageHint positionHint = BufferUsageHint.StaticDraw, bool debug = false)
        {
            mBoneIndices = new float[vertSet.BoneInfluences.Length * 4];
            mBoneWeights = new float[vertSet.BoneInfluences.Length * 4];

            Positions = vertSet.Positions;
            Ids = vertSet.Ids;

            int pos = 0;
            for(int i = 0; i < vertSet.BoneInfluences.Length; i++)
            {
                VertexBone influence = vertSet.BoneInfluences[i];

                mBoneWeights[pos] = influence.Weight0;
                mBoneWeights[pos + 1] = influence.Weight1;
                mBoneWeights[pos + 2] = influence.Weight2;
                mBoneWeights[pos + 3] = influence.Weight3;
                
                if (faceSet.BoneIndices.Length > influence.Bone0)
                    mBoneIndices[pos] = (float)faceSet.BoneIndices[influence.Bone0];
                else
                    mBoneIndices[pos] = (float)influence.Bone0;

                if (faceSet.BoneIndices.Length > influence.Bone1)
                    mBoneIndices[pos + 1] = (float)faceSet.BoneIndices[influence.Bone1];
                else
                    mBoneIndices[pos + 1] = (float)influence.Bone1;

                if (faceSet.BoneIndices.Length > influence.Bone2)
                    mBoneIndices[pos + 2] = (float)faceSet.BoneIndices[influence.Bone2];
                else
                    mBoneIndices[pos + 2] = (float)influence.Bone2;

                if (faceSet.BoneIndices.Length > influence.Bone3)
                    mBoneIndices[pos + 3] = (float)faceSet.BoneIndices[influence.Bone3];
                else
                    mBoneIndices[pos + 3] = (float)influence.Bone3;

                pos += 4;
            }

            float zbias = 0.0f;
            float lastZPos = 0.0f;
            for(int i = 0; i < vertSet.Positions.Length; i++)
            {
                zbias += vertSet.Positions[i].Z - lastZPos;
                lastZPos = vertSet.Positions[i].Z;
            }

            ZBias = zbias;
            Console.WriteLine($"ZBias: {ZBias}");


            VertexArray = GL.GenVertexArray();

            GL.BindVertexArray(VertexArray);

            PositionBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, PositionBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertSet.Positions.Length * 3 * sizeof(float), vertSet.Positions, positionHint);

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
            GL.BufferData(BufferTarget.ArrayBuffer, mBoneIndices.Length * sizeof(float), mBoneIndices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
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

        public void UpdateVertexPosition(int index, Vector3 position)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, PositionBuffer);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(index * 3 * sizeof(float)), 3 * sizeof(float), ref position);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Draw(Shader shader, Camera camera)
        {
            shader.Use();

            GL.BindVertexArray(VertexArray);

            shader.Uniform("uProjection", camera.GetProjectionMatrix());
            shader.Uniform("uView", camera.GetViewMatrix());

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
