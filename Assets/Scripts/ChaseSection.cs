using UnityEngine;

public class ChaseSection : MonoBehaviour
{
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private Transform monsterSpawnPoint;
    private MonsterAI monster;

    private void Start() {
        monster = FindFirstObjectByType<MonsterAI>();

        if (monsterSpawnPoint == null) {
            Debug.LogError("Monster spawn point not assigned in the inspector");
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            monster.EnterChaseSection(monsterSpawnPoint.position, chaseSpeed);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            monster.ExitChaseSection();
        }
    }
}
