using System;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Modules;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Extensions;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Events;
using SiliconStudio.Paradox.UI.Panels;

namespace ForwardLighting
{
    public class ForwardLightingGame : Game
    {
        private bool allowShadows;

        private Entity characterEntity;

        private LightComponent[] directionalLight;
        private LightComponent spotLight;
        private LightComponent pointLight;
        private CameraComponent camera;
        private Button buttonShadow0;
        private Button buttonShadow1;
        private Button buttonSpotShadow;
        private float rotationFactor;
        private float lastDrag;

        private readonly Vector3 cameraInitPos = new Vector3(750, 0, 60);
        private readonly Vector3 characterInitPos = new Vector3(0, 0, 20);

        public ForwardLightingGame()
        {
            allowShadows = false;
            var graphicsProfile = GraphicsProfile.Level_9_1;

            // change profile based on platform
            switch (Platform.Type)
            {
                case PlatformType.Windows:
                    allowShadows = true;
                    graphicsProfile = GraphicsProfile.Level_11_0;
                    break;
                case PlatformType.Shared:
                case PlatformType.WindowsPhone:
                case PlatformType.WindowsStore:
                case PlatformType.Android:
                case PlatformType.iOS:
                    allowShadows = false;
                    graphicsProfile = GraphicsProfile.Level_9_1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GraphicsDeviceManager.PreferredDepthStencilFormat = PixelFormat.D24_UNorm_S8_UInt;
            GraphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.None;
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { graphicsProfile }; // should be the same as the one in the pdxpkg...
        }

        protected override async Task LoadContent()
        {
            CreatePipeline();

            await base.LoadContent();

            IsMouseVisible = true;

            // load the model
            characterEntity = Asset.Load<Entity>("character_00");
            characterEntity.Transformation.Rotation = Quaternion.RotationAxis(Vector3.UnitX, (float) (0.5*Math.PI));
            characterEntity.Transformation.Translation = characterInitPos;
            // remove self shadowing
            foreach (var subMesh in characterEntity.Get<ModelComponent>().Model.Meshes)
                subMesh.Parameters.Set(LightingKeys.ReceiveShadows, false);
            Entities.Add(characterEntity);

            // create the stand
            var lighting = Asset.Load<LightingConfigurationsSet>("Effects/LightingParams");
            var material = Asset.Load<Material>("character_00_material_mc00_1");
            var standEntity = CreateStand(material, lighting);
            standEntity.Transformation.Translation = new Vector3(0, 0, -80);
            standEntity.Transformation.Rotation = Quaternion.RotationAxis(Vector3.UnitX, (float)(0.5*Math.PI));
            Entities.Add(standEntity);

            var standBorderEntity = CreateStandBorder(material, lighting);
            standBorderEntity.Transformation.Translation = new Vector3(0, 0, -80);
            standBorderEntity.Transformation.Rotation = Quaternion.RotationAxis(Vector3.UnitX, (float)(0.5 * Math.PI));
            Entities.Add(standBorderEntity);

            // set the lights
            var directLight0 = CreateDirectLight(new Vector3(-1, 1, -1), new Color3(1, 1, 1), 0.2f, ShadowMapFilterType.Variance);
            Entities.Add(directLight0);

            var directLight1 = CreateDirectLight(new Vector3(-1, -1, -1), new Color3(1, 1, 1), 0.5f, ShadowMapFilterType.Variance);
            Entities.Add(directLight1);

            directionalLight = new LightComponent[2];
            directionalLight[0] = directLight0.Get<LightComponent>();
            directionalLight[1] = directLight1.Get<LightComponent>();

            var spotLightEntity = CreateSpotLight(new Vector3(0, -500, 600), new Vector3(0, -200, 0), 15, 20, new Color3(1, 1, 1), 0.35f);
            Entities.Add(spotLightEntity);
            spotLight = spotLightEntity.Get<LightComponent>();

            var pointLightEntity = CreatePointLight(new Vector3(100, 100, 0), new Color3(1, 1, 1));
            Entities.Add(pointLightEntity);
            pointLight = pointLightEntity.Get<LightComponent>();

            // set the camera
            var targetEntity = new Entity(characterInitPos);
            var cameraEntity = CreateCamera(cameraInitPos, targetEntity, (float)GraphicsDevice.BackBuffer.Width / (float)GraphicsDevice.BackBuffer.Height);
            camera = cameraEntity.Get<CameraComponent>();
            Entities.Add(cameraEntity);
            RenderSystem.Pipeline.SetCamera(camera);

            // UI
            CreateUI();

            // Add a custom script
            Script.Add(GameScript1);
        }

        private void CreatePipeline()
        {
            RenderPipelineLightingFactory.CreateDefaultForward(this, "ForwardLightingEffectMain", Color.DarkBlue, allowShadows, true, "ParadoxBackground");
        }

        private void CreateUI()
        {
            VirtualResolution = new Vector3(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height, 1);

            var font = Asset.Load<SpriteFont>("Font");
            var canvas = new Canvas();
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Left,
                MinimumWidth = 160
            };

            var buttonLightDirect0 = CreateButton("direct0", GetButtonTextOnOff("Direct light 0: ", directionalLight[0].Enabled), font, Thickness.UniformRectangle(5));
            buttonLightDirect0.Click += ToggleLight;

            var buttonLightDirect1 = CreateButton("direct1", GetButtonTextOnOff("Direct light 1: ", directionalLight[1].Enabled), font, Thickness.UniformRectangle(5));
            buttonLightDirect1.Click += ToggleLight;

            var buttonLightPoint = CreateButton("point", GetButtonTextOnOff("Point light: ", pointLight.Enabled), font, Thickness.UniformRectangle(5));
            buttonLightPoint.Click += ToggleLight;

            var buttonLightSpot = CreateButton("spot", GetButtonTextOnOff("Spot light: ", spotLight.Enabled), font, Thickness.UniformRectangle(5));
            buttonLightSpot.Click += ToggleLight;

            if (allowShadows)
            {
                buttonShadow0 = CreateButton("direct0", GetButtonTextOnOff("Shadow 0: ", directionalLight[0].ShadowMap),
                    font, new Thickness(20, 5, 5, 5));
                buttonShadow0.Opacity = directionalLight[0].Enabled ? 1.0f : 0.3f;
                buttonShadow0.CanBeHitByUser = directionalLight[0].Enabled;
                buttonShadow0.Click += ToggleShadowMap;

                buttonShadow1 = CreateButton("direct1", GetButtonTextOnOff("Shadow 1: ", directionalLight[0].ShadowMap),
                    font, new Thickness(20, 5, 5, 5));
                buttonShadow1.Opacity = directionalLight[1].Enabled ? 1.0f : 0.3f;
                buttonShadow1.CanBeHitByUser = directionalLight[1].Enabled;
                buttonShadow1.Click += ToggleShadowMap;

                buttonSpotShadow = CreateButton("spot", GetButtonTextOnOff("Shadow: ", spotLight.ShadowMap),
                    font, new Thickness(20, 5, 5, 5));
                buttonSpotShadow.Opacity = spotLight.Enabled ? 1.0f : 0.3f;
                buttonSpotShadow.CanBeHitByUser = spotLight.Enabled;
                buttonSpotShadow.Click += ToggleShadowMap;
            }

            stackPanel.Children.Add(buttonLightDirect0);
            if (allowShadows)
                stackPanel.Children.Add(buttonShadow0);
            stackPanel.Children.Add(buttonLightDirect1);
            if (allowShadows) 
                stackPanel.Children.Add(buttonShadow1);
            stackPanel.Children.Add(buttonLightPoint);
            stackPanel.Children.Add(buttonLightSpot);
            if (allowShadows)
                stackPanel.Children.Add(buttonSpotShadow);
            canvas.Children.Add(stackPanel);
            UI.RootElement = canvas;
        }

        private void ToggleShadowMap(Object sender, RoutedEventArgs args)
        {
            var button = (Button)sender;
            if (button.Name == "direct0")
            {
                directionalLight[0].ShadowMap = !directionalLight[0].ShadowMap;
                ((TextBlock) button.Content).Text = GetButtonTextOnOff("Shadow 0: ", directionalLight[0].ShadowMap);
            }
            else if (button.Name == "direct1")
            {
                directionalLight[1].ShadowMap = !directionalLight[1].ShadowMap;
                ((TextBlock) button.Content).Text = GetButtonTextOnOff("Shadow 1: ", directionalLight[1].ShadowMap);
            }
            else if (button.Name == "spot")
            {
                spotLight.ShadowMap = !spotLight.ShadowMap;
                ((TextBlock)button.Content).Text = GetButtonTextOnOff("Shadow: ", spotLight.ShadowMap);
            }
        }

        private void ToggleLight(Object sender, RoutedEventArgs args)
        {
            var button = (Button) sender;
            if (button.Name == "direct0")
            {
                directionalLight[0].Enabled = !directionalLight[0].Enabled;
                ((TextBlock) button.Content).Text = GetButtonTextOnOff("Direct light 0: ", directionalLight[0].Enabled);
                if (buttonShadow0 != null)
                {
                    buttonShadow0.Opacity = directionalLight[0].Enabled ? 1.0f : 0.3f;
                    buttonShadow0.CanBeHitByUser = directionalLight[0].Enabled;
                }
            }
            else if (button.Name == "direct1")
            {
                directionalLight[1].Enabled = !directionalLight[1].Enabled;
                ((TextBlock) button.Content).Text = GetButtonTextOnOff("Direct light 1: ", directionalLight[1].Enabled);
                if (buttonShadow1 != null)
                {
                    buttonShadow1.Opacity = directionalLight[1].Enabled ? 1.0f : 0.3f;
                    buttonShadow1.CanBeHitByUser = directionalLight[1].Enabled;
                }
            }
            else if (button.Name == "spot")
            {
                spotLight.Enabled = !spotLight.Enabled;
                ((TextBlock) button.Content).Text = GetButtonTextOnOff("Spot light: ", spotLight.Enabled);
            }
            else if (button.Name == "point")
            {
                pointLight.Enabled = !pointLight.Enabled;
                ((TextBlock) button.Content).Text = GetButtonTextOnOff("Point light: ", pointLight.Enabled);
            }
        }

        private async Task GameScript1()
        {
            var dragValue = 0f;

            while (IsRunning)
            {
                // Wait next rendering frame
                await Script.NextFrame();

                // rotate character
                var characterAnimationPeriod = 2 * Math.PI * (UpdateTime.Total.TotalMilliseconds % 10000) / 10000;
                characterEntity.Transformation.Rotation = Quaternion.RotationAxis(Vector3.UnitX, (float)(0.5 * Math.PI));
                characterEntity.Transformation.Rotation *= Quaternion.RotationAxis(Vector3.UnitZ, (float)characterAnimationPeriod);

                characterEntity.Transformation.Translation = characterInitPos + new Vector3(0, 0, 10 * (float)Math.Sin(3 * characterAnimationPeriod));

                // rotate camera
                dragValue = 0.95f*dragValue;
                if (Input.PointerEvents.Count > 0)
                {
                    dragValue = Input.PointerEvents.Sum(x => x.DeltaPosition.X);
                }
                rotationFactor -= dragValue;
                camera.Position = Vector3.Transform(cameraInitPos, Quaternion.RotationZ((float)(2 * Math.PI * rotationFactor)));
            }
        }

        #region Helper functions

        private static string GetButtonTextOnOff(string baseString, bool enabled)
        {
            return baseString + (enabled ? "On" : "Off");
        }

        private static Button CreateButton(string name, string text, SpriteFont font, Thickness thickness)
        {
            return new Button
            {
                Name = name,
                Margin = thickness,
                Content = new TextBlock { Text = text, Font = font, TextAlignment = TextAlignment.Center },
            };
        }

        private Entity CreateStand(Material material, LightingConfigurationsSet lighting)
        {
            var mesh = new Mesh
            {
                Draw = GeometricPrimitive.Cylinder.New(GraphicsDevice, 10, 720, 64, 6).ToMeshDraw(),
                Material = material
            };
            mesh.Parameters.Set(LightingKeys.ReceiveShadows, true);
            mesh.Parameters.Set(LightingKeys.CastShadows, false);
            mesh.Parameters.Set(LightingKeys.LightingConfigurations, lighting);
            return new Entity()
            {
                new ModelComponent
                {
                    Model = new Model()
                    {
                        mesh
                    },
                    Parameters =
                    {
                        {TexturingKeys.Texture0, Asset.Load<Texture2D>("TrainingFloor")},
                        {TexturingKeys.Sampler0, GraphicsDevice.SamplerStates.AnisotropicWrap},
                        {MaterialKeys.SpecularColorValue, 0.1f*Color4.White}
                    }
                }
            };
        }

        private Entity CreateStandBorder(Material material, LightingConfigurationsSet lighting)
        {
            var mesh = new Mesh
            {
                Draw = GeometricPrimitive.Torus.New(GraphicsDevice, 720, 10, 64).ToMeshDraw(),
                Material = material
            };
            mesh.Parameters.Set(LightingKeys.ReceiveShadows, true);
            mesh.Parameters.Set(LightingKeys.CastShadows, false);
            mesh.Parameters.Set(LightingKeys.LightingConfigurations, lighting);
            return new Entity()
            {
                new ModelComponent
                {
                    Model = new Model()
                    {
                        mesh
                    },
                    Parameters =
                    {
                        {TexturingKeys.Texture0, Asset.Load<Texture2D>("red")},
                        {TexturingKeys.Sampler0, GraphicsDevice.SamplerStates.AnisotropicWrap},
                        {MaterialKeys.SpecularColorValue, 0.3f*Color4.White}
                    }
                }
            };
        }

        private static Entity CreateCamera(Vector3 position, Entity target, float aspectRatio)
        {
            return new Entity()
            {
                new CameraComponent
                {
                    AspectRatio = aspectRatio,
                    FarPlane = 7000,
                    NearPlane = 10,
                    Target = target,
                    TargetUp = Vector3.UnitZ,
                    VerticalFieldOfView = (float) Math.PI*0.2f
                },
                new TransformationComponent {Translation = position}
            };
        }

        private static Entity CreateSpotLight(Vector3 position, Vector3 target, float beamAngle, float fieldAngle, Color3 color, float intensity)
        {
            return new Entity()
            {
                new LightComponent
                {
                    Type = LightType.Spot,
                    Color = color,
                    Deferred = false,
                    Enabled = true,
                    Intensity = intensity,
                    DecayStart = 500,
                    Layers = RenderLayers.RenderLayerAll,
                    LightDirection = target - position,
                    SpotBeamAngle = beamAngle,
                    SpotFieldAngle = fieldAngle,
                    ShadowMap = false,
                    ShadowNearDistance = 1,
                    ShadowFarDistance = 1000,
                    ShadowMapCascadeCount = 1,
                    ShadowMapFilterType = ShadowMapFilterType.Variance,
                },
                new TransformationComponent {Translation = position}
            };
        }

        private static Entity CreatePointLight(Vector3 position, Color3 color)
        {
            return new Entity()
            {
                new LightComponent
                {
                    Type = LightType.Point,
                    Color = color,
                    Deferred = false,
                    Enabled = true,
                    Intensity = 1,
                    DecayStart = 300,
                    Layers = RenderLayers.RenderLayerAll,
                    ShadowMap = false
                },
                new TransformationComponent {Translation = position}
            };
        }

        private static Entity CreateDirectLight(Vector3 direction, Color3 color, float intensity, ShadowMapFilterType filterType)
        {
            return new Entity()
            {
                new LightComponent
                {
                    Type = LightType.Directional,
                    Color = color,
                    Deferred = false,
                    Enabled = true,
                    Intensity = intensity,
                    LightDirection = direction,
                    Layers = RenderLayers.RenderLayerAll,
                    ShadowMap = false,
                    ShadowFarDistance = 3000,
                    ShadowNearDistance = 10,
                    ShadowMapMaxSize = 1024,
                    ShadowMapMinSize = 512,
                    ShadowMapCascadeCount = 4,
                    ShadowMapFilterType = ShadowMapFilterType.Variance,
                    BleedingFactor = 0,
                    MinVariance = 0
                }
            };
        }

        #endregion
    }
}
