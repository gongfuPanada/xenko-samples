using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Audio;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Input;

namespace SimpleAudio
{
    /// <summary>
    /// The main script in charge of the sound.
    /// </summary>
    public class SoundScript : AsyncScript
    {
        /// <summary>
        /// The left wave entity.
        /// </summary>
        public Entity LeftWave;

        /// <summary>
        /// The right wave entity.
        /// </summary>
        public Entity RightWave;

        public override void Start()
        {
            base.Start();
            var music = Asset.Load<SoundMusic>("AmbientMusic");

            // start ambient music
            music.IsLooped = true;
            music.Play();
        }

        public override async Task Execute()
        {
            var fontColor = Color.Transparent;
            var effect = Asset.Load<SoundEffect>("SoundEffect");
            var originalPositionX = RightWave.Transform.Position.X;
            
            while (Game.IsRunning)
            {
                if (Input.PointerEvents.Any(item => item.State == PointerState.Down)) // New click
                {
                    // reset wave position
                    LeftWave.Transform.Position.X = -originalPositionX;
                    RightWave.Transform.Position.X = originalPositionX;

                    // reset transparency
                    fontColor = Color.White;

                    // play the sound effect on each touch on the screen
                    effect.Stop();
                    effect.Play();
                }
                else
                {
                    // moving wave position
                    LeftWave.Transform.Position.X -= 0.01f;
                    RightWave.Transform.Position.X += 0.01f;

                    // changing font transparency
                    fontColor = 0.93f * fontColor;
                    LeftWave.Get<SpriteComponent>().Color = fontColor;
                    RightWave.Get<SpriteComponent>().Color = fontColor;
                }

                // wait for next frame
                await Script.NextFrame();
            }
        }
    }
}
