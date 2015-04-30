using System.Threading.Tasks;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;

namespace SimpleTerrain
{
    /// <summary>
    /// This sample shows how to create and utilize dynamic vertex and index buffers, in a terrain example.
    /// It uses a simple algorithm to create a normalized height map called "Fault formation" which is then used to create a terrain.
    /// Each time a terrain is (re)create, data in both vertex and index buffers change accordingly.
    /// 
    /// Moreover, there are a number of parameters you could set which affects shape, and size of terrain.
    /// It also shows a basic forward lighting that uses point, directional light emitter.
    /// </summary>
    public class SimpleTerrainGame : Game
    {
        public SimpleTerrainGame()
        {
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_10_0 };
            GraphicsDeviceManager.PreferredBackBufferWidth = 1136;
            GraphicsDeviceManager.PreferredBackBufferHeight = 640;
        }

        /// <summary>
        /// Creates and Initializes Pipeline, UI, Camera, and a terrain model
        /// </summary>
        /// <returns></returns>
        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // load the entity to display
            var scene = Asset.Load<Scene>("TerrainScene");
            SceneSystem.SceneInstance = new SceneInstance(Services, scene);
        }
    }
}
