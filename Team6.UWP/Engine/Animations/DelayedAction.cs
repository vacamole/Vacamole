using System;

namespace Team6.Engine.Animations
{
    public class DelayedAction : DispatcherObject
    {
        private float delay;
        private Action callback;
        private float elapsedSinceStart;
        private Dispatcher dispatcher;
        private int repetitionsLeft;
        private Action finalCallback;

        public DelayedAction(Dispatcher dispatcher, float delay, Action callback, int repetitions = 1)
        {
            this.delay = delay;
            this.callback = callback;
            this.repetitionsLeft = repetitions;
            this.dispatcher = dispatcher;
        }

        public DelayedAction ThenDelay(float delay, Action callback)
        {
            return this.dispatcher.Delay(this.delay * repetitionsLeft + delay, callback);
        }

        /// <summary>
        /// call callback after all repetitions
        /// </summary>
        /// <param name="callback"></param>
        public void Then(Action callback)
        {
            finalCallback = callback;
        }

        public override void Update(float elapsedSeconds, float totalSeconds)
        {
            if (!IsRunning)
                return;

            elapsedSinceStart += elapsedSeconds;
            
            if(elapsedSinceStart >= delay)
            {
                if (repetitionsLeft > 0)
                    repetitionsLeft--;

                elapsedSinceStart = elapsedSinceStart - delay;
                IsFinished = repetitionsLeft == 0;

                callback();

                if (IsFinished && finalCallback != null)
                    finalCallback();
            }
        }
    }
}