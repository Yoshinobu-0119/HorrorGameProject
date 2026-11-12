using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour
{
    [Header("UI設定")]
    [Tooltip("画面に表示する「Fキーを押す」のUIオブジェクト")]
    public GameObject promptUI;

    [Header("操作設定")]
    [Tooltip("一度きりしか使えないスイッチにする場合はチェック")]
    public bool oneTimeOnly = false;

    [Tooltip("プレイヤーの視線（カメラ正面）が向いている時だけ反応させるか")]
    public bool requireLookAt = false;
    public float lookAngleThreshold = 35f; // 視線の許容角度

    [Header("実行するギミック")]
    public UnityEvent onInteract;

    private bool isPlayerInRange = false;
    private bool hasInteracted = false;
    private Transform playerCamera;

    void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        if (Camera.main != null)
            playerCamera = Camera.main.transform;
    }

    void Update()
    {
        // 範囲内にいない、または一度きり設定で使用済みの場合は何もしない
        if (!isPlayerInRange || (oneTimeOnly && hasInteracted))
        {
            SetPromptActive(false);
            return;
        }

        // 視線判定が有効な場合、カメラがオブジェクトの方向を向いているか確認
        bool canInteract = true;
        if (requireLookAt && playerCamera != null)
        {
            Vector3 dirToObj = (transform.position - playerCamera.position).normalized;
            float angle = Vector3.Angle(playerCamera.forward, dirToObj);
            if (angle > lookAngleThreshold)
            {
                canInteract = false;
            }
        }

        // UIの表示切り替え
        SetPromptActive(canInteract);

        // Fキーが押されたらイベント実行
        if (canInteract && Input.GetKeyDown(KeyCode.F))
        {
            hasInteracted = true;
            if (oneTimeOnly) SetPromptActive(false);

            // 登録されたギミックをすべて実行
            onInteract.Invoke();
        }
    }

    private void SetPromptActive(bool isActive)
    {
        if (promptUI != null && promptUI.activeSelf != isActive)
        {
            promptUI.SetActive(isActive);
        }
    }

    // プレイヤーが範囲に入った時
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    // プレイヤーが範囲から出た時
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            SetPromptActive(false);
        }
    }
}