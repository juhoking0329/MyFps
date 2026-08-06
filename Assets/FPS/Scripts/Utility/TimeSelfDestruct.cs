using UnityEngine;

namespace Unity.FPS.Utility
{
    /// <summary>
    /// 오브젝트가 생성(활성화)된 후 정해진 시간이 지나면 자동으로 스스로를 파괴하는 유틸리티 컴포넌트입니다.
    /// 주로 탄피, 피격 이펙트 파티클 등 일회성 오브젝트를 메모리에서 정리할 때 사용합니다.
    /// </summary>
    public class TimeSelfDestruct : MonoBehaviour
    {
        #region Variables
        [Header("Settings")]
        [Tooltip("오브젝트가 파괴되기까지 대기할 시간 (초 단위)")]
        public float lifeTime = 5f;
        #endregion

        #region Unity Event Methods
        private void Start()
        {
            // 이 스크립트가 붙은 게임 오브젝트를 lifeTime(기본 5초) 뒤에 파괴(Destroy)합니다.
            Destroy(gameObject, lifeTime);
        }
        #endregion
    }
}
