using System;
using System.Collections.Generic;

namespace Unity.RemoteConfig.Core.StateMachine
{
    public class State<TState, TTrigger> : IState<TState, TTrigger>

    {
    private readonly IDictionary<TTrigger, TState> m_stateTransitions = new Dictionary<TTrigger, TState>();
    private readonly IList<Action> m_entryTriggers = new List<Action>();
    private readonly IList<Action> m_exitTriggers = new List<Action>();

    public TState state { get; private set; }

    public State(TState state)
    {
        this.state = state;
    }

    public IDictionary<TTrigger, TState> transitions
    {
        get
        {
            IDictionary<TTrigger, TState> d_Transitions = new Dictionary<TTrigger, TState>();

            foreach (KeyValuePair<TTrigger, TState> transition in m_stateTransitions)
            {
                if (d_Transitions.ContainsKey(transition.Key))
                {
                    throw new InvalidOperationException("The trigger '" + transition.Key +
                                                        "' is already present in one of the super state transitions of the state '" +
                                                        transition.Value + "'.");
                }

                d_Transitions.Add(transition);
            }

            return d_Transitions;
        }
    }

    public ICollection<TTrigger> permittedTriggers
    {
        get { return transitions.Keys; }
    }

    public bool CanHandle(TTrigger trigger)
    {
        return transitions.ContainsKey(trigger);
    }

    public void AddTransition(TTrigger trigger, TState state)
    {
        if (m_stateTransitions.ContainsKey(trigger))
        {
            throw new InvalidOperationException("The trigger '" + trigger +
                                                "' is already present in one of the transitions of the state '" +
                                                state + "'.");
        }

        m_stateTransitions.Add(trigger, state);
    }

    public TState GetTransitionState(TTrigger trigger)
    {
        if (!transitions.ContainsKey(trigger))
        {
            throw new KeyNotFoundException("No transition present for trigger " + trigger);
        }

        return transitions[trigger];
    }

    public void OnEnter(IStateTransitions<TState, TTrigger> transition)
    {
        foreach (Action action in m_entryTriggers)
        {
            action();
        }
    }

    public void OnExit(IStateTransitions<TState, TTrigger> transition)
    {
        foreach (Action action in m_exitTriggers)
        {
            action();
        }
    }

    public void AddEntryTrigger(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action", "Action parameter must not be null");
        }

        if (m_entryTriggers.Contains(action))
        {
            throw new ArgumentException("Action " + action + " is already added to entryActions");
        }

        m_entryTriggers.Add(action);
    }

    public void AddExitTrigger(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action", "Action parameter must not be null");
        }

        if (m_exitTriggers.Contains(action))
        {
            throw new ArgumentException("Action " + action + " is already added to exitActions");
        }

        m_exitTriggers.Add(action);
    }

    public bool Includes(TState state)
    {
        return this.state.Equals(state);
    }

    }
}