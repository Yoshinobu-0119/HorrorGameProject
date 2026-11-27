using UnityEngine;

[System.Serializable]
public class EnemySpawnSetting
{
    //生成する敵プレファブ
    public GameObject enemyPrefab;
    //生成する敵の数
    [Min(0)]
    public int spawnCount = 1;
}
