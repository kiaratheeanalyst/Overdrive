using System;
using UnityEngine;
using NUnit.Framework;
using Unity.RemoteConfig.Core.StateMachine;

namespace Unity.RemoteConfig.Editor.Tests
{
    public class StateMachineTests
    {
        enum Trigger
        {
            jumpSignal
        }

        enum State
        {
            root,
            idle,
            jump
        }
        
        [Test]
        public void OnInitializationOfStateMachineCreatesAStateMachine()
        {
            IStateMachine<State, Trigger> motionStateMachine = new StateMachine<State, Trigger>(State.root);
            motionStateMachine.Configure(State.root).Permit(Trigger.jumpSignal, State.jump);
            motionStateMachine.Configure(State.jump)
                .OnEnter(() => { StartTimer(); })
                .OnExit(() => { StopTimer(); })
                .Permit(Trigger.jumpSignal, State.idle);
            
            Assert.True(motionStateMachine.CurrentState == State.root);
        }
        
        [Test]
        public void StartingStateIsRootState()
        {

            IStateMachine<State, Trigger> motionStateMachine = new StateMachine<State, Trigger>(State.root);
            Assert.True(motionStateMachine.IsInState(State.root));
        }
        
        [Test]
        public void TriggerAdvancesState()
        {
            IStateMachine<State, Trigger> motionStateMachine = new StateMachine<State, Trigger>(State.root);
            motionStateMachine.Configure(State.root).Permit(Trigger.jumpSignal, State.jump);
            motionStateMachine.Configure(State.jump)
                .OnEnter(() => { StartTimer(); })
                .OnExit(() => { StopTimer(); })
                .Permit(Trigger.jumpSignal, State.idle);
            
            Fire(motionStateMachine, Trigger.jumpSignal);
            
            Assert.True(motionStateMachine.CurrentState == State.jump);
        }

        [Test]
        public void OnInitializationContainsNoTriggers()
        {
            IStateMachine<State, Trigger> motionStateMachine = new StateMachine<State, Trigger>(State.root);
            motionStateMachine.Configure(State.root);
            Assert.True(motionStateMachine.PermittedTriggers.Count == 0);
        }

        [Test]
        public void UnConfiguredTriggersAreNotRecognized()
        {
            IStateMachine<State, Trigger> motionStateMachine = new StateMachine<State, Trigger>(State.root);
            motionStateMachine.Configure(State.root);
            Assert.False(motionStateMachine.GetStateConfigs(State.root).CanHandle(Trigger.jumpSignal));
        }

        
        static void Fire(IStateMachine<State, Trigger> motionStateMachine, Trigger trigger)
        {
            Debug.Log("Triggering: " + trigger);
            motionStateMachine.Fire(trigger);
        }
        
        static void StartTimer()
        {
            Debug.Log("Jump started at " + DateTime.Now);
        }

        static void StopTimer()
        {
            Debug.Log("Jump ended at " + DateTime.Now);
        }
    }
}