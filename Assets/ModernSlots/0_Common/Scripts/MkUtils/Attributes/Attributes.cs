using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
    using UnityEditor;
#endif

/*
  24.03.2021 - add property drawers
 */
namespace Mkey
{
    public class NamedArrayAttribute : PropertyAttribute
    {
        public readonly string[] names;
        public NamedArrayAttribute(string[] names) { this.names = names; }
    }

    public class ArrayElementTitleAttribute : PropertyAttribute
    {
        public string Varname;
        public ArrayElementTitleAttribute(string ElementTitleVar)
        {
            Varname = ElementTitleVar;
        }
    }

    public class ShowOnlyAttribute : PropertyAttribute
    {
        // https://answers.unity.com/questions/489942/how-to-make-a-readonly-property-in-inspector.html
    }

    public class ReadOnlyAttribute : PropertyAttribute
    {
        // https://answers.unity.com/questions/489942/how-to-make-a-readonly-property-in-inspector.html
    }

    public class ShowIfTrueAttribute : PropertyAttribute
    {
        public string Varname;

        public ShowIfTrueAttribute(string boolVariable)
        {
            Varname = boolVariable;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(NamedArrayAttribute))]
    public class NamedArrayDrawer : PropertyDrawer
    {
        //https://forum.unity.com/threads/how-to-change-the-name-of-list-elements-in-the-inspector.448910/
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            try
            {
                int pos = int.Parse(property.propertyPath.Split('[', ']')[1]);
                EditorGUI.ObjectField(rect, property, new GUIContent(((NamedArrayAttribute)attribute).names[pos]));
            }
            catch
            {
                EditorGUI.ObjectField(rect, property, label);
            }
        }
    }

    [CustomPropertyDrawer(typeof(ArrayElementTitleAttribute))]
    public class ArrayElementTitleDrawer : PropertyDrawer
    {
        /*
            https://forum.unity.com/threads/how-to-change-the-name-of-list-elements-in-the-inspector.448910/
        */

        private SerializedProperty TitleNameProp;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        protected virtual ArrayElementTitleAttribute Atribute
        {
            get { return (ArrayElementTitleAttribute)attribute; }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string FullPathName = property.propertyPath + "." + Atribute.Varname;
            TitleNameProp = property.serializedObject.FindProperty(FullPathName);
            string newlabel = GetTitle();
            if (string.IsNullOrEmpty(newlabel))
                newlabel = label.text;
            EditorGUI.PropertyField(position, property, new GUIContent(newlabel, label.tooltip), true);
        }

        private string GetTitle()
        {
            switch (TitleNameProp.propertyType)
            {
                case SerializedPropertyType.Generic:
                    break;
                case SerializedPropertyType.Integer:
                    return TitleNameProp.intValue.ToString();
                case SerializedPropertyType.Boolean:
                    return TitleNameProp.boolValue.ToString();
                case SerializedPropertyType.Float:
                    return TitleNameProp.floatValue.ToString();
                case SerializedPropertyType.String:
                    return TitleNameProp.stringValue;
                case SerializedPropertyType.Color:
                    return TitleNameProp.colorValue.ToString();
                case SerializedPropertyType.ObjectReference:
                    {
                        return TitleNameProp.objectReferenceValue != null ? TitleNameProp.objectReferenceValue.name : "";
                    }
                case SerializedPropertyType.LayerMask:
                    break;
                case SerializedPropertyType.Enum:
                    return TitleNameProp.enumNames[TitleNameProp.enumValueIndex];
                case SerializedPropertyType.Vector2:
                    return TitleNameProp.vector2Value.ToString();
                case SerializedPropertyType.Vector3:
                    return TitleNameProp.vector3Value.ToString();
                case SerializedPropertyType.Vector4:
                    return TitleNameProp.vector4Value.ToString();
                case SerializedPropertyType.Rect:
                    break;
                case SerializedPropertyType.ArraySize:
                    break;
                case SerializedPropertyType.Character:
                    break;
                case SerializedPropertyType.AnimationCurve:
                    break;
                case SerializedPropertyType.Bounds:
                    break;
                case SerializedPropertyType.Gradient:
                    break;
                case SerializedPropertyType.Quaternion:
                    break;
                default:
                    break;
            }
            return "";
        }
    }

    [CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
    public class ShowOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            string valueStr;
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    valueStr = prop.intValue.ToString();
                    break;
                case SerializedPropertyType.Boolean:
                    valueStr = prop.boolValue.ToString();
                    break;
                case SerializedPropertyType.Float:
                    valueStr = prop.floatValue.ToString("0.00000");
                    break;
                case SerializedPropertyType.String:
                    valueStr = prop.stringValue;
                    break;
                default:
                    valueStr = "(not supported)";
                    break;
            }

            EditorGUI.LabelField(position, label.text, valueStr);
        }
    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }

    [CustomPropertyDrawer(typeof(ShowIfTrueAttribute))]
    public class ShowIfTrueDrawer : PropertyDrawer
    {
        bool hide = false;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (!hide) ?  EditorGUI.GetPropertyHeight(property, label, true) : 0;
        }

        protected virtual ShowIfTrueAttribute Attribute
        {
            get { return (ShowIfTrueAttribute)attribute; }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedObject  sO = property.serializedObject;
            SerializedProperty boolProp = sO.FindProperty(Attribute.Varname);
            hide = !((boolProp != null && boolProp.boolValue) || (boolProp == null));
            if (!hide) EditorGUI.PropertyField(position, property, label, true);
        }
    }
#endif
}
