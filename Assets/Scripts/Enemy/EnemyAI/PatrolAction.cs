using UnityEngine;

public class PatrolAction : IEnemyAction
{
    public int Priority => 10;

    private IEnemyMovement movement;

    private float patrolRadius;
    private float waitTime;

    private float timer;
    private bool waiting;

    public PatrolAction(IEnemyMovement movement,
        float patrolRadius = 10f,
        float waitTime = 2f)
    {
        this.movement = movement;
        this.patrolRadius = patrolRadius;
        this.waitTime = waitTime;
    }

    public bool CanExecute()
    {
        //巡回は常に実行可能
        return true;
    }

    public void Enter()
    {
        Debug.Log("巡回開始");


        waiting = false;
        SetNextDestination();
    }

    public void Execute()
    {
        if (waiting)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                waiting = false;
                SetNextDestination();
            }
            return;
        }

        if (movement.HasReachedDestination())
        {
            movement.Stop();

            waiting = true;
            timer = waitTime;
        }
    }

    private void SetNextDestination()
    {
        if (movement.TryGetRandomPoint(
            patrolRadius, out Vector3 point))
        {
            movement.MoveTo(point);
        }
        else
        {
            //目的地が見つからなかったらやり直す
            waiting = true;
            timer = waitTime;
        }
    }

    public void Exit()
    {
        Debug.Log("巡回終了");
        movement.Stop();
    }

}
