using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.UI;

namespace GameMenu
{
    public abstract class UISceneBase : ScriptContext, IScript
    {
        protected readonly List<object> LoadedAssets = new List<object>();

        protected GameMenuGame UIGame;

        protected bool SceneCreated;
        
        public UIElement RootElement { get; protected set; }

        public bool IsRunning { get; set; }

        protected UISceneBase(GameMenuGame uiGame)
            : base(uiGame.Services)
        {
            UIGame = uiGame;
        }

        protected void CreateScene()
        {
            if(!SceneCreated)
                LoadScene();

            SceneCreated = true;
        }

        protected T LoadAsset<T>(string assetName) where T : class
        {
            LoadedAssets.Add(Asset.Load<T>(assetName));

            return (T)LoadedAssets[LoadedAssets.Count-1];
        }

        protected abstract void LoadScene();
        
        public async Task Execute()
        {
            IsRunning = true;

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

            SceneCreated = false;
            RootElement = null;

            foreach (var asset in LoadedAssets)
                Asset.Unload(asset);

            LoadedAssets.Clear();
        }
    }
}