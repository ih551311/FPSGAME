using UnityEngine;

// 모든 타겟에 붙이는 스크립트 (로봇, 고정 타겟, 구체 전부 이거 하나로 씀)
// 총에 한 방 맞으면 파괴됨
// 회전 없이 프리팹 방향 그대로 앞만 봄
public class RangeTarget : MonoBehaviour
{
    public float lifeTime = 0f;       // 몇 초 뒤에 저절로 사라질지 (0이면 안 사라짐)
    public bool canMove = false;      // 좌우로 움직일지 (로봇만 체크)
    public float moveSpeed = 2f;      // 움직이는 속도

    // 아래 3개는 스포너가 알아서 넣어줌
    public RangeSpawner spawner;      // 나를 만든 스포너
    public float minX;                // 움직일 수 있는 왼쪽 끝
    public float maxX;                // 움직일 수 있는 오른쪽 끝

    // 스크립트 안에서만 쓰는 변수
    private float timer = 0f;         // 생긴 뒤 지난 시간
    private float targetX;            // 지금 가고 있는 목표 x 위치

    void Start()
    {
        targetX = Random.Range(minX, maxX);
    }

    void Update()
    {
        // 1. 시간이 다 되면 사라짐
        if (lifeTime > 0f)
        {
            timer += Time.deltaTime;

            if (timer >= lifeTime)
            {
                Miss();
                return;
            }
        }

        // 2. 좌우로 움직이기 (canMove 체크된 것만)
        if (canMove)
        {
            MoveSideToSide();
        }
    }

    // 좌우 이동
    void MoveSideToSide()
    {
        if (transform.position.x < targetX - 0.1f)
        {
            // 목표가 오른쪽 → 오른쪽으로
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
        }
        else if (transform.position.x > targetX + 0.1f)
        {
            // 목표가 왼쪽 → 왼쪽으로
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;
        }
        else
        {
            // 도착 → 새 목표 정하기
            targetX = Random.Range(minX, maxX);
        }
    }

    // 총에 맞았을 때 (플레이어가 호출)
    public void Hit()
    {
        if (spawner != null)
        {
            spawner.OnTargetHit(this);
        }
        Destroy(gameObject);
    }

    // 시간 다 돼서 놓쳤을 때
    void Miss()
    {
        if (spawner != null)
        {
            spawner.OnTargetMiss(this);
        }
        Destroy(gameObject);
    }
}