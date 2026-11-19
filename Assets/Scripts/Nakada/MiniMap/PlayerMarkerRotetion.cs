using UnityEngine;

public class PlayerMarkerRotetion : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;

    private void Update()
    {
        //マーカーとプレイヤーの回転を同期させる
        transform.rotation = Quaternion.Euler(0, targetObject.transform.eulerAngles.y, 0);
    }
}
