using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 5;
    [SerializeField] private bool isCinematicKill = false;
    
    private static int _currentAttackId;   // shared across all hitboxes
    private int _lastAttackIdApplied = -1;     // per hitbox

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_lastAttackIdApplied == _currentAttackId) return;
        
        _lastAttackIdApplied = _currentAttackId;

        var player = Object.FindFirstObjectByType<playerController>();
        if (player == null) return;
        if (isCinematicKill)
        {
            player.KillPlayer();
        }
        else
        {
            player.takeDamage(damage);
        }
    }

    // Called by CreatureAI at the start of each attack animation
    public static void BeginNewAttack()
    {
        _currentAttackId++;
    }
}
