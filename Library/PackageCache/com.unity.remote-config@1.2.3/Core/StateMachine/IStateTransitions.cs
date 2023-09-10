namespace Unity.RemoteConfig.Core.StateMachine
{
    public interface IStateTransitions <TState, TTrigger>
    {
        TState CurrentState { get; }
        
        TState FutureState { get; }
        
        TTrigger Trigger { get; }
        
        bool isLoopback { get; }
        
    }
}