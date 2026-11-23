

using UnityEngine;

[RequireComponent(typeof(EnemyBrain))]
[RequireComponent(typeof(EnemySensor))]
[RequireComponent(typeof(NavMeshEnemyMovement))]
public class NormalEnemyActions : MonoBehaviour
{
    [Header("èÑâÒê›íË")]
    [SerializeField] private float patrolRadius = 10f;
    [SerializeField] private float waitTime = 1f;

    [Header("ë{çıê›íË")]
    [SerializeField] private float searchDuration = 5f;

    private EnemyBrain brain;
    private EnemySensor sensor;
    private IEnemyMovement movement;

    private bool actionsRegistered;

    private void Awake()
    {
        brain = GetComponent<EnemyBrain>();
        sensor = GetComponent<EnemySensor>();
        movement = GetComponent<NavMeshEnemyMovement>();
    }

    public void RegisterActions(Transform player)
    {
        if (actionsRegistered)
            return;

        PatrolAction patrol = new PatrolAction(
            movement,
            patrolRadius,
            waitTime
        );

        SearchAction search = new SearchAction(
            movement,
            sensor,
            searchDuration
        );

        ChaseAction chase = new ChaseAction(
            movement,
            sensor,
            player
        );

        brain.RegisterAction(patrol);
        brain.RegisterAction(search);
        brain.RegisterAction(chase);

        actionsRegistered = true;

        Debug.Log("ìGÇÃçsìÆìoò^äÆóπ");
    }
}