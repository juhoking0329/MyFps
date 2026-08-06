using UnityEngine;
using Unity.FPS.Game;
using Unity.FPS.Utility;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// 충전 무기 효과를 처리하는 클래스
    /// </summary>
    public class ChargeWeaponEffectHandler : MonoBehaviour
    {
        #region Variables
        public GameObject chargingObject;               // 충전되는 오브젝트
        public GameObject spiningFrame;                 // 회전하는 프레임 오브젝트
        public GameObject diskOrbitParticlePrefab;      // 회전하는 디스크 파티클 프리팹

        public MinMaxVector3 scale;                     // 충전량에 따른 스케일 범위

        public Vector3 offset;                          // 충전 오브젝트 위치 오프셋
        public Transform parentTransform;               // 충전 오브젝트의 부모 Transform

        public MinMaxFloat orbitY;                      // 회전하는 디스크 파티클의 Y축 회전 범위
        public MinMaxVector3 radius;                    // 회전하는 디스크 파티클의 반경 범위

        public MinMaxFloat spiningSpeed;                // 회전하는 프레임의 회전 속도 범위

        //SFX
        public AudioClip chargingSFX;                   // 충전 사운드
        public AudioClip loopChargeWeaponSFX;           // 충전 중 반복 사운드 - 회전효과음

        public float fadeLoopDuration = 0.5f;           // 반복 사운드 페이드 아웃 시간
        public bool useProceduralPitchOnLoop = false;   // 절차적 피치 값 사용 여부
        [Range(1f, 5f)] public float maxProceduralPitchValue = 2.0f;    // 최대 절차적 피치 값

        public GameObject particleInstance { get; private set; }    // 회전하는 디스크 파티클 인스턴스
        private ParticleSystem diskOrbitParticle;
        private ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule;
        
        //참조
        private WeaponController weaponController;                   // 무기 컨트롤러 참조
        private AudioSource audioSource;
        private AudioSource audioSourceLoop;

        private float lastChargeTriggerTimeTamp;    // 마지막 충전 트리거 시간 임시 저장
        private float endChargeTime;                // 충전 종료 시간

        private float chargeRatio;                  // 현재 충전량 비율 (0~1)


        #endregion

        #region Unity Event Methods
        private void Awake()
        {
            //충전 사운드
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = chargingSFX;
            audioSource.playOnAwake = false;

            //충전 반복 사운드
            audioSourceLoop = gameObject.AddComponent<AudioSource>();
            audioSourceLoop.clip = loopChargeWeaponSFX;
            audioSourceLoop.playOnAwake = false;
            audioSourceLoop.loop = true;
        }

        private void Update()
        {
            //particleInstance null 체크
            if (particleInstance == null)
            {
                SpawnParticleSystem();  //1회만
            }

            particleInstance.gameObject.SetActive(weaponController.IsWeaponActive);

            chargeRatio = weaponController.CurrentChargeRatio;

            //충전되는 오브젝트
            chargingObject.transform.localScale = scale.GetValueFromRatio(chargeRatio);
            if(spiningFrame)
            {
                spiningFrame.transform.localRotation *= Quaternion.Euler(0f, spiningSpeed.GetValueFromRatio(chargeRatio) * Time.deltaTime, 0f);
            }
            //파티클 회전
            particleInstance.transform.localScale = radius.GetValueFromRatio(chargeRatio);
            velocityOverLifetimeModule.orbitalY = orbitY.GetValueFromRatio(chargeRatio);

            //SFX
            if(chargeRatio > 0f)
            {
                if(!audioSource.isPlaying && weaponController.lastChargeTriggerTimeTamp > lastChargeTriggerTimeTamp)
                {
                    lastChargeTriggerTimeTamp = weaponController.lastChargeTriggerTimeTamp;
                    if(useProceduralPitchOnLoop == false)
                    {
                        //페이드 효과
                        endChargeTime = Time.time + chargingSFX.length;
                        audioSource.Play();
                    }
                    audioSourceLoop.Play();
                }

                if(useProceduralPitchOnLoop == false)
                {
                    //사운드 페이드 효과
                    float volumeRatio = Mathf.Clamp01((endChargeTime - Time.time - fadeLoopDuration) / fadeLoopDuration);
                    audioSource.volume = volumeRatio;
                    audioSourceLoop.volume = 1f - volumeRatio;
                }
                else
                {
                    audioSourceLoop.pitch = Mathf.Lerp(1.0f, maxProceduralPitchValue, chargeRatio);
                }
            }
            else if (chargeRatio <= 0f)
            {
                audioSource.Stop();
                audioSourceLoop.Stop();
            }
        }
        #endregion

        #region Custom Methods
        //파티클 스폰하기
        private void SpawnParticleSystem()
        {
            particleInstance = Instantiate(diskOrbitParticlePrefab, parentTransform != null ? parentTransform : transform);
            particleInstance.transform.localPosition += offset;

            FindReference();
        }

        //참조
        private void FindReference()
        {
            diskOrbitParticle = particleInstance.GetComponent<ParticleSystem>();
            velocityOverLifetimeModule = diskOrbitParticle.velocityOverLifetime;

            weaponController = GetComponent<WeaponController>();
        }
        #endregion
    }
}