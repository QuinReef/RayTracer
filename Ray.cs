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
	//Class used to store and construct Rays
	class Ray
	{
		//Initialised at constructor
		public Vector3 origin;
		public Vector3 direction;
		private float T;
		
		//Getter/Setter of t automatically updates intersection point
		public float t
		{
			get { return T; }
			//Once a new t is set the intersect location corresponding to new t is updated
			set
			{
				T = value;
				intersectLocation = origin + T * direction;
			}
		}
		public Vector3 intersectLocation;
		public bool doesIntersect;
		public Intersection intersection;

		//Constructor setting t to max and 
		public Ray(Vector3 _origin, Vector3 _direction)
		{
			origin = _origin;
			direction = Vector3.Normalize(_direction);

			T = float.MaxValue;
			t = float.MaxValue;

			intersectLocation = origin + T * direction;

			intersection = new Intersection();
			doesIntersect = false;
		}


	}
}
