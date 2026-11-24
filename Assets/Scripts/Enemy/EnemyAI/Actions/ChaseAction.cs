using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class ChaseAction : IEnemyAction
{
    public int Priority => 80;

    private IEnemyMovement movement;
    private EnemySensor sensor;
    private Transform player;

    private float updateTimer;
    private float retryTimer;
    private int retryCount;
    private bool isRecovering;

    private const float UpdateInterval = 0.2f;
    private const float RetryDelay = 1f;
    private const int MaxRetries = 3;
    private const float RetryResetTime = 2f;

    private readonly NavMeshEnemyMovement navMovement;

    public ChaseAction(IEnemyMovement movement,
            EnemySensor sensor,
            Transform player)
    {
        this.movement = movement;
        this.sensor = sensor;
        this.player = player;
        navMovement = movement as NavMeshEnemyMovement;
    }

    public bool CanExecute()
    {
        return player != null && sensor.CanSeePlayer;
    }

    public void Enter()
    {
        Debug.Log("追跡開始");

        updateTimer = 0f;
        retryTimer = 0f;
        retryCount = 0;
        isRecovering = false;
    }

    public void Execute()
    {
        if (player == null) { return; }

        //一定時間正常に移動出来たら再試行回数リセット
        if (navMovement != null &&
             retryCount > 0 &&
             navMovement.HealthyMovementTime >= RetryResetTime)
        {
            Debug.Log("追跡が安定したため再試行回数をリセット");
            retryCount = 0;
        }

        //移動失敗を検知
        if (movement.Status == EnemyMoveStatus.Failed &&
            !isRecovering)
        {
            retryCount++;

            if (retryCount > MaxRetries)
            {
                Debug.LogWarning("追跡の再試行回数を超過しました");

                isRecovering = true;
                retryTimer = RetryDelay * 3f;
            }
            else
            {
                Debug.LogWarning(
                    $"追跡の再試行 {retryCount}/{MaxRetries}");

                isRecovering = true;
                retryTimer = RetryDelay;
            }
        }

        //復帰待機中
        if (isRecovering)
        {
            retryTimer -= Time.deltaTime;

            if (retryTimer > 0f) { return; }

            isRecovering = false;

            //再執行
            movement.MoveTo(player.position);
            updateTimer = UpdateInterval;
            return;
        }

        //通常追跡
        updateTimer -= Time.deltaTime;

        if (updateTimer <= 0f)
        {
            updateTimer = UpdateInterval;
            movement.MoveTo(player.position);
        }
    }

    public void Exit()
    {
        Debug.Log("追跡終了");
        movement.Stop();

        isRecovering = false;
        retryCount = 0;
    }
}
