using System;
using OpenTK;

namespace MiguModelViewer.Renderer
{
    public class Camera
    {
        public static CameraMode Mode = CameraMode.FollowTarget;

        // Those vectors are directions pointing outwards from the camera to define how it rotated.
        private static Vector3 _front = -Vector3.UnitZ;

        private static Vector3 _up = Vector3.UnitY;

        private static Vector3 _right = Vector3.UnitX;

        // Rotation around the X axis (radians)
        private static float _pitch;

        // Rotation around the Y axis (radians)
        private static float _yaw = -MathHelper.PiOver2; // Without this, you would be started rotated 90 degrees right.

        // The field of view of the camera (radians)
        private static float _fov = MathHelper.PiOver2;

        // The position of the camera
        public static Vector3 Position = new Vector3(0.0f, 0.0f, 5.0f);

        // The target of the camera (for CameraMode.LockAtTarget)
        public static Vector3 Target = new Vector3(0.0f, 0.0f, 0.0f);

        // This is simply the aspect ratio of the viewport, used for the projection matrix.
        public static float AspectRatio = 960.0f / 540.0f;

        public static Vector3 Front => _front;

        public static Vector3 Up => _up;

        public static Vector3 Right => _right;

        public static readonly float Sensitivity = 0.0005f;

        // We convert from degrees to radians as soon as the property is set to improve performance.
        public static float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                // We clamp the pitch value between -89 and 89 to prevent the camera from going upside down, and a bunch
                // of weird "bugs" when you are using euler angles for rotation.
                // If you want to read more about this you can try researching a topic called gimbal lock
                var angle = MathHelper.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        // We convert from degrees to radians as soon as the property is set to improve performance.
        public static float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        // The field of view (FOV) is the vertical angle of the camera view.
        // This has been discussed more in depth in a previous tutorial,
        // but in this tutorial, you have also learned how we can use this to simulate a zoom feature.
        // We convert from degrees to radians as soon as the property is set to improve performance.
        public static float FieldOfViewAngle
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 45f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        // Get the view matrix using the amazing LookAt function described more in depth on the web tutorials
        public static Matrix4 GetViewMatrix()
        {
            switch(Mode)
            {
                case CameraMode.FollowTarget:
                    return Matrix4.LookAt(Position, Position + _front, _up);

                case CameraMode.LockAtTarget:
                    return Matrix4.LookAt(Position, Target, _up);
            }

            return Matrix4.Identity;
        }

        // Get the projection matrix using the same method we have used up until this point
        public static Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.2f, 100f);
        }

        // This function is going to update the direction vertices using some of the math learned in the web tutorials.
        private static void UpdateVectors()
        {
            // First, the front matrix is calculated using some basic trigonometry.
            _front.X = (float)(Math.Cos(_pitch) * Math.Cos(_yaw));
            _front.Y = (float)Math.Sin(_pitch);
            _front.Z = (float)(Math.Cos(_pitch) * Math.Sin(_yaw));

            // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
            _front = Vector3.Normalize(_front);

            // Calculate both the right and the up vector using cross product.
            // Note that we are calculating the right from the global up; this behaviour might
            // not be what you need for all cameras so keep this in mind if you do not want a FPS camera.
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }

        public static void UpdateAspect(int width, int height)
        {
            AspectRatio = (float)width / (float)height;
        }
    }

    public enum CameraMode
    {
        FollowTarget,
        LockAtTarget
    }
}

/*using System;
using OpenTK;

namespace MiguModelViewer.Renderer
{
    public class Camera
    {
        public static CameraMode Mode = CameraMode.FollowTarget;

        public static Vector3 Position = new Vector3(0.0f, 0.0f, 5.0f);
        public static Vector3 Target = new Vector3(0.0f, 0.0f, 0.0f);

        public static float FieldOfViewAngle
        {
            get => FieldOfViewAngle;
            set
            {
                FieldOfViewAngle = MathHelper.DegreesToRadians(MathHelper.Clamp(value, 0.0f, 90.0f));
            }
        }

        public static float AspectRatio = 960.0f / 540.0f;

        public static Vector3 Front = -Vector3.UnitZ;
        private static Vector3 Up = Vector3.UnitY;

        public static float Yaw = -MathHelper.PiOver2;
        public static float Pitch = 0.0f;

        public static readonly float Sensitivity = 0.0005f;

        public static Matrix4 GetViewMatrix()
        {
            // ----------------------------------------------------------------------------
            //Front.X = (float)Math.Cos(MathHelper.DegreesToRadians(Pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(Yaw));
            Front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(Pitch));
            //Front.Z = (float)Math.Cos(MathHelper.DegreesToRadians(Pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(Yaw));
            //Front = Vector3.Normalize(Front);
            //

            Pitch = MathHelper.Clamp(Pitch, -89.0f, 89.0f);

            if (Mode == CameraMode.FollowTarget)
                return Matrix4.LookAt(Position, Position + Front, Up);
            else if (Mode == CameraMode.LockAtTarget)
                return Matrix4.LookAt(Position, Target, Up);

            return Matrix4.Identity;
        }

        public static Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FieldOfViewAngle), AspectRatio, 0.01f, 100f);
        }
    }

    public enum CameraMode
    {
        FollowTarget,
        LockAtTarget
    }
}
*/