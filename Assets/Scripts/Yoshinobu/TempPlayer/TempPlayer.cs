using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("参照")]
    private Player player;
    [SerializeField] private MainGameUIManager uiManager;
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
    private bool isInventoryOpen = false;
    public bool IsInventoryOpen => isInventoryOpen;

    [Header("インベントリ設定")]
    [SerializeField] private int selectedSlotIndex = 0;
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

        UpdateCursorLockState();
    }

    void Update()
    {
        PlayerMovement();

        Vector3 finalMove = moveVelocity;
        finalMove.y = verticalVelocity;
        characterController.Move(finalMove * Time.deltaTime);

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

        // インベントリを開いている間はカメラ回転を止める
        if (!isInventoryOpen)
        {
            camDirection += inputR * rotateSpeed * Time.deltaTime;
            camDirection.y = Mathf.Clamp(camDirection.y, -80f, 80f);
            cam.transform.rotation = Quaternion.Euler(-camDirection.y, camDirection.x, 0.0f);
        }
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

    private void HandleScrollInput()
    {
        if (isInventoryOpen) return;

        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0 && player != null && player.Inventory != null)
        {
            int maxSlots = player.Inventory.MaxScrollSlots;
            if (maxSlots <= 0) return;

            if (scroll > 0)
                selectedSlotIndex = (selectedSlotIndex - 1 + maxSlots) % maxSlots;
            else if (scroll < 0)
                selectedSlotIndex = (selectedSlotIndex + 1) % maxSlots;
        }
    }

    /// <summary>
    /// インベントリ開閉状態に応じてカーソルロックを切り替える
    /// </summary>
    public void SetInventoryOpenState(bool isOpen)
    {
        isInventoryOpen = isOpen;
        lockMouse = !isOpen;
        UpdateCursorLockState();
    }

    private void UpdateCursorLockState()
    {
        if (lockMouse)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // --- アイテム操作の入力ハンドラ ---

    void OnUse(InputValue value)
    {
        // インベントリを開いている時はアイテム使用（攻撃・回復）を無効化
        if (isInventoryOpen) return;

        if (value.isPressed && player != null)
        {
            player.UseItem(selectedSlotIndex);
        }
    }

    void OnScroll(InputValue value)
    {
        if (isInventoryOpen) return;

        Vector2 scrollValue = value.Get<Vector2>();
        if (scrollValue.y != 0 && player != null && player.Inventory != null)
        {
            int maxSlots = player.Inventory.MaxScrollSlots;
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

    public void OnInventory(InputValue value)
    {
        if (value.isPressed && uiManager != null)
        {
            uiManager.ToggleInventory();
        }
    }
    void OnMove(InputValue value) => inputL = value.Get<Vector2>();
    void OnLook(InputValue value) => inputR = value.Get<Vector2>();
}