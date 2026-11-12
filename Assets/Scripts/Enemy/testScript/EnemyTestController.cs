
using UnityEngine;

[RequireComponent(typeof(EnemyBrain))]
[RequireComponent(typeof(EnemySensor))]
public class EnemyTestController : MonoBehaviour
{
    private TestEnemyAction patrol;
    private TestEnemyAction chase;

    private EnemySensor sensor;

    private void Start()
    {
        EnemyBrain brain = GetComponent<EnemyBrain>();
        sensor = GetComponent<EnemySensor>();

        patrol = new TestEnemyAction("巡回", 10);
        chase = new TestEnemyAction("追跡", 80);

        brain.RegisterAction(patrol);
        brain.RegisterAction(chase);

        patrol.SetExecutable(true);
    }

    private void Update()
    {
        // プレイヤーの視認状態で追跡を切り替える
        chase.SetExecutable(sensor.CanSeePlayer);
    }
}
