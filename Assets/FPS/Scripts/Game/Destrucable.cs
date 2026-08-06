using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 죽었을때 오브젝트(Health)를 가지고 있는)를 킬하는 클래스
    /// </summary>
    public class Destrucable : MonoBehaviour
    {
        #region Variables
        //참조
        private Health health;

        //kill delay
        [SerializeField] private float delayTime = 0f;
        #endregion

        #region Unity Event Methods
        private void Awake()
        {
            //참조
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            //health 이벤트 함수 등록
            health.onDamaged += OnDamaged;
            health.onDeath += OnDie;
        }

        private void OnDisable()
        {
            //health 이벤트 함수 해제
            health.onDamaged -= OnDamaged;
            health.onDeath -= OnDie;
        }
        #endregion

        #region Custom Method
        //데미지 입었을때 호출되어 실행되는 함수
        private void OnDamaged(float damage, GameObject damageSource)
        {
            //ToDo : 데미지 구현 내용
        }

        //죽었을때 호출되어 실행되는 함수
        private void OnDie()
        {
            //킬
            Destroy(this.gameObject, delayTime);
        }
        #endregion
    }
}