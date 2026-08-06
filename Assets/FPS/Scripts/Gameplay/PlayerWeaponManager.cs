using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// 플레이어가 가지고 다니는 무기<WeaponController>들을 관리하는 클래스
    /// </summary>
    public class PlayerWeaponManager : MonoBehaviour
    {
        #region Variables
        // 참조 - 인풋 처리
        private PlayerInputHandler inputHandler;

        // 무기 장착
        // 유저에게 처음 지급되는 무기 리스트 (과제 조건: 시작시 무기 3개 등록)
        public List<WeaponController> startingWeapons = new List<WeaponController>();

        // 무기가 장착될 부모 오브젝트
        public Transform weaponParentSocket;

        // 플레이어가 게임중에 들고 다닐 수 있는 무기 리스트
        public WeaponController[] weaponSlots = new WeaponController[9];
        // 무기 리스트(슬롯)을 관리하는 인덱스 - 현재 사용하고 있는 무기의 인덱스
        public int ActiveWeaponIndex { get; private set; }

        // 무기 교체 상태
        public enum WeaponSwitchState
        {
            Up,                     // 무기 들고 있는 상태
            Down,                   // 무기 내려가 있는 상태
            PutDownPrevious,        // 무기 교체하기 위해 내리려는 상태
            PutUpNew,               // 다운상태에서 무기 교체후 올리려는 상태
        }

        // 무기 교체시 등록된 함수 호출하는 이벤트 함수
        public UnityAction<WeaponController> OnSwitchToWeapon;
        public UnityAction<WeaponController> OnAddedWeapon;
        public UnityAction<WeaponController> OnRemovedWeapon;

        // 무기 교체 상태 변수
        private WeaponSwitchState weaponSwitchState;

        // 연산되는 무기의 최종 위치
        private Vector3 weaponMainLocalPosition;

        // [수정 포인트 1] TrackedReference -> Transform 으로 변경!
        public Transform defaultWeaponPosition;      // 무기 up 위치
        public Transform downWeaponPosition;         // 무기 down 위치
        public Transform aimingWeaponPosition;       // 무기 조준 위치

        // 교체 연출에 필요한 변수
        private int weaponSwitchNewWeaponIndex;
        private float weaponSwitchTimeStarted = 0f;
        [SerializeField] private float weaponSwitchDelay = 1f;

        //적 타겟팅
        public bool IsPointingAtEnemy { get; private set; }     //적 타겟팅 여부
        public Camera weaponCamera;                             //무기 전용 카메라

        //카메라
        private PlayerCharacterController playerCharacterController;
        public float defaultFov = 60f;                          //FOV 기본값
        public float weaponFovMultiplier = 1f;                  //무기 FOV 계수값

        //조준
        public bool IsAiming { get; private set; }              //조준 여부
        public float aimingAnimationSpeed = 10f;                //연출 속도

        [Header("Sniper Scope")]
        public GameObject scopeUI;
        public bool IsScopeOn { get; private set; }

        //무기 흔들림 (Weapon Bob)
        [Header("Weapon Bob")]
        public float bobFrequency = 10f;                // 흔들림 속도
        public float bobSharpness = 10f;                // 흔들림 부드러움 정도
        public float defaultBobAmount = 0.05f;          // 기본 흔들림 폭
        public float aimingBobAmount = 0.01f;           // 조준 시 흔들림 폭
        private float weaponBobFactor;                  // 싸인 곡선 누적 시간
        private Vector3 weaponBobLocalPosition;         // 최종 흔들림 위치

        //무기 반동 (Weapon Recoil)
        [Header("Weapon Recoil")]
        public float recoilSharpness = 50f;             // 뒤로 밀리는 속도 Lerp 계수
        public float maxRecoilDistance = 0.5f;          // 무기가 뒤로 밀리는 최대 거리
        public float recoilRepositionSharpness = 10f;   // 제자리로 돌아오는 속도 Lerp 계수

        public Vector3 accumulateRecoil;                // 누적된 반동 목표 위치
        public Vector3 weaponRecoilLocalPosition;       // 최종 반동 위치
        #endregion

        #region Unity Event Method
        private void Start()
        {
            // 참조
            inputHandler = GetComponent<PlayerInputHandler>();
            playerCharacterController = GetComponentInParent<PlayerCharacterController>();

            // 초기화
            ActiveWeaponIndex = -1;
            weaponSwitchState = WeaponSwitchState.Down;

            // 무기 교체 이벤트 등록
            OnSwitchToWeapon += OnWeaponSwitched;

            // 지급 받은 무기 장착하기
            foreach (var W in startingWeapons)
            {
                AddWeapon(W);
            }
            SwitchWeapon(true);

            // ScopeUI 자동 탐색
            if (scopeUI == null)
            {
                Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
                foreach (Transform t in transforms)
                {
                    if (t.name == "ScopeUI" && t.gameObject.scene.isLoaded)
                    {
                        scopeUI = t.gameObject;
                        break;
                    }
                }
            }
            if (scopeUI != null) scopeUI.SetActive(false);
        }

        private void Update()
        {
            //현재 손에 들고 있는 무기(액티브 무기) 가져오기
            WeaponController activeWeapon = GetActiveWeapon();

            //무기 교체 입력 처리 (무기를 들고 있거나 내린 상태에서만 입력 처리 가능하도록 제한)
            if (weaponSwitchState == WeaponSwitchState.Up || weaponSwitchState == WeaponSwitchState.Down)
            {
                int switchInput = inputHandler.GetSwitchWeaponInput();
                if (switchInput != 0)
                {
                    bool isAscending = switchInput > 0;
                    SwitchWeapon(isAscending);
                }
            }

            //조준 처리 (무기를 완전히 들고 있는 상태일 때만 조준 가능)
            if (weaponSwitchState == WeaponSwitchState.Up)
            {
                if (inputHandler.GetAimInputDown()) IsAiming = true;
                if (inputHandler.GetAimInputReleased()) IsAiming = false;
            }
            else
            {
                IsAiming = false;
            }

            //적 타켓팅
            IsPointingAtEnemy = false;
            if (activeWeapon)
            {
                // 발사 입력 처리 (무기가 완전히 올라온 상태일 때만 쏠 수 있음)
                if (weaponSwitchState == WeaponSwitchState.Up)
                {
                    bool fireInputDown = inputHandler.GetFireInputDown();
                    bool fireInputHeld = inputHandler.GetFireInputHeld();
                    bool fireInputReleased = inputHandler.GetFireInputReleased();

                    // 수동 재장전 입력 처리
                    if (inputHandler.GetReloadInputDown())
                    {
                        activeWeapon.StartReload();
                    }

                    bool hasFired = activeWeapon.HandleShootInputs(fireInputDown, fireInputHeld, fireInputReleased);
                    if (hasFired)
                    {
                        // 반동 트리거 (뒤로 밀림값을 누적)
                        accumulateRecoil += Vector3.back * activeWeapon.recoilForce;
                        
                        // 최대 밀림 거리 제한
                        accumulateRecoil.z = Mathf.Max(accumulateRecoil.z, -maxRecoilDistance);
                    }
                }

                if (Physics.Raycast(weaponCamera.transform.position, 
                    weaponCamera.transform.forward, out RaycastHit hit, 100f))
                {
                    //Debug.Log($"hit {hit.collider.gameObject.name}");
                    Health enemyHealth = hit.collider.GetComponentInParent<Health>();
                    if(enemyHealth)
                    {
                        IsPointingAtEnemy = true;
                    }
                }
            }
        }

        


        private void LateUpdate()
        {
            UpdateWeaponSwitching();
            UpdateWeaponAiming();
            UpdateWeaponBob();
            UpdateWeaponRecoil();

            // 무기의 최종 위치 적용 (기본위치 + 흔들림위치 + 반동위치)
            if (weaponParentSocket != null)
            {
                weaponParentSocket.localPosition = weaponMainLocalPosition + weaponBobLocalPosition + weaponRecoilLocalPosition;
            }
        }
        #endregion

        #region Custom Method
        //FOV 조정하기
        public void SetFov(float fov)
        {
            playerCharacterController.PlayerCamera.fieldOfView = fov;
            weaponCamera.fieldOfView = fov * weaponFovMultiplier;
        }

        //무기 조준 연출 : 디폴트위치 <-> 조준위치
        private void UpdateWeaponAiming()
        {
            //상태 체크 (무기가 완전히 올라온 상태에서만 위치 연산 진행)
            if (weaponSwitchState != WeaponSwitchState.Up)
                return;

            WeaponController activeWeapon = GetActiveWeapon();
            if (activeWeapon == null) return;

            bool isSniper = activeWeapon.shootType == WeaponShootType.Sniper;

            if (IsAiming)
            {
                Vector3 targetPosition = aimingWeaponPosition.localPosition + activeWeapon.aimOffset;
                weaponMainLocalPosition = Vector3.Lerp(weaponMainLocalPosition, targetPosition, aimingAnimationSpeed * Time.deltaTime);

                if (isSniper)
                {
                    float distance = Vector3.Distance(weaponMainLocalPosition, targetPosition);
                    if (distance <= 0.5f)
                    {
                        IsScopeOn = true;
                    }

                    if (IsScopeOn)
                    {
                        // 줌인 연산
                        float targetFov = activeWeapon.aimZoomRatio * defaultFov;
                        SetFov(Mathf.Lerp(playerCharacterController.PlayerCamera.fieldOfView,
                            targetFov, aimingAnimationSpeed * Time.deltaTime));

                        // 줌인이 거의 완료되었을 때만 UI 활성화 및 무기 숨김
                        if (Mathf.Abs(playerCharacterController.PlayerCamera.fieldOfView - targetFov) <= 1f)
                        {
                            if (scopeUI != null && !scopeUI.activeSelf) scopeUI.SetActive(true);
                            if (activeWeapon.weaponRoot.activeSelf) activeWeapon.weaponRoot.SetActive(false);
                        }
                    }
                    else
                    {
                        // 이동 중에는 줌인 보류
                        SetFov(Mathf.Lerp(playerCharacterController.PlayerCamera.fieldOfView,
                            defaultFov, aimingAnimationSpeed * Time.deltaTime));
                    }
                }
                else
                {
                    // 일반 무기 줌인
                    SetFov(Mathf.Lerp(playerCharacterController.PlayerCamera.fieldOfView,
                        activeWeapon.aimZoomRatio * defaultFov, aimingAnimationSpeed * Time.deltaTime));
                }
            }
            else
            {
                // 조준 끝 해제
                weaponMainLocalPosition = Vector3.Lerp(weaponMainLocalPosition,
                    defaultWeaponPosition.localPosition, aimingAnimationSpeed * Time.deltaTime);

                if (isSniper)
                {
                    IsScopeOn = false;
                    if (scopeUI != null) scopeUI.SetActive(false);
                    // 무기 교체 사운드 재생을 막기 위해 SetActive 직접 사용
                    if (!activeWeapon.weaponRoot.activeSelf && activeWeapon.IsWeaponActive)
                    {
                        activeWeapon.weaponRoot.SetActive(true);
                    }
                }

                // 줌 해제 처리
                if (playerCharacterController != null && playerCharacterController.PlayerCamera != null)
                {
                    SetFov(Mathf.Lerp(playerCharacterController.PlayerCamera.fieldOfView,
                        defaultFov, aimingAnimationSpeed * Time.deltaTime));
                }
            }
        }

        //무기 흔들림 연출 (이동 속도에 비례하여 싸인 곡선으로 계산)
        private void UpdateWeaponBob()
        {
            if (Time.deltaTime > 0f)
            {
                // 플레이어의 현재 이동 속도 비율 계산
                float characterMovementFactor = 0f;
                if (playerCharacterController != null && playerCharacterController.IsGrounded)
                {
                    Vector3 playerCharacterVelocity = playerCharacterController.CharacterVelocity;
                    characterMovementFactor = Mathf.Clamp01(playerCharacterVelocity.magnitude / playerCharacterController.MaxSpeedOnGround);
                }

                // 이동 시에만 싸인 곡선 누적
                weaponBobFactor += characterMovementFactor * bobFrequency * Time.deltaTime;

                // 조준 상태에 따라 흔들림 폭 결정
                float bobAmount = IsAiming ? aimingBobAmount : defaultBobAmount;

                // 좌우(x), 위아래(y) 흔들림 계산 (위아래는 2배 속도)
                float hBobValue = Mathf.Sin(weaponBobFactor) * bobAmount * characterMovementFactor;
                float vBobValue = Mathf.Sin(weaponBobFactor * 2f) * bobAmount * characterMovementFactor;

                Vector3 bobTargetPosition = new Vector3(hBobValue, vBobValue, 0f);

                // 현재 흔들림 위치에서 부드럽게 목표 흔들림 위치로 이동
                weaponBobLocalPosition = Vector3.Lerp(weaponBobLocalPosition, bobTargetPosition, bobSharpness * Time.deltaTime);
            }
        }

        // 무기 반동 연출 (발사 시 무기가 뒤로 밀린 뒤 서서히 제자리로 돌아옴)
        private void UpdateWeaponRecoil()
        {
            // 1. 누적된 반동 목표(accumulateRecoil)는 서서히 제자리(Vector3.zero)로 돌아옴
            accumulateRecoil = Vector3.Lerp(accumulateRecoil, Vector3.zero, recoilRepositionSharpness * Time.deltaTime);

            // 2. 실제 무기의 반동 위치는 누적된 목표 위치(accumulateRecoil)를 향해 빠르게 밀림
            weaponRecoilLocalPosition = Vector3.Lerp(weaponRecoilLocalPosition, accumulateRecoil, recoilSharpness * Time.deltaTime);
        }

        // 무기 상태 변화로 무기 교체 연출 : 디폴트위치 <-> 아래위치
        private void UpdateWeaponSwitching()
        {
            // Lerp 계수
            float switchingTimeFactor = 0f;
            if (weaponSwitchDelay == 0f)
            {
                switchingTimeFactor = 1f;
            }
            else
            {
                switchingTimeFactor = Mathf.Clamp01((Time.time - weaponSwitchTimeStarted) / weaponSwitchDelay);
            }

            // 타이머 완료
            if (switchingTimeFactor >= 1f)
            {
                if (weaponSwitchState == WeaponSwitchState.PutDownPrevious)
                {
                    // 현재 무기를 false, 새로운 무기를 true
                    WeaponController oldWeapon = GetActiveWeapon();
                    if (oldWeapon != null)
                    {
                        oldWeapon.ShowWeapon(false);
                    }
                    ActiveWeaponIndex = weaponSwitchNewWeaponIndex;
                    WeaponController newWeapon = GetWeaponAtSlotIndex(weaponSwitchNewWeaponIndex);
                    OnSwitchToWeapon?.Invoke(newWeapon);

                    // 연출 초기화
                    switchingTimeFactor = 0f;
                    if (newWeapon != null)
                    {
                        // 올라가는 연출 시작
                        weaponSwitchTimeStarted = Time.time;
                        weaponSwitchState = WeaponSwitchState.PutUpNew;
                    }
                    else
                    {
                        weaponSwitchState = WeaponSwitchState.Down;
                    }
                }
                else if (weaponSwitchState == WeaponSwitchState.PutUpNew)   // 올리는 연출 완료
                {
                    weaponSwitchState = WeaponSwitchState.Up;
                }
            }

            // [수정 포인트 2] .localPosition 호출 가능하도록 수정
            if (weaponSwitchState == WeaponSwitchState.PutDownPrevious)
            {
                weaponMainLocalPosition = Vector3.Lerp(
                    defaultWeaponPosition.localPosition,
                    downWeaponPosition.localPosition,
                    switchingTimeFactor
                );
            }
            else if (weaponSwitchState == WeaponSwitchState.PutUpNew)
            {
                weaponMainLocalPosition = Vector3.Lerp(
                    downWeaponPosition.localPosition,
                    defaultWeaponPosition.localPosition,
                    switchingTimeFactor
                );
            }
        }

        // 지급 받은 무기<WeaponController>를 무기 슬롯에 추가하기
        public bool AddWeapon(WeaponController weaponPrefab)
        {
            if (HasWeapon(weaponPrefab))
            {
                Debug.Log("Have Same Weapon !");
                return false;
            }

            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (weaponSlots[i] == null)
                {
                    WeaponController weaponInstance = Instantiate(weaponPrefab, weaponParentSocket, false);

                    weaponInstance.Owner = gameObject;
                    weaponInstance.SourcePrefab = weaponPrefab.gameObject;
                    weaponInstance.ShowWeapon(false);

                    weaponSlots[i] = weaponInstance;
                    
                    if (OnAddedWeapon != null)
                        OnAddedWeapon.Invoke(weaponInstance);
                        
                    return true;
                }
            }

            Debug.Log("Weapon Slots Full !");
            return false;
        }

        public void RemoveWeapon(WeaponController weaponInstance)
        {
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (weaponSlots[i] == weaponInstance)
                {
                    weaponSlots[i] = null;
                    if (OnRemovedWeapon != null) OnRemovedWeapon.Invoke(weaponInstance);
                    Destroy(weaponInstance.gameObject);
                    return;
                }
            }
        }

        public WeaponController HasWeapon(WeaponController weaponPrefab)
        {
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                var w = weaponSlots[i];
                if (w != null && w.SourcePrefab == weaponPrefab.gameObject)
                {
                    return w;
                }
            }
            return null;
        }

        public WeaponController GetWeaponAtSlotIndex(int index)
        {
            if (index < 0 || index >= weaponSlots.Length)
                return null;

            return weaponSlots[index];
        }

        public WeaponController GetActiveWeapon()
        {
            return GetWeaponAtSlotIndex(ActiveWeaponIndex);
        }

        public void SwitchWeapon(bool ascendingOrder)
        {
            int newWeaponIndex = -1;
            int closestSlotDistance = weaponSlots.Length;
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (i != ActiveWeaponIndex && GetWeaponAtSlotIndex(i) != null)
                {
                    int distanceToActiveIndex = GetDistanceBetweenWeaponSlots(ActiveWeaponIndex, i, ascendingOrder);
                    if (distanceToActiveIndex < closestSlotDistance)
                    {
                        closestSlotDistance = distanceToActiveIndex;
                        newWeaponIndex = i;
                    }
                }
            }

            SwitchWeaponIndex(newWeaponIndex);
        }

        private void SwitchWeaponIndex(int newWeaponIndex)
        {
            if (newWeaponIndex < 0 || newWeaponIndex >= weaponSlots.Length || newWeaponIndex == ActiveWeaponIndex)
                return;

            weaponSwitchNewWeaponIndex = newWeaponIndex;
            weaponSwitchTimeStarted = Time.time;

            if (GetActiveWeapon() == null)
            {
                // [수정 포인트 3] .localPosition 정상 접근
                weaponMainLocalPosition = downWeaponPosition.localPosition;
                weaponSwitchState = WeaponSwitchState.PutUpNew;

                ActiveWeaponIndex = weaponSwitchNewWeaponIndex;
                WeaponController newWeapon = GetWeaponAtSlotIndex(weaponSwitchNewWeaponIndex);
                OnSwitchToWeapon?.Invoke(newWeapon);
            }
            else
            {
                weaponSwitchState = WeaponSwitchState.PutDownPrevious;
            }
        }

        private int GetDistanceBetweenWeaponSlots(int fromIndex, int toIndex, bool ascendingOrder)
        {
            int distance = 0;

            if (ascendingOrder)
            {
                distance = toIndex - fromIndex;
            }
            else
            {
                distance = fromIndex - toIndex;
            }

            if (distance < 0)
            {
                distance = distance + weaponSlots.Length;
            }

            return distance;
        }

        private void OnWeaponSwitched(WeaponController newWeapon)
        {
            if (newWeapon != null)
            {
                newWeapon.ShowWeapon(true);
            }
        }
        #endregion
    }
}