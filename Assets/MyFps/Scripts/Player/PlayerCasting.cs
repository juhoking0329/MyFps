using UnityEngine;

namespace MyFps
{
    /// <summary>
    /// 플레이어 정면에 있는 오브젝트와의 거리 구하기
    /// </summary>
    public class PlayerCasting : MonoBehaviour
    {
        #region Variables
        private static float distanceFromTarget = 0;
        private float castingDistance = 100f;

        //디버깅
        public float toTarget;
        #endregion

        #region Property
        public static float DistanceFromTarget => distanceFromTarget;
        #endregion

        #region Unity Event Method
        private void Update()
        {
            //충돌체 오브젝트와 거리 구하기
            RaycastHit hit;
            if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit,
                castingDistance))
            {
                //Debug.Log($"hit Object : {hit.collider.gameObject.name}");
                distanceFromTarget = hit.distance;
                toTarget = distanceFromTarget;
            }
        }

        //레이캐스트 기즈모 그리기
        private void OnDrawGizmosSelected()
        {
            RaycastHit hit;
            bool isHit = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit,
                castingDistance);

            Gizmos.color = Color.red;
            if(isHit)
            {
                Gizmos.DrawRay(transform.position, transform.forward * hit.distance);
            }
            else
            {
                Gizmos.DrawRay(transform.position, transform.forward * castingDistance);
            }
        }
        #endregion
    }
}