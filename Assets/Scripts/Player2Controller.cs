using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player2Controller : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.2f;
    public Transform cameraTransform;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public float fireRate = 0.2f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private float verticalRotation = 0f;
    private float fireTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        moveInput = Vector2.zero;

        if (fireTimer > 0f)
        {
            fireTimer -= Time.deltaTime;
        }

        if (Keyboard.current == null) return;

        // 플레이어 1의 설정창이 열려 커서가 풀리면 조작 중지
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current.upArrowKey.isPressed) moveZ += 1f;
        if (Keyboard.current.downArrowKey.isPressed) moveZ -= 1f;
        if (Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
        if (Keyboard.current.rightArrowKey.isPressed) moveX += 1f;

        moveInput = new Vector2(moveX, moveZ).normalized;

        ProcessMouseLook();

        if (Mouse.current != null &&
            Mouse.current.rightButton.isPressed &&
            fireTimer <= 0f)
        {
            Shoot();
            fireTimer = fireRate;
        }
    }

    void FixedUpdate()
    {
        Vector3 moveDirection =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        rb.linearVelocity = new Vector3(
            moveDirection.x * moveSpeed,
            rb.linearVelocity.y,
            moveDirection.z * moveSpeed
        );
    }

    void ProcessMouseLook()
    {
        if (Mouse.current == null || cameraTransform == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        cameraTransform.localEulerAngles =
            new Vector3(verticalRotation, 0f, 0f);
    }

    void Shoot()
    {
        if (bulletPrefab == null || cameraTransform == null) return;

        Instantiate(
            bulletPrefab,
            cameraTransform.position + cameraTransform.forward * 1.0f,
            cameraTransform.rotation
        );

        fireTimer = fireRate;
    }
}