using UnityEngine;
using Unity.FPS.Game;
using Unity.FPS.Utility;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// 발사체 기본형
    /// </summary>
    public class ProjectileStandard : ProjectileBase
    {
        #region Variables
        [Header("General")]
        public float maxLifeTime = 5f;              // 최대 생존 시간
        public float speed = 20f;                  // 이동 속도
        public float gravityDownAcceleration = 0f;  // 중력 가속도

        [Header("Damage")]
        public float damage = 10f;                  // 데미지

        [Header("Effects")]
        public GameObject impactVfx;                // 충돌 이펙트
        public AudioClip impactSfx;                 // 충돌 사운드

        public Vector3 velocity;                   // 현재 속도
        private bool hasTrajectoryOverride;
        private bool hasHit;                        // 충돌 여부
        #endregion

        #region Unity Event Method
        private void OnEnable()
        {
            // 부모의 이벤트 구독
            onShoot += OnShoot;
            Destroy(gameObject, maxLifeTime);       // 생성과 동시에 라이프 타임 설정
        }

        private void OnDisable()
        {
            onShoot -= OnShoot;
        }

        private void Update()
        {
            if (hasHit) return;

            // 중력 적용
            velocity += Vector3.down * gravityDownAcceleration * Time.deltaTime;

            // 이동 방향 계산
            Vector3 moveDelta = velocity * Time.deltaTime;

            // 충돌 체크 (현재 위치에서 다음 위치까지 레이캐스트)
            if (Physics.Raycast(transform.position, velocity.normalized, out RaycastHit hit, moveDelta.magnitude))
            {
                // 자기 자신(Owner) 충돌 무시
                if (Owner != null && hit.collider.transform.root == Owner.transform.root)
                {
                    // 무시하고 그냥 이동
                    transform.position += moveDelta;
                }
                else
                {
                    OnHit(hit);
                }
            }
            else
            {
                // 충돌이 없으면 그냥 이동
                transform.position += moveDelta;
                transform.forward = velocity.normalized;
            }
        }
        #endregion

        #region Custom Method
        private void OnShoot()
        {
            // 발사 초기 속도 설정
            velocity = InitialDirection * speed + InheritedMuzzleVelocity;
        }

        private void OnHit(RaycastHit hit)
        {
            hasHit = true;

            // 충돌 이펙트 생성
            if (impactVfx != null)
            {
                GameObject vfx = Instantiate(impactVfx, hit.point, Quaternion.LookRotation(hit.normal));
                TimeSelfDestruct tsd = vfx.AddComponent<TimeSelfDestruct>();
                tsd.lifeTime = 2f;
            }

            // 충돌 사운드 재생
            if (impactSfx != null)
            {
                AudioUtility.CreateSFX(impactSfx, hit.point);
            }

            // 데미지 처리
            DamageArea damageArea = GetComponent<DamageArea>();
            if (damageArea != null)
            {
                // 범위 데미지 처리 (DamageArea 스크립트 위임)
                // -1: Everything 레이어 마스크, QueryTriggerInteraction.Ignore: 트리거 무시
                damageArea.InflictDamageArea(damage, hit.point, -1, QueryTriggerInteraction.Ignore, Owner);
            }
            else
            {
                // 단일 타겟 데미지 (Blaster, ShotGun)
                Health health = hit.collider.GetComponentInParent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage, Owner); // 데미지 적용
                }
            }

            // 발사체 파괴
            Destroy(gameObject);
        }
        #endregion
    }
}