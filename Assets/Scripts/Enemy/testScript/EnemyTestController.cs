

using UnityEngine;

[RequireComponent(typeof(EnemyBrain))]
[RequireComponent(typeof(EnemySensor))]
[RequireComponent(typeof(NavMeshEnemyMovement))]
public class EnemyTestController : MonoBehaviour
{
    [SerializeField] private Transform player;

    [Header("„‰ñİ’è")]
    [SerializeField] private float patrolRadius = 10f;
    [SerializeField] private float waitTime = 2f;

    private void Start()
    {
        EnemyBrain brain = GetComponent<EnemyBrain>();
        EnemySensor sensor = GetComponent<EnemySensor>();

        IEnemyMovement movement =
            GetComponent<NavMeshEnemyMovement>();

        // „‰ñs“®
        PatrolAction patrol = new PatrolAction(
            movement,
            patrolRadius,
            waitTime
        );

        // ’ÇÕs“®
        ChaseAction chase = new ChaseAction(
            movement,
            sensor,
            player
        );

        // s“®‚ğ“o˜^
        brain.RegisterAction(patrol);
        brain.RegisterAction(chase);
    }
}
