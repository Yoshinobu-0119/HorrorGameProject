using UnityEngine;

public class CannonController : MonoBehaviour
{
    [Header("発射設定")]
    [Tooltip("発射する弾のPrefab（Rigidbody必須）")]
    [SerializeField] private Rigidbody projectilePrefab;

    [Tooltip("弾を生成する発射口の位置と向き（未指定の場合はこのオブジェクト自身）")]
    [SerializeField] private Transform firePoint;

    [Header("パラメータ設定")]
    [Tooltip("発射する勢い（初速）")]
    [SerializeField] private float launchForce = 30f;

    [Tooltip("連続発射の間隔（秒）")]
    [SerializeField] private float fireInterval = 1.0f;

    [Tooltip("自動発射を有効にするか")]
    [SerializeField] private bool autoFire = true;

    private float _fireTimer;

    private void Awake()
    {
        // firePointが未設定なら自分自身のTransformを使う
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    private void Update()
    {
        if (!autoFire) return;

        _fireTimer += Time.deltaTime;
        if (_fireTimer >= fireInterval)
        {
            Fire();
            _fireTimer = 0f;
        }
    }

    /// <summary>
    /// 弾を1発発射する（外部スクリプトやUIボタンからも呼び出し可能）
    /// </summary>
    public void Fire()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("Projectile Prefab が設定されていません。", this);
            return;
        }

        // firePointの位置とRotationで弾を生成
        Rigidbody projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Unity 6 の新API (linearVelocity) で初速を直接与える
        // ※質量(Mass)の影響を受けずに一定の勢いで飛ばすため、AddForceではなくlinearVelocityを使用
        projectile.linearVelocity = firePoint.forward * launchForce;
    }

    // Sceneビューで発射方向を視覚的に確認するためのギズモ
    private void OnDrawGizmosSelected()
    {
        Transform target = firePoint != null ? firePoint : transform;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(target.position, target.forward * 3f);
    }
}