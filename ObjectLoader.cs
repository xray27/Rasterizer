using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using OpenTK;

namespace Rasterizer
{
    /*
     * This is our replacement of meshLoader.
     * It has the same functionality as meshLoader, but can also import 3d objects that consist of multiple meshes.
     * Multiple textures can be loaded in, but this is not automated.
     * To load multiple textures, a dictonary should be given with mesh-names and texture pairs.
     * 
     * We are using Assimp loader
     * Source: https://www.assimp.org/
     */

    class ObjectLoader
    {
        List<Mesh.ObjVertex> objVertices;
        List<Mesh.ObjTriangle> objTriangles;
        List<Mesh.ObjQuad> objQuads;
        int totalTriangles = 0, totalQuads = 0;

        public SceneGraphNode Load(string fileName, Shader shader, Dictionary<string, Texture> textureBook, Material material)
        {
            // load the AssimpModel
            AssimpContext context = new AssimpContext();
            const PostProcessSteps flags = PostProcessSteps.GenerateNormals | PostProcessSteps.GenerateUVCoords
                                           | PostProcessSteps.FlipUVs       | PostProcessSteps.FlipWindingOrder;
            Scene model = context.ImportFile(fileName, flags);

            // convert the AssimpModel to our SceneGraph
            totalTriangles = 0; totalQuads = 0;
            SceneGraphNode rootNode = new SceneGraphNode();
            ConvertNodeToSceneGraph(rootNode, model.RootNode, model, shader, textureBook, material);
            Console.WriteLine("Loaded {0}, with {1} Triangles and {2} Quads", fileName, totalTriangles, totalQuads);
            return rootNode;
        }

        void ConvertNodeToSceneGraph(SceneGraphNode graphNode, Node node, Scene model, Shader shader, Dictionary<string, Texture> textureBook, Material material)
        {
            // Add the transform to the graphNode
            graphNode.Transform = convertMatrix(node.Transform);

            // Add each mesh to the graphNode
            List<Mesh> meshes = new List<Mesh>();

            // We add triangles and quads to different meshes, because otherwise things break
            for (int vertexCount = 3; vertexCount <= 4; vertexCount++)
            {
                foreach (int meshIndex in node.MeshIndices)
                {
                    // Generate the mesh
                    Mesh mesh = new Mesh();
                    Assimp.Mesh assimpMesh = model.Meshes[meshIndex];
                    if (!assimpMesh.HasFaces || !assimpMesh.HasNormals || !assimpMesh.HasVertices)
                        continue;

                    // Reset objVertices, objTriangles and objQuads
                    objVertices = new List<Mesh.ObjVertex>();
                    objTriangles = new List<Mesh.ObjTriangle>();
                    objQuads = new List<Mesh.ObjQuad>();

                    //Add each face to Vertices and Triangles/Quads
                    foreach (Face face in assimpMesh.Faces)
                    {
                        // Only supports Triangle & Quads.
                        if (face.IndexCount != vertexCount)
                            continue;

                        List<int> vertexIndexes = new List<int>();
                        Vector3 vertex = new Vector3();
                        Vector2 texCoord = new Vector2();
                        Vector3 normal = new Vector3();

                        // Add each vertex to objVertices
                        foreach (int vertexIndex in face.Indices)
                        {
                            Vector3D assimpVertex = assimpMesh.Vertices[vertexIndex];
                            vertex = new Vector3(assimpVertex.X, assimpVertex.Y, assimpVertex.Z);
                            Vector3D assimpNormal = assimpMesh.Normals[vertexIndex];
                            normal = new Vector3(assimpNormal.X, assimpNormal.Y, assimpNormal.Z);
                            texCoord = new Vector2(assimpMesh.TextureCoordinateChannels[0][vertexIndex].X, assimpMesh.TextureCoordinateChannels[0][vertexIndex].Y);
                            vertexIndexes.Add(AddObjVertex(ref vertex, ref texCoord, ref normal));
                        }

                        // Add the Trianle or Quad object to list.
                        if (face.IndexCount == 3)
                        {
                            Mesh.ObjTriangle objTriangle = new Mesh.ObjTriangle();
                            objTriangle.Index0 = vertexIndexes[0];
                            objTriangle.Index1 = vertexIndexes[1];
                            objTriangle.Index2 = vertexIndexes[2];
                            objTriangles.Add(objTriangle);
                            totalTriangles++;
                        }
                        else
                        {
                            Mesh.ObjQuad objQuad = new Mesh.ObjQuad();
                            objQuad.Index0 = vertexIndexes[0];
                            objQuad.Index1 = vertexIndexes[1];
                            objQuad.Index2 = vertexIndexes[2];
                            objQuad.Index3 = vertexIndexes[3];
                            objQuads.Add(objQuad);
                            totalQuads++;
                        }
                    }

                    //finalize the mesh
                    if (!textureBook.ContainsKey(assimpMesh.Name))
                    {
                        if (!textureBook.ContainsKey(model.Materials[assimpMesh.MaterialIndex].Name))
                        {
                            //throw new ArgumentException("texture of " + assimpMesh.Name + " not given!");
                            Console.WriteLine(assimpMesh.Name);
                            Console.WriteLine(model.Materials[assimpMesh.MaterialIndex].Name);
                            mesh.texture = new Texture("../../assets/stone_house/textures/Wood_col.tga.png");
                        }
                        else
                            mesh.texture = textureBook[model.Materials[assimpMesh.MaterialIndex].Name];
                    }
                    else
                        mesh.texture = textureBook[assimpMesh.Name];

                    mesh.vertices = objVertices.ToArray();
                    mesh.triangles = objTriangles.ToArray();
                    mesh.quads = objQuads.ToArray();
                    mesh.material = material;
                    meshes.Add(mesh);
                }
            }

            // finalize graphNode
            graphNode.Meshes = meshes.ToArray();
            graphNode.Shader = shader;

            // Add each child from the model to the graphNode
            foreach (Node child in node.Children)
            {
                SceneGraphNode graphChild = new SceneGraphNode();
                graphNode.AddChildNode(graphChild);
                ConvertNodeToSceneGraph(graphChild, child, model, shader, textureBook, material);
            }

        }

        // Generate ObjVertex and add to list.
        int AddObjVertex(ref Vector3 vertex, ref Vector2 texCoord, ref Vector3 normal)
        {
            Mesh.ObjVertex newObjVertex = new Mesh.ObjVertex();
            newObjVertex.Vertex = vertex;
            newObjVertex.TexCoord = texCoord;
            newObjVertex.Normal = normal;
            objVertices.Add(newObjVertex);
            return objVertices.Count - 1;
        }

        // Convert from Matrix4x4 to Matrix4
        static Matrix4 convertMatrix(Matrix4x4 M)
        {
            Matrix4 m4 = new Matrix4();
            m4.M11 = M.A1; m4.M12 = M.B1; m4.M13 = M.C1; m4.M14 = M.D1;
            m4.M21 = M.A2; m4.M22 = M.B2; m4.M23 = M.C2; m4.M24 = M.D2;
            m4.M31 = M.A3; m4.M32 = M.B3; m4.M33 = M.C3; m4.M34 = M.D3;
            m4.M41 = M.A4; m4.M42 = M.B4; m4.M43 = M.C4; m4.M44 = M.D4;
            return m4;
        }
    }
}
