using OpenTK;
using System;

namespace Rasterizer
{
    public class Light
    {
        public Vector3 position;
        public Vector3 color;
        public Vector3 specularColor;
        public float strength;          // How stronger the light, the slower it fades out on a larger distance.

        public Light(Vector3 position, Vector3 color, Vector3 specularColor, float strength)
        {
            this.position = position;
            this.color = color;
            this.specularColor = specularColor;
            this.strength = strength;
        }
    }

    public class Spotlight : Light
    {
        public Vector3 dir; // the direction of the spotlight
        public float angle; // the cos of the angle of the spotlights cone
        public Spotlight(Vector3 position, Vector3 color, Vector3 specularColor, float strength, Vector3 dir, float angle) : base(position, color, specularColor, strength)
        {
            this.position = position;
            this.color = color;
            this.specularColor = specularColor;
            this.strength = strength;
            this.dir = dir;
            this.angle = (float)Math.Cos(angle);
        }
    }
}
