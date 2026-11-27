using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("生成設定")]
    [SerializeField]
    private List<EnemySpawnSetting> enemySettings
    = new List<EnemySpawnSetting>();
    [SerializeField] private Transform player;

    [Header("安全確認")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private int maxAttempts = 50;
    [SerializeField] private float sampleDistance = 0.5f;
    [SerializeField] private float minPlayerDistance = 5f;
    [SerializeField] private float safetyMargin = 0.1f;

    //外部から敵の生産を開始する     
    public void SpawnEnemies()
    {
        // プレイヤー参照の確認
        if (player == null)
        {
            return;
        }

        // 敵の生成設定を確認
        if (enemySettings == null ||
            enemySettings.Count == 0)
        {
            return;
        }

        // NavMeshの情報を取得
        NavMeshTriangulation navMesh =
            NavMesh.CalculateTriangulation();

        if (navMesh.indices.Length < 3)
        {
            return;
        }

        // 敵の種類ごとに生成処理を行う
        foreach (EnemySpawnSetting setting in enemySettings)
        {
            if (setting == null)
                continue;

            // 生成数が0以下ならスキップ
            if (setting.spawnCount <= 0)
                continue;

            // Prefabが設定されていなければスキップ
            if (setting.enemyPrefab == null)
            {
                continue;
            }

            // 指定された数だけ敵を生成
            for (int i = 0; i < setting.spawnCount; i++)
            {
                SpawnEnemy(setting.enemyPrefab, navMesh);
            }
        }
    }

    private void SpawnEnemy(
        GameObject prefab,
        NavMeshTriangulation navMesh)
    {
        CapsuleCollider capsule =
           prefab.GetComponent<CapsuleCollider>();

        NavMeshAgent prefabAgent =
            prefab.GetComponent<NavMeshAgent>();

        if (capsule == null || prefabAgent == null ||
            prefab.GetComponent<EnemyAIController>() == null)
        {
            return;
        }

        // 今回はY軸方向のカプセルを使用
        if (capsule.direction != 1)
        {
            return;
        }

        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = prefabAgent.agentTypeID,
            areaMask = prefabAgent.areaMask
        };

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 candidate =
                GetRandomNavMeshPoint(navMesh);
            // 敵が利用可能なNavMesh上か確認
            if (!NavMesh.SamplePosition(
                candidate,
                out NavMeshHit hit,
                sampleDistance,
                filter))
            {
                continue;
            }

            Vector3 spawnPosition = hit.position;

            // プレイヤーとの距離を確認
            if (Vector3.Distance(
                spawnPosition,
                player.position) < minPlayerDistance)
            {
                continue;
            }

            // 壁や障害物との重なりを確認
            if (!IsSafePosition(spawnPosition, capsule, prefabAgent))
            {
                continue;
            }

            // 安全な位置に敵を生成
            GameObject enemy = Instantiate(
                prefab,
                spawnPosition,
                Quaternion.identity
            );

            EnemyAIController controller =
               enemy.GetComponent<EnemyAIController>();
            NavMeshAgent agent =
                enemy.GetComponent<NavMeshAgent>();

            // 生成後もNavMesh上にいるか確認
            if (!agent.isOnNavMesh)
            {
                Destroy(enemy);
                continue;
            }

            // 敵AIの初期化
            controller.Initialize(player);

            if (!controller.IsInitialized)
            {
                Destroy(enemy);
                return;
            }

            // 敵AIの起動
            controller.StartAI();

            // 次の敵の生成時に衝突判定へ反映
            Physics.SyncTransforms();
            return;
        }
    }

    private Vector3 GetRandomNavMeshPoint(
        NavMeshTriangulation navMesh)
    {
        int triangleIndex = Random.Range(
            0,
            navMesh.indices.Length / 3
        ) * 3;

        Vector3 a = navMesh.vertices[
            navMesh.indices[triangleIndex]];
        Vector3 b = navMesh.vertices[
            navMesh.indices[triangleIndex + 1]];
        Vector3 c = navMesh.vertices[
            navMesh.indices[triangleIndex + 2]];

        float u = Random.value;
        float v = Random.value;

        if (u + v > 1f)
        {
            u = 1f - u;
            v = 1f - v;
        }

        return a + (b - a) * u + (c - a) * v;
    }

    // 敵の大きさを考慮した衝突確認
    private bool IsSafePosition(
        Vector3 position,
        CapsuleCollider capsule,
        NavMeshAgent agent)
    {
        Vector3 scale = capsule.transform.lossyScale;

        float radius = capsule.radius *
            Mathf.Max(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.z)
            );

        float height = Mathf.Max(
            capsule.height * Mathf.Abs(scale.y),
            radius * 2f
        );

        Vector3 center = position +
            Vector3.up * agent.baseOffset +
            Vector3.Scale(capsule.center, scale);

        float halfSegment =
            Mathf.Max(0f, height / 2f - radius);
        Vector3 top =
            center + Vector3.up * halfSegment;
        Vector3 bottom =
            center - Vector3.up * halfSegment;

        bool blocked = Physics.CheckCapsule(
            top,
            bottom,
            radius + safetyMargin,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        return !blocked;
    }
}
