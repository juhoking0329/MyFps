using UnityEngine;

namespace MyFps
{
    /// <summary>
    /// 필드에 떨어진 아이템 줍기용 부모 추상화 클래스
    /// </summary>
    public abstract class PickupItem : MonoBehaviour
    {
        //추상메서드 - 구현하도록 강제하는 기능 정의
        #region Abstract Methods
        protected abstract bool OnPickup();     //아이템 획득 성공 true, 실패 false
        #endregion

        #region Variables
        [Header("Floating Settings")]
        [SerializeField]
        private float floatSpeed = 2f;            // 위아래 움직임 속도
        [SerializeField]
        private float floatAmplitude = 0.2f;      // 움직임 높이 범위

        [Header("Rotation Settings")]
        [SerializeField]
        private float rotateSpeed = 50f;          // 회전 속도 (도/초)

        private Vector3 startPosition;            // 시작 위치 저장용
        #endregion

        #region Unity Event Methods
        private void Start()
        {
            //00 오브젝트의 초기 월드 위치 저장
            startPosition = transform.position;
        }

        protected virtual void Update()
        {
            //00 매 프레임마다 아이템을 회전시키고 위아래로 움직임
            FloatAndRotate();
        }

        private void OnTriggerEnter(Collider other)
        {
            //00 플레이어가 트리거 콜라이더에 진입했는지 체크
            if (other.gameObject.CompareTag("Player"))
            {
                //00 아이템 획득 성공 시 오브젝트 파괴
                if (OnPickup())
                {
                    Destroy(gameObject);
                }
            }
        }
        #endregion

        #region Custom Methods
        //01 아이템을 위아래 사인 곡선으로 움직이고 회전시키는 함수
        private void FloatAndRotate()
        {
            // 위아래 사인파 좌표 연산
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);

            // Y축 기준 월드 공간에서 회전
            transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);
        }
        #endregion

    }
}
