using UnityEngine;

public class EnemySpawnTester : MonoBehaviour
{
    [SerializeField] private EnemySpawner spawner;

    private void Update()
    {
        // PƒL[‚Å“G‚ğ¶¬
        if (Input.GetKeyDown(KeyCode.P))
        {
            spawner.SpawnEnemies();
        }
    }
}
