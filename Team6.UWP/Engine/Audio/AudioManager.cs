using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Team6.Engine.Entities;
using Team6.Engine.Components;
using Team6.Engine.Misc;
using Team6.Game.Misc;
using Team6.Engine.Content;

namespace Team6.Engine.Audio
{
    public class AudioManager : EngineComponent
    {
        private ConcurrentDictionary<string, Song> songCache = new ConcurrentDictionary<string, Song>();
        private ConcurrentDictionary<string, AudioBuffer> soundEffectCache = new ConcurrentDictionary<string, AudioBuffer>();
        private float totalTime;

        public event Action<AudioManager> PlaybackStateChanged;

        public AudioManager(MainGame game) : base(game)
        {
            Listener = new AudioListener();
            Listener.Velocity = TransformTo3DPlane(Vector2.Zero);
            Listener.Position = TransformTo3DPlane(Vector2.Zero, z: 20f);
            MediaPlayer.ActiveSongChanged += MediaPlayer_ActiveSongChanged;
            MediaPlayer.MediaStateChanged += MediaPlayer_MediaStateChanged;

            LoadVolumeSettings();
            SoundEffect.DistanceScale = 50;
        }

        private void MediaPlayer_MediaStateChanged(object sender, EventArgs e)
        {
            PlaybackStateChanged?.Invoke(this);
        }

        private void MediaPlayer_ActiveSongChanged(object sender, EventArgs e)
        {
            PlaybackStateChanged?.Invoke(this);
        }

        public static Vector3 TransformTo3DPlane(Vector2 vector2, float z = 0)
        {
            return new Vector3(vector2, z);
        }

        public AudioListener Listener { get; }

        public bool TryGetSoundEffect(string assetName, out WrappedSoundEffectInstance result, bool throttle = true)
        {
            AudioBuffer buffer = soundEffectCache.GetOrAdd(assetName, LoadSoundEffect);
            bool available;
            if (available = buffer.TryGetInstance(throttle, totalTime, out result))
                result.Instance.Volume *= SoundVolume;

            // return values
            return available;
        }


        private AudioBuffer LoadSoundEffect(string assetName)
        {
            AudioDefinition definition;
            try
            {
                definition = Game.Content.LoadFromJson<AudioDefinition>(assetName);
            }
            catch (System.IO.FileNotFoundException)
            {
                definition = new AudioDefinition();
            }
            return new AudioBuffer(assetName, Game.Content.Load<SoundEffect>(assetName), definition);
        }

        public void PreloadSoundEffects(params string[] assets)
        {
            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (string asset in assets)
                soundEffectCache.GetOrAdd(asset, LoadSoundEffect);
        }

        public void PreloadSongs(params string[] assets)
        {
            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (string asset in assets)
                songCache.GetOrAdd(asset, LoadSong);
        }

        private Song LoadSong(string assetName)
        {
            return Game.Content.Load<Song>(assetName);
        }

        public Song GetSong(string assetName)
        {
            Song song = songCache.GetOrAdd(assetName, LoadSong);
            return song;
        }

        public void PlaySong(string asset, bool loop = true)
        {
            Song song = GetSong(asset);
            if (MediaPlayer.Queue.ActiveSong != song)
            {
                MediaPlayer.IsRepeating = loop;
                MediaPlayer.Play(song);
            }
        }

        internal void Free(WrappedSoundEffectInstance instance)
        {
            instance.Dispose();
        }


        public override void Update(GameTime gameTime, float elapsedSeconds, float totalSeconds)
        {
            totalTime = totalSeconds;
            base.Update(gameTime, elapsedSeconds, totalSeconds);
        }

        public float MusicVolume { get; set; } = 1f;

        /// <summary>
        /// Allows to relatively tune music volume in contrast to sound effect volume
        /// </summary>
        public float MusicScaling { get; set; } = 1f;

        public float SoundVolume { get; set; } = 1f;


        public void UpdateVolumes()
        {
            MediaPlayer.Volume = MusicVolume * MusicScaling;
            if (Game.CurrentScene != null)
            {
                // [FOREACH PERFORMANCE] Should not allocate garbage
                foreach (var entity in Game.CurrentScene.Entities)
                {
                    foreach (var audioSource in entity.GetAllComponents<AudioSourceComponent>())
                        audioSource.UpdateVolume();
                }

                Game.CurrentScene.NonPositionalAudio.UpdateVolume();
            }
        }

        public void LoadVolumeSettings()
        {
            MusicVolume = Settings<GameSettings>.Value.MasterVolume * Settings<GameSettings>.Value.MusicVolume;
            SoundVolume = Settings<GameSettings>.Value.MasterVolume * Settings<GameSettings>.Value.SoundVolume;

            UpdateVolumes();
        }
    }
}
