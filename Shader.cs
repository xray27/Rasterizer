using System;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace Rasterizer
{
	public class Shader
	{
		// data members
		public int programID, vsID, fsID;
		public int attribute_vpos;
		public int attribute_vnrm;
		public int attribute_vuvs;

		public int uniform_TprojectView;
		public int uniform_Tmodel;
		public int uniform_camPos;
		
		public int uniform_Kd;
		public int uniform_Ks;
		public int uniform_Ka;
		
		public int uniform_glosinness;
		public int uniform_ambient;
		public int uniform_light1_position;
		public int uniform_light1_color;
		public int uniform_light1_specularColor;
		public int uniform_light1_strength;
		public int uniform_light2_position;
		public int uniform_light2_color;
		public int uniform_light2_specularColor;
		public int uniform_light2_strength;

		// constructor
		public Shader( string vertexShader, string fragmentShader )
		{
			// compile shaders
			programID = GL.CreateProgram();
			Load( vertexShader, ShaderType.VertexShader, programID, out vsID );
			Load( fragmentShader, ShaderType.FragmentShader, programID, out fsID );
			GL.LinkProgram( programID );
			Console.WriteLine( GL.GetProgramInfoLog( programID ) );

			// get locations of shader parameters
			attribute_vpos = GL.GetAttribLocation(programID, "vPosition" );				// untransformed vertex position
			attribute_vnrm = GL.GetAttribLocation(programID, "vNormal" );				// untransformed vertex normal
			attribute_vuvs = GL.GetAttribLocation(programID, "vUV" );					// vertex uv coordinate

			uniform_TprojectView = GL.GetUniformLocation(programID, "TprojectView" );   // transformation matrix from world space to clip space
			uniform_Tmodel = GL.GetUniformLocation(programID, "Tmodel");                // transformation matrix from local space to world space
			uniform_camPos = GL.GetUniformLocation(programID, "camPos");                // camera position in world space
			
			uniform_Kd = GL.GetUniformLocation(programID, "Kd"); ;						// mesh diffuse coefficient. Diffuse color will be taken from texture, but multiplied by this coefficient
			uniform_Ks = GL.GetUniformLocation(programID, "Ks"); ;						// mesh specular color
			uniform_Ka = GL.GetUniformLocation(programID, "Ka"); ;						// mesh abient color
			uniform_glosinness = GL.GetUniformLocation(programID, "glosiness"); ;       // mesh specular glosiness 
			
			uniform_ambient = GL.GetUniformLocation(programID, "ambient");              // ambient room color
			uniform_light1_position = GL.GetUniformLocation(programID, "lights[0].position");
			uniform_light1_color = GL.GetUniformLocation(programID, "lights[0].color");
			uniform_light1_specularColor = GL.GetUniformLocation(programID, "lights[0].specularColor");
			uniform_light1_strength = GL.GetUniformLocation(programID, "lights[0].strength");
			uniform_light2_position = GL.GetUniformLocation(programID, "lights[1].position");
			uniform_light2_color = GL.GetUniformLocation(programID, "lights[1].color");
			uniform_light2_specularColor = GL.GetUniformLocation(programID, "lights[1].specularColor");
			uniform_light2_strength = GL.GetUniformLocation(programID, "lights[1].strength");
		}

		// loading shaders
		void Load( string filename, ShaderType type, int program, out int ID )
		{
			// source: http://neokabuto.blogspot.nl/2013/03/opentk-tutorial-2-drawing-triangle.html
			ID = GL.CreateShader( type );
			using( StreamReader sr = new StreamReader( filename ) ) GL.ShaderSource( ID, sr.ReadToEnd() );
			GL.CompileShader( ID );
			GL.AttachShader( program, ID );
			Console.WriteLine( GL.GetShaderInfoLog( ID ) );
		}
	}
}
