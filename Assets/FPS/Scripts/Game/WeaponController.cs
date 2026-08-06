using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using Unity.FPS.Utility;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 조준점 데이터 정의
    /// 이미지, 크기, 컬러
    /// </summary>
    [System.Serializable]
    public struct CrossHairData
    {
        public Sprite CrossHairSprite;
        public float CrossHairSize;
        public Color CrossHairColor;
    }

    /// <summary>
    /// 무기별 슛 타입 정의
    /// </summary>
    public enum WeaponShootType
    {
        Manual,
        Automatic,
        Charge,
        Sniper,
        //..
    }

    /// <summary>
    /// 총기류 무기를 관리하는 클래스
    /// </summary>
    [RequireComponent (typeof(AudioSource))] 
    public class WeaponController : MonoBehaviour
    {
        #region Variables
        //무기 활성화, 비활성화
        public GameObject weaponRoot;

        public GameObject Owner { get; set; }               //무기 주인
        public GameObject SourcePrefab { get; set; }        //무기를 생성한 프리팹
        public bool IsWeaponActive { get; private set; }    //무기 활성화 여부

        //슛팅 오디오
        private AudioSource shootAudioSource;
        public AudioClip switchWeaponSfx;           //무기 교체 효과음

        //크로스헤어 - 기본
        public CrossHairData crossHairDefault;          //기본
        public CrossHairData crossHairTargetInSight;    //적 포착시(타겟팅)

        //조준
        [Range(0, 1)] public float aimZoomRatio = 1f;       //조준시 줌 비율
        public Vector3 aimOffset = Vector3.zero;            //조준 위치 이동시 무기별 위치 조정값

        //반동
        [Header("Weapon Recoil")]
        public float recoilForce = 0.1f;                    // 무기 발사 시 뒤로 밀리는 힘

        //슛팅
        public WeaponShootType shootType;                   //슛팅 타입

        [SerializeField] private float maxAmmo = 0f;        //최대 탄환 갯수
        public float MaxAmmo { get { return maxAmmo; } }
        private float currentAmmo;                          //현재 탄환 갯수
        public float CurrentAmmo { get { return currentAmmo; } }

        public float CurrentChargeRatio { 
            get { 
                if (shootType == WeaponShootType.Charge && maxChargeTime > 0f && IsCharge) 
                {
                    return Mathf.Clamp01(currentChargeTime / maxChargeTime);
                }
                return 0f;
            } 
        }

        [Header("Reloading")]
        public float reloadDuration = 1f;                   // 장전 시간
        public AudioClip reloadSfx;                         // 장전 소리
        public bool IsReloading { get; private set; }       // 장전 중인지 여부

        [SerializeField] private float delayBetweenShots = 0.5f;  //연사 방지, 초당 발사 갯수
        private float lastTimeShot;

        //차지(충전) 샷
        [Header("Charge Weapon")]
        public float maxChargeTime = 2f;            // 100% 충전에 걸리는 시간
        private float currentChargeTime = 0f;
        public bool IsCharge { get; private set; }  // 현재 충전 여부
        public float LastChargeTriggerTimestamp { get; private set; } // 마지막으로 차지를 시작한 시간
        public float lastChargeTriggerTimeTamp => LastChargeTriggerTimestamp; // 오타 방지용 소문자 프로퍼티

        //슛팅 이펙트
        [Header("Shoot Effects")]
        public GameObject muzzleFlashPrefab;        //머즐 플래쉬(이펙트)
        public Transform muzzleTransform;           //머즐 플래쉬 위치(총구, 파이어포인트)
        public AudioClip shootSfx;                  //발사 효과음

        //발사체 Projectile
        [Header("Projectile Settings")]
        public int bulletsPerShot = 1;                                  // 1번 발사당 발사체 수 (산탄용)
        public float bulletSpreadAngle = 0f;                            // 퍼지는 각도

        public Vector3 MuzzleWorldVelocity { get; private set; }        //총구 이동 속도
        private Vector3 lastMuzzlePosition;
        public float CurrentCharge { get; private set; }                //충전 량

        public ProjectileBase projectilePrefab;                         //발사체 프리팹
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            //참조
            shootAudioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            //초기화
            currentAmmo = maxAmmo;
            lastTimeShot = Time.time;
            lastMuzzlePosition = muzzleTransform.position;

        }

        private void Update()
        {
            //이번 프레임의 총구 이동 속도는
            if (Time.deltaTime > 0)
            {
                MuzzleWorldVelocity = (muzzleTransform.position - lastMuzzlePosition) / Time.deltaTime;
                //이번 프레임의 위치 저장
                lastMuzzlePosition = muzzleTransform.position;
            }

            // 자동 장전 로직 (탄환이 0이고 장전 중이 아닐 때)
            if (currentAmmo <= 0 && !IsReloading)
            {
                StartReload();
            }
        }
        #endregion

        #region Custom Method
        //무기 활성화, 비활성화
        public void ShowWeapon(bool show)
        {
            weaponRoot.SetActive(show);
            if(show == true && switchWeaponSfx != null)
            {
                //무기 교체 효과음 플레이
                shootAudioSource.PlayOneShot(switchWeaponSfx);
            }
            IsWeaponActive = show;
        }

        //인풋에 따른 발사 처리
        public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
        {
            switch(shootType)
            {
                case WeaponShootType.Manual:
                case WeaponShootType.Sniper: // 스나이퍼도 기본적으로 단발이라고 가정
                    if(inputDown)
                    {
                        return TryShoot();
                    }
                    break;

                case WeaponShootType.Automatic:
                    if (inputHeld)
                    {
                        return TryShoot();
                    }
                    break;

                case WeaponShootType.Charge:
                    if (inputHeld)
                    {
                        return TryBeginCharge();
                    }
                    
                    if (inputUp)
                    {
                        if (IsCharge)
                        {
                            // 방아쇠를 놓으면 발사
                            return TryShoot();
                        }
                    }
                    break;
            }

            return false;
        }

        //발사 처리
        private bool TryShoot()
        {
            // 장전 중이면 발사 불가
            if (IsReloading) return false;

            //ammo 체크, 연사방지 체크
            if (currentAmmo >= 1f && lastTimeShot + delayBetweenShots <= Time.time)
            {
                Debug.Log("Shoot!!!!!!");
                currentAmmo -= 1f;
                lastTimeShot = Time.time; // 발사 시간 기록
                Debug.Log($"currentAmmo : {currentAmmo}");

                // 발사 직전에 현재 차지량 저장
                if (shootType == WeaponShootType.Charge && maxChargeTime > 0f)
                {
                    CurrentCharge = Mathf.Clamp01(currentChargeTime / maxChargeTime);
                }
                else
                {
                    CurrentCharge = 0f;
                }

                // 차지 초기화
                IsCharge = false;
                currentChargeTime = 0f;

                HandleShoot();

                return true;
            }

            return false;
        }

        //슛 연출 처리
        private void HandleShoot()
        {
            // 발사 사운드 재생
            if (shootSfx != null && shootAudioSource != null)
            {
                shootAudioSource.PlayOneShot(shootSfx);
            }

            // 머즐 이펙트 생성
            if (muzzleFlashPrefab != null && muzzleTransform != null)
            {
                GameObject muzzleFlashInstance = Instantiate(muzzleFlashPrefab, muzzleTransform.position, muzzleTransform.rotation, muzzleTransform);
                TimeSelfDestruct tsd = muzzleFlashInstance.AddComponent<TimeSelfDestruct>();
                tsd.lifeTime = 2f; // 2초 뒤 삭제 (찌꺼기 방지)
            }

            // 발사체(Projectile) 생성 (산탄 포함)
            if (projectilePrefab != null && muzzleTransform != null)
            {
                for (int i = 0; i < bulletsPerShot; i++)
                {
                    // 산탄 퍼짐 각도 계산
                    Vector3 spreadOffset = Vector3.zero;
                    if (bulletSpreadAngle > 0f && bulletsPerShot > 1)
                    {
                        spreadOffset = new Vector3(
                            Random.Range(-bulletSpreadAngle, bulletSpreadAngle),
                            Random.Range(-bulletSpreadAngle, bulletSpreadAngle),
                            0f
                        );
                    }

                    // 퍼짐 각도를 반영한 회전값 적용
                    Quaternion bulletRotation = muzzleTransform.rotation * Quaternion.Euler(spreadOffset);
                    ProjectileBase newProjectile = Instantiate(projectilePrefab, muzzleTransform.position, bulletRotation);
                    newProjectile.Shoot(this); // ProjectileBase에 정의된 발사 속성 초기화 호출
                }
            }
        }

        // 수동 장전 시작
        public void StartReload()
        {
            if (IsReloading || currentAmmo >= maxAmmo) return;
            
            StartCoroutine(ReloadCoroutine());
        }

        private IEnumerator ReloadCoroutine()
        {
            IsReloading = true;
            
            // 장전 사운드 재생
            if (reloadSfx != null && shootAudioSource != null)
            {
                shootAudioSource.PlayOneShot(reloadSfx);
            }

            // 장전 시간만큼 대기
            yield return new WaitForSeconds(reloadDuration);

            // 탄창 꽉 채우기
            currentAmmo = maxAmmo;
            IsReloading = false;
        }

        // 차지 시작 및 진행
        private bool TryBeginCharge()
        {
            if (!IsCharge)
            {
                LastChargeTriggerTimestamp = Time.time;
            }
            
            IsCharge = true;
            currentChargeTime += Time.deltaTime;
            
            // 100% 충전이 되면 더 이상 충전되지 않도록 제한 (자동 발사 X)
            if (currentChargeTime > maxChargeTime)
            {
                currentChargeTime = maxChargeTime;
            }

            return false;
        }
        #endregion
    }
}