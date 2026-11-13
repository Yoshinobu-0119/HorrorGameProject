using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform rotateTransform;
    [Space(10)]
    [SerializeField] private float visibleRange = 30f;

    // 全Billboardでメインカメラを共有し、Start時の検索負荷を削減
    private static Transform _cachedMainCamera;

    private float _sqrVisibleRange;
    private GameObject _rotateObject;
    private bool _isVisible;

    private void Awake()
    {
        // 距離の2乗を事前計算（毎フレームの掛け算を省略）
        _sqrVisibleRange = visibleRange * visibleRange;

        if (rotateTransform != null)
        {
            _rotateObject = rotateTransform.gameObject;
            _isVisible = _rotateObject.activeSelf;
        }
    }

    private void Start()
    {
        if (targetTransform == null)
        {
            if (_cachedMainCamera == null && Camera.main != null)
            {
                _cachedMainCamera = Camera.main.transform;
            }
            targetTransform = _cachedMainCamera;
        }
    }

    private void Update()
    {
        if (targetTransform == null || _rotateObject == null) return;

        // 方向ベクトルを先に計算（1フレームのズレを解消）
        Vector3 targetVec = targetTransform.position - transform.position;

        // Distance(平方根計算)ではなく、2乗のまま比較して高速化
        if (targetVec.sqrMagnitude < _sqrVisibleRange)
        {
            // 非表示だった時だけSetActiveを呼ぶ
            if (!_isVisible)
            {
                _rotateObject.SetActive(true);
                _isVisible = true;
            }

            // ゼロベクトル時の警告防止
            if (targetVec != Vector3.zero)
            {
                rotateTransform.rotation = Quaternion.LookRotation(targetVec);
            }
        }
        else
        {
            // 表示中だった時だけSetActiveを呼ぶ
            if (_isVisible)
            {
                _rotateObject.SetActive(false);
                _isVisible = false;
            }
        }
    }

    // インスペクターでvisibleRangeを変更した際に2乗値も更新する
    private void OnValidate()
    {
        _sqrVisibleRange = visibleRange * visibleRange;
    }
}