using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Games;

namespace GameMenu
{
    public abstract class UISceneBase : AsyncScript
    {
        protected readonly List<object> LoadedAssets = new List<object>();

        protected Game UIGame;

        protected bool IsRunning;

        protected bool SceneCreated;

        public override void Start()
        {
            base.Start();

            IsRunning = true;

            UIGame = (Game)Services.GetServiceAs<IGame>();

            AdjustVirtualResolution(this, EventArgs.Empty);
            Game.Window.ClientSizeChanged += AdjustVirtualResolution;

            CreateScene();
        }

        private void AdjustVirtualResolution(object sender, EventArgs e)
        {
            var backBufferSize = new Vector2(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height);
            Entity.Get<UIComponent>().VirtualResolution = new Vector3(backBufferSize, 1000);
        }

        protected void CreateScene()
        {
            if (!SceneCreated)
                LoadScene();

            SceneCreated = true;
        }

        protected T LoadAsset<T>(string assetName) where T : class
        {
            LoadedAssets.Add(Asset.Load<T>(assetName));

            return (T)LoadedAssets[LoadedAssets.Count - 1];
        }

        protected abstract void LoadScene();

        public override async Task Execute()
        {
            while (IsRunning)
            {
                await Script.NextFrame();

                UpdateScene(Game.UpdateTime);
            }
        }

        protected virtual void UpdateScene(GameTime time)
        {
        }

        protected override void Destroy()
        {
            base.Destroy();

            IsRunning = false;

            SceneCreated = false;

            foreach (var asset in LoadedAssets)
                Asset.Unload(asset);

            LoadedAssets.Clear();
        }
    }
}