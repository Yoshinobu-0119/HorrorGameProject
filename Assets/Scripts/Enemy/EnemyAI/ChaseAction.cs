using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class ChaseAction : IEnemyAction
{
    public int Priority => 80;

    private IEnemyMovement movement;
    private EnemySensor sensor;
    private Transform player;

    private float updateTimer;
    private const float UpdateInterval = 0.2f;

    public ChaseAction(IEnemyMovement movement,
            EnemySensor sensor,
            Transform player)
    {
        this.movement = movement;
        this.sensor = sensor;
        this.player = player;
    }

    public bool CanExecute()
    {
        return player != null && sensor.CanSeePlayer;
    }

    public void Enter()
    {
        Debug.Log("í«ê’äJén");

        updateTimer = 0f;
    }

    public void Execute()
    {
        if (player == null) { return; }

        updateTimer -= Time.deltaTime;

        if (updateTimer <= 0f)
        {
            updateTimer = UpdateInterval;

            movement.MoveTo(player.position);
        }
    }

    public void Exit()
    {
        Debug.Log("í«ê’èIóπ");
        movement.Stop();
    }
}
