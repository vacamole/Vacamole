using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine.Misc;

namespace Team6.Engine.Components
{
    public abstract class StateBasedAIBehaviorComponent<T> : Component, IUpdateableComponent, IAIComponent
    {
        protected StateBasedAIComponent<T> stateAI;
        protected float ElapsedSecondsSinceStateActivation { get; private set; }
        public OutgoingState<T>[] OutgoingStates { get; private set; }

        public float Weight { get; set; } = 1f;

        HashSet<T> states;
        public IEnumerable<T> States { get { return states; } }

        public StateBasedAIBehaviorComponent(T state, params OutgoingState<T>[] outgoingStates)
        {
            this.states = new HashSet<T>() { state };
            this.OutgoingStates = outgoingStates;
        }

        public StateBasedAIBehaviorComponent(IEnumerable<T> states, IEnumerable<OutgoingState<T>> outgoingStates)
        {
            this.states = new HashSet<T>(states);
            this.OutgoingStates = outgoingStates.ToArray();
        }

        public override void Initialize()
        {
            this.stateAI = Entity.GetComponent<StateBasedAIComponent<T>>();
        }

        public void Update(float elapsedSeconds, float totalSeconds)
        {
            if (states.Contains(stateAI.CurrentState))
            {
                ElapsedSecondsSinceStateActivation += elapsedSeconds;
                UpdateBehaviour(elapsedSeconds, totalSeconds);
            }
        }

        protected abstract void UpdateBehaviour(float elapsedSeconds, float totalSeconds);

        /// <summary>
        /// Is called when the current behaviour is no longer active
        /// </summary>
        public virtual void Deactivated() { }

        /// <summary>
        /// Is called when the current behaviour is now the active behaviour
        /// </summary>
        public virtual void Activated()
        {
            ElapsedSecondsSinceStateActivation = 0;
        }

        /// <summary>
        /// Is called when the current behaviour is already active but a new request
        /// to switch to it was issued
        /// </summary>
        public virtual void Reactivated() { }

        public void SwitchToOutputState()
        {
            float random = RandomExt.GetRandomFloat(0, 1);
            float accumulated = 0;

            T nextState = stateAI.CurrentState;

            for (int i = 0; i < OutgoingStates.Length; i++)
            {
                OutgoingState<T> state = OutgoingStates[i];
                accumulated += state.Probabiliy;

                if (random <= accumulated)
                {
                    nextState = state.State;
                    break;
                }
            }

            stateAI.ChangeState(nextState);
        }
    }

    public struct OutgoingState<T>
    {
        public OutgoingState(float probability, T state)
        {
            Probabiliy = probability;
            State = state;
        }
        public float Probabiliy { get; set; }
        public T State { get; set; }
    }
}
