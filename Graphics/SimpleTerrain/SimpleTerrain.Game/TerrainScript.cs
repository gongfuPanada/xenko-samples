using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Materials;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

using Buffer = SiliconStudio.Paradox.Graphics.Buffer;



namespace SimpleTerrain
{
    /// <summary>
    /// This script rotates around Oy the entity it is attached to.
    /// </summary>
    public class TerrainScript : StartupScript
    {
        public Entity TerrainEntity;        // Manipulate the entity components: Transformation component
        public Entity UIEntity;
        public Entity CameraEntity;
        public Entity DirectionalLight0;
        public Entity DirectionalLight1;
        public Entity PointLight;

        // Cache loaded asset for later use and dispose
        private readonly Dictionary<string, object> loadedAssets = new Dictionary<string, object>();

        // Terrain Parameters
        private const int MinTerrainSizePowerFactor = 2;
        private const int MaxTerrainSizePowerFactor = 9;

        private Mesh terrainMesh;           // Update a number of element to draw
        private Buffer terrainVertexBuffer; // Set Vertex Buffer on the fly
        private Buffer terrainIndexBuffer;  // Set Index Buffer on the fly

        // Fault formation algorithm Parameters
        private int terrainSizePowerFactor                      = MaxTerrainSizePowerFactor;
        private int iterationPowerFactor                        = 5;
        private float filterHeightBandStrength                  = 0.7f;
        private float terrainHeightScale                        = 200f;

        // Light Parameters
        private LightComponent[] directionalLight;

        // Camera Parameters
        private static readonly Vector3 CameraStartPosition     = new Vector3(0, 500, -100);
        private static readonly Vector3 TargetPosition          = new Vector3(0, 0, 20);
        //private Entity cameraEntity;
        private Vector3 cameraRotation                          = Vector3.Zero;
        private float zoomFactor                                = 0.3f;

        // UI Parameters
        private ModalElement loadingModal;
        private TextBlock loadingTextBlock;

        // Raster states (Wireframe / Normal) Parameters
        private bool renderWireFrame;
        private static RasterizerState wireFrameRasterizerState;
        private static RasterizerState defaultRasterizerState;



        // Layer Texture
        private static readonly ParameterKey<Texture>   DiffuseMap1 = ParameterKeys.New<Texture>( null, "Material.DiffuseMap.i1" );
        private static readonly ParameterKey<Texture>   DiffuseMap2 = ParameterKeys.New<Texture>( null, "Material.DiffuseMap.i2" );


        #region Fault formation properties
        private int TerrainSizePowerFactor
        {
            get { return terrainSizePowerFactor; }
            set
            {
                if (value < MinTerrainSizePowerFactor) value = MinTerrainSizePowerFactor;
                if (value > MaxTerrainSizePowerFactor) value = MaxTerrainSizePowerFactor;
                terrainSizePowerFactor = value;
            }
        }

        private int IterationPowerFactor
        {
            get { return iterationPowerFactor; }
            set
            {
                if (value < 0) value = 0;
                if (value > 7) value = 7;
                iterationPowerFactor = value;
            }
        }

        private float FilterHeightBandStrength
        {
            get { return filterHeightBandStrength; }
            set
            {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                filterHeightBandStrength = value;
            }
        }

        private float TerrainHeightScale
        {
            get { return terrainHeightScale; }
            set
            {
                if (value < 1) value = 1;
                terrainHeightScale = value;
            }
        }
        #endregion Fault formation terrain generator properties

        /// <summary>
        /// Creates and Initializes Pipeline, UI, Camera, and a terrain model
        /// </summary>
        /// <returns></returns>
        public override void Start()
        {
            //RenderPipelineLightingFactory.CreateDefaultForward(this, "SimpleTerrainEffectMain", Color.DarkBlue, false, true, "ParadoxBackground");

            CreateUI();

            CreateCamera();

            CreateLight();


            float   radToDeg = (float)(180/Math.PI);
            Matrix  camera_0= Matrix.LookAtLH( CameraStartPosition, TargetPosition, new Vector3(0, 1, 0) );
            Matrix  camera= camera_0;
            //camera.Invert();
            Vector3 outrot;
            Vector3 position;
            camera.DecomposeXYZ( out outrot );
            position= camera.TranslationVector;
            var DX= outrot.X * radToDeg;
            var DY= outrot.Y * radToDeg;
            var DZ= outrot.Z * radToDeg;

            var maxTerrainSize = (int)Math.Pow(2, MaxTerrainSizePowerFactor);
            var maxVerticesCount = maxTerrainSize * maxTerrainSize;
            var maxIndicesCount = 2 * maxTerrainSize * maxTerrainSize; // each index appear on average twice since the mesh is rendered as triangle strips
            CreateTerrainModelEntity(maxVerticesCount, maxIndicesCount);

            Script.AddTask(GenerateTerrain);

            defaultRasterizerState = GraphicsDevice.Parameters.Get(Effect.RasterizerStateKey);
            wireFrameRasterizerState = RasterizerState.New(GraphicsDevice, new RasterizerStateDescription(CullMode.None) { FillMode = FillMode.Wireframe });

            Script.AddTask(UpdateInput);
        }

        /// <summary>
        /// Creates UI showing parameters of Fault formation algorithm
        /// </summary>
        private void CreateUI()
        {
            //var arial = LoadAsset<SpriteFont>("Arial");
            var arial = LoadAsset<SpriteFont>("Font");

            var virtualResolution = new Vector3(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height, 1);

            loadingModal = new ModalElement { Visibility = Visibility.Collapsed };

            loadingTextBlock = new TextBlock { Font = arial, Text = "Loading a model...", Visibility = Visibility.Collapsed, TextColor = Color.White, TextSize = 22 };

            loadingTextBlock.SetCanvasPinOrigin(new Vector3(0.5f, 0.5f, 1f));
            loadingTextBlock.SetCanvasRelativePosition(new Vector3(0.5f, 0.5f, 0));

            // Parameters Grid
            var parametersGrid = new Grid();
            parametersGrid.LayerDefinitions.Add(new StripDefinition(StripType.Auto));
            parametersGrid.RowDefinitions.Add(new StripDefinition(StripType.Auto));
            parametersGrid.RowDefinitions.Add(new StripDefinition(StripType.Auto));
            parametersGrid.RowDefinitions.Add(new StripDefinition(StripType.Auto));
            parametersGrid.RowDefinitions.Add(new StripDefinition(StripType.Auto));
            parametersGrid.RowDefinitions.Add(new StripDefinition(StripType.Auto));
            parametersGrid.ColumnDefinitions.Add(new StripDefinition(StripType.Auto));
            parametersGrid.ColumnDefinitions.Add(new StripDefinition(StripType.Star));
            parametersGrid.ColumnDefinitions.Add(new StripDefinition(StripType.Fixed, 30));
            parametersGrid.ColumnDefinitions.Add(new StripDefinition(StripType.Fixed, 30));

            // Terrain Size
            var terrainSizeText = new TextBlock { Font = arial, Text = "" + (int)Math.Pow(2, terrainSizePowerFactor), TextAlignment = TextAlignment.Center, 
                VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, MinimumWidth = 60};
            terrainSizeText.SetGridRow(0);
            terrainSizeText.SetGridColumn(1);

            var terrainSizeIncButton = new Button { Content = new TextBlock { Text = "+", Font = arial, TextAlignment = TextAlignment.Center} };
            terrainSizeIncButton.SetGridRow(0);
            terrainSizeIncButton.SetGridColumn(3);
            
            var terrainSizeDecButton = new Button { Content = new TextBlock { Text = "-", Font = arial, TextAlignment = TextAlignment.Center} };
            terrainSizeDecButton.SetGridRow(0);
            terrainSizeDecButton.SetGridColumn(2);

            terrainSizeIncButton.Click += (s, e) =>
            {
                TerrainSizePowerFactor++;
                terrainSizeText.Text = "" + (int)Math.Pow(2, TerrainSizePowerFactor);
            };

            terrainSizeDecButton.Click += (s, e) =>
            {
                TerrainSizePowerFactor--;
                terrainSizeText.Text = "" + (int)Math.Pow(2, TerrainSizePowerFactor);
            };

            var terrainSizeDescription = new TextBlock
            {
                Font = arial, Text = "Terrain Size:", TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            terrainSizeDescription.SetGridRow(0);
            terrainSizeDescription.SetGridColumn(0);

            parametersGrid.Children.Add(terrainSizeDescription);
            parametersGrid.Children.Add(terrainSizeText);
            parametersGrid.Children.Add(terrainSizeDecButton);
            parametersGrid.Children.Add(terrainSizeIncButton);

            // Iteration
            var iterationText = new TextBlock { Font = arial, Text = "" + (int)Math.Pow(2, IterationPowerFactor), TextAlignment = TextAlignment.Center, 
                VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center};

            iterationText.SetGridRow(1);
            iterationText.SetGridColumn(1);

            var iterationIncButton = new Button { Content = new TextBlock { Text = "+", Font = arial, TextAlignment = TextAlignment.Center } };
            iterationIncButton.SetGridRow(1);
            iterationIncButton.SetGridColumn(3);

            var iterationDecButton = new Button { Content = new TextBlock { Text = "-", Font = arial, TextAlignment = TextAlignment.Center} };
            iterationDecButton.SetGridRow(1);
            iterationDecButton.SetGridColumn(2);

            iterationIncButton.Click += (s, e) =>
            {
                IterationPowerFactor++;
                iterationText.Text = "" + (int)Math.Pow(2, IterationPowerFactor);
            };

            iterationDecButton.Click += (s, e) =>
            {
                IterationPowerFactor--;
                iterationText.Text = "" + (int)Math.Pow(2, IterationPowerFactor);
            };

            var iterationDescription = new TextBlock
            {
                Font = arial,
                Text = "Iteration:",
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            iterationDescription.SetGridRow(1);
            iterationDescription.SetGridColumn(0);

            parametersGrid.Children.Add(iterationDescription);
            parametersGrid.Children.Add(iterationText);
            parametersGrid.Children.Add(iterationDecButton);
            parametersGrid.Children.Add(iterationIncButton);

            // Filter Intensity
            var filterIntensityText = new TextBlock { Font = arial, Text = "" + FilterHeightBandStrength, TextAlignment = TextAlignment.Center, 
                VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center};

            filterIntensityText.SetGridRow(2);
            filterIntensityText.SetGridColumn(1);

            var filterIncButton = new Button { Content = new TextBlock { Text = "+", Font = arial, TextAlignment = TextAlignment.Center } };
            filterIncButton.SetGridRow(2);
            filterIncButton.SetGridColumn(3);

            var filterDecButton = new Button { Content = new TextBlock { Text = "-", Font = arial, TextAlignment = TextAlignment.Center } };
            filterDecButton.SetGridRow(2);
            filterDecButton.SetGridColumn(2);

            filterIncButton.Click += (s, e) =>
            {
                FilterHeightBandStrength += 0.1f;
                filterIntensityText.Text = "" + FilterHeightBandStrength;
            };

            filterDecButton.Click += (s, e) =>
            {
                FilterHeightBandStrength -= 0.1f;
                filterIntensityText.Text = "" + FilterHeightBandStrength;
            };

            var filterIntensityDescription = new TextBlock
            {
                Font = arial,
                Text = "Filter Intensity:",
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            filterIntensityDescription.SetGridRow(2);
            filterIntensityDescription.SetGridColumn(0);

            parametersGrid.Children.Add(filterIntensityDescription);
            parametersGrid.Children.Add(filterIntensityText);
            parametersGrid.Children.Add(filterDecButton);
            parametersGrid.Children.Add(filterIncButton);

            // Height Scale
            var heightScaleText = new TextBlock { Font = arial, Text = "" + TerrainHeightScale, TextAlignment = TextAlignment.Center, 
                VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center};

            heightScaleText.SetGridRow(3);
            heightScaleText.SetGridColumn(1);

            var heightScaleIncButton = new Button { Content = new TextBlock { Text = "+", Font = arial, TextAlignment = TextAlignment.Center } };
            heightScaleIncButton.SetGridRow(3);
            heightScaleIncButton.SetGridColumn(3);

            var heightScaleDecButton = new Button { Content = new TextBlock { Text = "-", Font = arial, TextAlignment = TextAlignment.Center } };
            heightScaleDecButton.SetGridRow(3);
            heightScaleDecButton.SetGridColumn(2);

            heightScaleIncButton.Click += (s, e) =>
            {
                TerrainHeightScale++;
                heightScaleText.Text = "" + TerrainHeightScale;
            };

            heightScaleDecButton.Click += (s, e) =>
            {
                TerrainHeightScale--;
                heightScaleText.Text = "" + TerrainHeightScale;
            };

            var heightScaleDescription = new TextBlock
            {
                Font = arial,
                Text = "Height Scale:",
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            heightScaleDescription.SetGridRow(3);
            heightScaleDescription.SetGridColumn(0);

            parametersGrid.Children.Add(heightScaleDescription);
            parametersGrid.Children.Add(heightScaleText);
            parametersGrid.Children.Add(heightScaleDecButton);
            parametersGrid.Children.Add(heightScaleIncButton);

            // Zoom
            var zoomFactorIncButton = new Button { Content = new TextBlock { Text = "+", Font = arial, TextAlignment = TextAlignment.Center } };
            zoomFactorIncButton.SetGridRow(4);
            zoomFactorIncButton.SetGridColumn(3);

            var zoomFactorDecButton = new Button { Content = new TextBlock { Text = "-", Font = arial, TextAlignment = TextAlignment.Center } };
            zoomFactorDecButton.SetGridRow(4);
            zoomFactorDecButton.SetGridColumn(2);

            zoomFactorIncButton.Click += (s, e) =>
            {
                zoomFactor -= 0.05f;
                UpdateCamera();
            };

            zoomFactorDecButton.Click += (s, e) =>
            {
                zoomFactor += 0.05f;
                UpdateCamera();
            };

            var zoomDescription = new TextBlock
            {
                Font = arial,
                Text = "Zoom",
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            zoomDescription.SetGridRow(4);
            zoomDescription.SetGridColumn(0);

            parametersGrid.Children.Add(zoomDescription);
            parametersGrid.Children.Add(zoomFactorDecButton);
            parametersGrid.Children.Add(zoomFactorIncButton);

            // Wireframe toggle button
            var wireFrameToggleButton = new Button { Content = new TextBlock { Text = "Wire frame On", Font = arial }, HorizontalAlignment = HorizontalAlignment.Left };

            wireFrameToggleButton.Click += (s, e) =>
            {
/* DELETE
                RenderWireFrame = !RenderWireFrame;
                ((TextBlock)wireFrameToggleButton.Content).Text = (RenderWireFrame) ? "Wire frame Off" : "Wire frame On";
*/
            };

            // Light toggle button
            var lightToggleButton = new Button { Content = new TextBlock { Text = "Directional Light Off", Font = arial }, HorizontalAlignment = HorizontalAlignment.Left };

            lightToggleButton.Click += (s, e) =>
            {
                directionalLight[0].Enabled = !directionalLight[0].Enabled;
                directionalLight[1].Enabled = !directionalLight[1].Enabled;
                ((TextBlock)lightToggleButton.Content).Text = (directionalLight[0].Enabled) ? "Directional Light Off" : "Directional Light On";
            };

            // Re-create terrain
            var reCreateTerrainButton = new Button { Content = new TextBlock { Text = "Recreate terrain", Font = arial }, HorizontalAlignment = HorizontalAlignment.Left };

            reCreateTerrainButton.Click += async (s, e) => await GenerateTerrain();

            var descriptionCanvas = new StackPanel
            {
                Children =
                {
                    new TextBlock { Font = arial, Text = "Fault formation parameters", TextSize = 19}, 
                    parametersGrid,
                    wireFrameToggleButton,
                    lightToggleButton,
                    reCreateTerrainButton
                }
            };

            var activeButton = new Button { Content = new TextBlock{ Text = "Description Off", Font = arial}, 
                Padding = new Thickness(10,10,10,10), Margin = new Thickness(0,0,0,20), HorizontalAlignment = HorizontalAlignment.Left};

            var isDescriptionOn = true;

            activeButton.Click += (s, e) =>
                {
                    isDescriptionOn = !isDescriptionOn;
                    ((TextBlock)activeButton.Content).Text = isDescriptionOn ? "Description Off" : "Description On";
                    descriptionCanvas.Visibility = isDescriptionOn ? Visibility.Visible : Visibility.Collapsed;
                };

            var buttonDescription = new StackPanel { Orientation = Orientation.Vertical, Children = {activeButton, descriptionCanvas }};

            //UIComponent.RootElement = new Canvas { Children = { buttonDescription, loadingModal, loadingTextBlock } }; 
            UIEntity.Add<UIComponent>( UIComponent.Key, new UIComponent { RootElement = new Canvas { Children = { buttonDescription, loadingModal, loadingTextBlock } }, VirtualResolution = virtualResolution } ); 
        }

        private void UpdateCamera()
        {
            var rotationQuat = Quaternion.RotationX(cameraRotation.X) * Quaternion.RotationY(cameraRotation.Y) * Quaternion.RotationZ(cameraRotation.Z);
            CameraEntity.Transform.Position = Vector3.Transform(CameraStartPosition * zoomFactor, rotationQuat);
        }


        /// <summary>
        /// Creates a target camera, and add it to the pipeline
        /// </summary>
        private void CreateCamera()
        {
            /* DELETE
            var cameraComponent = new CameraComponent
            {
                NearPlane = 1f,
                FarPlane = 5000,
                TargetUp = Vector3.UnitZ,
                Target = new Entity("CameraTarget"),
                AspectRatio = (float)GraphicsDevice.BackBuffer.Width / GraphicsDevice.BackBuffer.Height,
            };

            // Create camera entity
            cameraEntity = new Entity("Camera") { cameraComponent };

            // Setup the camera for the rendering pipeline
            Game.RenderSystem.Pipeline.SetCamera(cameraComponent);

            cameraEntity.Transformation.Translation = CameraStartPosition;
            cameraComponent.Target.Transformation.Translation = TargetPosition;

            Game.Entities.Add(cameraEntity);
            Game.Entities.Add(cameraComponent.Target);

            */
            UpdateCamera();
        }

        /// <summary>
        /// Creates one point light source and two directional light sources which could be disabled with the UI
        /// </summary>
        private void CreateLight()
        {
            directionalLight = new LightComponent[2];
            directionalLight[0] = DirectionalLight0.Get<LightComponent>();
            directionalLight[0].Enabled = true;
            directionalLight[1] = DirectionalLight1.Get<LightComponent>();
            directionalLight[1].Enabled = true;

            var pointLight = PointLight.Get(LightComponent.Key);
            pointLight.Enabled = true;

            /*
            var PointLightEntity = CreatePointLight(Vector3.UnitY * 500f, new Color3(1, 1, 1), 0.2f);

            // set the lights
            var directLight0 = CreateDirectLight(new Vector3(-0.3f, -0.9f, 0), new Color3(1, 1, 1), 1.2f);
            Game.Entities.Add(directLight0);

            var directLight1 = CreateDirectLight(new Vector3(1f, -1, 0), new Color3(1, 1, 1), 0.3f);
            Game.Entities.Add(directLight1);

            directionalLight = new LightComponent[2];
            directionalLight[0] = directLight0.Get<LightComponent>();
            directionalLight[0].Enabled = true;

            directionalLight[1] = directLight1.Get<LightComponent>();
            directionalLight[1].Enabled = true;

            Game.Entities.Add(PointLightEntity);
            var pointLight = PointLightEntity.Get(LightComponent.Key);
            pointLight.Enabled = true;
            */
        }

        /// <summary>
        /// Creates an Entity that contains our dynamic Vertex and Index buffers.
        /// This Entity will be rendered by the model renderer.
        /// </summary>
        /// <param name="verticesCount"></param>
        /// <param name="indicesCount"></param>
        private void CreateTerrainModelEntity(int verticesCount, int indicesCount)
        {
            // Compute sizes
            var vertexDeclaration = VertexNormalTexture.VertexDeclaration;
            var vertexBufferSize = verticesCount * vertexDeclaration.CalculateSize();
            var indexBufferSize = indicesCount * sizeof(int);

            // Create Vertex and Index buffers
            terrainVertexBuffer = Buffer.Vertex.New(GraphicsDevice, vertexBufferSize, GraphicsResourceUsage.Dynamic);
            terrainIndexBuffer = Buffer.New(GraphicsDevice, indexBufferSize, BufferFlags.IndexBuffer, GraphicsResourceUsage.Dynamic);

            // Prepare mesh and entity
            var meshDraw = new MeshDraw
            {
                PrimitiveType = PrimitiveType.TriangleStrip,
                VertexBuffers = new[] { new VertexBufferBinding(terrainVertexBuffer, vertexDeclaration, verticesCount) },
                IndexBuffer = new IndexBufferBinding(terrainIndexBuffer, true, indicesCount),
            };

//            var effect1 = EffectSystem.LoadEffect("SimpleTerrainEffectMain").WaitForResult();
//            var effect2 = EffectSystem.LoadEffect("VertexTextureTerrain");
            var effectMaterial = LoadAsset<Material>("mt_rock");

            Texture tex0= (Texture)effectMaterial.Parameters[ MaterialKeys.DiffuseMap ]; // rock
            Texture tex1= (Texture)effectMaterial.Parameters[ DiffuseMap1 ];    // grass
            Texture tex2= (Texture)effectMaterial.Parameters[ DiffuseMap2 ];    // water

            effectMaterial.Parameters.Add( VertexTextureTerrainKeys.MeshTexture0, tex1 );
            effectMaterial.Parameters.Add( VertexTextureTerrainKeys.MeshTexture1, tex0 );
            effectMaterial.Parameters.Add( VertexTextureTerrainKeys.MeshTexture2, tex2 );

            effectMaterial.Parameters.Add( MaterialKeys.DiffuseMap, tex1 );

            //terrainMesh = new Mesh { Draw = meshDraw /*, Material = LoadAsset<Material>("TerrainMaterial")*/ };
            terrainMesh = new Mesh { Draw = meshDraw, MaterialIndex = 0 };

            //terrainEntity = new Entity { { ModelComponent.Key, new ModelComponent { Model = new Model { terrainMesh } } } };
            //TerrainEntity.Add<ModelComponent>( ModelComponent.Key, new ModelComponent { Model = new Model { terrainMesh } } );
            TerrainEntity.Add<ModelComponent>( ModelComponent.Key, new ModelComponent { Model = new Model { terrainMesh, effectMaterial  } } );
        }

        /// <summary>
        /// Updates touch input for controlling the camera by polling to check pointer events
        /// </summary>
        /// <returns></returns>
        public async Task UpdateInput()
        {
            var rotY = 0f;
            var rotX = (float)(Math.PI / 5f);

            while (Game.IsRunning)
            {
                await Script.NextFrame();

                if (Input.PointerEvents.Count > 0)
                {
                    var sumDelta = (2 * (float)Math.PI) * Input.PointerEvents.Aggregate(Vector2.Zero, (current, pointerEvent) => current + pointerEvent.DeltaPosition);
                    rotY += sumDelta.X;
                    rotX -= sumDelta.Y;
                }
                // Rotate the terrain
                rotY += (float)(2 * Math.PI * (Game.UpdateTime.Elapsed.TotalMilliseconds % 20000) / 20000);

                TerrainEntity.Transform.Rotation = Quaternion.RotationAxis(Vector3.UnitY, rotY) * Quaternion.RotationAxis(Vector3.UnitX, rotX);
            }
        }

        /// <summary>
        /// Generates new terrain and initializes it in vertex and index buffer asynchronously.
        /// Note that, this method does not block the main thread.
        /// </summary>
        /// <returns></returns>
        private async Task GenerateTerrain()
        {
            // Show loading modal and text
            loadingModal.Visibility = Visibility.Visible;
            loadingTextBlock.Visibility = Visibility.Visible;

//          Entity.Remove(terrainEntity);

            await Task.Run(() =>
            {
                var heightMap = HeightMapFactory.CreateDataWithFaultFormation((int)Math.Pow(2, terrainSizePowerFactor),
                    (int)Math.Pow(2, iterationPowerFactor), 0, 256, TerrainHeightScale, filterHeightBandStrength);

                InitializeBuffersFromTerrain(heightMap);

                //var height = heightMap[heightMap.DataSize/2];
                var height = heightMap.GetScaledHeight(heightMap.Size/2, heightMap.Size/2);
                TerrainEntity.Transform.Position.Y = -(height + 100);
            });

//          Entity.Add(terrainEntity);
            
            // Dismiss loading modal and text
            loadingModal.Visibility = Visibility.Collapsed;
            loadingTextBlock.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Initializes Vertex and Index buffers with a given height map
        /// </summary>
        /// <param name="heightMap"></param>
        private void InitializeBuffersFromTerrain(HeightMap heightMap)
        {
            // Set data in VertexBuffer
            var mappedSubResource = GraphicsDevice.MapSubresource(terrainVertexBuffer, 0, MapMode.WriteDiscard);
            SetVertexDataFromHeightMap(heightMap, mappedSubResource.DataBox.DataPointer);
            GraphicsDevice.UnmapSubresource(mappedSubResource);

            // Set data in IndexBuffer
            mappedSubResource = GraphicsDevice.MapSubresource(terrainIndexBuffer, 0, MapMode.WriteDiscard);
            var elementCount = SetIndexDataForTerrain(heightMap.Size, mappedSubResource.DataBox.DataPointer);
            GraphicsDevice.UnmapSubresource(mappedSubResource);

            terrainMesh.Draw.DrawCount = elementCount;
        }

        /// <summary>
        /// Initializes Index buffer data from the given size of terrain for a square Triangle Strip (Brute force) rendering
        /// </summary>
        /// <param name="size"></param>
        /// <param name="indexBuffer"></param>
        /// <returns></returns>
        private static unsafe int SetIndexDataForTerrain(int size, IntPtr indexBuffer)
        {
            var ib = (int*)indexBuffer;
            var currentIndex = 0;

            for (var iZ = 0; iZ < size - 1; ++iZ)
            {
                for (var iX = 0; iX < size; ++iX)
                {
                    ib[currentIndex++] = size * iZ + iX;
                    ib[currentIndex++] = size * (iZ + 1) + iX;
                }

                ib[currentIndex] = ib[currentIndex - 1];
                ++currentIndex;
                ib[currentIndex++] = size * (iZ + 1);
            }
            return currentIndex - 1;
        }

        /// <summary>
        /// Initializes Vertex buffer data by a given heightmap
        /// </summary>
        /// <param name="heightMap"></param>
        /// <param name="vertexBuffer"></param>
        private static unsafe void SetVertexDataFromHeightMap(HeightMap heightMap, IntPtr vertexBuffer)
        {
            var vb = (VertexNormalTexture*)vertexBuffer;

            var halfSize = heightMap.Size * 0.5f;

            for (var iZ = 0; iZ < heightMap.Size; ++iZ)
                for (var iX = 0; iX < heightMap.Size; ++iX)
                {
                    vb[iZ * heightMap.Size  + iX] = new VertexNormalTexture
                    {
                        Position = new Vector4(iX - halfSize, heightMap.GetScaledHeight(iX, iZ), -iZ + halfSize, 1),
                        Normal = GetNormalVector(heightMap, iX, iZ),
                        TextureCoordinate = new Vector2((float)iX / heightMap.Size, (float)iZ / heightMap.Size)
                    };
                }
        }

        /// <summary>
        /// Gets a normal vector for a given x, z coordinate and the corresponding heightmap
        /// </summary>
        /// <param name="heightMap"></param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private static Vector4 GetNormalVector(HeightMap heightMap, int x, int z)
        {
            var currentP = new Vector3(x, heightMap.GetScaledHeight(x, z), z);
            Vector3 p1;
            Vector3 p2;

            if (x == heightMap.Size - 1 && z == heightMap.Size - 1) // Bottom right pixel
            {
                p1 = new Vector3(x, heightMap.GetScaledHeight(x, z - 1), z - 1);
                p2 = new Vector3(x - 1, heightMap.GetScaledHeight(x - 1, z), z);
            }
            else if (x == heightMap.Size - 1) // Right border
            {
                p1 = new Vector3(x - 1, heightMap.GetScaledHeight(x - 1, z), z);
                p2 = new Vector3(x, heightMap.GetScaledHeight(x, z + 1), z + 1);
            }
            else if (z == heightMap.Size - 1) // Bottom border
            {
                p1 = new Vector3(x + 1, heightMap.GetScaledHeight(x + 1, z), z);
                p2 = new Vector3(x, heightMap.GetScaledHeight(x, z - 1), z - 1);
            }
            else // The rest of pixels
            {
                p1 = new Vector3(x, heightMap.GetScaledHeight(x, z + 1), z + 1);
                p2 = new Vector3(x + 1, heightMap.GetScaledHeight(x + 1, z), z);
            }
            return new Vector4(Vector3.Normalize(Vector3.Cross(p1 - currentP, p2 - currentP)), 1);
        }

        /// <summary>
        /// Creates a point light entity
        /// </summary>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <param name="intensity"></param>
        /// <returns></returns>
        /*
        private static Entity CreatePointLight(Vector3 position, Color3 color, float intensity)
        {
            return new Entity 
            { 
                new LightComponent 
                    {
                        Type = LightType.Point, Color = color,
                        Deferred = false,
                        Enabled = true,
                        Intensity = intensity,
                        DecayStart = 1000,
                        Layers = Game.RenderLayers.RenderLayerAll,
                        ShadowMap = false
                    }, 
                new TransformationComponent { Translation = position } 
            };
        }
        */

        /// <summary>
        /// Creates a directional light entity
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="color"></param>
        /// <param name="intensity"></param>
        /// <returns></returns>
        /*
        private static Entity CreateDirectLight(Vector3 direction, Color3 color, float intensity)
        {
            return new Entity
            {
                new LightComponent
                {
                    Type = LightType.Directional,
                    Color = color,
                    Deferred = false,
                    Enabled = true,
                    Intensity = intensity,
                    LightDirection = direction,
                    Layers = Game.RenderLayers.RenderLayerAll,
                    ShadowMap = false,
                    ShadowFarDistance = 3000,
                    ShadowNearDistance = 10,
                    ShadowMapMaxSize = 1024,
                    ShadowMapMinSize = 512,
                    ShadowMapCascadeCount = 4,
                    ShadowMapFilterType = Game.ShadowMapFilterType.Variance,
                    BleedingFactor = 0,
                    MinVariance = 0
                }
            };
        }
        */

        /// <summary>
        /// Loads an Assets with Asset manager and cache it in a dictionary for later use and/or dispose
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        private T LoadAsset<T>(string path) where T : class
        {
            if (loadedAssets.ContainsKey(path))
                return (T)loadedAssets[path];

            var asset = Asset.Load<T>(path);
            loadedAssets.Add(path, asset);

            return asset;
        }
    }

    /// <summary>
    /// Vertex attribute uses in VertexTextureTerrain shader
    /// </summary>
    struct VertexNormalTexture
    {
        /// <summary>
        /// Gets a declaration of Vertex attribute which consists of Position::Vector4, Normal::Vector4, TextureCoordinate::Vector2
        /// </summary>
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(VertexElement.Position<Vector4>(), 
            VertexElement.Normal<Vector4>(), VertexElement.TextureCoordinate<Vector2>());

        /// <summary>
        /// Gets or sets a Position of a vertex
        /// </summary>
        public Vector4 Position;

        /// <summary>
        /// Gets or sets a Normal vector of a vertex 
        /// </summary>
        public Vector4 Normal;

        /// <summary>
        /// Gets or sets a texture coordinate of a vertex
        /// </summary>
        public Vector2 TextureCoordinate;
    }

    public static class TerrainRegionKeys
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct TerrainRegion
        {
            public float MinimumHeight;
            public float OptimalHeight;
            public float MaximumHeight;
            public float Padding;
        }

        public static readonly ParameterKey<TerrainRegion> TerrainRegion0 = ParameterKeys.New(new TerrainRegion { MinimumHeight = -20f, OptimalHeight = 80f, MaximumHeight = 135f });
        public static readonly ParameterKey<TerrainRegion> TerrainRegion1 = ParameterKeys.New(new TerrainRegion { MinimumHeight = 120f, OptimalHeight = 160f, MaximumHeight = 180f });
        public static readonly ParameterKey<TerrainRegion> TerrainRegion2 = ParameterKeys.New(new TerrainRegion { MinimumHeight = 170f, OptimalHeight = 190f, MaximumHeight = 250f });
    }
}

