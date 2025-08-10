using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

/*
    05.03.2021
 */
namespace Mkey
{
	public class ExpectNotNullAttribute : PropertyAttribute
	{
		
	}

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ExpectNotNullAttribute))]
    public class ExpectNotNullDrawer : PropertyDrawer 
    {
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, prop); // without begin - get error
            if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue == null)
            {
                // EditorGUI.HelpBox(position, "NULL refernce", MessageType.Error); // EditorGUI.LabelField(position, label.text, valueStr);
                GUI.color = Color.red;
                GUIContent gc = new GUIContent("Null reference: " + prop.displayName);
                EditorGUI.PropertyField(position, prop, gc);
            }
            else
            {
                EditorGUI.PropertyField(position, prop);
            }
            EditorGUI.EndProperty();
        }
    }

#endif
}
