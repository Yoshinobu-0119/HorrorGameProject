using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyBrain))]
[RequireComponent(typeof(EnemySensor))]
[RequireComponent(typeof(NavMeshEnemyMovement))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAIController : MonoBehaviour
{
    private EnemyBrain brain;
    private EnemySensor sensor;
    private NavMeshEnemyMovement movement;
    private NavMeshAgent agent;

    // s“®“o˜^—pƒCƒ“ƒ^[ƒtƒF[ƒX
    private IEnemyActionRegistrar actionRegistrar;
    public bool IsInitialized { get; private set; }

    private void Awake()
    {
        brain = GetComponent<EnemyBrain>();
        sensor = GetComponent<EnemySensor>();
        movement = GetComponent<NavMeshEnemyMovement>();
        agent = GetComponent<NavMeshAgent>();

        //“G‚Ìí—Ş–â‚í‚¸s“®“o˜^—pƒNƒ‰ƒX‚ğæ“¾
        actionRegistrar =
            GetComponent<IEnemyActionRegistrar>();
    }

    public void Initialize(Transform player)
    {
        //“ñd‰Šú‰»‚ğ’â~
        if (IsInitialized) { return; }

        if (player == null)
        {
            Debug.LogWarning(
                "EnemyAIController: Player‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ"
            );
            return;
        }

        if (actionRegistrar == null)
        {
            Debug.LogError("s“®“o˜^ƒNƒ‰ƒX‚ª‘¶İ‚µ‚Ü‚¹‚ñ");
            return;
        }

        //ƒvƒŒƒCƒ„[QÆ‚ğİ’è
        sensor.SetPlayer(player);

        //“GŒÅ—L‚Ìs“®‚ğ“o˜^
        actionRegistrar.RegisterActions(player);
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

        if (!agent.isOnNavMesh@|| !agent.isActiveAndEnabled)
        {
            Debug.LogWarning(
                "“G‚ªNavMeshã‚É”z’u‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ"
            );
            return;
        }

        sensor.enabled = true;
        brain.StartBrain();
        Debug.Log("“GAI‹N“®");
    }

    public void StopAI()
    {
        brain.StopBrain();

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            movement.Stop();
        }

        //’â~’†‚Íõ“G‚à’â~
        Debug.Log("“GAI’â~");
    }
}
