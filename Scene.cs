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
	//Class used to construct objects and compute ray calculations
	class Scene
	{
		//List of objects that can be used in the scene
		public List<Light> lights = new List<Light>();
		public List<Primitive> objects = new List<Primitive>();
		Vector3 Ia = new Vector3(0.1F, 0.1F, 0.1F);
		public string sceneTitle;
		//Materials
		Material sand = new Material(new Vector3(0.76f, 0.69f, 0.5f), new Vector3(0.2f, 0.08f, 0.08f), false, false);
		Material c = new Material(new Vector3(0.8f, 0.7f, 0.1f), new Vector3(0.2f, 0.08f, 0.08f), false, false);
		Material a = new Material(new Vector3(0.3f, 0.4f, 0.5f), new Vector3(0.08f, 0.1f, 0.08f), false, false);
		Material b = new Material(new Vector3(0.08f, 1f, 0.6f), new Vector3(0.1f, 0.08f, 0.3f), false, false);
		Material white = new Material(new Vector3(1, 1, 1), new Vector3(0.08f, 0.08f, 0.08f), false, false);
		Material red = new Material(new Vector3(1, 0, 0), new Vector3(0.08f, 0.08f, 0.08f), false, false);
		Material green = new Material(new Vector3(0, 1, 0), new Vector3(0.08f, 0.08f, 0.08f), false, false);
		Material greenShiny = new Material(new Vector3(0, 1, 0), new Vector3(0.3f, 0.3f, 0.3f), false, false);
		Material blue = new Material(new Vector3(0, 0, 1), new Vector3(0.0f, 0.0f, 0.2f), false, false);
		Material turquoise = new Material(new Vector3(0.8f, 0.8f, 0.5f), new Vector3(0.1f, 0.1f, 0.1f), false, false);
		Material floor = new Material(new Vector3(1, 1, 1), new Vector3(0.08f, 0.08f, 0.08f), false, true);
		Material gold = new Material(new Vector3(1, 0.843f, 0), new Vector3(0.1f, 0.0843f, 0), true, false);
		Material silver = new Material(new Vector3(1, 1, 1), new Vector3(0.0843f, 0.0843f, 0.0843f), true, false);

		public void Build(int choice)
		{
			lights = new List<Light>();
			objects = new List<Primitive>();
            switch (choice)
            {
				case 0:
					Scene4();
					break;
				case 5:
					Scene1();
					break;
				case 1:
					Scene6();
					break;
				case 2:
					Scene2();
					break;
				case 3:
					Scene3();
					break;
				case 4:
					Scene5();
					break;
				case 6:
					Scene7();
					break;
				default:
					Scene4();
					break;
			}
          
		
		}

		
		

		// Core functions
			///Intersection

		/// <summary>
		///	Iterates all primitives and intersects it with Ray r
		/// </summary>
		/// <param name="r">Ray</param>
		/// <returns>Returns the intersection of a ray within a scene</returns>
		public (Intersection, Vector3) GetIntersection(Ray r)
		{
			foreach (Primitive obj in objects)
			{
				obj.Intersect(r);
			}
			return (r.intersection, r.intersectLocation);
		}

			///Coloring
		

		/// <summary>
		///	Calculates a color according to a 
		/// </summary>
		/// <returns>Color vector</returns>
		public Vector3 CalculatePrimitiveColor(Intersection I, Light light, Vector3 dirToLight, Ray rCam, Vector3 Kd)
		{	
			//Specular shade power
			const int n = 1;
			//Length to light
			float d = dirToLight.Length;

			//Material color
			//Vector3 Kd = I.nearest.material.color;


			Vector3 Rvec = (dirToLight - 2 * (Vector3.Dot(dirToLight, I.normal)) * I.normal).Normalized();

			//Diffuse shading
			Vector3 diffuseShading = Kd * (float)Math.Max(0, Vector3.Dot(dirToLight.Normalized(), I.normal));

			//SpecularShading
			Vector3 specularShading = I.nearest.material.Ks * (float)Math.Pow(Math.Max(0, Vector3.Dot(rCam.direction.Normalized(), Rvec)), n);

			//Return all shadings combined			
			return (float)(1 / (d * d)) * light.color * light.intensity * (specularShading + diffuseShading); ;
		}

		/// <summary>
		/// Calculates a mathematical texture
		/// </summary>
		/// <param name="intersectionPoint"></param>
		/// <returns></returns>
		public Vector3 CalculateTextureColor(Vector3 intersectionPoint)
		{
			//f(𝑢, 𝑣) = (int(𝑢) + int(𝑣)) & 1
			int compareValue = ((int)intersectionPoint.X + (int)intersectionPoint.Z);
			if ( (compareValue & 1) == 0)
            {
				return new Vector3(1f, 0.2f, 0.1f);
			}
			return new Vector3(0.1f, 0.2f, 1f);

		}

		/// <summary>
		/// Calculates a color vector based on a light
		/// </summary>
		/// <param name="intersectionPoint"></param>
		/// <param name="light"></param>
		/// <param name="I"></param>
		/// <param name="rCam"></param>
		/// <returns></returns>
		public Vector3 LightIlumination(Vector3 intersectionPoint, Light light, Intersection I, Ray rCam, Vector3 Kd)
		{
			//light direction vector
			Vector3 dirToLight = light.position - intersectionPoint;
			//ShadowRay
			Ray r = new Ray(intersectionPoint, dirToLight);
			float epsilon = 0.01F;

			//In the case the light is a spot
			if (light.GetType() == typeof(Spot))
            {
                Spot spot = (Spot)light;

				//Calculate incoming angle and compare it to the spots angle
                double dotproductAngle = Vector3.Dot(dirToLight, spot.spotDirection) / (dirToLight.Length * spot.spotDirection.Length);

                if (dotproductAngle > Math.Cos(spot.spotWidthFactor))
                {
					//return black if it does not meet the spots angle and direection
                    return new Vector3(0, 0, 0);
                }
            }
			
			//Check each primitive for a intersection
            foreach (Primitive prim in objects)
			{
				prim.Intersect(r);
				//On a intersection return the black color as intersection point is in shadow
                if (epsilon < r.intersection.distance && r.intersection.distance < dirToLight.Length - epsilon)
                {
                    return new Vector3(0, 0, 0);
                }
            }

			//If nothing intercepted the shadow ray, calculate primitives color
			return CalculatePrimitiveColor(I, light, dirToLight, rCam, Kd);
		}

		/// <summary>
		/// Combines colors of a primitive when all lights are considered
		/// </summary>
		/// <param name="intersectionPoint"></param>
		/// <param name="I"></param>
		/// <param name="r"></param>
		/// <returns></returns>
		public Vector3 DirectIlumination(Vector3 intersectionPoint, Intersection I, Ray r)
		{ 
			Vector3 Kd = I.nearest.material.color;
			if (I.nearest.material.hasTexture)
			{
				if (I.nearest.textureImage == null)
				{
					Kd = CalculateTextureColor(intersectionPoint);

				}
				else
				{
					Kd = I.nearest.GetTexel(intersectionPoint);
				}


			}
			//Accumulates the color with the result of all the lights affecting the intersection
			Vector3 color = new Vector3(0, 0, 0);

			foreach (Light light in lights)
			{
				color += LightIlumination(intersectionPoint, light, I, r, Kd);
			}
			//Return final color with the ambient factor added 
			return color + Kd * Ia;
		}

		/// <summary>
		/// For all lights print their shadow rays
		/// </summary>
		/// <param name="intersectionPoint"></param>
		/// <param name="I"></param>
		/// <param name="r"></param>
		/// <param name="rt"></param>
		public void DirectIluminationDebug(Vector3 intersectionPoint, Intersection I, Ray r, RayTracer rt)
		{
			foreach (Light light in lights)
			{
				LightIluminationDebug(intersectionPoint, light, I, r, rt);
			}
		}

		/// <summary>
		/// Same as Light Ilumination but draws the shadow rays if a point is in the shadow
		/// </summary>
		/// <param name="intersectPoint"></param>
		/// <param name="light"></param>
		/// <param name="I"></param>
		/// <param name="rCam"></param>
		/// <param name="rt"></param>
		public void LightIluminationDebug(Vector3 intersectPoint, Light light, Intersection I, Ray rCam,RayTracer rt)
		{
			Vector3 dirToLight = light.position - intersectPoint;
			Ray r = new Ray(intersectPoint, dirToLight);

			if (light.GetType() == typeof(Spot))
			{
				Spot spot = (Spot)light;
				double dotproduct = Vector3.Dot(dirToLight, spot.spotDirection) / (dirToLight.Length * spot.spotDirection.Length);
				if (dotproduct > Math.Cos(spot.spotWidthFactor))
				{
					return;
				}
			}

			foreach (Primitive prim in objects)
			{
				prim.Intersect(r);
				Vector3 intersectPointRel = r.intersectLocation - rt.camera.position;
			

				if ( 0.01 < r.intersection.distance && r.intersection.distance < dirToLight.Length - 0.01)
				{
					float angleNorRayDir = Vector3.Dot(rCam.intersection.normal, r.direction);
					if(angleNorRayDir >= 0)
                    {
						rt.surface.Line(rt.TranslateX(r.origin.X - rt.camera.position.X), rt.TranslateY(r.origin.Z - rt.camera.position.Z), rt.TranslateX(intersectPointRel.X), rt.TranslateY(intersectPointRel.Z), 16777215);

					}
				}

			}

		}

		// OTHER: MOVEMENT
		/// <summary>
		/// Moves a primitive in a horizontal manner
		/// </summary>
		/// <param name="a"></param>
		/// <param name="primitive"></param>
		public void MoveCircle(float a, Primitive primitive)
		{
			int steps = 100;
			float pointOnCircleAx = (float)(0 + (5 * Math.Cos(a * Math.PI * 2 / steps)));
			float pointOnCircleAy = (float)(0 + (5 * Math.Sin(a * Math.PI * 2 / steps)));
			Vector3 newPosition = new Vector3(pointOnCircleAx, 5, pointOnCircleAy);
			primitive.primitiveOrigin = newPosition;
		}

		/// <summary>
		/// Moves a primitive vertically 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="primitive"></param>
		public void MoveVertical(float a, Primitive primitive)
		{
			primitive.primitiveOrigin.Y = a;
		}

		/// <summary>
		/// Moves a primitve horizontally
		/// </summary>
		/// <param name="a"></param>
		/// <param name="primitive"></param>
		public void MoveHorizontal(float a, Primitive primitive)
		{
			primitive.primitiveOrigin.X = a;

		}

		//Other: Scenes

		/// <summary>
		/// Colored lights
		/// </summary>
		private void Scene1()
		{
			sceneTitle = "Primary colors and lights";
			Ia = new Vector3(0.05f, 0.05f, 0.05f);
			objects.Add(new Sphere(blue, new Vector3(-2, 2, 4), 1F));
			objects.Add(new Plane(white, new Vector3(0, -2, 0), new Vector3(0, 1, 0)));

			objects.Add(new Sphere(red, new Vector3(3, 0, 6), 0.5f));

			objects.Add(new Sphere(green, new Vector3(0, 0, 4), 1f));

			lights.Add(new Light(new Vector3(-1, 6, 2), new Vector3(0,1,0), 10f));
			lights.Add(new Light(new Vector3(1, 6, 4), new Vector3(1, 0, 0), 10f));
			lights.Add(new Light(new Vector3(0, 6, 6), new Vector3(0, 0, 1), 10f));


		}

		/// <summary>
		/// Skybox demo // Remember to turn on the skybox
		/// </summary>
		private void Scene2()
		{
			sceneTitle = "Skybox demo, turn on skybox with (b)";
			Ia = new Vector3(0.08f, 0.08f, 0.08f);
			
			objects.Add(new Sphere(silver, new Vector3(0, 0, 6), 2f));

			objects.Add(new Sphere(gold, new Vector3(-3, 0, 2), 1f));
			objects.Add(new Sphere(greenShiny, new Vector3(-2, -7, 4), 1f));

			lights.Add(new Light(new Vector3(0, 6, 2), new Vector3(1, 1, 1), 6f));
			lights.Add(new Light(new Vector3(-2, -2, 3), new Vector3(1, 1, 1), 7f));
			lights.Add(new Light(new Vector3(8, 5, 0), new Vector3(1, 1, 1), 9f));
			lights.Add(new Light(new Vector3(1, 8, 3), new Vector3(1, 1, 1), 8f));

		}

		/// <summary>
		/// Spotlight demo
		/// </summary>
		private void Scene3()
		{
			sceneTitle = "Demonstrating colored spotlights";
			Ia = new Vector3(0.07f, 0.07f, 0.07f);
			objects.Add(new Sphere(white, new Vector3(-3, 0, 6), 1F));
			objects.Add(new Plane(white, new Vector3(0, -2, 0), new Vector3(0, 1, 0)));

			objects.Add(new Sphere(white, new Vector3(2, 0, 2), 0.5f));

			objects.Add(new Sphere(white, new Vector3(0, -1, 3), 1f));

            lights.Add(new Spot(new Vector3(0, 3, 3), new Vector3(1, 0, 0), new Vector3(-0.5f, -0.8f, 0.6f), 35,8f));

            lights.Add(new Spot(new Vector3(0, 3, 3), new Vector3(0, 1, 0), new Vector3(0.5f, -0.8f, -0.6f), 35, 8f));

            lights.Add(new Spot(new Vector3(0, 3, 3), new Vector3(0, 0, 1), new Vector3(0f, -0.8f, 0f), 35, 8f));


        }

		/// <summary>
		/// Demonstrationg textures to all required primitives
		/// </summary>
		private void Scene7()
		{

			objects.Add(new Plane(floor, new Vector3(0, -3, 10), new Vector3(0, 1, 0)));
			Sphere sphere = new Sphere(floor, new Vector3(0, 0, 4), 1F);
			sphere.textureImage = new Bitmap("../../textures/globe.jpg");
			objects.Add(sphere);

			sceneTitle = "Textures to all required primitves, make sure multithreading is turned off (Y)";
			Ia = new Vector3(0.2f, 0.2f, 0.2f);
		
			lights.Add(new Light(new Vector3(0, 6, 2), new Vector3(1, 1, 1), 10f));
			lights.Add(new Spot(new Vector3(2, 5, 4), new Vector3(1, 1, 0.2f), new Vector3(0.1f, -0.8f, 0.4f), 35, 8f));
		

		}

		private void Scene4()
		{
			sceneTitle = "Triangles, Spheres and Planes";
			Ia = new Vector3(0.13f, 0.13f, 0.13f);

			objects.Add(new Sphere(a, new Vector3(-2, 3, 4), 1F));
			objects.Add(new Plane(blue, new Vector3(0, 0, 10), new Vector3(0, 0, -1)));

		
			objects.Add(new Sphere(b, new Vector3(-3, 2, 4), 0.5f));
			objects.Add(new Sphere(gold, new Vector3(3, 0, 6), 0.5f));

			objects.Add(new Sphere(green, new Vector3(0, 0, 4), 1f));

			objects.Add(new Triangle(red, new Vector3(0, 2, 5), new Vector3(2, 0, 5), new Vector3(2, 2, 5)));

			lights.Add(new Light(new Vector3(0, 6, 2), new Vector3(1, 1, 1), 10f));
			lights.Add(new Spot(new Vector3(2, 5, 4), new Vector3(1, 1, 0.2f), new Vector3(0.1f, -0.8f, 0.4f), 35, 8f));
			Plane plane = new Plane(floor, new Vector3(0, -2, 0), new Vector3(0, 1, 0));
			objects.Add(plane);


		}
		private void Scene6()
		{
			sceneTitle = "Triangles, Spheres and Planes2";
			Ia = new Vector3(0.2f, 0.2f, 0.2f);

			objects.Add(new Sphere(c, new Vector3(1, -0.15f, 2), 0.33F));
			objects.Add(new Sphere(silver, new Vector3(5, 2, 4), 1f));
			objects.Add(new Sphere(b, new Vector3(-3, 0, 4), 0.5f));
			objects.Add(new Sphere(gold, new Vector3(3, 0, 6), 0.5f));

			objects.Add(new Sphere(green, new Vector3(0, 0, 4), 1f));

			objects.Add(new Triangle(red, new Vector3(0, 2, 5), new Vector3(2, 0, 5), new Vector3(2, 2, 5)));
			objects.Add(new Triangle(c, new Vector3(-3, 2, 5), new Vector3(-1, 0, 5), new Vector3(-1, 2, 5)));

			lights.Add(new Light(new Vector3(0, 6, 2), new Vector3(1, 1, 1), 10f));
			lights.Add(new Spot(new Vector3(2, 5, 4), new Vector3(1, 1, 0.2f), new Vector3(0.1f, -0.8f, 0.4f), 35, 8f));
			objects.Add(new Plane(red, new Vector3(0, -2, 0), new Vector3(0, 1, 0)));
			objects.Add(new Plane(green, new Vector3(0, 10, 0), new Vector3(0, -1, 0)));

		}

		private void Scene5()
		{
			sceneTitle = "Triangle pryamid, in desert, press (b)";

			Ia = new Vector3(0.13f, 0.13f, 0.13f);

			objects.Add(new Triangle(gold, new Vector3(-5, -1, 5), new Vector3(5, -1, 5), new Vector3(0, 5, 7.5f)));
			objects.Add(new Triangle(silver, new Vector3(-5, -1, 5), new Vector3(-5, -1, 10), new Vector3(0, 5, 7.5f)));
			objects.Add(new Triangle(gold, new Vector3(5, -1, 5), new Vector3(5, -1, 10), new Vector3(0, 5, 7.5f)));
			objects.Add(new Triangle(silver, new Vector3(-5, -1,10), new Vector3(5, -1, 10), new Vector3(0, 5, 7.5f)));

			lights.Add(new Light(new Vector3(0, 6, 2), new Vector3(1, 1, 1), 10f));
			lights.Add(new Light(new Vector3(0, 8, 7.5f), new Vector3(1, 1, 0.2f), 10f));

			objects.Add(new Plane(sand, new Vector3(0, -1, 0), new Vector3(0, 1, 0)));

		}

	}
}
