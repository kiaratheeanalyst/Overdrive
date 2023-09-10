
namespace Unity.RemoteConfig.Core.StateMachine
{
    public class StateTransitions<TState, TTrigger> : IStateTransitions<TState, TTrigger>
    {
        public TState CurrentState { get; private set; }
        public TState FutureState { get; private set; }
        public TTrigger Trigger { get; private set; }

        public StateTransitions(TState currentState, TState futureState, TTrigger trigger)
        {
            this.CurrentState = currentState;
            this.FutureState = futureState;
            this.Trigger = trigger;
        }
        
        public bool isLoopback => CurrentState.Equals(FutureState);
    }
}