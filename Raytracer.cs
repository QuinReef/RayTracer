using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Collections.Generic;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Template
{
	class RayTracer
	{
		//Used for skybox path
		const string folder = "Sodermalmsallen", ext = ".jpg";
		// Recursive trace calls
		const int TRESHOLD = 2;
		//Used for antiAliasing
		Random random = new Random();
		//Skybox faces
		Bitmap xpos, xneg, ypos, yneg, zpos, zneg;
		//Distribute rendering over coords tuples
		List<(int, int)> coords;
		Vector3 ambientLightingFactor = new Vector3(0.9f,0.7f,0.5f);
		//Other vars
		Scene scene;
		public Camera camera;
		public Surface surface;


		public RayTracer(Scene _scene, Camera _camera, Surface _surface)
		{
			scene = _scene;
			camera = _camera;
			surface = _surface;

			//Generate all coordinate tuples once
			coords = GenerateCoords(surface.width, surface.height);

			//Init all faces of the skybox
			InitSkyboxFaces(folder, ext);
		
		}


		//INIT FUNCTIONS
		
		/// <summary>
		/// Initialises the skybox faces with corresponding image path files in correct format
		/// </summary>
		/// <param name="folder">Folder path</param>
		/// <param name="ext">Image extension</param>
		public void InitSkyboxFaces(string folder, string ext)
        {
			xpos = new Bitmap("../../skyboxes/" +folder + "/posx" + ext);
			xneg = new Bitmap("../../skyboxes/" + folder + "/negx" + ext);
			ypos = new Bitmap("../../skyboxes/" + folder + "/posy" + ext);
			yneg = new Bitmap("../../skyboxes/" + folder + "/negy" + ext);
			zneg = new Bitmap("../../skyboxes/" + folder + "/negz" + ext);
			zpos = new Bitmap("../../skyboxes/" + folder + "/posz" + ext);
		}

		/// <summary>
		/// Generate a camera ray
		/// </summary>
		/// <param name="x">X on screen</param>
		/// <param name="y">Y on screen</param>
		/// <param name="offset">offset</param>
		/// <returns></returns>
		public Ray GenerateRay(int x, int y, float offset)
        {
			float a = (float)x / (float)surface.width;
			float b = (float)y / (float)surface.height;

			Vector3 pointOnScreenPlane = camera.PointOnScreenPlane(a, b);
			Vector3 direction = pointOnScreenPlane - camera.position;
			Ray r = new Ray(camera.position, direction);

			return r;
        }
		/// <summary>
		/// Returns a list containing all coordinate pairs and its corresponding Ray 
		/// </summary>
		/// <param name="screenX"> Width of screen</param>
		/// <param name="screenY"> Heigth of screen</param>
		/// <returns></returns>
		private List<(int,int)> GenerateCoords(int screenX, int screenY)
        {

			List<(int,int)> coords = new List<(int, int)>();
			for (int y = 0; y < screenY; y++)
			{
				for (int x = 0; x < screenX; x++)
				{
					coords.Add((x, y));
				}
			}
			return coords;
		}



		//TRACE FUNCTIONS 

		/// <summary>
		/// Recursive ray trace function capped on THRESHOLD
		/// </summary>
		/// <param name="r">Ray</param>
		/// <param name="recursiveCalls">Current level of depth</param>
		/// <returns></returns>
		Vector3 Trace(Ray r, int recursiveCalls)
		{
			//Get closest intersection of the ray
			(Intersection I, Vector3 intersectionPoint) = scene.GetIntersection(r);

			//Ray intersects with an object
            if (r.doesIntersect)
            {
				//Object is a mirror and threshold is not met
                if (I.nearest.material.isMirror && recursiveCalls <= TRESHOLD)
				{
					//Reflected ray direction
					Vector3 Rvec = r.direction - 2 * (Vector3.Dot(r.direction, I.normal)) * I.normal;

					//Reflected ray with starting point at the intersection of the old ray with object
					Ray reflectionRay = new Ray(intersectionPoint, Rvec);

					//Trace the reflected ray and mix the color with the mirror objects color
					return Trace(reflectionRay, recursiveCalls++) * I.nearest.material.color;
				}
				
				//Hits an object that is not a mirror
				//Calculate objects color under its circumstances
				return scene.DirectIlumination(intersectionPoint, I, r);
            }
			//Ray did not hit anything and skybox is enabled
            else if(camera.skyboxEnable)
            {
				//Color of skybox * ambientLighting factor
				return GetEnvironmentColor(r.direction) * ambientLightingFactor;
			}
			//Return black if not hit anything
			return new Vector3(0, 0, 0);
        }

		/// <summary>
		/// Trace function for the debug view
		/// </summary>
		/// <param name="r">Ray</param>
		/// <param name="recursiveCalls">Recursive threshold</param>
		/// <returns></returns>
		bool TraceDebug(Ray r, int recursiveCalls)
		{
			(Intersection I, Vector3 intersectionPoint) = scene.GetIntersection(r);
			//eg. function Trace
			if (r.doesIntersect)
			{
				//Draws ray from its origin to the 
				DrawPrimaryAndNormalRay(r.intersectLocation, r.intersection.normal, r.origin);

				//eg. function Trace
				if (I.nearest.material.isMirror && recursiveCalls <= TRESHOLD)
				{
					//eg.
					Vector3 Rvec = r.direction - 2 * (Vector3.Dot(r.direction, I.normal)) * I.normal;
					Ray reflectionRay = new Ray(intersectionPoint, Rvec);

					//Traces reflected ray
					TraceDebug(reflectionRay, recursiveCalls++);

				}
				//draws debug lines
				scene.DirectIluminationDebug(intersectionPoint, I, r, this);
				return true;
			}
		
			return false;
		}

		/// <summary>
		/// Calculates rays and display them
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void DeterminePixelColor(int x, int y)
		{
			Ray r = GenerateRay(x, y, 0);
			Vector3 color = Trace(r, 0);

			//Foreach step of the AntiAlaising 
			for (int i = 0; i < camera.AA - 1; i++)
			{
				//Generate random offset within pixel
				float offset = (float)(random.Next(-50, 50) / 100.0f);

				//Create new ray with offset and its color
				r = GenerateRay(x, y, offset);
				Vector3 newColor = Trace(r, 0);

				//Get average of the new ray with the previous
				color = new Vector3((float)(color.X + newColor.X) / 2.0f, (float)(color.Y + newColor.Y) / 2.0f, (float)(color.Z + newColor.Z) / 2.0f);
			}

			//Set pixel to traces color
			surface.Plot(x, y, MixColor(color));
		}

		/// <summary>
		/// Get skybox color pixel of a ray
		/// </summary>
		/// <param name="rDir">Ray direction</param>
		/// <returns></returns>
		Vector3 GetEnvironmentColor(Vector3 rDir)
		{
			//Simplify components
			float X = rDir.X;
			float Y = rDir.Y;
			float Z = rDir.Z;

			//Calculate Absolute value of components
			float ZAbs = Math.Abs(Z);
			float XAbs = Math.Abs(X);
			float YAbs = Math.Abs(Y);
			//Save calculations to quicly check cases
			bool YZ = YAbs < ZAbs;
			bool XZ = XAbs < ZAbs;
			bool YX = YAbs < XAbs;
			float u, v;

			//If looking in a Z direction
			if (XZ && YZ)
			{
				//Forward/Backward
				switch (Z > 0)
				{
					case true:
						//Give a value between 0-1 corresponding to the ray "hit"
						u = (X / Z + 1) / 2;
						v = (Y / -Z + 1) / 2;
						return Texture(u, v, zpos);
					case false:
						u = (X / Z + 1) / 2;
						v = (Y / Z + 1) / 2;
						return Texture(u, v, zneg);
				}

			}
			//looking in a X direction
			else if (YX && !XZ)
			{

				switch (X > 0)
				{
					case true:
						u = (Z / -X + 1) / 2;
						v = (Y / -X + 1) / 2;
						return Texture(u, v, xpos);
					case false:
						u = (Z / -X + 1) / 2;
						v = (Y / X + 1) / 2;
						return Texture(u, v, xneg);
				}
			}

			//Looking in a Y direction
			else if (Y > 0 && !YX && !YZ)
			{
				switch (Y > 0)
				{
					case true:
						u = (X / Y + 1) / 2;
						v = (Z / Y + 1) / 2;
						return Texture(u, v, ypos);
					case false:
						u = (X / -Y + 1) / 2;
						v = (Z / -Y + 1) / 2;

						return Texture(u, v, yneg);
				}

			}
			return new Vector3();
		}

		/// <summary>
		/// Returns a pixel color based on image color
		/// </summary>
		/// <param name="u"></param>
		/// <param name="v"></param>
		/// <param name="image"></param>
		/// <returns></returns>
		Vector3 Texture(float u, float v, Bitmap image)
		{
			//Translate 0-1 to pexel
			int x = (int)(u * (image.Width - 1));
			int y = (int)(v * (image.Height - 1));

			//Get color from bitmap
			System.Drawing.Color pixelColor = image.GetPixel(x, y);

			//Return converted pixel
			return new Vector3((float)(pixelColor.R / 255f), (float)(pixelColor.G / 255f), (float)(pixelColor.B / 255f));
		}


		//UTILITY FUNCTIONS

		/// <summary>
		/// Translates a screenplane pixel to a screen pixel
		/// </summary>
		/// <param name="desiredX"></param>
		/// <returns></returns>
		public int TranslateX(double desiredX)
		{
			float step = surface.width /16;
			return (int)((surface.width - 1) / 2 + desiredX * step);
		}
		public int TranslateY(double desiredY)
		{

			float step = surface.height / 9;
			return (int)(surface.height - 1 - desiredY * step);
		}

		/// <summary>
		///	Mixes a Vector color to a int color for display representation
		/// </summary>
		int MixColor(Vector3 colorVector)
		{
			//Each RGB component gets a value between 0 and 255
			int red = (int)(colorVector.X * 255);
			int green = (int)(colorVector.Y * 255);
			int blue = (int)(colorVector.Z * 255);

			//Shift them accordinly
			int result = (red << 16) + (green << 8) + blue;

			return result;
		}


		//RENDER METHODS

		/// <summary>
		/// Foreach pixel in the 2D coordinate system draw the pixel color
		/// </summary>
		public void Render3d()
		{
			foreach((int x,int y) in coords)
            {
				DeterminePixelColor(x, y );
            }
		}

		/// <summary>
		/// Renders view using multithreading
		/// </summary>
		public void RenderMultiThread()
		{

			//Get max amount of availible cores
			int MaxDegreeOfParallelism = Convert.ToInt32(Environment.ProcessorCount);

			//Achieve multithreading throught the division amongst the cores and letting them calculate different pixels in parallel;
			Parallel.ForEach(coords, new ParallelOptions() { MaxDegreeOfParallelism = MaxDegreeOfParallelism }, coord =>
			{
				(int x, int y) = coord;
				DeterminePixelColor(x, y);

			});
			
		}

		/// <summary>
		/// Renders debugview
		/// </summary>
		/// <param name="objects"></param>
		/// <param name="lights"></param>
		public void RenderDebugWindow(List<Primitive> objects, List<Light> lights)
		{
		

			// Init with black screen 
			surface.Bar(0, 0, surface.width-1, surface.height-1, 0);
			// Amount of rendered viewing rays

			//Draws all primitves
			DrawPrimitives(objects, lights);

			for (int x = 0; x <= camera.NUMB_RAYS; x++)
            {
				//
				float a = (float)x / (float)camera.NUMB_RAYS;
				float b = 0.5f;

				//Generate new ray
				Vector3 pointOnScreenPlane = camera.PointOnScreenPlane(a, b);
				Vector3 direction = pointOnScreenPlane - camera.position;
				Ray r = new Ray(camera.position, direction);
				
				//Check if ray intersects with any object
                foreach (Primitive prim in objects)
                {

                    if (prim.GetType() == typeof(Sphere))
                    {
                        Sphere sphere = (Sphere)prim;

                        sphere.Intersect(r);
                    }

                }

				if(!TraceDebug(r, 2))
                {
					//did not intersect
					Vector3 intersectPoint = r.origin + 100 * r.direction - camera.position ;
					surface.Line(TranslateX(0), TranslateY(0), TranslateX(intersectPoint.X), TranslateY(intersectPoint.Z), 12000);
				}

			}
			//Draw viewport
			surface.Line(TranslateX(camera.p0.X - camera.position.X), TranslateY(camera.p0.Z - camera.position.Z), TranslateX(camera.p1.X - camera.position.X), TranslateY(camera.p1.Z - camera.position.Z), Color.Cyan.ToArgb());

		}

		/// <summary>
		/// Draws scene objects and lights
		/// </summary>
		/// <param name="objects"></param>
		/// <param name="lights"></param>
		void DrawPrimitives(List<Primitive> objects, List<Light> lights)
        {
			//Draw each circle
			foreach (Primitive prim in objects)
			{

				if (prim.GetType() == typeof(Sphere))
				{
					Sphere sphere = (Sphere)prim;

					surface.DrawCircle(sphere.primitiveOrigin.X - camera.position.X, sphere.primitiveOrigin.Z - camera.position.Z, sphere.radius, MixColor(sphere.material.color));
				}

			}

			//Draw each light
			foreach (Light light in lights)
			{

				surface.DrawCircle(light.position.X - camera.position.X, light.position.Z - camera.position.Z, 0.1f, System.Drawing.Color.Yellow.ToArgb());

			}
		}

		/// <summary>
		/// Draws the primary and normal rays when a sphere is hit
		/// </summary>
		/// <param name="intersectPoint"></param>
		/// <param name="normal"></param>
		/// <param name="origin"></param>
		void DrawPrimaryAndNormalRay( Vector3 intersectPoint, Vector3 normal, Vector3 origin)
        {
			//Corrected to camera position as in the debugger it is always 0,0
			Vector3 intersectPointRel = intersectPoint - camera.position;
			Vector3 normalPoint = normal + intersectPointRel;

			//Draw the primary ray
			surface.Line(TranslateX(origin.X - camera.position.X), TranslateY(origin.Z - camera.position.Z), TranslateX(intersectPointRel.X), TranslateY(intersectPointRel.Z), 125667);

			//Draws normal vector from the intersectionPoint
			surface.Line(TranslateX(intersectPointRel.X), TranslateY(intersectPointRel.Z), TranslateX(normalPoint.X), TranslateY(normalPoint.Z), System.Drawing.Color.Red.ToArgb());
		}
	}
}
