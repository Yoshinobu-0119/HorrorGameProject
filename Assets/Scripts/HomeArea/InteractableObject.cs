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

    [Header("お金・スコア設定（0なら無料）")]
    [Tooltip("Playerスクリプトが付いているプレイヤーオブジェクトをアサイン")]
    public Player player;
    [Tooltip("支払う金額（0の場合はお金を消費せずそのままギミック発動）")]
    public int cost = 0;

    [Header("実行するギミック（支払い成功時）")]
    public UnityEvent onInteract;

    [Header("お金が足りない時のイベント（任意）")]
    public UnityEvent onNotEnoughMoney;

    // どのオブジェクトが現在UIを表示しているかを共有して記録する
    private static InteractableObject currentActivePromptOwner = null;

    private bool isPlayerInRange = false;
    private bool hasInteracted = false;
    private Transform playerCamera;

    void Start()
    {
        currentActivePromptOwner = null;

        if (promptUI != null)
            promptUI.SetActive(false);

        if (Camera.main != null)
            playerCamera = Camera.main.transform;
    }

    void Update()
    {
        if (!isPlayerInRange || (oneTimeOnly && hasInteracted))
        {
            ReleasePrompt();
            return;
        }

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
            RequestPrompt();

            if (Input.GetKeyDown(KeyCode.F))
            {
                // 金額が1以上設定されている場合は支払い判定を行う
                if (cost > 0)
                {
                    if (player == null)
                    {
                        Debug.LogWarning("InteractableObjectにPlayerが設定されていません！");
                        return;
                    }

                    // お金が足りなければ失敗イベントを呼んで中断
                    if (!player.MinusScore(cost))
                    {
                        onNotEnoughMoney.Invoke();
                        return;
                    }
                }

                // 無料、または支払いに成功した場合のみ実行
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
        if (currentActivePromptOwner == null || currentActivePromptOwner == this)
        {
            currentActivePromptOwner = this;
            if (promptUI != null && !promptUI.activeSelf)
                promptUI.SetActive(true);
        }
    }

    private void ReleasePrompt()
    {
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
            // InspectorでPlayerを入れ忘れても、触れた相手から自動取得する保険
            if (player == null)
            {
                player = other.GetComponentInParent<Player>();
            }
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