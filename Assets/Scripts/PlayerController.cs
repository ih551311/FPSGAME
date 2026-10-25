using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 6f;         // 이동 속도
    public float jumpForce = 7f;         // 점프력

    private bool isGrounded;             // 바닥 충돌 여부
    private Rigidbody rb;
    private Animator anim;

    private int isWalkingHash = Animator.StringToHash("isWalking");
    private int JumpTriggerHash = Animator.StringToHash("Jump");

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // 1. WASD 입력
        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current.wKey.isPressed) moveZ += 1f;
        if (Keyboard.current.sKey.isPressed) moveZ -= 1f;
        if (Keyboard.current.aKey.isPressed) moveX -= 1f;
        if (Keyboard.current.dKey.isPressed) moveX += 1f;

        Vector3 inputDir = new Vector3(moveX, 0f, moveZ).normalized;
        Vector3 moveDirection = transform.right * inputDir.x + transform.forward * inputDir.z;

        // 2. 실제 물리 이동 (공중에서 W를 눌러도 앞으로 이동함)
        rb.linearVelocity = new Vector3(moveDirection.x * walkSpeed, rb.linearVelocity.y, moveDirection.z * walkSpeed);

        // 3. 점프 처리
        if (isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);

            if (anim != null)
            {
                anim.ResetTrigger(JumpTriggerHash);
                anim.SetTrigger(JumpTriggerHash);
            }
        }

        // 4. 애니메이션 제어 (땅에 닿아있을 때만 걷기 애니메이션 발동)
        if (anim != null)
        {
            bool isWalking = isGrounded && (inputDir.sqrMagnitude > 0f);
            anim.SetBool(isWalkingHash, isWalking);
        }
    }

    // 5. 충돌 판정
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;

            // 착지 시 공중 잔여 점프 트리거 삭제
            if (anim != null)
            {
                anim.ResetTrigger(JumpTriggerHash);
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