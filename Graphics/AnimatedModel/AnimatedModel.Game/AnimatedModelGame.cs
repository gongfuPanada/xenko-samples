using System;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace AnimatedModel
{
    /// <summary>
    /// AnimatedModel shows how to easily play the animation for a model.
    /// In this sample, a knight is displayed which can be rotated by drag gesture input from a user.
    /// 
    /// To rotate the knight, simply swipe the screen to the left or right.
    /// To change animation, press the button corresponding to each animation: Idle and Run.
    /// </summary>
    public class AnimatedModelGame : Game
    {
        private static readonly Vector3 GameVirtualResolution = new Vector3(640, 1136, 1f);

        private Entity knightEntity;
        private TransformationComponent knightTransformation;
        private float rotationFactor;

        public AnimatedModelGame()
        {
            // Target 9.1 profile by default
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
            GraphicsDeviceManager.PreferredBackBufferWidth = (int)GameVirtualResolution.X;
            GraphicsDeviceManager.PreferredBackBufferHeight = (int)GameVirtualResolution.Y;
        }

        /// <summary>
        /// Load and setup Knight, Camera, background, and UI
        /// </summary>
        /// <returns></returns>
        protected override async Task LoadContent()
        {
            await base.LoadContent();

            CreateRenderingPipeline();

            CreateUI();

            // Load the model
            knightEntity = Asset.Load<Entity>("knight");

            // Cache the transformation for future manipulation
            knightTransformation = knightEntity.Transformation;

            // Set the default animation
            knightEntity.Get<AnimationComponent>().Play("idle");
            
            // Create and set the camera
            var cameraTargetEntity = new Entity
            {
                Name = "CameraTarget",
                Transformation = {Translation = new Vector3(0, 0, 120)}
            };
            var cameraEntity = new Entity("Camera")
            {
                new TransformationComponent() {Translation = new Vector3(0, -200, 130)},
                new CameraComponent
                {
                    NearPlane = 1,
                    FarPlane = 10000,
                    TargetUp = Vector3.UnitZ,
                    Target = cameraTargetEntity,
                    AspectRatio = (float) GraphicsDevice.BackBuffer.Width/GraphicsDevice.BackBuffer.Height,
                }
            };

            // Set Camera component for the render pipeline
            RenderSystem.Pipeline.SetCamera(cameraEntity.Get<CameraComponent>());

            // Add entities to the scene
            Entities.Add(cameraEntity);
            Entities.Add(cameraTargetEntity);
            Entities.Add(knightEntity);

            // Set the lights
            GraphicsDevice.Parameters.Set(LightMultiDirectionalShadingPerPixelKeys.LightDirectionsVS, new Vector3[] { new Vector3(1, 0, -1), new Vector3(-1, 0, 0) });
            GraphicsDevice.Parameters.Set(LightMultiDirectionalShadingPerPixelKeys.LightColorsWithGamma, new Color3[] { new Color3(1, 1, 1), new Color3(1, 1, 1) });
            GraphicsDevice.Parameters.Set(LightMultiDirectionalShadingPerPixelKeys.LightIntensities, new float[] { 0.8f, 0.4f });
            
            // Display the mouse on window
            IsMouseVisible = true;

            // Add a task to the task scheduler that will be executed asynchronously 
            Script.Add(UpdateInput);
        }

        /// <summary>
        /// Create UI which contains two buttons for playing animations contained in StackPanel (Top-down placement) 
        /// </summary>
        private void CreateUI()
        {
            // Setting the virtual resolution for UI
            VirtualResolution = GameVirtualResolution;

            var arial16 = Asset.Load<SpriteFont>("Font");

            // Setup UI
            var idleAnimationButton = new Button { Content = new TextBlock { Text = "Play Idle", Font = arial16, }, Padding = new Thickness(10,10,10,10), Margin = new Thickness(0,0,0,20)};
            idleAnimationButton.Click += (s, e) => PlayAnimation("idle");

            var runAnimationButton = new Button { Content = new TextBlock { Text = "Play Run", Font = arial16, }, Padding = new Thickness(10,10,10,10)};
            runAnimationButton.Click += (s, e) => PlayAnimation("run");

            var layoutCanvas = new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(10,20,0,0)};
            layoutCanvas.Children.Add(idleAnimationButton);
            layoutCanvas.Children.Add(runAnimationButton);

            UI.RootElement = layoutCanvas;
        }

        /// <summary>
        /// Setup the default rendering pipeline for Camera, Background, Model and UI
        /// </summary>
        private void CreateRenderingPipeline()
        {
            RenderSystem.Pipeline.Renderers.Add(new CameraSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new BackgroundRenderer(Services, "ParadoxBackground"));
            RenderSystem.Pipeline.Renderers.Add(new ModelRenderer(Services, "AnimatedModelEffectMain"));
            RenderSystem.Pipeline.Renderers.Add(new UIRenderer(Services));
        }

        /// <summary>
        /// Update the knight (Rotating) for the drag gesture input from a user
        /// </summary>
        /// <returns>Task.</returns>
        private async Task UpdateInput()
        {
            var dragValue = 0f;

            // Start infinite loop that will quit when the engine shutdown
            while (IsRunning)
            {
                // Wait for the nextFrame.
                await Script.NextFrame();

                // Determine drag gesture input to manipulate knight's transformation 
                dragValue = 0.95f * dragValue;
                if (Input.PointerEvents.Count > 0)
                {
                    dragValue = Input.PointerEvents.Sum(x => x.DeltaPosition.X);
                }
                rotationFactor += dragValue;
                knightTransformation.Rotation = Quaternion.RotationZ((float)(2 * Math.PI * rotationFactor));
            }
        }

        /// <summary>
        /// Play animation for the knight by a given animation name, in crossfade mode with 0.2s
        /// </summary>
        /// <param name="animationName"></param>
        private void PlayAnimation(string animationName)
        {
            knightEntity.Get<AnimationComponent>().Crossfade(animationName, TimeSpan.FromSeconds(0.2));
        }
    }
}
