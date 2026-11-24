using UnityEngine;
using UnityEngine.UI;

public class DrawMiniMapMarker : MonoBehaviour
{
    private Image goalMarker;

    //円の中心からフチまでの距離と、方向計算に使う参照。
    private Transform player;
    private const float markerRadius = 130f;
    private float mapScale;

    private void Start()
    {
        foreach (var gm in FindObjectsByType<Image>(FindObjectsSortMode.None))
        {
            if (gm.name == "GoalMarker")
            {
                goalMarker = gm;
            }
        }

        player = GameObject.Find("Player")?.transform;
        RectTransform mapImage = GameObject.Find("MapRawImage")?.GetComponent<RectTransform>();
        Camera miniMapCamera = player?.Find("MiniMapCamera")?.GetComponent<Camera>();

        //倍率とフチまでの距離を一度だけ計算する。
        float mapWidth = mapImage.rect.width;
        mapScale = mapWidth / (2f * miniMapCamera.orthographicSize);
    }

    void Update()
    {
        //ゴールの位置をプレイヤー基準に変換し、XZに変換してマップ上の座標に変換する
        Vector3 localGoal = player.InverseTransformPoint(transform.position);
        Vector2 mapPosition = new Vector2(localGoal.x, localGoal.z) * mapScale;

        //円の外に出る場合だけ、同じ方向のフチまで縮める。
        goalMarker.rectTransform.anchoredPosition =
            Vector2.ClampMagnitude(mapPosition, markerRadius);
    }
}
