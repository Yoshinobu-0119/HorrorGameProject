using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテムのマスターデータ（IDやアイコンなどを格納）するクラス
/// </summary>
[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("アイテム表示関連")]
    [Tooltip("アイテムのID(使用するかは未定)\n型 : string")]
    public string id;
    [Tooltip("アイテム名\n型 : string")]
    public string itemName;
    [Tooltip("アイテムのアイコン画像\n型 : Sprite")]
    public Sprite icon;
    [Tooltip("アイテムの3Dモデル\n何も入れない場合はSpriteを使用する。\n型 : GameObject")]
    public GameObject itemModel;

    [Tooltip("3Dモデル表示時の基準位置からのオフセット座標\n型 : Vector3")]
    public Vector3 offsetPos = Vector3.zero;

    [Tooltip("3Dモデル表示時のスケール倍率（初期値: 1, 1, 1）\n型 : Vector3")]
    public Vector3 offsetScale = Vector3.one;

    [Tooltip("3Dモデル表示時の回転角度（オイラー角 X, Y, Z）\n型 : Vector3")]
    public Vector3 offsetRotation = Vector3.zero;
    [Space(10)]

    [Header("アイテムの仕様")]
    [Tooltip("ベースとなる売値\n型 : int")]
    public int baseSellValue;
    [Tooltip("最大スタック数\n型 : int")]
    public int maxStackSize = 1;
    [Tooltip("使用したときの効果音\n型 : AudioClip")]
    public AudioClip useSound;
    [Tooltip("アイテムの使用間隔\n型 : float")]
    public float executeDuration;

    [Space(10)]
    [Header("アイテムの効果")]
    [Tooltip("アイテムを使用したときに個数を減らすかどうか\n型 : bool")]
    public bool reduceAmountOnUse;

    // ポリモーフィズム対応（インスペクターでアクションを追加・変更可能）
    [SerializeReference] public List<IItemAction> actions = new();
}