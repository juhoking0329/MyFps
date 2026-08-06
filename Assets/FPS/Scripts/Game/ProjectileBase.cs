using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 모든 발사체들의 부모가 되는 추상 클래스
    /// </summary>
    public abstract class ProjectileBase : MonoBehaviour
    {
        #region Variables
        //발사체 발사시 등록된 함수 호출
        public UnityAction onShoot;
        #endregion

        #region Property
        public GameObject Owner { get; private set; }                   //발사체를 발사한 무기의 주인
        public Vector3 InitialPosition { get; private set; }            //발사체의 초기 위치
        public Vector3 InitialDirection { get; private set; }           //발사체의 초기 발사 방향
        public Vector3 InheritedMuzzleVelocity { get; private set; }    //발사할때의 총구의 속도
        public float InitialCharge { get; private set; }                //충전 발사체의 발사시 충전 값
        #endregion

        #region Custom Method
        //무기(WeaponController)가 발사체를 발사하는 순간 호출
        public void Shoot(WeaponController weaponController)
        {
            //속성 값 초기화
            Owner = weaponController.Owner;
            InitialPosition = transform.position;
            InitialDirection = transform.forward;
            InheritedMuzzleVelocity = weaponController.MuzzleWorldVelocity;
            InitialCharge = weaponController.CurrentCharge;

            //등록된 함수 호출
            onShoot?.Invoke();
        }

        // AI 등에서 직접 속성을 지정하여 발사할 때 호출
        public void Shoot(GameObject owner, Vector3 direction, Vector3 inheritedVelocity, float charge = 0f)
        {
            //속성 값 초기화
            Owner = owner;
            InitialPosition = transform.position;
            InitialDirection = direction;
            InheritedMuzzleVelocity = inheritedVelocity;
            InitialCharge = charge;

            //등록된 함수 호출
            onShoot?.Invoke();
        }
        #endregion
    }
}