using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MySample
{
    /// <summary>
    /// 큐브 컬러를 흰색에서 빨간색으로 바꾸기
    /// 메테리얼 바꿔치기로 컬러 바꾸기
    /// 직접 메테리얼의 컬러를 빨간색으로 바꾸기
    /// </summary>
    public class MaterialTest : MonoBehaviour
    {
        #region Variables
        //참조
        private Renderer renderer;

        //인풋
        public InputActionReference jumpAction;

        public Material damagedMaterial;
        private Material originalMaterial;

        //메테리얼의 속성값을 관리하는 객체
        private MaterialPropertyBlock materialPropertyBlock;
        #endregion

        #region Unity Event Methods
        private void Awake()
        {
            //참조
            renderer = GetComponent<Renderer>();
        }

        private void Start()
        {
            //원래 메테리얼 저장
            originalMaterial = renderer.material;
        }

        private void OnEnable()
        {
            jumpAction.action.Enable();
        }

        private void OnDisable()
        {
            jumpAction.action.Disable();
        }

        private void Update()
        {
            //점프 버튼을 눌렀을 때
            if (jumpAction.action.WasPressedThisFrame())
            {
                //Debug.Log("Jump button pressed");
                //큐브 컬러를 빨간색으로 변경
                //ChangeMaterial();
                
                ChangeMaterialColor();
            }
        }
        #endregion

        #region Custom Methods
        private void ChangeMaterial()
        {
            renderer.material = damagedMaterial;
        }

        private void ResetMaterial()
        {
            renderer.material = originalMaterial;
        }

        // 직접 메테리얼의 컬러를 빨간색으로 바꾸기
        private void ChangeMaterialColor()
        {
            //renderer.material.SetColor("_BaseColor", Color.red);
            renderer.sharedMaterial.SetColor("_BaseColor", Color.red);
        }

        //해당 오브젝트만 컬러 변경, 배칭도 깨지지 않고
        //
        #endregion
    }
}