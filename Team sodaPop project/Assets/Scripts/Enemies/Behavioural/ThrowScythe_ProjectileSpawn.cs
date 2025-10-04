using UnityEngine;

public class ThrowScythe_ProjectileSpawn : StateMachineBehaviour
{
    [SerializeField] private Vector3 spawnOffset = new Vector3(1f, 0.5f, 0f);

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ReaperAI reaper = animator.GetComponent<ReaperAI>();
        GameObject prefab = reaper.GetFlyingScythePrefab();
        if (reaper == null || prefab == null) return;

        Vector3 spawnPos = reaper.transform.position +
                           reaper.transform.forward * spawnOffset.z +
                           reaper.transform.up * spawnOffset.y +
                           reaper.transform.right * spawnOffset.x;

        GameObject scythe = GameObject.Instantiate(prefab, spawnPos, Quaternion.identity);

        var projectile = scythe.GetComponent<Scythe_Projectile>();
        if (projectile != null)
        {
            projectile.SetPullTarget(reaper.transform);
            reaper.activeProjectile = projectile;
        }
        reaper.HideScythe();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
