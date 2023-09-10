
namespace Unity.RemoteConfig.Editor.Validation
{
    public class ValidationStateMachine
    {
        private enum State { Unvalidated, Unvalidated_Remote, Unvalidated_Cache, Validation_In_Progress, Validated, Validated_Remote, Validated_Cache, Failed_Validation ,Failed_Remote, Failed_Cached, Validation_Error, Unknown_Error }

        private enum Trigger { Validate, Validation_Success, Validation_Fail, Validation_Error, Unknown_Error }

        private enum ValidationTarget { All, Remote, Cache }


        private State m_state;
        

        public ValidationStateMachine() 
        {
            InitializeValidationStateMachine();
        }

        private void InitializeValidationStateMachine()
        {
            
//            m_stateMachine = new StateMachine<State, Trigger>(State.Unvalidated);
//            m_validateTrigger = m_stateMachine.SetTriggerParameters<string>(Trigger.Validate);
//
//
//            m_stateMachine.Configure(State.Unvalidated)
//                .Permit(Trigger.Validate, State.Validation_In_Progress);
//
//            m_stateMachine.Configure(State.Validation_In_Progress)
//                .SubstateOf(State.Unvalidated)
//                .OnEntryFrom(Trigger.Validate, ()=> OnRequestToValidate(ValidationTarget.All))
//                .PermitReentry(Trigger.Validate)
//                .Permit(Trigger.Validate, State.Validated);
//
//            m_stateMachine.Configure(State.Validated)
//                .Permit(Trigger.Validation_Success, State.Validation_In_Progress);
//            
//            m_stateMachine.Configure(State.Failed_Validation)
//                .Permit(Trigger.Validation_Error, State.Validation_In_Progress);
//            
//            m_stateMachine.Configure(State.Validation_Error)
//                .Permit(Trigger.Validation_Fail, State.Validation_In_Progress);
//            
//            m_stateMachine.Configure(State.Unknown_Error)
//                .Permit(Trigger.Unknown_Error, State.Validation_In_Progress);
        }
        
        private void OnRequestToValidate(ValidationTarget target)
        {
            if (target == ValidationTarget.Cache)
            {
                _ValidateCache();
            }
            else if (target == ValidationTarget.Remote)
            {
                _ValidateRemote();
            }
            else
            {
                _ValidateCache();
                _ValidateRemote();
            }
        }

        private void _ValidateRemote()
        {
        }
        
        private void _ValidateCache()
        {
        }

    }
}