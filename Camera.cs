using OpenTK;
using OpenTK.Input;
using System;


namespace Template
{
    class Camera
	{
		GameWindow window;

		// Width/length ratio of the window
		const float ratio = (float)16 / (float)9;

		public Vector3
			position,                   // Camera position
			viewDirection, right, up,   // orthonormal basis of camera
			planeCenter,                // Position of the center of the image plane
			p0, p1,						// Positions of top left and top right corner of the image plane
			u, v;						// Width and height of image plane
		
		// Represents up direction of the scene (constant)
		public Vector3 sceneUp = new Vector3(0, 1, 0);

		// Distance from camera to image plane
		public float distanceToIP;
		
		// Degree of the arc formed by the camera and the edges of the image plane
		float deg;

		public bool skyboxEnable = false, multithreading = false;

		public int 
			AA = 1,						// Numbers of rays used for anti-aliasing
			NUMB_RAYS = 80;				// Number of viewing rays used in debug mode

		public Camera(Vector3 _position, Vector3 _lookatDirection, Vector3 _right, Vector3 _up, float _fov, GameWindow _window)
		{
			//Store all orthonormal camera vectors
			viewDirection = Vector3.Normalize(_lookatDirection - position);
			up = _up;
			right = _right;

			//Camera position
			position = _position;
			
			//Field of view
			deg = _fov / 2;

			window = _window;

			CalculatePlaneDetails();
			window.KeyDown += handleKeyPress;

		}


		/// <summary>
		/// Calculate location and dimensions of the image plane based on the camera position and direction and the fov
		/// </summary>
		private void CalculatePlaneDetails()
		{
				
				// Calculate distance from camera to image plane using the provided angle based on the desired field of view
				 distanceToIP = (float)( ratio * right.Length / Math.Tan((Math.PI / 180) * deg));
			    
				// move the center of the image plane based on the calculated distance
				planeCenter = position + distanceToIP * viewDirection;

				//Top Left corner 
				p0 = planeCenter + up - ratio * right;
				//Top right corner
				p1 = planeCenter + up + ratio * right;
				//Lower left corner
				Vector3 p2 = planeCenter - up - ratio * right;

				//Image plane width and height
				u = new Vector3(p1 - p0);
				v = new Vector3(p2 - p0);

		}


		/// <summary>
		/// Returns a Vector3 pointing to a point on the "screenPlane" (image plane) when provided the width and height location of a pixel on the screen 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public Vector3 PointOnScreenPlane(float a, float b)
		{
			Vector3 pointScreenPlane = p0 + a * u + b * v;
			return pointScreenPlane;
		}

		// Change horizontal direction of the camera

		/// <summary>
		/// Change horizontal direction of the camera by rotating around the global up vector
		/// </summary>
		/// <param name="deg">Angle of rotation in degrees</param>
		private void LookHorizontal(float deg)
		{
			float cosDeg = (float)(Math.Cos(deg));
			float sinDeg = (float)(Math.Sin(deg));

			// Use Rodriques' rotation formula to rotate the viewDirection vector around the globalUp vector. Rotation around the camera up vector causes the horizon to tilt
			viewDirection = (viewDirection * cosDeg + Vector3.Cross(sceneUp, viewDirection) * sinDeg + sceneUp * (Vector3.Dot(sceneUp, viewDirection) * (1 - cosDeg)));
			right = (right * cosDeg + Vector3.Cross(sceneUp, right) * sinDeg + sceneUp * (Vector3.Dot(sceneUp, right) * (1 - cosDeg)));

			up = Vector3.Cross(viewDirection, right).Normalized();
			right = Vector3.Cross(up, viewDirection).Normalized();

		}

		/// <summary>
		/// Change vertical direction of the camera by rotating around the x-axis
		/// </summary>
		/// <param name="deg"> Angle of rotation in degrees</param>
		private void LookVertical(float deg)
		{
			// Calculate sine and cosine of the rotation angle
			float cosDeg = (float)(Math.Cos(deg));
			float sinDeg = (float)(Math.Sin(deg));

			// Rotate the viewDirection around the x-axis
			float newDirY = cosDeg * viewDirection.Y - sinDeg * viewDirection.Z;
			float newDirZ = -sinDeg * viewDirection.Y + cosDeg * viewDirection.Z;
			viewDirection = new Vector3(viewDirection.X, newDirY, newDirZ).Normalized();
			right = Vector3.Cross(up, viewDirection).Normalized();
			up = Vector3.Cross(viewDirection, right).Normalized();

		}


		// Handle keypresses by calling the relevant method for each keypress followed by recalculating the location/dimensions of the image plane
		private void handleKeyPress(object sender, KeyboardKeyEventArgs e)
		{
			switch (e.Key)
			{	
			// Move camera:

				// Forwards
				case Key.W:
					position += viewDirection;
					break;
				
				// Backwards
				case Key.A:
					position -= right;
					break;
				
				// To the right
				case Key.D:
					position += right;
					break;
				
				// To the left
				case Key.S:
					position -= viewDirection;
					break;
				
				// Up
				case Key.Q:
					position += up;
					break;
				
				// Down
				case Key.E:
					position -= up;
					break;
				
				// Look right
				case Key.Right:
					LookHorizontal(0.1f);
					break;
				
				// Look left
				case Key.Left:
					LookHorizontal(-0.1f);
					break;
				
				// Look up
				case Key.Up:
					LookVertical(-0.1f);
					break;
				
				// Look down
				case Key.Down:
					LookVertical(0.1f);
					break;
				
				// Change field of view
				case Key.F:
					deg += 0.5f % 90;
					break;
				
				// Toggle skybox on/off
				case Key.B:
					multithreading = false;
					skyboxEnable = !skyboxEnable;
					break;
				
				// Increase anti-aliasing
				case Key.I:
					AA++;
					Console.WriteLine("Increased AA sampling to " + AA + " Rays/Pixel");
					break;
				
				// Decrease anti-aliasing
				case Key.U:
					AA -= AA-1 >= 1 ? 1 : 0;
					Console.WriteLine("Decreased AA sampling to " + AA + " Rays/Pixel");
					break;
				
				// Increase number of viewing rays in debug mode
				case Key.R:
					NUMB_RAYS++;
					break;

				//	Decrease number of viewing rays in debug mode
				case Key.T:
					NUMB_RAYS--;
					break;

				// Toggle multithreading on/off
				case Key.Y:
					
					multithreading = !multithreading;
					if (multithreading == true)
					{
						Console.WriteLine("Switched to multithreading");
						skyboxEnable = false;
					}
                    else
                    {
						Console.WriteLine("Switched to single core mode");
					}
					break;

			}

			//Recalculate the location/dimensions of the image plane
			CalculatePlaneDetails();

		}


	}
}
