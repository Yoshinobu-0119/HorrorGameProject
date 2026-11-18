using UnityEngine;

public class SearchAction : IEnemyAction
{
    public int Priority => 60;

    private readonly IEnemyMovement movement;
    private readonly EnemySensor sensor;
    private readonly float searchDuration;

    private bool isSearching;
    private bool hasArrived;
    private float searchTimer;

    //最後に目撃された目撃番号
    private int handledDetectionVersion;

    public SearchAction(
        IEnemyMovement movement,
        EnemySensor sensor,
        float searchDuration = 5f)
    {
        this.movement = movement;
        this.sensor = sensor;
        this.searchDuration = searchDuration;
    }

    public bool CanExecute()
    {
        //プレイヤーが見えている場合は追跡を優先
        if (sensor.CanSeePlayer) { return false; }

        //捜索中なら継続
        if (isSearching) { return true; }

        //新しい目撃情報がある場合のみ捜索
        return sensor.HasSeenPlayer &&
            sensor.DetectionVersion > handledDetectionVersion;
    }

    public void Enter()
    {
        Debug.Log("捜索開始");

        isSearching = true;
        hasArrived = false;
        searchTimer = searchDuration;

        handledDetectionVersion = sensor.DetectionVersion;

        movement.MoveTo(sensor.LastKnownPosition);
    }

    public void Execute()
    {
        if (!isSearching) { return; }

        //最後にプレイヤーを見た位置へ到達した場合
        if (!hasArrived &&
            movement.HasReachedDestination())
        {
            hasArrived = true;

            movement.Stop();

            Debug.Log("目撃地点に到達、周囲を探索");
        }

        //到達後、一定時間待機
        if (hasArrived)
        {
            searchTimer -= Time.deltaTime;

            if (searchTimer <= 0f)
            {
                Debug.Log("捜索終了");

                isSearching = false;
            }
        }
    }

    public void Exit()
    {
        movement.Stop();

        isSearching = false;
        hasArrived = false;
    }
}
