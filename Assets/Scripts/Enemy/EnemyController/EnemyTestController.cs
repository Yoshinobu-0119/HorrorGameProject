

using UnityEngine;

[RequireComponent(typeof(EnemyBrain))]
[RequireComponent(typeof(EnemySensor))]
[RequireComponent(typeof(NavMeshEnemyMovement))]
public class EnemyTestController : MonoBehaviour
{
    [SerializeField] private Transform player;

    [Header("„‰ñİ’è")]
    [SerializeField] private float patrolRadius = 10f;
    [SerializeField] private float waitTime = 1f;

    [Header("‘{õİ’è")]
    [SerializeField] private float searchDuration = 5f;

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

        //‘{õs“®
        SearchAction search = new SearchAction(
            movement,
            sensor,
            searchDuration
        );

        // ’ÇÕs“®
        ChaseAction chase = new ChaseAction(
            movement,
            sensor,
            player
        );

        // s“®‚ğ“o˜^
        brain.RegisterAction(patrol);
        brain.RegisterAction(search);
        brain.RegisterAction(chase);
    }
}
