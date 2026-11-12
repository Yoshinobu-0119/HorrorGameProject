using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("参照")]
    private Player player;
    private CharacterController characterController;
    private GameObject cam;

    [Header("移動パラメータ")]
    public float curSpeed;
    public float maxWalkSpeed = 7.5f;
    public float maxRunSpeed = 10.0f;
    public float gravity = 9.81f;
    public float jumpPow = 5.0f;
    public float acceleration = 20f;
    public float deceleration = 25f;
    private Vector3 moveVelocity;
    private float verticalVelocity;

    [Header("カメラ")]
    public Vector2 camDirection;
    public float rotateSpeed = 100f;

    [Header("入力")]
    [SerializeField] private Vector2 inputL;
    [SerializeField] private Vector2 inputR;

    [Header("トグル")]
    public bool lockMouse = true;

    [Header("インベントリ設定")]
    [SerializeField] private int selectedSlotIndex = 0; // 選択中スロット
    public int SelectedSlotIndex => selectedSlotIndex;

    private PlayerInput input;
    private InputAction sprint;
    private InputAction jump;

    void Start()
    {
        Application.targetFrameRate = 60;

        player = GetComponent<Player>();
        input = GetComponent<PlayerInput>();
        sprint = input.actions["Sprint"];
        jump = input.actions["Jump"];
        cam = Camera.main.gameObject;
        characterController = GetComponent<CharacterController>();

        if (lockMouse)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void Update()
    {
        PlayerMovement();

        Vector3 finalMove = moveVelocity;
        finalMove.y = verticalVelocity;
        characterController.Move(finalMove * Time.deltaTime);

        // マウスホイールによるスロット切り替え
        HandleScrollInput();
    }

    public void PlayerMovement()
    {
        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        bool isSprintPressed = sprint.IsPressed();
        curSpeed = isSprintPressed ? maxRunSpeed : maxWalkSpeed;
        Vector3 inputDir = (forward * inputL.y + right * inputL.x).normalized;
        float targetSpeed = inputL.magnitude > 0.1f ? curSpeed : 0f;

        Vector3 targetVelocity = inputDir * targetSpeed;
        float rate = (inputDir != Vector3.zero) ? acceleration : deceleration;
        moveVelocity = Vector3.MoveTowards(moveVelocity, targetVelocity, rate * Time.deltaTime);

        CalculateGravity();

        camDirection += inputR * rotateSpeed * Time.deltaTime;
        camDirection.y = Mathf.Clamp(camDirection.y, -80f, 80f);
        cam.transform.rotation = Quaternion.Euler(-camDirection.y, camDirection.x, 0.0f);
    }

    private void CalculateGravity()
    {
        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0) verticalVelocity = -2f;
            if (jump.WasPressedThisFrame()) verticalVelocity = Mathf.Sqrt(jumpPow * 2f * gravity);
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
    }

    // ホイールの回転を監視してスロットを変更
    private void HandleScrollInput()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0 && player != null && player.Inventory != null)
        {
            int maxSlots = player.Inventory.MaxSlots;
            if (maxSlots <= 0) return;

            if (scroll > 0)
            {
                // 上スクロール (前のスロット)
                selectedSlotIndex = (selectedSlotIndex - 1 + maxSlots) % maxSlots;
            }
            else if (scroll < 0)
            {
                // 下スクロール (次のスロット)
                selectedSlotIndex = (selectedSlotIndex + 1) % maxSlots;
            }
        }
    }

    // --- アイテム操作の入力ハンドラ ---

    void OnUse(InputValue value)
    {
        if (value.isPressed && player != null)
        {
            player.UseItem(selectedSlotIndex);
        }
    }

    void OnScroll(InputValue value)
    {
        // New Input SystemのScroll値を受け取る場合
        Vector2 scrollValue = value.Get<Vector2>();
        if (scrollValue.y != 0 && player != null && player.Inventory != null)
        {
            int maxSlots = player.Inventory.MaxSlots;
            if (maxSlots <= 0) return;

            if (scrollValue.y > 0)
                selectedSlotIndex = (selectedSlotIndex - 1 + maxSlots) % maxSlots;
            else if (scrollValue.y < 0)
                selectedSlotIndex = (selectedSlotIndex + 1) % maxSlots;
        }
    }

    public void SelectSlot(int index)
    {
        selectedSlotIndex = index;
    }

    void OnMove(InputValue value) => inputL = value.Get<Vector2>();
    void OnLook(InputValue value) => inputR = value.Get<Vector2>();
}