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

    class Application
    {

        public Application(Surface _screen, GameWindow window)
        {
            render3DView = false;
            currentScreen = 0;
            currentSkybox = 0;

            scene = new Scene();
            scene.Build(currentScreen);


            screen = _screen;

            camera = new Camera(new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(0, 1, 0), 90, window);

            rt = new RayTracer(scene, camera, screen);
            rtSingle = new RayTracer(scene, camera, screen);

            //Handles keypresses
            window.KeyDown += CaptureKeys;

            skyboxfaces.Add(("CNTower", ".jpg"));
            skyboxfaces.Add(("skybox1", ".png"));
            skyboxfaces.Add(("SaintlazarusChurch", ".jpg"));
            skyboxfaces.Add(("UnionSquare", ".jpg"));
            skyboxfaces.Add(("Medborgarplatsen", ".jpg"));

        }

        Scene scene;
        Camera camera;
        public Surface screen;
        RayTracer rt, rtSingle;
        bool render3DView;
        int currentScreen, currentSkybox;
        List<(string, string)> skyboxfaces = new List<(string, string)>();
       

        public void Start()
        {
            Stopwatch watch = Stopwatch.StartNew();
            watch.Start();

            if (render3DView)
            {
                if (camera.multithreading)
                {
                    rt.RenderMultiThread();
                }
                else
                {
                    rtSingle.Render3d();
                }
            }
            else
            {
                rt.RenderDebugWindow(scene.objects, scene.lights);
            }
            //Print rendertime on screen
            screen.Print(watch.ElapsedMilliseconds + " ms", 0, 0, Color.Red.ToArgb());
            screen.Print(scene.sceneTitle, 100, 0, Color.White.ToArgb());

            watch.Stop();

        }
        private void CaptureKeys(object sender, KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.P:
                    Environment.Exit(200);
                    break;
                case Key.V:
                    render3DView = !render3DView;
                    break;
                case Key.Z:
                    //next present scene
                    scene.Build(currentScreen-- %7);
                    break;
                case Key.X:
                    //previous scene
                    scene.Build(currentScreen++ %7);
                    break;
                case Key.N:
                    //Next Skybox
                    (string folder, string ext) = skyboxfaces[currentSkybox++ % skyboxfaces.Count];
                    rt.InitSkyboxFaces(folder, ext);
                    break;
            }
        }

    }

}