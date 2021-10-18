using System.Collections.Generic;
using OpenTK;

namespace Rasterizer
{
    class SceneGraphNode
    {
        public Mesh[] Meshes { get; set; }                      // The meshes to be rendered, one graphnode can have multiple meshes, using the same transform
        public Matrix4 Transform { get; set; }                  // Transformation matrix, transfroming from object space to world space.
        public Shader Shader { get; set; }                      // Shader to use for rendering.
        public Light[] Lights { get; set; }                     // Each node can have optional lights

        List<SceneGraphNode> childNodes;    // All the child nodes of this node

        public SceneGraphNode(Mesh[] meshes, Matrix4 transform, Shader shader, Light[] lights = null, List<SceneGraphNode> childNodes = null)
        {
            this.Meshes = meshes;
            this.Transform = transform;
            this.Shader = shader;
            this.childNodes = childNodes;
            if (this.childNodes == null)
                this.childNodes = new List<SceneGraphNode>();
            this.Lights = lights;
        }

        public SceneGraphNode()
        {
            this.Meshes = null;
            this.Transform = Matrix4.Identity;
            this.Shader = null;
            this.childNodes = new List<SceneGraphNode>();
            this.Lights = null;
        }

        // Main render method, render the SceneGraph given a camera matrix
        public void Render(Camera camera, Vector3 ambient)
        {
            //First get all lights, but at maximum 12 because the shader doesn't expect more.
            Light[] allLights = new Light[12];
            getAllLights(allLights);

            Matrix4 Tproject = Matrix4.CreatePerspectiveFieldOfView(camera.Fov, camera.Aspect, .1f, 1000);    // Create the projection transformation

            // On the first call, Tmodel is the identety matrix; the model matrix will be combined in the renderAll function.
            // TprojectView is the transformation matrix from world space to clip space.
            RenderAll(camera.Tcamera * Tproject, Matrix4.Identity, camera.Pos, allLights, ambient);
        }

        // Helpfunction for Render, render this node and all childNodes.
        public void RenderAll(Matrix4 TprojectView, Matrix4 Tmodel, Vector3 camPos, Light[] allLights, Vector3 ambient)
        {
            Tmodel = this.Transform * Tmodel;    // Combine Tmodel with local transform, so Tmodel now is relative to the parrent mesh.

            //Can only render if all properties are set.
            if (!(this.Meshes == null || this.Shader == null))
                foreach (Mesh mesh in Meshes)
                    mesh.Render(this.Shader, TprojectView, Tmodel, camPos, allLights, ambient);      // Render each mesh

            foreach (SceneGraphNode childNode in childNodes)
                childNode.RenderAll(TprojectView, Tmodel, camPos, allLights, ambient);               // Render each childNode
        }

        // Add a ChildNode to this Node.
        public void AddChildNode(SceneGraphNode childNode)
        {
            this.childNodes.Add(childNode);
        }

        // Get all lights in the sceneGraph
        int getAllLights(Light[] allLights, int i = 0)
        {
            if (i == allLights.Length)
                return i;

            // Add each light to the array
            if (this.Lights != null)
            {
                foreach (Light light in this.Lights)
                {
                    if (i == allLights.Length)
                        return i;
                    allLights[i] = light;
                    i++;
                }
            }

            // Recursivly add all lights to the list
            foreach (SceneGraphNode childNode in childNodes)
                i = childNode.getAllLights(allLights, i);

            return i;
        }

    }
}
