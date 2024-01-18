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
	/// Parent class of the primitives
	/// </summary>
	class Primitive
	{
		public Primitive()
		{
			material = new Material(new Vector3(0, 0, 1), new Vector3(1, 1, 1), false, false);
		}
		public Vector3 primitiveOrigin;
		public Material material;
		public Bitmap textureImage = null;
		
		/// <summary>
		/// Calculate if and where a ray intersects with a primitive
		/// </summary>
		/// <param name="ray"></param>
		public virtual void Intersect(Ray ray) { }
		public virtual Vector3 GetTexel(Vector3 intersectionpoint)
        {
			return new Vector3();
        }

		
		/// <summary>
		/// Returns Vector3 representing the color of the image at the provided u and v coordinates
		/// </summary>
		/// <param name="u"> Horizontal coordinate</param>
		/// <param name="v"> Vertical coordinate</param>
		/// <param name="image"> Image used for texture</param>
		/// <returns></returns>
		public Vector3 Texture(double u, double v, Bitmap image)
		{
			//Translate 0-1 to pexel
			int x = (int)(u * (image.Width - 1));
			int y = (int)(v * (image.Height - 1));

			//Get color from bitmap
			System.Drawing.Color pixelColor = image.GetPixel(x, y);

			//Return converted pixel
			return new Vector3((float)(pixelColor.R / 255f), (float)(pixelColor.G / 255f), (float)(pixelColor.B / 255f));
		}
	}
	class Plane : Primitive
	{
		public Plane(Material _material, Vector3 _position, Vector3 _normal)
		{
			primitiveOrigin = _position;
			material = _material;
			normal = _normal;

		}
		// Normal vector of the plane
		Vector3 normal;

		/// <summary>
		/// Calculate if and where a ray intersects with the plane
		/// </summary>
		/// <param name="ray"></param>
		public override void Intersect(Ray ray)
		{
			float denominator = Vector3.Dot(normal, ray.direction);

			if (Math.Abs(denominator) > 0.0001)
			{
				Vector3 difference = primitiveOrigin - ray.origin;
				float t = Vector3.Dot(difference, normal) / denominator;


				// If the intersection point is closer to the origin of the ray then the other intersections, updates the rays fields using the planes fields
				if (ray.t > t && t > 0.001)
				{
					ray.t = t;
					ray.intersection.distance = t;
					ray.intersection.nearest = this;
					ray.doesIntersect = true;
					ray.intersection.normal = normal;
				}
			}
		}

		public override Vector3 GetTexel(Vector3 IntersectionPoint)
        {
			Vector3 color = Texture(1 / IntersectionPoint.X, 1 / IntersectionPoint.Z, textureImage);
			return color;
        }

	}
	class Sphere : Primitive
	{
		public Sphere(Material _material, Vector3 _position, float _radius)
		{
			primitiveOrigin = _position;
			material = _material;
			radius = _radius;
			radius2 = _radius * _radius;

		}

		public float radius;		
		public float radius2;       // radius squared

		/// <summary>
		/// Calculate if and where a ray intersects with a plane using the quadratic formula
		/// </summary>
		/// <param name="ray"></param>
		public override void Intersect(Ray ray)
		{
			// Variables used in quadratic formula
			Vector3 eMinusS = ray.origin - primitiveOrigin;
			Vector3 d = ray.direction;
			float dot = 2 * Vector3.Dot(d, eMinusS);


			double discriminant = (dot * dot) - 4 * Vector3.Dot(d, d) * (Vector3.Dot(eMinusS, eMinusS) - radius2);

			if (discriminant < 0.1)
			{   // 0 hits
				return;
			}
			else
			{   // there will be one or two hits
				float front = -2.0f * Vector3.Dot(d, eMinusS);
				float denominator = 2.0f * Vector3.Dot(d, d);
				if (discriminant <= 0)
				{   // 1 hit
					float t = (float)(front + Math.Sqrt(discriminant)) / denominator;  // does not matter if +- discriminant


					// If the intersection point is closer to the origin of the ray then the other intersections, updates the rays fields using the spheres fields
					if ((t < ray.t) && (t > 0.1))
                    {
                        ray.t = t;
                        ray.intersection.distance = t;
                        ray.intersection.nearest = this;
                        ray.intersection.normal = Vector3.Normalize(ray.intersectLocation- primitiveOrigin);
                        ray.doesIntersect = true;
                    }
                   
				}
				else
				{  // 2 hits
					float t1 = (float)(front - Math.Sqrt(discriminant)) / denominator;  // smaller t value
					float t2 = (float)(front + Math.Sqrt(discriminant)) / denominator;  // larger t value
				

					if (t1 < 0.1) // sphere is "behind" start of ray
					{
						if ((t2 < ray.t) && (t2 > 0.1))
						{
							ray.t = t2;
							ray.intersection.distance = t2;
							ray.intersection.nearest = this;
							ray.intersection.normal = -Vector3.Normalize(ray.intersectLocation - primitiveOrigin);
							ray.doesIntersect = true;
						}
					}
					else
					{  // one of them is in front
						if (t1 >= 0)
						{
							if ((t1 < ray.t) && (t1 > 0.1))
							{
								ray.t = t1;
								ray.intersection.distance = t1;
								ray.intersection.nearest = this;
								ray.intersection.normal = Vector3.Normalize(ray.intersectLocation - primitiveOrigin);
								ray.doesIntersect = true;
							}
						}
						else
						{

						     // return second hit (ray's origin is inside the sphere)
							if ((t2 < ray.t) && (t2 > 0.1))
							{
								ray.t = t2;
								ray.intersection.distance = t2;
								ray.intersection.nearest = this;
								ray.intersection.normal = Vector3.Normalize(ray.intersectLocation - primitiveOrigin);
								ray.doesIntersect = true;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Calculates the u and v coordinates of the intersection point and calls Texture() to return a Vector3 representing the color of the spheres texture at that coordinate
		/// </summary>
		/// <param name="intersectPoint"> Intersection point</param>
		/// <returns></returns>
		public override Vector3 GetTexel(Vector3 intersectPoint)
		{
			double theta, phi;
			double u, v;

			theta = Math.Acos(Math.Min(1,(intersectPoint.Z - primitiveOrigin.Z) / radius));
			phi = Math.Atan2((intersectPoint.Y - primitiveOrigin.Y), (double)(intersectPoint.X - primitiveOrigin.X));

			u = ((phi + Math.PI) / (2 * Math.PI));
			v = ( theta / Math.PI);
			
			return Texture(u, v, textureImage);
		}
	}

	class Triangle : Primitive
	{
		
		// Vertices, edges and geometric normal of the triangle
		Vector3 vertexA, vertexB, vertexC, edge0, edge1, edge2, normal;
		float d;

		public Triangle(Material _material, Vector3 _vertexA, Vector3 _vertexB, Vector3 _vertexC)
		{
			material = _material;
			vertexA = _vertexA;
			vertexB = _vertexB;
			vertexC = _vertexC;
			normal = Vector3.Cross((vertexB - vertexA), (vertexC - vertexA)).Normalized();

			d = -Vector3.Dot(normal, vertexA);
			edge0 = vertexB - vertexA;
			edge1 = vertexC - vertexB;
			edge2 = vertexA - vertexC;
		}


		/// <summary>
		/// Calculate if and where a ray intersects with a triangle.
		/// Checks first if the ray intersect with the plane on which the triangle lies, then checks if the intersection point is whithin the edges of the triangle using WhithinEdge()
		/// </summary>
		/// <param name="ray"></param>
		public override void Intersect(Ray ray)
		{

			float denominator = Vector3.Dot(normal, ray.direction);

			if (Math.Abs(denominator) > 0.0001)
			{
			
				float t = -(Vector3.Dot(normal, ray.origin) + d ) / denominator;

				// If the intersection point is closer to the origin of the ray then the other intersections, updates the rays fields using the triangles fields
				if (ray.t > t && t > 0.001)
				{
					Vector3 intersectPoint = ray.direction * t + ray.origin;

					if (WithinEdge(edge0, intersectPoint, vertexA) && WithinEdge(edge1, intersectPoint, vertexB) && WithinEdge(edge2, intersectPoint, vertexC))
					{
						ray.t = t;
						ray.intersection.distance = t;
						ray.intersection.nearest = this;
						ray.doesIntersect = true;
						ray.intersection.normal = -normal;
					}
				}
			}
		}


		/// <summary>
		/// Checks if and intersection point is on the inner side of a triangles vertex
		/// </summary>
		/// <param name="edge"> Edge of the triangle</param>
		/// <param name="point"> Intersection point with the plane</param>
		/// <param name="vertex"> Vertex of the triangle</param>
		/// <returns></returns>
		private bool WithinEdge(Vector3 edge, Vector3 point, Vector3 vertex)
		{
			Vector3 vp = point - vertex;
			Vector3 cross = Vector3.Cross(edge, vp);
			return Vector3.Dot(normal, cross) > 0;
		}

	}
}
