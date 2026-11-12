using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform rotateTransform;
    [Space(10)]

    [SerializeField] private Vector3 targetVec;
    [SerializeField] private float visibleRange = 30f;

    [Space(20)]
    [SerializeField] private Vector3 thisPos;
    [SerializeField] private Vector3 targetPos;
    void Start()
    {
        if (targetTransform == null)
        {
            //とりあえずメインカメラを指定
            targetTransform = Camera.main.transform;
        }

    }

    void Update()
    {
        //座標取得
        thisPos = transform.position;
        targetPos = targetTransform.transform.position;

        targetVec = targetPos - thisPos;

        rotateTransform.rotation = Quaternion.LookRotation(targetVec);
    }
}
