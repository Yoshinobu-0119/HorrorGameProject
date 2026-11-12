using UnityEngine;

public class SampleGimick : MonoBehaviour
{
    [Header("動きの設定")]
    public Vector3 moveOffset = Vector3.zero;     // 移動量（例: Yに3上げると上に開く扉）
    public Vector3 rotateOffset = new Vector3(0, 0, 0); // 回転量（例: Yに90で開き戸、Xに45でレバー）
    public float speed = 3.0f;

    private Vector3 initialPos;
    private Quaternion initialRot;
    private Vector3 targetPos;
    private Quaternion targetRot;
    private bool isActivated = false;

    void Start()
    {
        initialPos = transform.localPosition;
        initialRot = transform.localRotation;
        targetPos = initialPos;
        targetRot = initialRot;
    }

    void Update()
    {
        // 目標の位置・角度へ滑らかに移動させる
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * speed);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * speed);
    }

    // UnityEventから呼び出す関数（押すたびに開閉・ON/OFFが切り替わる）
    public void ToggleGimmick()
    {
        isActivated = !isActivated;
        if (isActivated)
        {
            targetPos = initialPos + moveOffset;
            targetRot = initialRot * Quaternion.Euler(rotateOffset);
        }
        else
        {
            targetPos = initialPos;
            targetRot = initialRot;
        }
    }
}
