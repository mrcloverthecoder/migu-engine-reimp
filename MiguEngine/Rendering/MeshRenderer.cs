using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiguEngine.Objects;
using OpenTK.Graphics.OpenGL;

namespace MiguEngine.Rendering
{
    public static class MeshRenderer
    {
        public static List<MeshState> Meshes;

        public static void Add(GLObjectData obj)
        {
            if (Meshes == null)
                Meshes = new List<MeshState>();

            foreach(GLMesh mesh in obj.Meshes)
            {
                Meshes.Add(new MeshState()
                {
                    Mesh = mesh,
                    Shader = obj.Shader
                });
            }
        }

        public static void Clear()
        {
            Meshes.Clear();
        }

        public static void Draw(Camera camera)
        {
            //OpenTK.Matrix4 viewMatrix = camera.GetViewMatrix();

            foreach(MeshState state in Meshes)
            {
                if(!state.Mesh.Material.IsAlpha)
                    state.Mesh.Draw(state.Shader, camera);
            }

            Meshes = Meshes.OrderBy(m => m.Mesh.ZBias).Reverse().ToList();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.DepthMask(false);

            foreach (MeshState state in Meshes)
            {
                if (state.Mesh.Material.IsAlpha)
                    state.Mesh.Draw(state.Shader, camera);
            }

            GL.Disable(EnableCap.Blend);
            GL.DepthMask(true);

            Clear();
        }
    }

    public struct MeshState
    {
        public GLMesh Mesh;
        public Shader Shader;
    }
}
