using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine.Audio;
using Team6.Engine.Components;

namespace Team6.Engine.Audio
{
    /// <summary>
    /// Updates the audio manager's listener position for sound effects
    /// </summary>
    public class AudioListenerComponent : Component, IUpdateableComponent
    {
        private AudioManager soundManager;

        public override void Initialize()
        {
            base.Initialize();
            soundManager = Entity.Game.EngineComponents.Get<AudioManager>();
        }

        public void Update(float elapsedSeconds, float totalSeconds)
        {
            Body body = Entity.Body;
            AudioListener listener = soundManager.Listener;
            listener.Velocity = AudioManager.TransformTo3DPlane(body.LinearVelocity);
            listener.Position = AudioManager.TransformTo3DPlane(Entity.Game.Camera.Position, z: 20f);
        }

    }
}
