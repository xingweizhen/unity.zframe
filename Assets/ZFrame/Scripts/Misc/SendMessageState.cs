using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStateMachineEventReceiver
{
    void OnStateMachineEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
    void OnStateMachineExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
}

[SharedBetweenAnimators]
public class SendMessageState : StateMachineBehaviour
{
    [SerializeField] private bool m_OnEnter;
    [SerializeField] private bool m_OnExit;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_OnEnter) {
            var recv = animator.GetComponentInParent(typeof(IStateMachineEventReceiver)) as IStateMachineEventReceiver;
            if (recv != null) recv.OnStateMachineEnter(animator, stateInfo, layerIndex);
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_OnExit) {
            var recv = animator.GetComponentInParent(typeof(IStateMachineEventReceiver)) as IStateMachineEventReceiver;
            if (recv != null) recv.OnStateMachineExit(animator, stateInfo, layerIndex);
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}
