using UnityEngine;

namespace MyFps
{
    public enum EyeType
    {
        Left,
        Right
    }

    /// <summary>
    /// 퍼즐 아이템 (LeftEye, RightEye) 획득용 클래스 - Interactive 상속
    /// E키를 눌러 상호작용
    /// </summary>
    public class PickupPuzzleItem : Interactive
    {
        [Header("Puzzle Item Settings")]
        [SerializeField]
        private EyeType eyeType = EyeType.Left;

        [Header("Floating Settings")]
        [SerializeField]
        private float floatSpeed = 2f;
        [SerializeField]
        private float floatAmplitude = 0.2f;

        [Header("Rotation Settings")]
        [SerializeField]
        private float rotateSpeed = 50f;

        private Vector3 startPosition;

        protected override void Awake()
        {
            base.Awake();
            startPosition = transform.position;
            
            // 초기 액션 텍스트 설정
            action = "Pick up " + eyeType.ToString() + " Eye";
        }

        protected override void Update()
        {
            // 부모의 상호작용 로직 호출
            base.Update();

            // RightEye만 위아래로 움직이고 회전함
            if (eyeType == EyeType.Right)
            {
                FloatAndRotate();
            }
        }

        private void FloatAndRotate()
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
            transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);
        }

        protected override void DoAction()
        {
            if (eyeType == EyeType.Left)
            {
                Debug.Log("LeftEye 획득!");
                PlayerStats.Instance.HasLeftEye = true;
            }
            else if (eyeType == EyeType.Right)
            {
                Debug.Log("RightEye 획득!");
                PlayerStats.Instance.HasRightEye = true;
            }

            // 상호작용 UI 가리기
            HideActionUI();

            // 아이템 제거
            Destroy(gameObject);
        }
    }
}