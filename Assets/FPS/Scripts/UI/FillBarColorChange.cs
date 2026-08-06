using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    /// <summary>
    /// 게이지바 이미지의 컬러를 관리하는 클래스
    /// </summary> 
    public class FillBarColorChange : MonoBehaviour
    {
        #region Variables
        public Image foregroundImage;                   // 전경 이미지 (FillImage)

        public Color defaultForegroundColorFull;        // 기본 전경색 (100% 충전 시)
        public Color flashForegroundColorFull;          // 100% 충전 시 깜빡이는 전경색

        public Image backgroundImage;                   // 배경 이미지 (BackgroundImage)

        public Color defaultBackgroundColorFull;        // 기본 배경색 (100% 충전 시)
        public Color flashBackgroundColorEmpty;         // 탄환이 다 떨어졌을 때 깜빡이는 배경색

        public float fullValue = 1f;                    // 게이지가 100%일 때의 값
        public float emptyValue = 0f;                   // 게이지가 0%일 때의 값

        public float colorChangeSharpness = 5f;         // 색상 변화의 선명도 (Lerp 속도)
        private float m_PriviousValue = 0f;             // 이전 프레임의 게이지 값
        #endregion

        #region Unity Event Methods

        #endregion


        #region Custom Methods
        //초기화
        public void Initialize(float fullValueRatio, float emptyValueRatio)
        {
            fullValue = fullValueRatio;
            emptyValue = emptyValueRatio;

            m_PriviousValue = fullValueRatio;
        }

        //update 함수
        public void UpdateVisual(float currentRatio)
        {
            // 게이지가 풀로 차는 순간
            if(currentRatio == fullValue && currentRatio != m_PriviousValue)
            {
                // 전경색을 깜빡이는 색상으로 변경
                foregroundImage.color = flashForegroundColorFull;
            }
            else if(currentRatio <= emptyValue) // 게이지가 비어 있을때(특정 수치 이하로 떨어질때)
            {
                // 배경색을 깜빡이는 색상으로 변경
                backgroundImage.color = flashBackgroundColorEmpty;
            }
            else
            {
                foregroundImage.color = Color.Lerp(foregroundImage.color, defaultForegroundColorFull, Time.deltaTime * colorChangeSharpness);
                backgroundImage.color = Color.Lerp(backgroundImage.color, defaultBackgroundColorFull, Time.deltaTime * colorChangeSharpness);
            }

            m_PriviousValue = currentRatio;
        }
        #endregion
    }
}