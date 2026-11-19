using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshEnemyMovement : MonoBehaviour,IEnemyMovement
{
    [Header("スタック検知")]
    [SerializeField] private float stuckTimeout = 3f;
    [SerializeField] private float minimumMovement = 0.05f;

    [Header("スタック判定の精度")]
    [SerializeField] private float minimumDesiredSpeed = 0.1f;
    [SerializeField] private float arrivalTolerance = 0.1f;

    [Header("経路探索")]
    [SerializeField] private float pathTimeout = 3f;

    private NavMeshAgent agent;

    private float stuckTimer;
    private float pathTimer;
    private Vector3 lastPosition;

    //正常な移動が続いている時間
    private float healthyMovementTimer;

    //正常に移動しているか
    public bool IsMovingNormally { get; private set; }

    public EnemyMoveStatus Status { get; private set; } = EnemyMoveStatus.Idle;

    public float HealthyMovementTime => healthyMovementTimer;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (Status != EnemyMoveStatus.Moving) { return; }
        if (!agent.isOnNavMesh) 
        {
            SetFailed("NabMesh上に存在しません");
            return; 
        }

        //経路計算中
        if (agent.pathPending)
        {
            pathTimer += Time.deltaTime;

            if (pathTimer >= pathTimeout)
            {
                SetFailed("経路計算がタイムアウト");
            }
            return;
        }

        //経路が存在しない
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid) 
        {
            SetFailed("経路が無効");
            return; 
        }

        //完全な経路が見つからない
        if (agent.pathStatus == NavMeshPathStatus.PathPartial) 
        {
            SetFailed("目的地に到達できません");
            return; 
        }

        //目的地に到達
        if (agent.hasPath &&
            agent.remainingDistance <= agent.stoppingDistance &&
            !agent.pathPending)
        {
            Status = EnemyMoveStatus.Arrived;
            agent.isStopped = true;
            return;
        }

        //経路が消失した
        if (!agent.hasPath)
        {
            pathTimer += Time.deltaTime;

            if (pathTimer >= pathTimeout)
            {
                SetFailed("経路が見つかりません");
            }
            return;
        }

        pathTimer = 0f;

        //スタック検知
        float movedDistance = Vector3.Distance(
            transform.position,
            lastPosition
            );

    bool isNearDestination =
            agent.remainingDistance <=
            agent.stoppingDistance + arrivalTolerance;

        //agentが移動を要求しているか
        bool wantsToMove =
            agent.desiredVelocity.magnitude > minimumDesiredSpeed;

        //実際に移動できているか
        bool isActuallyMoving =
            agent.velocity.sqrMagnitude > 0.01f;

        //正常な移動状態
        IsMovingNormally =
            wantsToMove && isActuallyMoving && !isNearDestination;

        if (isNearDestination)
        {
            //到着直前の減速はスタック扱いしない
            stuckTimer = 0f;
            lastPosition = transform.position;
        }
        else if (movedDistance >= minimumMovement)
        {
            //実際に位置が変わっている
            stuckTimer = 0f;
            lastPosition = transform.position;
        }
        else if (wantsToMove)
        {
            //移動したいのに進めていない
            stuckTimer += Time.deltaTime;
        }
        else
        {
            //agentが移動を要求していない場合は
            //スタック判定を一時停止
            stuckTimer = 0f;
        }

        if (IsMovingNormally)
        {
            healthyMovementTimer += Time.deltaTime;
        }
        else
        {
            healthyMovementTimer = 0f;
        }

        if (stuckTimer > stuckTimeout)
        {
            SetFailed("移動が停止しています");
        }
    }

    public void MoveTo(Vector3 position)
    {
        if (!agent.isOnNavMesh) 
        {
            SetFailed("NabMesh上に存在しません");
            return; 
        }

        //新しい移動開始かどうかを判定
        bool isNewMovwment =
            Status != EnemyMoveStatus.Moving;

        agent.isStopped = false;

        if (!agent.SetDestination(position)) 
        {
            SetFailed("目的地を設定できません");
            return; 
        }

        Status = EnemyMoveStatus.Moving;
        //新しい移動の時だけ監視を初期化
        if(isNewMovwment)
        {
            ResetTimers();
        }
    }

    public void Stop()
    {
        if (agent.isOnNavMesh) 
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        Status = EnemyMoveStatus.Idle;
        ResetTimers();
    }

    public bool HasReachedDestination()
    {
        return Status == EnemyMoveStatus.Arrived;
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

    private void SetFailed(string reason)
    {
        if (Status == EnemyMoveStatus.Failed) { return; }

        Debug.LogWarning(
            $"[{gameObject.name}]移動失敗:{reason}"
            );

        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        Status = EnemyMoveStatus.Failed;
    }

    private void ResetTimers()
    {
        stuckTimer = 0f;
        pathTimer = 0f;
        healthyMovementTimer = 0f;

        IsMovingNormally = false;
        lastPosition = transform.position;
    }
}
