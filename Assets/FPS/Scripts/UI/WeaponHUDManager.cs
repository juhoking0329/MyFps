using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;

namespace Unity.FPS.UI
{
    /// <summary>
    /// 게임 중 플레이어가 획득한 무기의 UI를 생성, 삭제, 관리하는 매니저 클래스입니다.
    /// </summary>
    public class WeaponHUDManager : MonoBehaviour
    {
        #region Variables
        [Header("Weapon UI Settings")]
        [Tooltip("생성할 무기 UI 프리팹 (AmmoCounter)")]
        public GameObject ammoCounterPrefab;
        [Tooltip("무기 UI가 나열될 부모 패널 (주로 화면 하단)")]
        public Transform ammoCounterPanel;

        // 플레이어 무기 매니저 참조
        private PlayerWeaponManager weaponManager;
        
        // 현재 활성화된 무기 UI들을 추적하기 위한 딕셔너리
        private Dictionary<WeaponController, AmmoCounter> weaponUIs = new Dictionary<WeaponController, AmmoCounter>();
        #endregion

        #region Unity Event Methods
        private void Awake()
        {
            // 씬 내의 PlayerWeaponManager를 찾아 참조합니다.
            weaponManager = FindAnyObjectByType<PlayerWeaponManager>();
            if (weaponManager != null)
            {
                // 무기가 획득되거나 버려질 때, 혹은 무기가 교체될 때 실행될 이벤트(UnityAction)에 함수들을 등록합니다.
                weaponManager.OnAddedWeapon += AddWeaponUI;
                weaponManager.OnRemovedWeapon += RemoveWeaponUI;
                weaponManager.OnSwitchToWeapon += SwitchWeaponUI;
            }
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// 새로운 무기를 획득했을 때 UI를 생성합니다.
        /// </summary>
        private void AddWeaponUI(WeaponController newWeapon)
        {
            // 프리팹이나 부모 패널이 세팅되어 있지 않다면 생성하지 않고 돌아갑니다.
            if (ammoCounterPrefab == null || ammoCounterPanel == null) return;

            // 무기 UI 프리팹을 화면 패널에 생성(Instantiate)합니다.
            GameObject uiObj = Instantiate(ammoCounterPrefab, ammoCounterPanel);
            
            // 프리팹에서 AmmoCounter 컴포넌트를 가져오거나 없으면 새로 붙여줍니다.
            AmmoCounter counter = uiObj.GetComponent<AmmoCounter>();
            if (counter == null)
            {
                counter = uiObj.AddComponent<AmmoCounter>();
            }
            
            // PlayerWeaponManager의 슬롯 배열을 순회하며 방금 획득한 무기가 몇 번째 인덱스인지 찾습니다.
            int idx = -1;
            for (int i = 0; i < weaponManager.weaponSlots.Length; i++)
            {
                if (weaponManager.weaponSlots[i] == newWeapon)
                {
                    idx = i;
                    break;
                }
            }
            
            // AmmoCounter에게 자신의 무기 정보와 인덱스 번호를 전달하여 초기화합니다.
            counter.Initialize(newWeapon, idx);
            
            // 딕셔너리에 추가하여 나중에 지우거나 갱신할 때 쉽게 찾을 수 있도록 합니다.
            weaponUIs.Add(newWeapon, counter);

            // UI가 추가된 직후, 현재 활성화된 무기를 기준으로 UI들의 크기와 투명도를 다시 정렬합니다.
            SwitchWeaponUI(weaponManager.GetActiveWeapon());
        }

        /// <summary>
        /// 들고 있던 무기를 버리거나 삭제할 때 해당 UI를 지웁니다.
        /// </summary>
        private void RemoveWeaponUI(WeaponController oldWeapon)
        {
            // 딕셔너리에서 해당 무기에 연결된 UI를 찾아냅니다.
            if (weaponUIs.TryGetValue(oldWeapon, out AmmoCounter counter))
            {
                // UI 게임 오브젝트를 파괴하고 딕셔너리에서 제거합니다.
                Destroy(counter.gameObject);
                weaponUIs.Remove(oldWeapon);
            }
        }

        /// <summary>
        /// 마우스 휠이나 숫자키로 손에 쥐는 무기가 바뀌었을 때 호출됩니다.
        /// </summary>
        private void SwitchWeaponUI(WeaponController activeWeapon)
        {
            // 현재 화면에 있는 모든 무기 UI들을 순회합니다.
            foreach (var kvp in weaponUIs)
            {
                // 현재 검사 중인 UI의 무기(Key)가 유저가 들고 있는 무기(activeWeapon)인지 확인합니다.
                bool isActive = (kvp.Key == activeWeapon);
                
                // 해당 UI에게 활성화 여부를 전달하여 크기와 알파값(투명도)을 연출하게 합니다.
                kvp.Value.SetActiveWeapon(isActive);
            }
        }
        #endregion
    }
}
