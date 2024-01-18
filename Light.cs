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
	/// <summary>
	/// Point light that shines in all directions
	/// </summary>
	class Light
	{
        public Vector3 position, color;
		public float intensity;

        public Light(Vector3 _position, Vector3 _color, float _intensity)
        {
            position = _position;
            color = _color;
			intensity = _intensity;
        }

	}

	/// <summary>
	/// Spotlight that illuminates a cone shaped area
	/// </summary>
	class Spot : Light
    {
		public Vector3 spotDirection;		// Direction of the light
		public float spotWidthFactor;       // Width of the cone illuminated by the light

		public Spot(Vector3 _position, Vector3 _color, Vector3 _spotDirection, float _spotWidthFactor, float _intensity) : base( _position,  _color, _intensity)
		{
			spotDirection = _spotDirection;			
			spotWidthFactor = _spotWidthFactor;		
		}


	}
}
