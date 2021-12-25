using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace MiguLibrary.Objects
{
    public class Morph
    {
        public string Name;
        public int Panel;
        public VertexMorph[] Morphs;

        public Morph(string name, int panel, int vertexCount)
        {
            Name = name;
            Panel = panel;
            Morphs = new VertexMorph[vertexCount];
        }
    }

    public class VertexMorph
    {
        public int Id;
        public Vector3 Transform;

        public VertexMorph(int id, Vector3 trans)
        {
            Id = id;
            Transform = trans;
        }

        public VertexMorph()
        {
            Id = 0;
            Transform = new Vector3(0.0f, 0.0f, 0.0f);
        }
    }
}
