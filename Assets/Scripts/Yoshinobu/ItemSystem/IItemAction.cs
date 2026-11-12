using UnityEngine;

/// <summary>
/// アイテムごとに処理を分離、拡張可能にするクラス群
/// </summary>
public interface IItemAction
{
    string ActionName { get; }
    bool Execute(Player user, ItemInstance item);
}

[System.Serializable]
public class RangedAttackAction : IItemAction
{
    public string ActionName => "射撃";
    public int damage = 20;
    public float range = 50f;
    public GameObject bulletPrefab;
    public bool Execute(Player user, ItemInstance item)
    {
        user.PerformRangedAttack(damage, range, bulletPrefab);
        return true;
    }
}

[System.Serializable]
public class MeleeAttackAction : IItemAction
{
    public string ActionName => "近接攻撃";
    public int damage = 20;
    public float radius = 5f;
    string animTrigger = "NoName";  //仮。アニメーション追加時に変更

    public bool Execute(Player user, ItemInstance item)
    {
        user.PerformMeleeAttack(damage, radius, animTrigger);
        return true;
    }
}

[System.Serializable]
public class HealAction : IItemAction
{
    public string ActionName => "回復";
    public float amount = 20f;

    public bool Execute(Player user,ItemInstance item)
    {
        user.Heal(amount);
        return true;
    }
}
