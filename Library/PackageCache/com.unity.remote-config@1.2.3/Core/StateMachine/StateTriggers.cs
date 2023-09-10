using System;

namespace Unity.RemoteConfig.Core.StateMachine
{
    public class StateTriggers<TState, TTrigger> : IStateTriggers<TState, TTrigger>
    {
        public StateMachine<TState, TTrigger> m_machine { get; private set; }
        
        public IState<TState, TTrigger> m_state{ get; private set; }
        
        public TState state => m_state.state;
        
        public StateTriggers(StateMachine<TState, TTrigger> machine, TState state)
        {
            this.m_machine = machine;
            m_state = new State<TState, TTrigger>(state);
        }
        
        public IStateTriggers<TState, TTrigger> Permit(TTrigger trigger, TState state)
        {
            m_state.AddTransition(trigger, state);
            return this;
        }
        
        public IStateTriggers<TState, TTrigger> PermitIf(TTrigger trigger, TState state, Func<bool> condition)
        {
            if (condition() == true)
            {
                Permit(trigger, state);
            }

            return this;
        }
        
        public IStateTriggers<TState, TTrigger> PermitLoopback(TTrigger trigger)
        {
            Permit(trigger, state);
            return this;
        }

        public IStateTriggers<TState, TTrigger> PermitLoopbackIf(TTrigger trigger, Func<bool> condition)
        {
            if (condition() == true)
            {
                PermitLoopback(trigger);
            }

            return this;
        }
        
        public IStateTriggers<TState, TTrigger> OnEnter(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action", "action parameter must not be null");
            }

            m_state.AddEntryTrigger(action);
            return this;
        }
        
        public IStateTriggers<TState, TTrigger> OnExit(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action", "action parameter must not be null");
            }

            m_state.AddExitTrigger(action);
            return this;
        }

        public IStateTriggers<TState, TTrigger> SubstateOf(TState superState)
        {
            // Check for accidental identical cyclic configuration.
            if (state.Equals(superState))
            {
                throw new ArgumentException("Configuring " + state + " as a substate of " + superState + " creates an illegal cyclic configuration.");
            }

            m_state = m_machine.GetStateConfigs(superState);
            return this;
        }
    }
}