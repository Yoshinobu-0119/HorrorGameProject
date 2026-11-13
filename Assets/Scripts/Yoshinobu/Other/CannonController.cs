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

    [Header("デバッグ設定")]
    [Tooltip("画面右下に発射数とFPSを表示するか")]
    [SerializeField] private bool showDebugInfo = true;

    private float _fireTimer;
    private int _firedCount = 0;

    // FPS計測用
    private float _fpsTimer;
    private int _frameCount;
    private float _currentFps;

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
        // FPSの計測（0.5秒ごとに更新して数値を読みやすくする）
        if (showDebugInfo)
        {
            _fpsTimer += Time.unscaledDeltaTime;
            _frameCount++;
            if (_fpsTimer >= 0.5f)
            {
                _currentFps = _frameCount / _fpsTimer;
                _fpsTimer = 0f;
                _frameCount = 0;
            }
        }

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

        // 発射カウントを増やす
        _firedCount++;
    }

    // デバッグ用の画面表示（UIオブジェクト不要）
    private void OnGUI()
    {
        if (!showDebugInfo) return;

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            alignment = TextAnchor.LowerRight
        };
        style.normal.textColor = Color.white;

        // 2行分表示できるよう高さを確保
        float width = 300f;
        float height = 80f;
        float margin = 20f;
        Rect rect = new Rect(
            Screen.width - width - margin,
            Screen.height - height - margin,
            width,
            height
        );

        // 小数点第1位までFPSを表示し、その下に発射数を表示
        GUI.Label(rect, $"FPS: {_currentFps:F1}\n発射数: {_firedCount}", style);
    }

    // Sceneビューで発射方向を視覚的に確認するためのギズモ
    private void OnDrawGizmosSelected()
    {
        Transform target = firePoint != null ? firePoint : transform;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(target.position, target.forward * 3f);
    }
}