using UnityEngine;

namespace MySample
{
    /// <summary>
    /// 1초마다 좌우로 움직이는 벽을 위한 클래스
    /// dir 변수 사용 1이면 오른쪽 이동, -1이면 왼쪽 이동
    /// </summary>
    public class MoveWall : MonoBehaviour
    {
        [Tooltip("1이면 오른쪽, -1이면 왼쪽")]
        public int dir = 1;

        [Tooltip("이동 속도 (단위: 유닛/초)")]
        public float speed = 1f;

        [Tooltip("방향 전환 주기 (초)")]
        public float period = 1f;

        [Tooltip("로컬 축으로 이동할지 여부 (체크 안하면 월드 축)")]
        public bool useLocalSpace = false;

        float timer = 0f;

        void OnValidate()
        {
            // dir는 1 또는 -1로 제한
            dir = (dir >= 0) ? 1 : -1;
            if (period <= 0f) period = 1f;
            if (speed < 0f) speed = Mathf.Abs(speed);
        }

        void Update()
        {
            // 이동
            Vector3 right = useLocalSpace ? transform.right : Vector3.right;
            transform.Translate(right * dir * speed * Time.deltaTime, Space.World);

            // 타이머 증가 및 주기마다 방향 전환
            timer += Time.deltaTime;
            if (timer >= period)
            {
                dir = -dir;
                timer -= period;
            }
        }
    }
}

/*
1초마다 좌우로 움직이는 벽을 위한 클래스
dir 변수 사용 1이면 오른쪽 이동, -1이면 왼쪽 이동
스크립트 작성해줘
*/