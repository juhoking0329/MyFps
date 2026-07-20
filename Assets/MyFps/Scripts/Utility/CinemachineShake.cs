using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

namespace MyFps
{
    /// <summary>
    /// 화면 흔들림 효과 구현
    /// 흔들림 계수 : 흔들림 속도, 흔들림 크기, 흔들림 지속시간
    /// </summary>
    public class CinemachineShake : Singleton<CinemachineShake>
    {
        #region Variables
        //참조
        private CinemachineBasicMultiChannelPerlin multiChannelPerlin; // 시네머신 흔들림용 컴포넌트

        [Header("평상시 흔들림 (숨쉬기)")]
        [SerializeField] private float idleAmplitude = 0.1f; 
        [SerializeField] private float idleFrequency = 0.1f; 

        [Header("피격 시 흔들림")]
        [SerializeField] private float hitAmplitude = 3f; 
        [SerializeField] private float hitFrequency = 3f; 
        [SerializeField] private float hitDuration = 0.3f; 

        private float shakeTimer = 0f;
        private float shakeTimerTotal = 0f;
        private float startingAmplitude = 0f;
        private float startingFrequency = 0f;
        #endregion

        #region Unity Event Methods
        protected override void Awake()
        {
            base.Awake();
            multiChannelPerlin = GetComponent<CinemachineBasicMultiChannelPerlin>();
            
            if (multiChannelPerlin != null)
            {
                multiChannelPerlin.AmplitudeGain = idleAmplitude;
                multiChannelPerlin.FrequencyGain = idleFrequency;
            }
        }

        private void Update()
        {
            if (multiChannelPerlin == null) return;

            if (shakeTimer > 0)
            {
                shakeTimer -= Time.deltaTime;
                
                // 시간이 지날수록 점진적으로 평상시(Idle) 수치로 돌아감
                float progress = 1f - (shakeTimer / shakeTimerTotal);
                multiChannelPerlin.AmplitudeGain = Mathf.Lerp(startingAmplitude, idleAmplitude, progress);
                multiChannelPerlin.FrequencyGain = Mathf.Lerp(startingFrequency, idleFrequency, progress);
            }
            else
            {
                // 흔들림이 끝나면 평상시 수치 유지
                multiChannelPerlin.AmplitudeGain = idleAmplitude;
                multiChannelPerlin.FrequencyGain = idleFrequency;
            }
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// 피격 시 카메라를 강하게 흔듭니다.
        /// </summary>
        public void ShakeCamera()
        {
            if (multiChannelPerlin == null) return;

            multiChannelPerlin.AmplitudeGain = hitAmplitude;
            multiChannelPerlin.FrequencyGain = hitFrequency;
            
            startingAmplitude = hitAmplitude;
            startingFrequency = hitFrequency;
            
            shakeTimer = hitDuration;
            shakeTimerTotal = hitDuration;
        }
        #endregion
    }
}