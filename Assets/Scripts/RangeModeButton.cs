using UnityEngine;

// 바닥에 놓는 모드 선택 버튼
// 총으로 쏘면 정해둔 모드로 바뀜
public class RangeModeButton : MonoBehaviour
{
    public RangeMode mode;          // 이 버튼이 시작할 모드
    public RangeSpawner spawner;    // 씬에 있는 스포너

    // 총에 맞으면 호출됨
    public void Press()
    {
        spawner.SetMode(mode);
    }
}