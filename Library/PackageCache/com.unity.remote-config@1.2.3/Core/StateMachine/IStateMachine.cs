using System;
using System.Collections.Generic;

namespace Unity.RemoteConfig.Core.StateMachine
{
    public interface IStateMachine<TState, TTrigger>
    {
        TState CurrentState { get; }
        
        ICollection<TTrigger> PermittedTriggers { get; }
        
        IStateTriggers<TState, TTrigger> Configure(TState state);
        
        void Fire(TTrigger trigger);
        
        bool IsInState(TState state);
        
        void OnTransitioned(Action action);
        
        IState<TState, TTrigger> GetStateConfigs(TState state);
        
        string ToString();
    }
}