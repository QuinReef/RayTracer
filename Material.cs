using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Collections.Generic;
using System;
using System.Drawing;
using OpenTK.Input;
using System.Threading.Tasks;
using System.Diagnostics;
namespace Template
{
    //Class that stores all material vars
    class Material
    {
        public Material(Vector3 _color, Vector3 _Ks, bool _isMirror, bool _hasTexture)
        {
            color = _color;
            //Specular coefficient
            Ks = _Ks;
            isMirror = _isMirror;
            texturePath = null;
            hasTexture = _hasTexture;
        }

        public Vector3 color, Ks;
        public bool isMirror, hasTexture;
        public string texturePath;
    }
}
