using UnityEngine;
using UnityEditor;
using Unity.FPS.Game;

namespace Unity.FPS.EditorScripts
{
    [CustomEditor(typeof(WeaponController))]
    public class WeaponControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            
            SerializedProperty shootTypeProp = serializedObject.FindProperty("shootType");
            bool isCharge = shootTypeProp != null && shootTypeProp.enumValueIndex == (int)WeaponShootType.Charge;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false; 
                
                if (iterator.name == "m_Script")
                {
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(iterator, true);
                    GUI.enabled = true;
                    continue;
                }

                if (iterator.name == "maxChargeTime")
                {
                    if (isCharge)
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                    continue;
                }

                EditorGUILayout.PropertyField(iterator, true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
