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
