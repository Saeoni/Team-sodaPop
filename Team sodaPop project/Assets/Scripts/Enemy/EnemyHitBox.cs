using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 5;
    private static int _currentAttackId = 0;   // shared across all hitboxes
    private int _lastAttackIdApplied = -1;     // per hitbox

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Prevent double-hits in the same attack session
        if (_lastAttackIdApplied == _currentAttackId) return;

        Gamemanager.Instance.playerController.takeDamage(damage);
        _lastAttackIdApplied = _currentAttackId;
    }

    // Called by CreatureAI at the start of each attack animation
    public static void BeginNewAttack()
    {
        _currentAttackId++;
    }
}

