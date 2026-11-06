using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// 연습장 플레이어
// 이동, 점프, 마우스 시점, 설정창, 사격 담당
public class RangePlayerController : MonoBehaviour
{
    // 이동
    public float walkSpeed = 2f;
    public float runSpeed = 3f;
    public float jumpForce = 3f;

    // 마우스 시점
    public float mouseSensitivity = 0.2f;
    public TextMeshProUGUI sensitivityText;   // 감도 숫자 표시 (없어도 됨)

    // 설정창
    public GameObject settingCanvas;          // ESC로 여닫는 창

    // 사격
    public float fireRate = 0.2f;             // 몇 초마다 한 발 쏠 수 있는지
    public float range = 100f;                // 총이 닿는 거리
    public LayerMask hitMask;                 // 총에 맞을 레이어 (Player는 빼기)

    // 스크립트 안에서만 쓰는 변수
    private Rigidbody rb;
    private Transform cameraTransform;
    private float verticalRotation = 0f;      // 위아래로 고개 돌린 각도
    private bool isCanvasOpen = false;        // 설정창이 열려 있는지
    private bool isGrounded = false;          // 땅에 서 있는지
    private float fireTimer = 0f;             // 마지막으로 쏜 뒤 지난 시간

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraTransform = Camera.main.transform;

        // 설정창 닫고 시작
        settingCanvas.SetActive(false);

        // 마우스 커서 숨기고 화면 가운데 고정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // ESC 누르면 설정창 열고 닫기
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleSettingCanvas();
        }

        // 쏜 뒤 지난 시간 늘리기
        fireTimer += Time.deltaTime;

        // 설정창이 열려 있으면 아래는 전부 안 함
        if (isCanvasOpen)
        {
            return;
        }

        // 왼쪽 클릭을 누르고 있고, 쏠 시간이 됐으면 발사
        if (Mouse.current.leftButton.isPressed && fireTimer >= fireRate)
        {
            Shoot();
            fireTimer = 0f;
        }

        MouseLook();
        Move();
        Jump();
    }

    // 설정창 열고 닫기
    void ToggleSettingCanvas()
    {
        isCanvasOpen = !isCanvasOpen;   // true면 false로, false면 true로
        settingCanvas.SetActive(isCanvasOpen);

        if (isCanvasOpen)
        {
            // 창 열림 → 커서 보이게
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // 창 닫힘 → 커서 숨기기
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // 설정창의 감도 슬라이더에 연결하는 함수
    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;

        if (sensitivityText != null)
        {
            sensitivityText.text = sensitivity.ToString("F2");   // 소수점 2자리까지 표시
        }
    }

    // 사격
    void Shoot()
    {
        // 카메라에서 정면으로 레이(보이지 않는 선) 만들기
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        // 레이가 뭔가에 맞으면 그 정보가 hit에 들어옴
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range, hitMask))
        {
            // 맞은 게 타겟이면 파괴
            RangeTarget target = hit.collider.GetComponent<RangeTarget>();
            if (target != null)
            {
                target.Hit();
            }

            // 맞은 게 모드 버튼이면 모드 바꾸기
            RangeModeButton button = hit.collider.GetComponent<RangeModeButton>();
            if (button != null)
            {
                button.Press();
            }
        }
    }

    // 마우스로 둘러보기
    void MouseLook()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        // 좌우: 몸 전체 회전
        transform.Rotate(0f, mouseX, 0f);

        // 위아래: 카메라만 회전
        verticalRotation -= mouseY;

        // 너무 많이 꺾이지 않게 -90 ~ 90도로 막기
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        cameraTransform.localEulerAngles = new Vector3(verticalRotation, 0f, 0f);
    }

    // WASD 이동
    void Move()
    {
        // 내가 바라보는 방향 기준으로 이동 방향 더하기
        Vector3 moveDirection = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) moveDirection += transform.forward;
        if (Keyboard.current.sKey.isPressed) moveDirection -= transform.forward;
        if (Keyboard.current.dKey.isPressed) moveDirection += transform.right;
        if (Keyboard.current.aKey.isPressed) moveDirection -= transform.right;

        // 대각선으로 갈 때 빨라지지 않게 길이를 1로 맞추기
        moveDirection = moveDirection.normalized;

        // Shift 누르면 달리기
        float speed = walkSpeed;
        if (Keyboard.current.leftShiftKey.isPressed)
        {
            speed = runSpeed;
        }

        // 위아래 속도(y)는 중력 때문에 그대로 두고, 앞뒤좌우만 바꾸기
        rb.linearVelocity = new Vector3(moveDirection.x * speed, rb.linearVelocity.y, moveDirection.z * speed);
    }

    // 점프
    void Jump()
    {
        if (isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }
    }

    // Ground 태그 붙은 바닥에 닿으면 땅에 있는 것
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    // 바닥에서 떨어지면 공중
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}