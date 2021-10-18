using OpenTK;

namespace Rasterizer
{
    public struct Material
    {
        public float Kd;        //Diffuse coeficcent. The diffuse will be taken from the texture, but multiplied by this coeficcent.
        public Vector3 Ks;      //specular color
        public Vector3 Ka;      //ambient color
        public int glosiness; //specular glosiness

        public Material(float Kd, Vector3 Ks, Vector3 Ka, int glosiness)
        {
            this.Kd = Kd;
            this.Ks = Ks;
            this.Ka = Ka;
            this.glosiness = glosiness;
        }
    }
}
