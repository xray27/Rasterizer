using System.Diagnostics;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;

namespace Rasterizer
{
	class MyApplication
	{
		// member variables
		public Surface screen;                  // background surface for printing etc.
		const float PI = 3.1415926535f;         // PI
		float a = 0;                            // teapot rotation angle
		Stopwatch timer;                        // timer for measuring frame duration
		Shader shader;                          // shader to use for rendering
		Shader postproc;                        // shader to use for post processing
		RenderTarget target;                    // intermediate render target
		ScreenQuad quad;                        // screen filling quad for post processing
		bool useRenderTarget = true;

		Vector3 ambient;						// the ambient color
		SceneGraphNode sceneGraph;
		SceneGraphNode plant1;
		SceneGraphNode plant2;
		SceneGraphNode plant3;
		Light plantLight1;
		Light plantLight2;
		Light plantLight3;
		Light[] treeLights;
		Vector3 treeColor;
		Light foxLight;
		Vector3 foxColor;

		Camera camera;
		const float cameraRotationSpeed = 0.01f;
		const float cameraTranslationSpeed = 1f;
		const float cameraRollSpeed = 0.2f;

		// initialize
		public void Init()
		{
			// initialize stopwatch
			timer = new Stopwatch();
			timer.Reset();
			timer.Start();

			// initialize shaders
			shader = new Shader("../../shaders/vs.glsl", "../../shaders/fs.glsl");
			postproc = new Shader("../../shaders/vs_post.glsl", "../../shaders/fs_post.glsl");


			// create the render target
			target = new RenderTarget(screen.width, screen.height);
			quad = new ScreenQuad();



			// create camera
			camera = new Camera(1.2f, 1.3f, new Vector3(-27,28,158), new Vector3(1f/(float)Math.Sqrt(2),0,-1f / (float)Math.Sqrt(2)), new Vector3(0,1,0));

			// prepare matrix for vertex shader

			Material m = new Material(10000f, new Vector3(5000, 5000, 5000), new Vector3(1, 1, 1), 20);

			Dictionary<string, Texture> textureDict = new Dictionary<string, Texture>();
			textureDict.Add("LP_wood2", new Texture("../../assets/stone_house/textures/Wood_col.tga.png"));
			textureDict.Add("Stone", new Texture("../../assets/stone_house/textures/well_col.tga.png"));
			textureDict.Add("Ground", new Texture("../../assets/stone_house/textures/well_col.tga.png"));
			textureDict.Add("Roof", new Texture("../../assets/stone_house/textures/Roof_col.tga.png"));
			textureDict.Add("Wall", new Texture("../../assets/stone_house/textures/Wall_col.tga.png"));
			textureDict.Add("Cube.002", new Texture("../../assets/tower/textures/watchtower_Diffuse.png"));
			textureDict.Add("Plane.001", new Texture("../../assets/tower/textures/TexturesCom_Ground_SoilRocky2C_4x4_1K_albe.png"));
			textureDict.Add("Material #25", new Texture("../../assets/environment/textures/moshrom2.png"));
			textureDict.Add("Material #26", new Texture("../../assets/environment/textures/Untitled-1.png"));
			textureDict.Add("Material #27", new Texture("../../assets/environment/textures/dead_tree.png"));
			textureDict.Add("Material #28", new Texture("../../assets/environment/textures/character1.png"));
			textureDict.Add("Mesh", new Texture("../../assets/plant/textures/plant.jpg"));
			textureDict.Add("IDP_leaves", new Texture("../../assets/plant/textures/plant.jpg"));
			textureDict.Add("U3DMesh", new Texture("../../assets/fox/textures/foxtex.png"));

			ObjectLoader loader = new ObjectLoader();

			sceneGraph = loader.Load("../../assets/environment/Lost Treasure.fbx", shader, textureDict, m);
			sceneGraph.Transform = Matrix4.CreateScale(0.3f);

			SceneGraphNode tower = loader.Load("../../assets/tower/Watchtower.fbx", shader, textureDict, m);
			tower.Transform = Matrix4.CreateScale(0.6f) * Matrix4.CreateRotationY(-0.5f) * Matrix4.CreateTranslation(250,0,50);
			sceneGraph.AddChildNode(tower);

			SceneGraphNode house = loader.Load("../../assets/stone_house/stone_house.obj", shader, textureDict, m);
			house.Transform = Matrix4.CreateScale(20) * Matrix4.CreateRotationY(-1.2f) * Matrix4.CreateTranslation(-30,25,100);
			sceneGraph.AddChildNode(house);

			
			SceneGraphNode fox = loader.Load("../../assets/fox/FOX.fbx", shader, textureDict, m);
			fox.Transform = Matrix4.CreateRotationY(-1.2f) * Matrix4.CreateTranslation(0, 50, 300);
			sceneGraph.AddChildNode(fox);

			SceneGraphNode fox2 = loader.Load("../../assets/fox/FOX.fbx", shader, textureDict, m);
			fox2.Transform = Matrix4.CreateScale(1.3f) * Matrix4.CreateRotationY(-2.5f) * Matrix4.CreateTranslation(150, 50, 350);
			sceneGraph.AddChildNode(fox2);

			plant1  = loader.Load("../../assets/plant/plant.fbx", shader, textureDict, m);
			plant1.Transform = Matrix4.CreateRotationY(a) * Matrix4.CreateTranslation(150, -200, 600);
			sceneGraph.AddChildNode(plant1);

			plant2 = loader.Load("../../assets/plant/plant.fbx", shader, textureDict, m);
			plant2.Transform = Matrix4.CreateScale(0.2f) * Matrix4.CreateRotationY(a) * Matrix4.CreateTranslation(0, 200, 100);
			plant1.AddChildNode(plant2);

			plant3 = loader.Load("../../assets/plant/plant.fbx", shader, textureDict, m);
			plant3.Transform = Matrix4.CreateScale(0.7f) * Matrix4.CreateRotationY(a) * Matrix4.CreateTranslation(0, 0, 200);
			plant2.AddChildNode(plant3);

			//add lights
			Vector3 plantColor = new Vector3(0.7f, 0, 0);
			treeColor = new Vector3(252f/255, 3f/255, 215f/255);
			foxColor = new Vector3(235f / 255, 161f / 255, 52f / 255);


			Light light = new Light(new Vector3(73f, 139f, 18f), new Vector3(252f / 255, 248f / 255, 3f / 255), new Vector3(10,10,10), 0.3f);
			Light light2 = new Light(new Vector3(-18,69,45), new Vector3(1, 1, 1), new Vector3(0, 0, 0), 0.4f);
			foxLight = new Light(new Vector3(8.897167f, 48.17423f, 119.1841f), foxColor, new Vector3(0, 0, 0), 0.2f);

			Light tree1 = new Light(new Vector3(-54.37668f, 59.24127f, 137.7926f),treeColor, treeColor, 0.06f);
			Light tree2 = new Light(new Vector3(90.8361f, 79.11143f, 81.52566f), treeColor, treeColor, 0.08f);
			Light tree3 = new Light(new Vector3(-121.8605f, 238.4689f, -121.1556f), treeColor, treeColor, 1f);
			Light tree4 = new Light(new Vector3(56.31868f, 130.6668f, -45.99488f), treeColor, treeColor, 0.5f);
			treeLights = new Light[] { tree1, tree2, tree3, tree4 };

			plantLight1 = new Light(new Vector3(31.67615f, 100.4866f, 167.2415f), plantColor, plantColor, 0.2f);
			plantLight2 = new Light(new Vector3(), plantColor, plantColor, 0.03f);
			plantLight3 = new Light(new Vector3(), plantColor, plantColor, 0.03f);

			Spotlight spot = new Spotlight(new Vector3(-10.96501f, 18.96148f, 183.7313f), new Vector3(252f / 255, 248f / 255, 3f / 255), new Vector3(0, 0, 0), 0.3f, new Vector3(0.4997227f, 0.8501725f, -0.1657829f), 0.4f);
			Spotlight spot2 = new Spotlight(new Vector3(13.24524f, 40.627f, 112.4476f), new Vector3(0.7f, 0.3f, 0.3f), new Vector3(0, 0, 0), 0.3f, new Vector3(-0.04823443f, -0.9766878f, 0.2091758f), 0.3f);
			
			ambient = new Vector3(0.01f, 0.01f, 0.01f);
			sceneGraph.Lights = new Light[] { light, light2, foxLight, tree1, tree2, tree3, tree4, spot, spot2};
			plant1.Lights = new Light[] { plantLight1 };
			plant2.Lights = new Light[] { plantLight2 };
			plant3.Lights = new Light[] { plantLight3 };
		}

		// tick for background surface
		public void Tick()
		{
			screen.Clear(0);
			screen.Print("hello world", 2, 2, 0xffff00);
		}

		// tick for OpenGL rendering code
		public void RenderGL()
		{
			// measure frame duration
			float frameDuration = timer.ElapsedMilliseconds;
			timer.Reset();
			timer.Start();


			// update rotation
			a += 0.001f * frameDuration;
			if (a > 2 * PI) a -= 2 * PI;

			//update positions / rotations / lights
			plant1.Transform = Matrix4.CreateScale(0.8f) *Matrix4.CreateRotationY(a) * Matrix4.CreateTranslation(100, -150, 560);
			plant2.Transform = Matrix4.CreateScale(0.3f) * Matrix4.CreateRotationY(3*a) * Matrix4.CreateTranslation(0, 200, 100);
			plant3.Transform = Matrix4.CreateScale(0.7f) * Matrix4.CreateRotationY(7*a) * Matrix4.CreateTranslation(0, 0, -200);
			plantLight2.position = new Vector3(32f, 50f, 167f) - new Vector3(0, 0, -25) * Matrix3.CreateRotationY(a);
			plantLight3.position = plantLight2.position - new Vector3(0,5,0) -  new Vector3(0, 0, 15) * Matrix3.CreateRotationY(a * 4);

			foreach(Light light in treeLights)
            {
				light.color = treeColor * ((float)Math.Sin(a) / 3f + 0.5f);
				light.specularColor = light.color;
            }
			foxLight.color = foxColor * ((float)Math.Cos(a) / 3f + 0.5f);

			if (useRenderTarget)
			{
				// enable render target
				target.Bind();

				// render scene to render target
				sceneGraph.Render(camera, ambient);

				// render quad
				target.Unbind();
				quad.Render(postproc, target.GetTextureID());
			}
			else
			{
				// render scene directly to the screen
				sceneGraph.Render(camera, ambient);
			}
		}

		// Handle keypresses
		public void OnKeyboardPress(KeyboardState keyboard)
		{
			// Translate and Roll camera
			float x = 0f, y = 0f, z = 0f, roll = 0f;
			if (keyboard[OpenTK.Input.Key.W])
				z += cameraTranslationSpeed;
			if (keyboard[OpenTK.Input.Key.S])
				z -= cameraTranslationSpeed;
			if (keyboard[OpenTK.Input.Key.A])
				x += cameraTranslationSpeed;
			if (keyboard[OpenTK.Input.Key.D])
				x -= cameraTranslationSpeed;
			if (keyboard[OpenTK.Input.Key.R])
				y += cameraTranslationSpeed;
			if (keyboard[OpenTK.Input.Key.F])
				y -= cameraTranslationSpeed;
			if (keyboard[OpenTK.Input.Key.Q])
				roll += cameraRollSpeed;
			if (keyboard[OpenTK.Input.Key.E])
				roll -= cameraRollSpeed;
			if(keyboard[OpenTK.Input.Key.ShiftLeft])
            {
				x *= 5;
				y *= 5;
				z *= 5;
            }
			camera.MoveCamera(x: x, y: y, z: z, roll: roll);
		}

		// Handle MouseMovement
		public void OnMouseMove(int xDelta, int yDelta)
        {
			// Rotate the camera
			camera.MoveCamera(yaw: -xDelta*cameraRotationSpeed, pitch: -yDelta*cameraRotationSpeed);

		}

		public void OnResize(int width, int height)
        {
			screen = new Surface(width, height);
			target = new RenderTarget(width, height);
			camera.Aspect = (float)(width) / height;
		}
	}
}
