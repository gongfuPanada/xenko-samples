using Android.App;
using Android.OS;

using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Starter;

namespace NativeLinking
{
    [Activity(MainLauncher = true, 
              Icon = "@drawable/icon", 
              ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape,
              ConfigurationChanges = Android.Content.PM.ConfigChanges.UiMode)]
    public class NativeLinkingActivity : AndroidParadoxActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Game = new Game();
            Game.Run(GameContext);
        }

        protected override void OnDestroy()
        {
            Game.Dispose();

            base.OnDestroy();
        }
    }
}
