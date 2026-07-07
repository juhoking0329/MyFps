using UnityEngine;

namespace MyFps
{
    /// <summary>
    /// 탄약 줍기용 클래스 - PickupItem 상속
    /// </summary>
    public class PickupAmmo : PickupItem
    {
        #region Variables
        [Header("Ammo Settings")]
        [SerializeField] private int giveAmmo = 7;           // 지급할 탄약 갯수
        #endregion

        #region Abstract Methods
        protected override bool OnPickup()
        {
            //01 탄약 지급 및 로그 출력
            Debug.Log($"Ammo {giveAmmo}개를 획득하였습니다.");
            PlayerStats.Instance.AddAmmo(giveAmmo);
            return true;
        }
        #endregion
    }
}
