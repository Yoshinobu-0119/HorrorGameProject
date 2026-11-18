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

    // どのオブジェクトが現在UIを表示しているかを共有して記録する
    private static InteractableObject currentActivePromptOwner = null;

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
        // 範囲外、または一度きり設定で使用済みの場合はUIの権利を手放す
        if (!isPlayerInRange || (oneTimeOnly && hasInteracted))
        {
            ReleasePrompt();
            return;
        }

        // 視線判定
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

        if (canInteract)
        {
            // UIの表示権を要求して表示する
            RequestPrompt();

            if (Input.GetKeyDown(KeyCode.F))
            {
                hasInteracted = true;
                if (oneTimeOnly) ReleasePrompt();

                onInteract.Invoke();
            }
        }
        else
        {
            ReleasePrompt();
        }
    }

    private void RequestPrompt()
    {
        // 誰もUIを使っていない、または自分がすでに使っている場合のみ表示
        if (currentActivePromptOwner == null || currentActivePromptOwner == this)
        {
            currentActivePromptOwner = this;
            if (promptUI != null && !promptUI.activeSelf)
                promptUI.SetActive(true);
        }
    }

    private void ReleasePrompt()
    {
        // 自分がUIを表示していた場合のみ非表示にして権利を手放す
        if (currentActivePromptOwner == this)
        {
            currentActivePromptOwner = null;
            if (promptUI != null && promptUI.activeSelf)
                promptUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            ReleasePrompt();
        }
    }
}