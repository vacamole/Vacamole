using System;
using Microsoft.Xna.Framework.Audio;
using Team6.Engine.MemoryManagement;
using Microsoft.Xna.Framework;
using Team6.Engine.Misc;
using System.Threading;

namespace Team6.Engine.Audio
{
    public class AudioBuffer
    {
        private readonly AudioDefinition definition;
        private readonly string asset;
        private readonly SoundEffect soundEffect;
        private volatile int instanceCount = 0;
        private readonly ObjectPool<WrappedSoundEffectInstance> instancePool;
        private volatile float lastPlayed;
        private SpinLock spinLock = new SpinLock();


        public AudioBuffer(string asset, SoundEffect soundEffect, AudioDefinition definition)
        {
            this.asset = asset;
            this.soundEffect = soundEffect;
            this.definition = definition;
            instancePool = new ObjectPool<WrappedSoundEffectInstance>(LoadInstance, initialCapacity: 16);
        }

        private WrappedSoundEffectInstance LoadInstance()
        {
            return new WrappedSoundEffectInstance(asset, this, soundEffect.CreateInstance());
        }

        public bool TryGetInstance(bool throttle, float totalTime, out WrappedSoundEffectInstance result)
        {
            result = null;

            if (!(totalTime - lastPlayed > definition.MinimumTimeBetween) && throttle)
                return false;

            bool lockTaken = false;
            spinLock.Enter(ref lockTaken);
            try
            {
                if (instanceCount < definition.InstanceLimit || !throttle)
                    instanceCount++;
                else
                    return false;

                lastPlayed = totalTime;

            }
            finally
            {
                spinLock.Exit();
            }

            var pooledObject = instancePool.GetFree();

            result = pooledObject.Value;
            result.PoolObject = pooledObject;
            result.Instance.Volume = MathHelper.Clamp(RandomExt.GetRandomFloat(definition.MinVolume, definition.MaxVolume), 0, 1);
            result.Instance.Pitch = MathHelper.Clamp(RandomExt.GetRandomFloat(definition.MinPitchShift, definition.MaxPitchShift), 0, 1);


            return true;
        }

        internal void DecrementInstanceCount()
        {
            bool lockTaken = false;
            spinLock.Enter(ref lockTaken);
            try { instanceCount--; }
            finally
            {
                spinLock.Exit();
            }
        }
    }

    public class WrappedSoundEffectInstance : IDisposable
    {
        private AudioBuffer parentBuffer;

        public WrappedSoundEffectInstance(string assetName, AudioBuffer parentBuffer, SoundEffectInstance soundEffectInstance)
        {
            this.Instance = soundEffectInstance;
            this.parentBuffer = parentBuffer;
            this.AssetName = assetName;
        }

        public SoundEffectInstance Instance { get; }
        public PooledObject<WrappedSoundEffectInstance> PoolObject { get; internal set; }
        public string AssetName { get; }

        public void Dispose()
        {
            PoolObject.Dispose();
            parentBuffer.DecrementInstanceCount();
        }
    }
}