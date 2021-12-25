using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using MiguLibrary.IO;

namespace MiguLibrary.Objects
{
    public class Material
    {
        public bool HasDiffuseTexture;
        public string TextureName;
        public Color DiffuseColor;
        public Color AmbientColor;
        public Color SpecularColor;

        public Material(string texName, Color diffuseColor, Color ambientColor, Color specularColor, bool hasDiffuseTexture)
        {
            HasDiffuseTexture = hasDiffuseTexture;
            TextureName = texName;
            DiffuseColor = diffuseColor;
            AmbientColor = ambientColor;
            SpecularColor = specularColor;
        }

        // Materials use texture names defined in an external list so I won't implement the Read method here
    }
}
