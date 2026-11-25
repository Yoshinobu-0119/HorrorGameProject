using UnityEngine;

public class MiniMapFloorController : MonoBehaviour
{
    [SerializeField] private float floorHeight = 7f;

    private Camera miniMapCamera;
    private CharacterController player;

    private bool hasStartFloor;
    private float startFloorY;
    private int currentFloor;

    private void Start()
    {
        miniMapCamera = GetComponent<Camera>();
        player = GetComponentInParent<CharacterController>();

        //床の1層分上から、床の上下半層ずつを映す
        float halfFloorHeight = floorHeight * 0.5f;
        //カメラのnearClipPlaneとfarClipPlaneを床の上下半層に設定することで、
        //カメラの高さを変えずに、床の上下半層を映すことができる
        miniMapCamera.nearClipPlane = halfFloorHeight;
        miniMapCamera.farClipPlane = floorHeight + halfFloorHeight;
    }

    private void LateUpdate()
    {
        if (player.isGrounded)
        {
            //キャラコンでプレイヤーの足元の高さを取得する
            float feetY = player.bounds.min.y;

            //最初に接地した足元の高さを0層の基準にする
            if (!hasStartFloor)
            {
                startFloorY = feetY;
                hasStartFloor = true;
            }

            //現在の階層を計算する
            currentFloor = Mathf.RoundToInt((feetY - startFloorY) / floorHeight);
        }

        if (!hasStartFloor)
        {
            return;
        }

        //カメラの高さを現在の階層に合わせる
        float currentFloorY = startFloorY + currentFloor * floorHeight;
        transform.position = new Vector3(
            transform.position.x, currentFloorY + floorHeight, transform.position.z);
    }
}