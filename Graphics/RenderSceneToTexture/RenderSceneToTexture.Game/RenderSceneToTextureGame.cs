using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace RenderSceneToTexture
{
    /// <summary>
    /// This sample demonstrates how to render a scene to a texture, and use that texture.
    /// 
    /// It renders a scene twice with different camera setup: 
    /// 1. Render from behind a knight 
    /// 2. Render from front of a knight and render the texture from the first step on the top right corner of the screen.
    /// </summary>
    public class RenderSceneToTextureGame : Game
    {
        private static readonly Vector2 ScreenSize = new Vector2(640, 1136);

        private static readonly float TextureDestinationWidth = ScreenSize.X * 0.3f;
        private static readonly float TextureDestinationHeight = ScreenSize.Y * 0.3f;

        private readonly RectangleF textureDestination = new RectangleF(ScreenSize.X - TextureDestinationWidth, 0, TextureDestinationWidth, TextureDestinationHeight);

        private Texture targetRenderTexture;
        private SpriteBatch spriteBatch;

        public RenderSceneToTextureGame()
        {
            // Target 9.1 profile by default
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
            GraphicsDeviceManager.PreferredBackBufferWidth = (int)ScreenSize.X;
            GraphicsDeviceManager.PreferredBackBufferHeight = (int)ScreenSize.Y;
        }

        /// <summary>
        /// Initializes the pipeline, the knight and creates two cameras
        /// </summary>
        /// <returns></returns>
        protected override async Task LoadContent()
        {
            await base.LoadContent();

            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch.VirtualResolution = VirtualResolution;

            var knightEntity = Asset.Load<Entity>("knight");
            knightEntity.Get(AnimationComponent.Key).Play("Idle");

            var firstCamera = CreateCamera(new Vector3(0, 200, 130), new Vector3(0, 0, 120), TextureDestinationWidth / TextureDestinationHeight);
            var secondCamera = CreateCamera(new Vector3(0, -200, 130), new Vector3(0, 0, 120), (float)GraphicsDevice.BackBuffer.Width / GraphicsDevice.BackBuffer.Height);

            CreatePipeline(firstCamera.Get<CameraComponent>(), secondCamera.Get<CameraComponent>());

            // Add entities to the scene
            Entities.Add(knightEntity);

            Entities.Add(firstCamera);
            Entities.Add(firstCamera.Get<CameraComponent>().Target);

            Entities.Add(secondCamera);
            Entities.Add(secondCamera.Get<CameraComponent>().Target);
        }

        /// <summary>
        /// Creates a camera entity given its position and its target's position
        /// </summary>
        /// <param name="cameraPosition"></param>
        /// <param name="targetPosition"></param>
        /// <param name="aspectRatio"></param>
        /// <returns></returns>
        private static Entity CreateCamera(Vector3 cameraPosition, Vector3 targetPosition, float aspectRatio)
        {
            // Create and set the camera
            var cameraTargetEntity = new Entity
            {
                Name = "CameraTarget",
                Transformation = { Translation = targetPosition }
            };

            var cameraEntity = new Entity("Camera")
            {
                new TransformationComponent {Translation = cameraPosition},
                new CameraComponent
                {
                    NearPlane = 1,
                    FarPlane = 10000,
                    TargetUp = Vector3.UnitZ,
                    Target = cameraTargetEntity,
                    AspectRatio = aspectRatio,
                }
            };

            return cameraEntity;
        }

        /// <summary>
        /// Setup the default rendering pipeline
        /// </summary>
        private void CreatePipeline(CameraComponent firstCamera, CameraComponent secondCamera)
        {
            targetRenderTexture = Texture.New2D(GraphicsDevice, (int) ScreenSize.X, (int)ScreenSize.Y, PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget);

            // 1st pass
            RenderSystem.Pipeline.Renderers.Add(new CameraSetter(Services) { Camera = firstCamera });
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services) { RenderTarget = targetRenderTexture });
            RenderSystem.Pipeline.Renderers.Add(new BackgroundRenderer(Services, "ParadoxBackground"));
            RenderSystem.Pipeline.Renderers.Add(new ModelRenderer(Services, "RenderSceneToTextureEffectMain"));

            // 2nd pass
            RenderSystem.Pipeline.Renderers.Add(new CameraSetter(Services) { Camera = secondCamera });
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new BackgroundRenderer(Services, "ParadoxBackground"));
            RenderSystem.Pipeline.Renderers.Add(new ModelRenderer(Services, "RenderSceneToTextureEffectMain"));
            RenderSystem.Pipeline.Renderers.Add(new DelegateRenderer(Services) { Render = RenderTextureToScene });
        }

        /// <summary>
        /// Renders a texture to the scene with SpriteBatch
        /// </summary>
        /// <param name="renderContext"></param>
        private void RenderTextureToScene(RenderContext renderContext)
        {
            spriteBatch.Begin();

            spriteBatch.Draw(targetRenderTexture, textureDestination, Color.White);

            spriteBatch.End();
        }
    }
}
