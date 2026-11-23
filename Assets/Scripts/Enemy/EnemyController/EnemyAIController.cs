using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyBrain))]
[RequireComponent(typeof(EnemySensor))]
[RequireComponent(typeof(NavMeshEnemyMovement))]
public class EnemyAIController : MonoBehaviour
{
    private EnemyBrain brain;
    private EnemySensor sensor;
    private NavMeshEnemyMovement movement;
    private NavMeshAgent agent;

    public bool IsInitialized { get; private set; }
    private void Awake()
    {
        brain = GetComponent<EnemyBrain>();
        sensor = GetComponent<EnemySensor>();
        movement = GetComponent<NavMeshEnemyMovement>();
        agent = GetComponent<NavMeshAgent>();
    }

    public void Initialize(Transform player)
    {

        if (player == null)
        {
            Debug.LogWarning(
                "EnemyAIController: Player‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ"
            );
            return;
        }

        sensor.SetPlayer(player);

        GetComponent<EnemyTestController>().RegisterActions(player);
        IsInitialized = true;

        Debug.Log("“GAI‰Šú‰»Š®—¹");
    }

    public void StartAI()
    {
        if (!IsInitialized)
        {
            Debug.LogWarning(
                "“GAI‚ª‰Šú‰»‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ"
            );
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning(
                "“G‚ªNavMeshã‚É”z’u‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ"
            );
            return;
        }

        brain.StartBrain();

        Debug.Log("“GAI‹N“®");
    }

    public void StopAI()
    {
        brain.StopBrain();
        movement.Stop();

        Debug.Log("“GAI’â~");
    }
}
