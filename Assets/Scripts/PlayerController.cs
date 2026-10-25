using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Stats")]
    public float walkSpeed = 2f;
    public float runSpeed = 3f;
    public float jumpForce = 3f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.2f;
    public Transform cameraTransform;
    public TextMeshProUGUI sensitivityText;
    private float verticalRotation = 0f;

    [Header("UI Settings")]
    public GameObject settingCanvas;
    private bool isCanvasOpen = false;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public float fireRate = 0.2f;
    private float fireTimer = 0f;

    private bool isGrounded;
    private Rigidbody rb;
    private Animator anim;

    private int isWalkingHash = Animator.StringToHash("isWalking");
    private int jumpTriggerHash = Animator.StringToHash("Jump");

    private void Awake()
    {
        settingCanvas.SetActive(false);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            isCanvasOpen = !isCanvasOpen;

            settingCanvas.SetActive(isCanvasOpen);

            Cursor.lockState = isCanvasOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isCanvasOpen;
        }

        if (fireTimer > 0f)
        {
            fireTimer -= Time.deltaTime;
        }

        if (Mouse.current != null && Mouse.current.leftButton.isPressed && fireTimer <= 0f)
        {
            Shoot();
            fireTimer = fireRate;
        }

        if (isCanvasOpen) return;

        ProcessMouseLook();

        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current.wKey.isPressed) moveZ += 1f;
        if (Keyboard.current.sKey.isPressed) moveZ -= 1f;
        if (Keyboard.current.aKey.isPressed) moveX -= 1f;
        if (Keyboard.current.dKey.isPressed) moveX += 1f;

        Vector3 inputDir = new Vector3(moveX, 0f, moveZ).normalized;
        Vector3 moveDirection = transform.right * inputDir.x + transform.forward * inputDir.z;

        bool isShiftPressed = Keyboard.current.leftShiftKey.isPressed;
        float currentSpeed = isShiftPressed ? runSpeed : walkSpeed;

        rb.linearVelocity = new Vector3(moveDirection.x * currentSpeed, rb.linearVelocity.y, moveDirection.z * currentSpeed);

        if (isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);

            if (anim != null)
            {
                anim.ResetTrigger(jumpTriggerHash);
                anim.SetTrigger(jumpTriggerHash);
            }
        }

        if (anim != null)
        {
            bool isWalking = isGrounded && (inputDir.sqrMagnitude > 0f);
            anim.SetBool(isWalkingHash, isWalking);
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null && cameraTransform != null)
        {
            Instantiate(bulletPrefab, cameraTransform.position + cameraTransform.forward * 1.0f, cameraTransform.rotation);
        }
    }

    void ProcessMouseLook()
    {
        if (Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        if (cameraTransform != null)
        {
            cameraTransform.localEulerAngles = new Vector3(verticalRotation, 0f, 0f);
        }
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;

        if (sensitivityText != null)
        {
            sensitivityText.text = sensitivity.ToString("F2");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;

            if (anim != null)
            {
                anim.ResetTrigger(jumpTriggerHash);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}