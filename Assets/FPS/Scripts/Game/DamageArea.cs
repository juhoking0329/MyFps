using UnityEngine;
using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.Utility;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// 일정 범위 안에 있는 모든 충돌체(damageable) 오브젝트에게 데미지 주기
    /// 폭발위치와의 거리에 반비례해서 데미지량 주기
    /// 하나의 Health에게는 한번만 데미지 주기
    /// </summary>
    public class DamageArea : MonoBehaviour
    {
        #region Variables
        //일정 범위, 폭발지점으로부터 데미지를 입는 반경
        public float areaOfEffectDistance = 5f;
        //거리에 따른 데미지량 계산하는 커브
        [SerializeField] private AnimationCurve damageRatioOverDistance;
        #endregion

        #region Unity Event Methods

        #endregion

        #region Custom Methods
        //폭발(범위 공격) 데미지 계산
        public void InflictDamageArea(float damage, Vector3 center, LayerMask layer, 
            QueryTriggerInteraction interaction, GameObject owner)
        {
            //하나의 Health에 Damageable 하나 등록
            Dictionary<Health, Damageable> uniqueDamagedHealth = new Dictionary<Health, Damageable>();

            //범위 안에 있는 모든 충돌체 가져오기
            Collider[] affectedColliders = Physics.OverlapSphere(center, areaOfEffectDistance, layer, interaction);
            foreach(Collider collider in affectedColliders)
            {
                // 무시해야 할 오브젝트(마커)인지 확인
                IgnoredDetection ignore = collider.GetComponent<IgnoredDetection>();
                if (ignore != null) continue;

                //충돌체에 Damageable 컴포넌트 가져오기
                Damageable damageable = collider.GetComponent<Damageable>();
                if (damageable != null)
                {
                    //Damageable의 Health 가져오기
                    Health health = damageable.GetComponentInParent<Health>();
                    if (health != null && uniqueDamagedHealth.ContainsKey(health) == false)
                    {
                        uniqueDamagedHealth.Add(health, damageable);
                    }
                }
            }

            //uniqueDamageHealth에 등록한 health에 데미지 주기
            foreach (var uniqueDamageable in uniqueDamagedHealth.Values)
            {
                //거리에 따른 데미지 비율 계산
                float distance = Vector3.Distance(center, uniqueDamageable.transform.position);
                float damageRatio = damageRatioOverDistance.Evaluate(distance / areaOfEffectDistance);
                
                //최종 데미지 계산
                float finalDamage = damage * damageRatio;
                
                //damageable에게 데미지 주기
                uniqueDamageable.InflictDamage(finalDamage, true, owner);
            }
        }
        #endregion
    }
}