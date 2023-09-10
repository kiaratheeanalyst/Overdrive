using System;

namespace Unity.RemoteConfig.Core.StateMachine
{
    public interface IStateTriggers<TState, TTrigger>
    {
        TState state { get; } 

        StateMachine<TState, TTrigger> m_machine { get; }
        IState<TState, TTrigger> m_state { get; }

        IStateTriggers<TState, TTrigger> Permit(TTrigger trigger, TState state);
        
        IStateTriggers<TState, TTrigger> PermitIf(TTrigger trigger, TState state, Func<bool> condition);
        
        IStateTriggers<TState, TTrigger> PermitLoopback(TTrigger trigger);
        
        IStateTriggers<TState, TTrigger> PermitLoopbackIf(TTrigger trigger, Func<bool> condition);
        
        IStateTriggers<TState, TTrigger> OnEnter(Action action);
        
        IStateTriggers<TState, TTrigger> OnExit(Action action);
        
    }
}