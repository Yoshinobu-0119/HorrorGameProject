using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshEnemyMovement : MonoBehaviour,IEnemyMovement
{
    private NavMeshAgent agent;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void MoveTo(Vector3 position)
    {
        if (!agent.isOnNavMesh) { return; }

        agent.isStopped = false;
        agent.SetDestination(position);
    }

    public void Stop()
    {
        if (!agent.isOnNavMesh) { return; }

        agent.isStopped = true;
        agent.ResetPath();
    }

    public bool HasReachedDestination()
    {
        if (!agent.isOnNavMesh) { return false; }
        if (agent.pathPending) { return false; }
        if (!agent.hasPath) { return false; }
        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
        { return false; }

        return agent.remainingDistance
            <= agent.stoppingDistance;
    }

    public bool TryGetRandomPoint(float radius, out Vector3 point)
    {
        point = transform.position;

        if (!agent.isOnNavMesh) { return false; }

        for (int i = 0; i < 15 ; i++)
        {
            Vector3 randomPosition =
                transform.position +
                Random.insideUnitSphere * radius;

            if (NavMesh.SamplePosition(randomPosition,
                out NavMeshHit hit,
                2f,
                agent.areaMask))
            {
                NavMeshPath path = new NavMeshPath();

                if (agent.CalculatePath(hit.position, path)
                    && path.status == NavMeshPathStatus.PathComplete)
                {
                    point = hit.position;
                    return true;
                }
            }
        }
        return false;
    }
}
