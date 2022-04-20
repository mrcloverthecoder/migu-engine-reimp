using System;
using System.IO;
using Sn = System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using MiguLibrary;
using MiguEngine.Extensions;

namespace MiguEngine
{
    public class Shader : IDisposable
    {
        public static Shader Default
        {
            get => new Shader(File.ReadAllText("Resource/Shader/OBJ_01.vert.shp"), File.ReadAllText("Resource/Shader/OBJ_01.frag.shp"));
        }

        public int Id;

        private bool mDisposed = false;

        public Shader(string vertexSrc, string fragSrc)
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

        public int GetUniformLocation(string uniformName) =>
            GL.GetUniformLocation(Id, uniformName);

        public void Uniform(string name, Matrix4 m)
        { 
            Use(); 
            GL.UniformMatrix4(GetUniformLocation(name), true, ref m);
        }
        
        public void Uniform(string name, Matrix4[] m)
        {
            Use();
            GL.UniformMatrix4(GetUniformLocation(name), m.Length, true, ref m[0].Row0.X);
        }

        public void Uniform(string name, bool transpose, ref Matrix4 m)
        {
            Use();
            GL.UniformMatrix4(GetUniformLocation(name), transpose, ref m);
        }

        public void Uniform(string name, float f) { Use(); GL.Uniform1(GetUniformLocation(name), f); }
        public void Uniform(string name, int i) { Use(); GL.Uniform1(GetUniformLocation(name), i); }

        public void Uniform(string name, bool b) { Use(); GL.Uniform1(GetUniformLocation(name), b ? 1 : 0); }

        // This is a mess, this should be much more regular and not go switching between
        // System.Numerics and OpenTK vectors too much!!
        // Gotta clean this up later

        public void Uniform(string name, Sn.Vector2 vec) =>
            Uniform(name, vec.ToGL());

        public void Uniform(string name, Sn.Vector3 vec) =>
            Uniform(name, vec.ToGL());

        public void Uniform(string name, Sn.Vector4 vec) =>
            Uniform(name, vec.ToGL());

        public void Uniform(string name, Sn.Vector4[] vecs)
        {
            Use();
            GL.Uniform4(GetUniformLocation(name), vecs.Length * 4, ref vecs[0].X);
        }

        public void Uniform(string name, Vector2 vec)
        {
            Use();
            GL.Uniform2(GetUniformLocation(name), vec.X, vec.Y);
        }

        public void Uniform(string name, Vector3 vec)
        {
            Use();
            GL.Uniform3(GetUniformLocation(name), vec.X, vec.Y, vec.Z);
        }

        public void Uniform(string name, Vector4 vec)
        {
            Use();
            GL.Uniform4(GetUniformLocation(name), vec.X, vec.Y, vec.Z, vec.W);
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
