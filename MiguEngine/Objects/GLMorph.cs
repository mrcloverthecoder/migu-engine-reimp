using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace MiguEngine.Objects
{
    public struct GLMorph
    {
        public string Name;
        public GLVertexMorph[] Morphs;
    }

    public struct GLVertexMorph
    {
        public int SetIndex;
        public int VertexIndex;

        public Vector3 Transform;
    }
}
