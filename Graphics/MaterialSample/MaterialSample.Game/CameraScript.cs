using System;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Input;

namespace MaterialSample
{
    public class CameraScript : AsyncScript
    {
        private readonly Vector3 upVector = new Vector3(0, 1, 0);
        private readonly Vector3 forwardVector = new Vector3(0, 0, -1);

        private float yaw = (float)(Math.PI * 0.25f);
        private float pitch = -(float)(Math.PI * 0.25f);
        private Vector3 position = new Vector3(10, 10, 10);

        private float moveSpeed;
        private float rotationSpeed;

        private CameraComponent CameraComponent { get { return Entity.Get<CameraComponent>(); } }

        /// <summary>
        /// Create a new instance of scene camera.
        /// </summary>
        public CameraScript()
        {
            moveSpeed = 2.5f;
            rotationSpeed = MathUtil.Pi / 2f;
        }

        public override async Task Execute()
        {
            Game.Window.ClientSizeChanged += OnWindowSizeChanged;

            CameraComponent.UseCustomViewMatrix = true;
            OnWindowSizeChanged(this, EventArgs.Empty);

            while (!IsDisposed)
            {
                // Capture/release mouse when the button is pressed/released
                if (Input.IsMouseButtonPressed(MouseButton.Right))
                {
                    Input.LockMousePosition();
                }
                else if (Input.IsMouseButtonReleased(MouseButton.Right))
                {
                    Input.UnlockMousePosition();
                }

                // Update rotation according to mouse deltas
                if (Input.IsMouseButtonDown(MouseButton.Right))
                {
                    yaw -= Input.MouseDelta.X * rotationSpeed;
                    pitch -= Input.MouseDelta.Y * rotationSpeed;
                }

                // Compute translation speed according to framerate and modifiers
                float translationSpeed = moveSpeed * (float)Game.UpdateTime.Elapsed.TotalSeconds;
                if (Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.RightShift))
                    translationSpeed *= 10;

                // Compute base vectors for camera movement
                var rotation = Quaternion.RotationYawPitchRoll(yaw, pitch, 0);

                var forward = Vector3.Transform(forwardVector, rotation);
                var up = Vector3.Transform(upVector, rotation);
                var right = Vector3.Cross(forward, up);

                // Update position according to movement input
                if (!IsModifierDown(false))
                {
                    if (Input.IsKeyDown(Keys.A))
                        position += -right * translationSpeed;
                    if (Input.IsKeyDown(Keys.D))
                        position += right * translationSpeed;
                    if (Input.IsKeyDown(Keys.S))
                        position += -forward * translationSpeed;
                    if (Input.IsKeyDown(Keys.W))
                        position += forward * translationSpeed;
                    if (Input.IsKeyDown(Keys.E))
                        position += -up * translationSpeed;
                    if (Input.IsKeyDown(Keys.Q))
                        position += up * translationSpeed;
                }

                // Update the camera view matrix 
                UpdateViewMatrix();

                await Script.NextFrame();
            }
        }

        private bool IsModifierDown(bool includeShift)
        {
            return (includeShift && (Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.RightShift)))
                   || Input.IsKeyDown(Keys.LeftCtrl) || Input.IsKeyDown(Keys.RightCtrl)
                   || Input.IsKeyDown(Keys.LeftAlt) || Input.IsKeyDown(Keys.RightAlt);
        }

        private void UpdateViewMatrix()
        {
            var rotation = Quaternion.Invert(Quaternion.RotationYawPitchRoll(yaw, pitch, 0));
            var viewMatrix = Matrix.Translation(-position) * Matrix.RotationQuaternion(rotation);
            CameraComponent.ViewMatrix = viewMatrix;
        }

        /// <summary>
        /// Called when the size of the windows changed.
        /// </summary>
        private void OnWindowSizeChanged(object sender, EventArgs eventArgs)
        {
            CameraComponent.AspectRatio = GraphicsDevice.BackBuffer.Width / (float)GraphicsDevice.BackBuffer.Height;
        }
    }
}