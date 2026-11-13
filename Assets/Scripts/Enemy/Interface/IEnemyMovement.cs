using UnityEngine;

public interface IEnemyMovement
{
    //指定された位置へ移動を開始する
    void MoveTo(Vector3 position);

    //移動を停止する
    void Stop();

    //目的地へ到着したか確認する
    bool HasReachedDestination();

    //敵の周囲からランダムな位置を探す
    bool TryGetRandomPoint(float radius, out Vector3 point);
}
