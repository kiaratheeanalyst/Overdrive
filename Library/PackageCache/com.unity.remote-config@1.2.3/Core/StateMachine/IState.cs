using System;
using System.Collections.Generic;

namespace Unity.RemoteConfig.Core.StateMachine
{
    public interface IState<TState, TTrigger>
    {
        TState state { get; }

        IDictionary<TTrigger, TState> transitions { get; }
        
        ICollection<TTrigger> permittedTriggers { get; }
        
        
        bool Includes(TState state);

        
        bool CanHandle(TTrigger trigger);
        
        void AddTransition(TTrigger trigger, TState state);
        
        TState GetTransitionState(TTrigger trigger);
        
        void OnEnter(IStateTransitions<TState, TTrigger> transition);
        
        void OnExit(IStateTransitions<TState, TTrigger> transition);
        
        void AddEntryTrigger(Action trigger);
        
        void AddExitTrigger(Action trigger);
    }
}