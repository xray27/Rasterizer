using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Rasterizer
{
	// mesh and loader based on work by JTalton; http://www.opentk.com/node/642

	public class Mesh
	{
		// data members
		public ObjVertex[] vertices;            // vertex positions, model space
		public ObjTriangle[] triangles;         // triangles (3 vertex indices)
		public ObjQuad[] quads;                 // quads (4 vertex indices)
		public Texture texture;                 // texture of this mesh
		public Material material;				// material of this mesh
		int vertexBufferId;                     // vertex buffer
		int triangleBufferId;                   // triangle buffer
		int quadBufferId;                       // quad buffer

		// constructor
		public Mesh( string fileName )
		{
			MeshLoader loader = new MeshLoader();
			loader.Load( this, fileName );
		}

		public Mesh()
        {
        }

		// initialization; called during first render
		public void Prepare( Shader shader )
		{
			if( vertexBufferId == 0 )
			{
				// generate interleaved vertex data (uv/normal/position (total 8 floats) per vertex)
				GL.GenBuffers( 1, out vertexBufferId );
				GL.BindBuffer( BufferTarget.ArrayBuffer, vertexBufferId );
				GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * Marshal.SizeOf( typeof( ObjVertex ) )), vertices, BufferUsageHint.StaticDraw );

				// generate triangle index array
				GL.GenBuffers( 1, out triangleBufferId );
				GL.BindBuffer( BufferTarget.ElementArrayBuffer, triangleBufferId );
				GL.BufferData( BufferTarget.ElementArrayBuffer, (IntPtr)(triangles.Length * Marshal.SizeOf( typeof( ObjTriangle ) )), triangles, BufferUsageHint.StaticDraw );

				// generate quad index array
				GL.GenBuffers( 1, out quadBufferId );
				GL.BindBuffer( BufferTarget.ElementArrayBuffer, quadBufferId );
				GL.BufferData( BufferTarget.ElementArrayBuffer, (IntPtr)(quads.Length * Marshal.SizeOf( typeof( ObjQuad ) )), quads, BufferUsageHint.StaticDraw );
			}
		}

		// render the mesh using the supplied shader and matrix
		public void Render( Shader shader, Matrix4 TprojectView, Matrix4 Tmodel, Vector3 camPos, Light[] allLights, Vector3 ambient)
		{
			// on first run, prepare buffers
			Prepare( shader );

			// safety dance
			GL.PushClientAttrib( ClientAttribMask.ClientVertexArrayBit );

			// enable texture
			int texLoc = GL.GetUniformLocation( shader.programID, "pixels" );

			// [ TEXTURE ]
			GL.Uniform1( texLoc, 0 );
			GL.ActiveTexture( TextureUnit.Texture0 );
			GL.BindTexture( TextureTarget.Texture2D, texture.id );

			// enable shader
			GL.UseProgram( shader.programID );

			// pass transforms to vertex shader
			GL.UniformMatrix4( shader.uniform_TprojectView, false, ref TprojectView );
			GL.UniformMatrix4( shader.uniform_Tmodel, false, ref Tmodel );
			GL.Uniform3(shader.uniform_camPos, camPos);

			// pass uniform variables to vertex shader
			GL.Uniform1(shader.uniform_Kd, material.Kd);
			GL.Uniform3(shader.uniform_Ks, material.Ks);
			GL.Uniform3(shader.uniform_Ka, material.Ka);
			GL.Uniform1(shader.uniform_glosinness, material.glosiness);

			GL.Uniform3(shader.uniform_ambient, ambient);

			//pass all lights to shader, in a very cheaty way
			int programID = shader.programID;
            for (int i = 0; i < 12; i++)
            {
				GL.Uniform3(GL.GetUniformLocation(programID, "lights[" + i.ToString() + "].color"), allLights[i].color);
				GL.Uniform3(GL.GetUniformLocation(programID, "lights[" + i.ToString() + "].position"), allLights[i].position);
				GL.Uniform3(GL.GetUniformLocation(programID, "lights[" + i.ToString() + "].specularColor"), allLights[i].specularColor);
				GL.Uniform1(GL.GetUniformLocation(programID, "lights[" + i.ToString() + "].strength"), allLights[i].strength);
				if(allLights[i].GetType() == typeof(Spotlight))
                {
					GL.Uniform1(GL.GetUniformLocation(programID, "lights[" + i.ToString() + "].isSpot"), 1);
					GL.Uniform1(GL.GetUniformLocation(programID, "lights[" + i.ToString() + "].angle"), ((Spotlight) allLights[i]).angle);
					GL.Uniform3(GL.GetUniformLocation(programID, "lights[" + i.ToString() + "].dir"), ((Spotlight) allLights[i]).dir);
				}
                else
					GL.Uniform1(GL.GetUniformLocation(programID, "lights[" + i.ToString() + "].isSpot"), 0);
			}


			// enable position, normal and uv attributes
			GL.EnableVertexAttribArray( shader.attribute_vpos );
			GL.EnableVertexAttribArray( shader.attribute_vnrm );
			GL.EnableVertexAttribArray( shader.attribute_vuvs );



			// bind interleaved vertex data
			GL.EnableClientState( ArrayCap.VertexArray );
			GL.BindBuffer( BufferTarget.ArrayBuffer, vertexBufferId );
			GL.InterleavedArrays( InterleavedArrayFormat.T2fN3fV3f, Marshal.SizeOf( typeof( ObjVertex ) ), IntPtr.Zero );

			// link vertex attributes to shader parameters 
			GL.VertexAttribPointer( shader.attribute_vuvs, 2, VertexAttribPointerType.Float, false, 32, 0 );
			GL.VertexAttribPointer( shader.attribute_vnrm, 3, VertexAttribPointerType.Float, true, 32, 2 * 4 );
			GL.VertexAttribPointer( shader.attribute_vpos, 3, VertexAttribPointerType.Float, false, 32, 5 * 4 );

			// bind triangle index data and render
			GL.BindBuffer( BufferTarget.ElementArrayBuffer, triangleBufferId );
			GL.DrawArrays( PrimitiveType.Triangles, 0, triangles.Length * 3 );

			// bind quad index data and render
			if( quads.Length > 0 )
			{
				GL.BindBuffer( BufferTarget.ElementArrayBuffer, quadBufferId );
				GL.DrawArrays( PrimitiveType.Quads, 0, quads.Length * 4 );
			}

			// restore previous OpenGL state
			GL.UseProgram( 0 );
			GL.PopClientAttrib();
		}

		// layout of a single vertex
		[StructLayout( LayoutKind.Sequential )]
		public struct ObjVertex
		{
			public Vector2 TexCoord;
			public Vector3 Normal;
			public Vector3 Vertex;
		}

		// layout of a single triangle
		[StructLayout( LayoutKind.Sequential )]
		public struct ObjTriangle
		{
			public int Index0, Index1, Index2;
		}

		// layout of a single quad
		[StructLayout( LayoutKind.Sequential )]
		public struct ObjQuad
		{
			public int Index0, Index1, Index2, Index3;
		}
	}
}