﻿using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core;
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

        [DataMember(Mask = LiveScriptingMask)] // keep the value when reloading the script (live-scripting)
        private float originalPositionX;

        [DataMember(Mask = LiveScriptingMask)] // keep the value when reloading the script (live-scripting)
        private Color fontColor;

        public override async Task Execute()
        {
            var soundMusic = Asset.Load<SoundMusic>("AmbientMusic");
            var soundEffect = Asset.Load<SoundEffect>("SoundEffect");

            if (!IsLiveReloading)
            {
                // start ambient music
                soundMusic.IsLooped = true;
                soundMusic.Play();

                fontColor = Color.Transparent;
                originalPositionX = RightWave.Transform.Position.X;
            }

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
                    soundEffect.Stop();
                    soundEffect.Play();
                }
                else
                {
                    // moving wave position
                    LeftWave.Transform.Position.X -= 0.025f;
                    RightWave.Transform.Position.X += 0.025f;

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
