using System.Collections.Generic;
using UnityEngine;
using TMPro;

// 연습 모드 종류
public enum RangeMode
{
    None,    // 정지
    Wander,  // 좌우로 움직이는 로봇
    Static,  // 가만히 있다가 사라지는 타겟
    Sphere   // 벽 앞에 나타났다 사라지는 구체
}

// 모드에 맞춰 타겟을 만들고 지우고 점수를 셈
public class RangeSpawner : MonoBehaviour
{
    // 타겟 프리팹 (전부 RangeTarget이 붙어 있어야 함)
    public RangeTarget robotPrefab;
    public RangeTarget staticTargetPrefab;
    public RangeTarget spherePrefab;

    // 바닥 구역 (로봇, 고정 타겟이 나오는 곳). 스포너 위치가 가운데
    public float floorWidth = 10f;    // 좌우 길이 (x)
    public float floorDepth = 10f;    // 앞뒤 길이 (z)

    // 벽 구역 (구체가 나오는 곳). sphereWall 위치가 가운데
    public Transform sphereWall;
    public float wallWidth = 6f;      // 좌우 길이
    public float wallHeight = 3f;     // 위아래 길이

    // 로봇 모드
    public int robotCount = 3;        // 동시에 나오는 로봇 수

    // 고정 타겟 모드
    public float staticSpawnTime = 0.8f;   // 몇 초마다 하나씩 만들지
    public int staticMaxCount = 3;         // 동시에 최대 몇 개

    // 구체 모드
    public float sphereSpawnTime = 0.5f;
    public int sphereMaxCount = 1;

    // 점수 표시 (없어도 됨)
    public TextMeshProUGUI scoreText;

    // 스크립트 안에서만 쓰는 변수
    private RangeMode currentMode = RangeMode.None;          // 지금 모드
    private List<RangeTarget> targets = new List<RangeTarget>();   // 지금 살아있는 타겟 목록
    private float spawnTimer = 0f;                           // 마지막으로 만든 뒤 지난 시간
    private int hitCount = 0;                                // 맞춘 수
    private int missCount = 0;                               // 놓친 수

    void Start()
    {
        UpdateScoreText();
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;

        switch (currentMode)
        {
            case RangeMode.Static:
                // 시간이 됐고, 최대 개수보다 적으면 하나 만들기
                if (spawnTimer >= staticSpawnTime && targets.Count < staticMaxCount)
                {
                    SpawnStatic();
                    spawnTimer = 0f;
                }
                break;

            case RangeMode.Sphere:
                if (spawnTimer >= sphereSpawnTime && targets.Count < sphereMaxCount)
                {
                    SpawnSphere();
                    spawnTimer = 0f;
                }
                break;
        }
    }

    // 모드 바꾸기 (모드 버튼이 호출)
    public void SetMode(RangeMode mode)
    {
        // 원래 있던 타겟 전부 지우기
        ClearTargets();

        // 새 모드로 바꾸고 점수 초기화
        currentMode = mode;
        spawnTimer = 0f;
        hitCount = 0;
        missCount = 0;
        UpdateScoreText();

        // 로봇 모드는 처음에 한 번에 다 만들기
        if (mode == RangeMode.Wander)
        {
            for (int i = 0; i < robotCount; i++)
            {
                SpawnRobot();
            }
        }
    }

    // 타겟 하나 만들기 (아래 3개 함수가 공통으로 씀)
    RangeTarget SpawnTarget(RangeTarget prefab, Vector3 position)
    {
        RangeTarget target = Instantiate(prefab);
        target.transform.position = position;
        target.spawner = this;    // "너를 만든 건 나" 알려주기
        targets.Add(target);      // 목록에 추가
        return target;
    }

    // 로봇 만들기
    void SpawnRobot()
    {
        RangeTarget robot = SpawnTarget(robotPrefab, RandomFloorPosition());

        // 좌우로 움직일 범위 알려주기
        robot.minX = transform.position.x - floorWidth / 2f;
        robot.maxX = transform.position.x + floorWidth / 2f;
    }

    // 고정 타겟 만들기
    void SpawnStatic()
    {
        SpawnTarget(staticTargetPrefab, RandomFloorPosition());
    }

    // 구체 만들기
    void SpawnSphere()
    {
        // 벽 가운데에서 좌우, 위아래로 랜덤하게 떨어진 위치
        float x = Random.Range(-wallWidth / 2f, wallWidth / 2f);
        float y = Random.Range(-wallHeight / 2f, wallHeight / 2f);

        Vector3 position = sphereWall.position + new Vector3(x, y, 0f);

        SpawnTarget(spherePrefab, position);
    }

    // 바닥 구역 안의 랜덤 위치
    Vector3 RandomFloorPosition()
    {
        float x = transform.position.x + Random.Range(-floorWidth / 2f, floorWidth / 2f);
        float z = transform.position.z + Random.Range(-floorDepth / 2f, floorDepth / 2f);

        return new Vector3(x, transform.position.y, z);
    }

    // 타겟이 총에 맞았을 때 (RangeTarget이 호출)
    public void OnTargetHit(RangeTarget target)
    {
        targets.Remove(target);
        hitCount++;
        UpdateScoreText();

        // 로봇 모드는 잡은 만큼 다시 채우기
        if (currentMode == RangeMode.Wander)
        {
            SpawnRobot();
        }
    }

    // 타겟을 놓쳤을 때 (RangeTarget이 호출)
    public void OnTargetMiss(RangeTarget target)
    {
        targets.Remove(target);
        missCount++;
        UpdateScoreText();
    }

    // 살아있는 타겟 전부 지우기
    void ClearTargets()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            Destroy(targets[i].gameObject);
        }
        targets.Clear();
    }

    // 점수 글자 바꾸기
    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Mode: " + currentMode + "   Hit: " + hitCount + "   Miss: " + missCount;
        }
    }
}