using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Collections.Generic;
using System;
using System.Drawing;

namespace Template
{

	class Ray 
    {
		public Ray(Vector3 _origin, Vector3 _direction)
		{
			origin = _origin;
			direction = Vector3.Normalize(_direction);
		}

		public Vector3 origin;
		public Vector3 direction;
		public float t = float.MaxValue;

		public int RayColor(List<Object> objects, List<Light> lights)
        {
            //Vector3 unit_direction = direction.Normalized();
            //double t = 0.5 * (unit_direction.Y + 1.0);
            //Color color = Color.FromArgb(1 * 255, 1 * 255, 1 * 255);

            
			//return (int)((1.0 - t) * Color.FromArgb(1 * 255, 1 * 255, 1 * 255).ToArgb() + t * (double)Color.FromArgb(127, 178, 255).ToArgb());         
			Object firstObject = new Object();
            firstObject.material = new Material(Color.Black, 0);
            foreach (Object obj in objects)
            {
				obj.Intersect(this);

				//If the ray hits anything
				if (t > 0)
                {
					foreach(Light light in lights)
                    {
                        if (obj.HasShadow(light, this))
                        {
							return Color.Black.ToArgb();
						}

                    }
					return obj.material.color.ToArgb();

				}
            }
            return firstObject.material.color.ToArgb();


        }

		public Vector3 At(float t) {
            return origin + t* direction;
		}


}
	
	class Material
    {
		public Material(Color _color, float _diffuseConst)
		{
			color = _color;
			diffuseConst = _diffuseConst;
		}
		public float diffuseConst;
		public Color color;
    }
	class Object
    {
		
		public Vector3 position;
		public Material material;
		public virtual void Intersect(Ray ray) {}
		public virtual bool HasShadow(Light light, Ray ray) { return false; }
	}
	class Light : Object
	{
		public Light(Material _material, Vector3 _position)
		{
			position = _position;
			material = _material;

		}
	}
	class Scene
    {
		public List<Light> lights = new List<Light>();
		public List<Object> objects = new List<Object>();


		Material red = new Material(Color.Red, 0);
		Material green = new Material(Color.Green, 0);
		
		public void Build()
        {
			objects.Add(new Sphere(red, new Vector3(0, 2, -30), 5));
			objects.Add(new Sphere(green, new Vector3(-40, -20, -50), 10));
			lights.Add(new Light(green, new Vector3(40, 40, 0)));
		}
    }
	class Camera
    {
		const float ratio = (float)16 / (float)9;
		public Vector3 cameraPosition, viewDirection, horizontal, vertical, p0;
		public int screenWidth, screenHeight;
		public Surface screen;
		float fov;
		public Camera(Vector3 _position,  int _viewportHeigth, float _fov, int _screenWidth, int _screenHeigth)
		{
			screenWidth = _screenWidth;
			screenHeight = _screenHeigth;
			//viewport heigth
			float viewportHeigth = _viewportHeigth;
			//viewport width
			float viewportWidth = _viewportHeigth * ratio;
			//FOV
			fov = _fov;

			//E camera position
			cameraPosition = _position;
			//Viewport X vector
			horizontal = new Vector3(viewportWidth, 0, 0);
			//Viewport Y vector
			vertical = new Vector3(0, viewportHeigth, 0);
			//Low Left corner 
			p0 = cameraPosition - horizontal / 2 - vertical / 2 - new Vector3(0, 0, fov);

			
		}
		public void Render(List<Object> objects, List<Light> lights)
        {

			

			for (int y =0; y < screenWidth; y++)
            {
				for(int x = 0; x < screenHeight; x++)
                {
					float u = (float)x / (screenHeight - 1);
					float v = (float)y / (screenWidth - 1);
					Vector3 dir = new Vector3(p0 + u * horizontal + v * vertical - cameraPosition);
					Ray r = new Ray(this.cameraPosition, dir);
					screen.Plot(x, y, r.RayColor(objects, lights)); ;
				}
            }
        }
		
	

	}
	//class Cube : Object
	//{
	//	public Cube(Material _material, Vector3 _position, float _width, float _heigth, float _depth)
	//	{
	//		position = _position;
	//		material = _material;
	//		width = _width;
	//		heigth = _heigth;
	//		depth = _depth;

	//	}
	//	float width, heigth, depth;

	//	public override void Intersect(Ray ray){

	//	}
	//}
	class Sphere : Object
    {
		public Sphere(Material _material, Vector3 _position, float _radius)
		{
			position = _position;
			material = _material;
			radius = _radius;
			radius2 = _radius * _radius;

		}

		float radius;
		float radius2;


        //public override void Intersect(Ray ray)
        //{

        //	Vector3 c = this.position - ray.origin;
        //	float t = Vector3.Dot(c, ray.direction);
        //	Vector3 q = c - t * ray.direction;
        //	float p2 = q.Length * q.Length;
        //	if (p2 > this.radius2) return;
        //	t -= (float)Math.Sqrt(this.radius2 - p2);
        //	if ((t < ray.t) && (t > 0)) ray.t = t;
        //}
        public override void Intersect(Ray ray)
        {
			Vector3 oc = ray.origin - this.position;
			float a = ray.direction.Length * ray.direction.Length;
			float b = (float)(Vector3.Dot(oc, ray.direction));
			float c = oc.Length * oc.Length - radius * radius;
			float discriminant = b * b - a * c;
			if (discriminant < 0)
            {
				ray.t = -1;
            }
            else
            {
				ray.t = (float)(-b - Math.Sqrt(discriminant) / a);
            }
		}
        public override bool HasShadow(Light light, Ray ray)
        {
			Vector3 epsilon = new Vector3(0.02F, 0.02F, 0.02F);
			Vector3 intersectionPoint = ray.origin + ray.t * ray.direction + epsilon;
			Vector3 light_pointVect = intersectionPoint - light.position;
			Ray shadowRay = new Ray(intersectionPoint, light_pointVect);
			this.Intersect(shadowRay);
			if (shadowRay.t > 0)
            {
				return true;
            }
			return false;
        }
    }

}