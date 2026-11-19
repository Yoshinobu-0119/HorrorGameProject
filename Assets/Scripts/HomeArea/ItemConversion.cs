using System.Xml.Schema;
using UnityEngine;
public class ItemConversion : MonoBehaviour
{
    [Header("スコア加算先のPlayer")]
    [SerializeField] private Player player;

    private void Start()
    {
        // Inspectorで設定し忘れていても、シーン内から自動でPlayerを探す保険
        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
        }
    }
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<DropItem>() != null)
        {
            int value = 0;

            int bv = collision.gameObject.GetComponent<DropItem>().itemData.baseSellValue ;
            int av = collision.gameObject.GetComponent<DropItem>().amount ;

            value = bv * av;

            player.PlusScore(value);

            Destroy(collision.gameObject);
        }

    }
}
