using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace GameMenu
{
    public class SplashScene : UISceneBase
    {
        public SplashScene(GameMenuGame uiGame)
            : base(uiGame)
        {
            CreateScene();
        }
        
        protected override void LoadScene()
        {
            var arial = LoadAsset<SpriteFont>(GameMenuGame.FontDirectory + "WesternFont");
            var uiImages = LoadAsset<UIImageGroup>(GameMenuGame.TextureDirectory + "SplashScreenImages");

            // Create and initialize "Paradox Samples" Text
            var paradoxSampleTextBlock = new ContentDecorator
            {
                BackgroundImage = uiImages["paradox_sample_text_bg"],
                Content = new TextBlock 
                {
                    Font = arial,
                    TextSize = 60,
                    Text = "Paradox Samples", 
                    TextColor = Color.White, 
                },
                Padding = new Thickness(35, 15, 35, 25),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            paradoxSampleTextBlock.SetPanelZIndex(1);

            // Create and initialize "UI" Text
            var uiTextBlock = new ContentDecorator
            {
                BackgroundImage = uiImages["ui_text_bg"],
                Content = new TextBlock
                {
                    Font = arial,
                    TextSize = 60,
                    Text = "UI", 
                    TextColor = Color.White,
                },
                Padding = new Thickness(15, 4, 15, 7),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            uiTextBlock.SetPanelZIndex(1);
            uiTextBlock.SetGridRow(1);

            // Create and initialize Paradox Logo
            var paradoxLogoImageElement = new ImageElement
            {
                Source = uiImages["Logo"],
                HorizontalAlignment = HorizontalAlignment.Center
            };

            paradoxLogoImageElement.SetPanelZIndex(1);
            paradoxLogoImageElement.SetGridRow(3);

            // Create and initialize "Touch Screen to Start"
            var touchStartLabel = new ContentDecorator
            {
                BackgroundImage = uiImages["touch_start_frame"],
                Content = new TextBlock
                {
                    Font = arial,
                    TextSize = 42,
                    Text = "Touch Screen to Start",
                    TextColor = Color.White
                },
                Padding = new Thickness(30, 20, 30, 25),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            touchStartLabel.SetPanelZIndex(1);
            touchStartLabel.SetGridRow(5);

            var grid = new Grid
            {
                MaximumWidth = 600,
                MaximumHeight = 900,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            grid.RowDefinitions.Add(new StripDefinition(StripType.Auto));
            grid.RowDefinitions.Add(new StripDefinition(StripType.Auto));
            grid.RowDefinitions.Add(new StripDefinition(StripType.Star, 2));
            grid.RowDefinitions.Add(new StripDefinition(StripType.Star, 5));
            grid.RowDefinitions.Add(new StripDefinition(StripType.Star, 2));
            grid.RowDefinitions.Add(new StripDefinition(StripType.Auto));
            grid.ColumnDefinitions.Add(new StripDefinition());
            grid.LayerDefinitions.Add(new StripDefinition());

            grid.Children.Add(paradoxSampleTextBlock);
            grid.Children.Add(uiTextBlock);
            grid.Children.Add(paradoxLogoImageElement);
            grid.Children.Add(touchStartLabel);

            // Add the background
            var background = new ImageElement { Source = uiImages["background_uiimage"], StretchType = StretchType.Fill };
            background.SetPanelZIndex(-1);

            RootElement = new UniformGrid {Children = {background, grid}};
        }

        protected override void UpdateScene(GameTime time)
        {
            base.UpdateScene(time);

            if(Input.PointerEvents.Count > 0)
                UIGame.GoToMainScene();
        }
    }
}