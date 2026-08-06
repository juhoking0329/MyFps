using UnityEngine;
using Unity.FPS.Utility;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// 충전 발사체의 충전량에 따른 속성값 설정하기
    /// </summary>
    public class ProjectileChargeParameters : MonoBehaviour
    {
        [Tooltip("차지 발사 시 데미지 범위 (최소~최대)")]
        public MinMaxFloat damage = new MinMaxFloat { min = 10f, max = 30f };
        
        [Tooltip("차지 발사 시 속도 범위 (최소~최대)")]
        public MinMaxFloat speed = new MinMaxFloat { min = 20f, max = 40f };
        
        [Tooltip("차지 발사 시 중력 가속도 범위 (최소~최대)")]
        public MinMaxFloat gravity = new MinMaxFloat { min = 0f, max = 5f };
        
        [Tooltip("차지 발사 시 크기 배율 범위 (최소~최대)")]
        public MinMaxFloat scale = new MinMaxFloat { min = 1f, max = 2f };

        [Tooltip("차지 발사 시 폭발 반경 범위 (최소~최대)")]
        public MinMaxFloat radius = new MinMaxFloat { min = 0f, max = 2f };

        private ProjectileStandard projectileStandard;

        private void Awake()
        {
            // 발사체 스크립트 컴포넌트를 가져옵니다.
            projectileStandard = GetComponent<ProjectileStandard>();
        }

        private void OnEnable()
        {
            if (projectileStandard != null)
            {
                projectileStandard.onShoot += OnShoot;
            }
        }

        private void OnDisable()
        {
            if (projectileStandard != null)
            {
                projectileStandard.onShoot -= OnShoot;
            }
        }

        private void OnShoot()
        {
            if (projectileStandard == null) return;

            float charge = projectileStandard.InitialCharge;

            // 충전량에 따라 ProjectileStandard의 값들을 덮어씌웁니다.
            projectileStandard.damage = damage.GetValueFromRatio(charge);
            projectileStandard.speed = speed.GetValueFromRatio(charge);
            projectileStandard.gravityDownAcceleration = gravity.GetValueFromRatio(charge);
            
            DamageArea damageArea = GetComponent<DamageArea>();
            if (damageArea != null)
            {
                damageArea.areaOfEffectDistance = radius.GetValueFromRatio(charge);
            }
            
            // 크기 스케일링 적용
            float currentScale = scale.GetValueFromRatio(charge);
            projectileStandard.transform.localScale = projectileStandard.transform.localScale * currentScale;

            // 속도가 변경되었으므로 ProjectileStandard의 속도(velocity)를 재계산해서 덮어씌웁니다.
            projectileStandard.velocity = projectileStandard.InitialDirection * projectileStandard.speed + projectileStandard.InheritedMuzzleVelocity;
        }
    }
}