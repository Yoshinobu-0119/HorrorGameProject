using UnityEngine;
using UnityEngine.Events;

public class TriggerInteractable : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("操作設定")]
    [Tooltip("一度きりしか発動しないギミックにする場合はチェック")]
    public bool oneTimeOnly = false;

    [Header("お金・スコア設定（0なら無料）")]
    [Tooltip("Playerスクリプトが付いているプレイヤーオブジェクトをアサイン（未設定なら触れた相手から自動取得）")]
    public Player player;
    [Tooltip("発動に必要な最低スコア（この値以上なら発動）")]
    public int requiredScore = 0;

    [Header("実行するギミック（発動・支払い成功時）")]
    public UnityEvent onInteract;

    [Header("お金が足りない時のイベント（任意）")]
    public UnityEvent onNotEnoughMoney;

    private bool hasInteracted = false;

    private void OnTriggerEnter(Collider other)
    {
        // Playerタグ以外、または「1回きり設定」で既に発動済みの場合は何もしない
        if (!other.CompareTag("Player") || (oneTimeOnly && hasInteracted))
        {
            return;
        }

        // InspectorでPlayerを入れ忘れても、触れた相手から自動取得する保険
        if (player == null)
        {
            player = other.GetComponentInParent<Player>();
        }

        // 必要スコアが1以上設定されている場合は判定を行う
        if (requiredScore > 0)
        {
            if (player == null)
            {
                Debug.LogWarning("TriggerInteractableObjectにPlayerが設定されていません！");
                return;
            }

            // お金が足りなければ失敗イベントを呼んで中断
            if (player.goldscore < requiredScore)
            {
                onNotEnoughMoney.Invoke();
                return;
            }
        }

        // 無料、または支払いに成功した場合のみ実行
        hasInteracted = true;
        onInteract.Invoke();
    }
}
