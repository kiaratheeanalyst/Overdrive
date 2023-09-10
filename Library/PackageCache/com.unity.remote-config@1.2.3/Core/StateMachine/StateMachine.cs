using System;
using System.Collections.Generic;

namespace Unity.RemoteConfig.Core.StateMachine
{
    public partial class StateMachine<TState, TTrigger>: IStateMachine<TState, TTrigger>
    {
        public IDictionary<TState, IStateTriggers<TState, TTrigger>> m_stateConfigurations = new Dictionary<TState, IStateTriggers<TState, TTrigger>>();
        public Action e_OnUpdate = delegate { };
        
        public TState CurrentState { get;  set; }
        
        public StateMachine(TState initialState)
        {
            CurrentState = initialState;
        }
        
        public ICollection<TTrigger> PermittedTriggers
        {
            get
            {
                return currentStateConfigs.permittedTriggers;
            }
        }
        
        private IState<TState, TTrigger> currentStateConfigs
        {
            get
            {
                return GetStateConfigs(CurrentState);
            }
        }
        
        public IState<TState, TTrigger> GetStateConfigs(TState state)
        {
            if (!m_stateConfigurations.ContainsKey(state))
            {
                throw new NotSupportedException("State " + state + " is not configured yet so no representation exists for it!");
            }

            return m_stateConfigurations[state].m_state;
        }
        
        public IStateTriggers<TState, TTrigger> Configure(TState state)
        {
            if (m_stateConfigurations.ContainsKey(state))
            {
                return m_stateConfigurations[state];
            }
            else
            {
                IStateTriggers<TState, TTrigger> configuration = new StateTriggers<TState, TTrigger>(this, state);
                m_stateConfigurations.Add(state, configuration);
                return configuration;
            }
        }
        
        public void Fire(TTrigger trigger)
        {
            if (!PermittedTriggers.Contains(trigger))
            {
                throw new NotSupportedException("'" + trigger + "' trigger is not configured for '" + CurrentState + "' state.");
            }

            TState oldState = CurrentState;
            TState newState = currentStateConfigs.GetTransitionState(trigger);
            IState<TState, TTrigger> oldStateRepresentation = GetStateConfigs(oldState);
            IState<TState, TTrigger> newStateRepresentation = GetStateConfigs(newState);

            if (e_OnUpdate != null)
            {
                e_OnUpdate();
            }

            IStateTransitions<TState, TTrigger> transition = new StateTransitions<TState, TTrigger>(oldState, newState, trigger);
            oldStateRepresentation.OnExit(transition);
            newStateRepresentation.OnEnter(transition);
            CurrentState = newState;
        }
        
        public bool IsInState(TState state)
        {
            return CurrentState.Equals(state);
        }

        public bool CanFire(TTrigger trigger)
        {
            return currentStateConfigs.CanHandle(trigger);
        }
        
        public void OnTransitioned(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action", "Action parameter must not be null.");
            }

            e_OnUpdate = action;
        }


        public override string ToString()
        {
            string triggers = string.Empty;

            foreach (TTrigger trigger in PermittedTriggers)
            {
                triggers += trigger + ", ";
            }

            return ("Current state: " + CurrentState + " | Permitted triggers: " + triggers);
        }
        
    }
}