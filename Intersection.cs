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
    //Stores data about a ray intersection
    class Intersection
    {
        public Intersection()
        {
            nearest = new Primitive();
        }
        public Primitive nearest; public float distance; public Vector3 normal;
    }
}
