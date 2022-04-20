using System;
using OpenTK;

namespace MiguEngine
{
    public class Camera
    {
        // Those vectors are directions pointing outwards from the camera to define how it rotated.
        private Vector3 mFront = -Vector3.UnitZ;

        private Vector3 mUp = Vector3.UnitY;

        private Vector3 mRight = Vector3.UnitX;

        // Rotation around the X axis (radians)
        private float mPitch;

        // Rotation around the Y axis (radians)
        private float mYaw = -MathHelper.PiOver2; // Without this, you would be started rotated 90 degrees right.

        private float mRoll;

        // The field of view of the camera (radians)
        private float mFov = MathHelper.PiOver2;

        // The position of the camera
        public Vector3 Position;

        public Vector3 ViewPosition
        {
            get => Position + mFront;
        }

        // The target of the camera (for CameraMode.LockAtTarget)
        public Vector3 Target;

        // This is simply the aspect ratio of the viewport, used for the projection matrix.
        public float AspectRatio;

        public Vector3 Front => mFront;

        public Vector3 Up => mUp;

        public Vector3 Right => mRight;

        public Camera()
        {
            Position = new Vector3(0.0f, 0.0f, 5.0f);
            Target = Vector3.Zero;
            AspectRatio = 960.0f / 540.0f; // You should update this on your code
        }

        // We convert from degrees to radians as soon as the property is set to improve performance.
        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(mPitch);
            set
            {
                // We clamp the pitch value between -89 and 89 to prevent the camera from going upside down, and a bunch
                // of weird "bugs" when you are using euler angles for rotation.
                // If you want to read more about this you can try researching a topic called gimbal lock
                float angle = MathHelper.Clamp(value, -89f, 89f);
                mPitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        // We convert from degrees to radians as soon as the property is set to improve performance.
        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(mYaw);
            set
            {
                mYaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        public float Roll
        {
            get => MathHelper.RadiansToDegrees(mRoll);
            set
            {
                mRoll = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        // The field of view (FOV) is the vertical angle of the camera view.
        // This has been discussed more in depth in a previous tutorial,
        // but in this tutorial, you have also learned how we can use this to simulate a zoom feature.
        // We convert from degrees to radians as soon as the property is set to improve performance.
        public float FieldOfViewAngle
        {
            get => MathHelper.RadiansToDegrees(mFov);
            set
            {
                float angle = MathHelper.Clamp(value, 1.0f, 90.0f);
                mFov = MathHelper.DegreesToRadians(angle);
            }
        }

        // Get the view matrix using the amazing LookAt function described more in depth on the web tutorials
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + mFront, mUp);
        }

        // Get the projection matrix using the same method we have used up until this point
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(mFov, AspectRatio, 0.1f, 100f);
        }

        // This function is going to update the direction vertices using some of the math learned in the web tutorials.
        private void UpdateVectors()
        {
            // First, the front matrix is calculated using some basic trigonometry.
            mFront.X = (float)(Math.Cos(mPitch) * Math.Cos(mYaw));
            mFront.Y = (float)Math.Sin(mPitch);
            mFront.Z = (float)(Math.Cos(mPitch) * Math.Sin(mYaw));

            // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
            mFront = Vector3.Normalize(mFront);

            // Calculate both the right and the up vector using cross product.
            // Note that we are calculating the right from the global up; this behaviour might
            // not be what you need for all cameras so keep this in mind if you do not want a FPS camera.
            mRight = Vector3.Normalize(Vector3.Cross(mFront, Vector3.UnitY));
            mUp = Vector3.Normalize(Vector3.Cross(mRight, mFront));

            mUp = mUp * new Matrix3(Matrix4.CreateFromAxisAngle(mFront, mRoll));
        }

        public void UpdateAspectRatio(int width, int height)
        {
            AspectRatio = (float)width / (float)height;
        }
    }
}