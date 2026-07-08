using UnityEngine;

namespace MyFps
{
    /// <summary>
    /// 열쇠 획득용 클래스 - PickupItem 상속
    /// </summary>
    public class PickupKey : PickupItem
    {
        #region Variables
        [Header("Light Flash Settings")]
        [SerializeField]
        private Light keyLight;                 // 반짝이게 할 노란색 라이트 컴포넌트
        [SerializeField]
        private float flashSpeed = 3f;          // 반짝임 속도
        [SerializeField]
        private float minIntensity = 0.2f;      // 최소 밝기
        [SerializeField]
        private float maxIntensity = 2.0f;      // 최대 밝기
        #endregion

        #region Unity Event Methods
        protected override void Update()
        {
            //00 부모 클래스의 Update (회전 및 위아래 둥둥 뜨기) 실행
            base.Update();

            //01 매 프레임마다 노란 불빛을 반짝이게 연출
            FlashLight();
        }
        #endregion

        #region Abstract Methods
        protected override bool OnPickup()
        {
            //01 열쇠 습득 처리 및 로그 출력
            Debug.Log("HeartKey를 획득하였습니다.");
            PlayerStats.Instance.HasHeartKey = true;
            return true;
        }
        #endregion

        #region Custom Methods
        //02 라이트 강도를 삼각함수를 이용하여 부드럽게 깜빡이게 하는 함수
        private void FlashLight()
        {
            if (keyLight == null) return;

            //00 삼각함수나 보간 대신 정수형 나눗셈 % 연산(Square Wave)으로 켜지고 꺼지는 깜빡임 구현
            bool isOn = (Mathf.FloorToInt(Time.time * flashSpeed) % 2) == 0;
            keyLight.intensity = isOn ? maxIntensity : minIntensity;
        }
        #endregion
    }
}
