using UnityEngine;

public class EnemySensor : MonoBehaviour
{
    [Header("プレイヤー")]
    [SerializeField] private Transform player;

    [Header("敵の視野")]
    [SerializeField] private float viewDistance = 5f;
    [SerializeField, Range(0, 360f)] private float viewAngle = 60f;
    [SerializeField] private float eyeHeight = 1.5f;
    [SerializeField] private float targetHeight = 1f;

    [Header("遮蔽物")]
    [SerializeField] private LayerMask obstacleMask;


    //プレイヤーを視認しているか
    public bool CanSeePlayer { get; private set; }

    //最後にプレイヤーを見た位置
    public Vector3 LastKnownPosition { get; private set; }

    //過去にプレイヤーを発見したか
    public bool HasSeenPlayer { get; private set; }

    //今回の視認開始を認識する番号
    public int DetectionVersion { get; private set; }

    private void Update()
    {
        bool wasVisible = CanSeePlayer;

        CanSeePlayer = CheckPlayerVisibility();

        if (CanSeePlayer)
        {
            LastKnownPosition = player.position;
            HasSeenPlayer = true;

            if (!wasVisible)
            {
                DetectionVersion++;
            }
        }
    }

    private bool CheckPlayerVisibility()
    {
        if (player == null) { return false; }

        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPosition = player.position + Vector3.up * targetHeight;
        Vector3 Direction = targetPosition - eyePosition;

        //距離判定
        if (Direction.magnitude > viewDistance) { return false; }
        //視野角判定
        float angle = Vector3.Angle(transform.forward, Direction);
        if (angle < viewAngle / 2f) { return false; }
        //遮蔽物判定
        if (Physics.Raycast(eyePosition, Direction.normalized,
            Direction.magnitude, obstacleMask))
        {
            return false;
        }
        return true;
    }

    // Sceneビューで視野を確認
    private void OnDrawGizmosSelected()
    {
        Vector3 eyePosition =
            transform.position + Vector3.up * eyeHeight;

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            eyePosition,
            viewDistance
        );

        Vector3 leftDirection =
            Quaternion.Euler(0, -viewAngle / 2f, 0)
            * transform.forward;

        Vector3 rightDirection =
            Quaternion.Euler(0, viewAngle / 2f, 0)
            * transform.forward;

        Gizmos.DrawRay(
            eyePosition,
            leftDirection * viewDistance
        );

        Gizmos.DrawRay(
            eyePosition,
            rightDirection * viewDistance
        );
    }
}
