using System.Collections;
using UnityEngine;

namespace MyFps
{
    /// <summary>
    /// 두번째 문 트리거 클래스
    /// 플레이어가 트리거에 들어오면 문이 열리고, 나가면 몇초 뒤에 문이 닫힌다
    /// 문이 열리면 적이 활성화된다
    /// </summary>
    public class TwoDoorTrigger : MonoBehaviour
    {
        #region Variables
        //참조
        [Header("참조")]
        [SerializeField] private Animator doorAnimator;     //문 애니메이터
        [SerializeField] private GameObject[] enemies;      //활성화할 적 오브젝트 배열

        //문 닫힘 딜레이
        [Header("문 닫힘 딜레이")]
        [SerializeField] private float closeDelay = 3f;     //몇초 뒤에 닫힐지 인스펙터에서 조절

        //적 활성화 여부 (한번만 활성화)
        private bool isEnemySpawned = false;
        #endregion

        #region Unity Event Method
        private void Start()
        {
            //적 비활성화
            SetEnemies(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            //플레이어가 트리거에 들어오면 문 열기
            if (other.CompareTag("Player"))
            {
                OpenDoor();

                //한번만 적 활성화
                if (!isEnemySpawned)
                {
                    isEnemySpawned = true;
                    SetEnemies(true);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            //플레이어가 트리거에서 나가면 딜레이 후 문 닫기
            if (other.CompareTag("Player"))
                StartCoroutine(CloseAfterDelay());
        }
        #endregion

        #region Custom Method
        void OpenDoor()
        {
            //문 열기 애니메이션 실행
            StopAllCoroutines();
            if (doorAnimator != null)
                doorAnimator.SetBool("IsOpen", true);
        }

        IEnumerator CloseAfterDelay()
        {
            //딜레이 후 문 닫기
            yield return new WaitForSeconds(closeDelay);

            if (doorAnimator != null)
                doorAnimator.SetBool("IsOpen", false);
        }

        void SetEnemies(bool isActive)
        {
            //적 활성화/비활성화
            foreach (GameObject enemy in enemies)
            {
                if (enemy != null)
                    enemy.SetActive(isActive);
            }
        }
        #endregion
    }
}