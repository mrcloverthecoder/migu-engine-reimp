using System;
using Sn = System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using MiguLibrary;

namespace MiguModelViewer.Renderer
{
    public class GLShader : IDisposable
    {
        public int Id;

        private bool mDisposed = false;

        public GLShader(string vertexSrc, string fragSrc)
        {
            int vertexShader, fragShader;

            vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexSrc);
            CompileShader(vertexShader);

            fragShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragShader, fragSrc);
            CompileShader(fragShader);

            Id = GL.CreateProgram();
            GL.AttachShader(Id, vertexShader);
            GL.AttachShader(Id, fragShader);

            LinkProgram(Id);

            GL.DetachShader(Id, vertexShader);
            GL.DetachShader(Id, fragShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragShader);
        }

        private static void CompileShader(int shader)
        {
            // Try to compile the shader
            GL.CompileShader(shader);

            // Check for compilation errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);

            if (code != (int)All.True)
            {
                // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred while compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            // We link the program
            GL.LinkProgram(program);

            // Check for linking errors
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                throw new Exception($"Error occurred while linking Program({program})");
            }
        }

        public void Use()
        {
            GL.UseProgram(Id);
        }

        public int GetUniformLoc(string uniformName) { return GL.GetUniformLocation(Id, uniformName); }

        public void Uniform(string name, Matrix4 m) { Use(); GL.UniformMatrix4(GetUniformLoc(name), true, ref m); }
        public void Uniform(string name, int i) { Use(); GL.Uniform1(GetUniformLoc(name), i); }

        public void Uniform(string name, Sn.Vector4 vec)
        {
            Use();
            GL.Uniform4(GetUniformLoc(name), vec.X, vec.Y, vec.Z, vec.W);
        }

        public void Uniform(string name, Sn.Vector4[] vecs)
        {
            Use();

            // Build a float array from the vectors
            // Probably not very optimistic but... eh, it's fine for now
            GL.Uniform4(GetUniformLoc(name), vecs.Length * 4, VectorHelper.ToFloatArray(vecs));
        }

        public void Uniform(string name, Matrix4[] m)
        {
            Use();
            for(int i = 0; i < m.Length; i++)
            {
                GL.UniformMatrix4(GetUniformLoc($"uBoneTransforms[{i}]"), true, ref m[i]);
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!mDisposed)
            {
                GL.DeleteProgram(Id);
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
