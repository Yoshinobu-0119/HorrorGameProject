using UnityEngine;

public class EnemyAITestStarter : MonoBehaviour
{
    [SerializeField] private EnemyAIController enemy;
    [SerializeField] private Transform player;

    private void Start()
    {
        enemy.Initialize(player);
        enemy.StartAI();
    }
}
