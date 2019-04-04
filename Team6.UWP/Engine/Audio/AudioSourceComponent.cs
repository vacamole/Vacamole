using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine.Audio;
using Team6.Engine.Components;
using Team6.Engine.Content;

namespace Team6.Engine.Audio
{
    public class AudioSourceComponent : Component, IUpdateableComponent, ILoadContent
    {
        private readonly AudioEmitter emitter = new AudioEmitter();
        private AudioManager soundManager;
        private readonly LinkedList<WrappedSoundEffectInstance> playingInstances = new LinkedList<WrappedSoundEffectInstance>();
        private string[] soundEffectsToPlay;
        private string[] songsToPlay;

        public AudioSourceComponent() : this(new string[] { }, new string[] { })
        {
        }

        public AudioSourceComponent(string[] soundEffectsToPlay, string[] songsToPlay)
        {
            this.songsToPlay = songsToPlay;
            this.soundEffectsToPlay = soundEffectsToPlay;
        }

        public override void Initialize()
        {
            base.Initialize();
            soundManager = Entity.Game.EngineComponents.Get<AudioManager>();
        }

        public void Update(float elapsedSeconds, float totalSeconds)
        {
            Body body = Entity.Body;
            AudioListener listener = soundManager.Listener;
            if (PositionalSound)
            {
                emitter.Velocity = AudioManager.TransformTo3DPlane(body.LinearVelocity);
                emitter.Position = AudioManager.TransformTo3DPlane(body.Position);
            }
            LinkedListNode<WrappedSoundEffectInstance> next = null;
            for (var soundInstanceNode = playingInstances.First; soundInstanceNode != null; soundInstanceNode = next)
            {
                var soundInstance = soundInstanceNode.Value;
                next = soundInstanceNode.Next;

                switch (soundInstance.Instance.State) // do nothing if paused
                {
                    case SoundState.Playing:
                        if (PositionalSound)
                            soundInstance.Instance.Apply3D(listener, emitter);
                        break;
                    case SoundState.Stopped:
                        soundManager.Free(soundInstance);
                        playingInstances.Remove(soundInstanceNode);
                        break;
                }
            }
        }


        /// <summary>
        /// Fire and forget playing a positional sound
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="volume"></param>
        public void PlaySound(string asset)
        {
            WrappedSoundEffectInstance instance;

            if (!soundManager.TryGetSoundEffect(asset, out instance))
                return;

            if (PositionalSound)
                instance.Instance.Apply3D(soundManager.Listener, emitter);

            try
            {
                instance.Instance.Play();
                playingInstances.AddLast(instance);
            }
            catch (InstancePlayLimitException)
            {
                soundManager.Free(instance);
            }
        }

        public void PlaySoundIfNotAlreadyPlaying(string asset)
        {
            var isPlaying = false;
            foreach (var instance in playingInstances)
            {
                if (instance.AssetName != asset) continue;
                isPlaying = true;
                break;
            }
            if (!isPlaying)
                PlaySound(asset);
        }

        internal void UpdateVolume()
        {
            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var effectInstance in playingInstances)
                effectInstance.Instance.Volume = soundManager.SoundVolume;
        }

        public void PauseAll()
        {
            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var effectInstance in playingInstances)
                effectInstance.Instance.Pause();
        }

        public void ResumeAll()
        {
            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var effectInstance in playingInstances)
                effectInstance.Instance.Resume();
        }

        public bool PositionalSound { get; set; } = true;

        public void LoadContent()
        {
            soundManager.PreloadSoundEffects(soundEffectsToPlay);
            soundManager.PreloadSongs(songsToPlay);
        }
    }
}
