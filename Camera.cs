using System;
using OpenTK;

namespace Rasterizer
{
    class Camera
    {
        public Matrix4 Tcamera { get; private set; }     // The camera 'view' transformation
        public float Fov { get; }                        // Angle of the field of view in the y direction (in radians)
        public float Aspect { get; set; }                // Aspect ratio of the view (width / height)
        public Vector3 Pos { get; private set; }         // Position of camera
        public Vector3 dir, up;                                 // Up and dir direction.

        public Camera(float fov, float aspect, Vector3 position, Vector3 direction, Vector3 up)
        {
            this.Fov = fov;
            this.Aspect = aspect;

            Pos = position;
            dir = direction;
            this.up = up;
            MoveCamera();
        }

        //Move camera relative to the current direction
        public void MoveCamera(float x = 0, float y = 0, float z = 0, float pitch = 0, float yaw = 0, float roll = 0)
        {
            Vector3 left = Vector3.Cross(dir, up);   //Vector facing to the left of screen plane;
            Pos += left * -x + up * y + dir * z;
            dir = Vector3.Normalize(RodriguesRotation(dir, left, pitch));
            up = Vector3.Normalize(RodriguesRotation(up, left, pitch));

            up = Vector3.Normalize(RodriguesRotation(up, dir, -roll));
            dir = Vector3.Normalize(RodriguesRotation(dir, up, yaw));
            Tcamera = Matrix4.LookAt(Pos, Pos + dir, up);
        }

        //return rotated v around k with angle a
        public static Vector3 RodriguesRotation(Vector3 v, Vector3 k, float a)
        {
            //Source: https://en.wikipedia.org/wiki/Rodrigues%27_rotation_formula
            return v * (float)Math.Cos(a) + Vector3.Cross(k, v) * (float)Math.Sin(a) + k * Vector3.Dot(k, v) * (1 - (float)Math.Cos(a));

        }
    }
}
