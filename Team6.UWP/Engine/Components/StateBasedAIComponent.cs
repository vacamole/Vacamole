using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine.Entities;
using Team6.Engine.MemoryManagement;
using Team6.Engine.Misc;

namespace Team6.Engine.Components
{
    public class StateBasedAIComponent<T> : Component, IUpdateableComponent, IAIComponent
    {
        private MultiKeyedSets<T, StateBasedAIBehaviorComponent<T>> behavioursByState;
        private List<AITrigger> triggers = new List<AITrigger>();

        public event Action<T, T> StateChanged;

        public StateBasedAIComponent(T initialState)
        {
            CurrentState = initialState;
        }

        public T CurrentState { get; private set; }


        public void ChangeState(T nextState)
        {
            if (!CurrentState.Equals(nextState))
            {
                T oldState = CurrentState;
                // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
                behavioursByState.GetAll(CurrentState).ForEach(b => b.Deactivated());
                CurrentState = nextState;
                // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
                behavioursByState.GetAll(CurrentState).ForEach(b => b.Activated());
                StateChanged?.Invoke(oldState, nextState);
            }
            else
                // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
                behavioursByState.GetAll(CurrentState).ForEach(b => b.Reactivated());
        }

        public override void Initialize()
        {
            behavioursByState = new MultiKeyedSets<T, StateBasedAIBehaviorComponent<T>>(t => t.States);
            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            Entity.GetAllComponents<StateBasedAIBehaviorComponent<T>>().ForEach(t => behavioursByState.Add(t));
            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            behavioursByState.GetAll(CurrentState).ForEach(a => a.Activated());
        }

        public StateBasedAIComponent<T> WithTrigger(AITrigger trigger)
        {
            triggers.Add(trigger);
            return this;
        }

        public void Update(float elapsedSeconds, float totalSeconds)
        {
            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var trigger in triggers)
                trigger(CurrentState, Entity, elapsedSeconds, totalSeconds);
        }


        public delegate void AITrigger(T currentState, Entity entity, float elapsedSeconds, float totalElapsedSeconds);
    }


}
