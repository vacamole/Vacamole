using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Team6.Engine.Misc;

namespace Team6.Engine.Animations
{
    public class Dispatcher
    {
        LinkedList<DispatcherObject> activeObjects = new LinkedList<DispatcherObject>();
        List<LinkedListNode<DispatcherObject>> forRemoval = new List<LinkedListNode<DispatcherObject>>();
        List<Action> nextFrameActions = new List<Action>(); // array list is faster and more memory efficient for this purpose
        List<Action> currentlyExecutionActions = new List<Action>();

        public void Update(float elapsedSeconds, float totalSeconds)
        {
            // swap lists (this has to be done to be able to reschedule something for the next thread from within a next frame action)
            List<Action> executions = nextFrameActions;
            nextFrameActions = currentlyExecutionActions;
            currentlyExecutionActions = executions;

            // execute and clear
            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (Action a in currentlyExecutionActions)
                a();

            currentlyExecutionActions.Clear();

            LinkedListNode<DispatcherObject> next = null;
            for (var currentObjectNode = activeObjects.First; currentObjectNode != null; currentObjectNode = next)
            {
                DispatcherObject currentObj = currentObjectNode.Value;
                next = currentObjectNode.Next;

                currentObj.Update(elapsedSeconds, totalSeconds);

                if (currentObj.IsFinished)
                    activeObjects.Remove(currentObjectNode);
            }
        }

        public Animation AddAnimation(Animation animation)
        {
            activeObjects.AddLast(animation);
            return animation;
        }

        public DelayedAction Delay(float delay, Action callback, int repetitions = 1)
        {
            var action = new DelayedAction(this, delay, callback, repetitions);
            activeObjects.AddLast(action);
            return action;
        }

        /// <summary>
        /// Allows for any action to be executed slightly later (i.e. if entities gets created etc... e.g. it is required
        /// to be out of the update loop of input components)
        /// </summary>
        /// <param name="action"></param>
        public void NextFrame(Action action)
        {
            nextFrameActions.Add(action);
        }

        /// <summary>
        /// Pauses the execution of all active dispatcher objects. Objects added after this call, will still be added as running.
        /// </summary>
        public void Pause()
        {
            // [FOREACH PERFORMANCE] Should not allocate garbage
            activeObjects.ForEach(d => d.IsRunning = false);
        }

        /// <summary>
        /// Resumes the execution of all active dispatcher objects.
        /// </summary>
        public void Unpause()
        {
            // [FOREACH PERFORMANCE] Should not allocate garbage
            activeObjects.ForEach(d => d.IsRunning = true);
        }
    }
}
