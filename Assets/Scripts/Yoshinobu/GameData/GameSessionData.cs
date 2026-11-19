using UnityEngine;

/// <summary>
/// シーン遷移をまたいでプレイヤーのステータスやインベントリを保持するシングルトン
/// </summary>
public class GameSessionData : MonoBehaviour
{
    public static GameSessionData Instance { get; private set; }

    // 引き継ぐデータ群
    public bool HasSavedData { get; private set; } = false;
    public float MaxHp { get; private set; }
    public float CurrentHp { get; private set; }
    public int GoldScore { get; private set; }
    public ItemInstance[] SavedSlots { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Player の子オブジェクトとして配置されていても DontDestroyOnLoad が効くようにルート階層へ切り離す
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// シーン遷移前（または随時）に現在の状態を保存する
    /// </summary>
    public void SaveSession(float maxHp, float currentHp, int goldScore, ItemInstance[] currentSlots)
    {
        MaxHp = maxHp;
        CurrentHp = currentHp;
        GoldScore = goldScore;

        if (currentSlots != null)
        {
            SavedSlots = new ItemInstance[currentSlots.Length];
            for (int i = 0; i < currentSlots.Length; i++)
            {
                if (currentSlots[i] != null && currentSlots[i].data != null)
                {
                    // 参照切り離しのためディープコピーして保存
                    ItemInstance copy = new ItemInstance(currentSlots[i].data, currentSlots[i].amount);
                    copy.currentDurability = currentSlots[i].currentDurability;
                    SavedSlots[i] = copy;
                }
                else
                {
                    SavedSlots[i] = null;
                }
            }
        }

        HasSavedData = true;
    }

    /// <summary>
    /// ニューゲーム時やゲームオーバー時などにデータをリセットする
    /// </summary>
    public void ClearSession()
    {
        HasSavedData = false;
        SavedSlots = null;
    }
}