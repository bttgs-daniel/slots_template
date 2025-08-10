using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
#endif
/*
    27.11.2020  ClearConsole()
    24.03.2021 - remove property drawers
    12.11.2021 - add session state
 */
namespace Mkey
{
    public class EditorExt
    {
#if UNITY_EDITOR
        #region temp vars

        #endregion temp vars

        #region showProperties
        public static void ShowProperties(SerializedObject serializedObject, string[] properties, bool showHierarchy)
        {
            for (int i = 0; i < properties.Length; i++)
            {
                ShowProperty(serializedObject, properties[i], showHierarchy);
            }
        }

        public static void ShowProperty(SerializedObject serializedObject, string property, bool showHierarchy)
        {
            SerializedProperty sP = serializedObject.FindProperty(property);
            if (sP != null)
            {
                EditorGUILayout.PropertyField(sP, showHierarchy);
            }
            else
            {
                EditorGUILayout.LabelField("Property no found: <" + property + ">");
            }
        }

        public static void ShowPropertiesBox(SerializedObject serializedObject, string[] properties, bool showHierarchy)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel += 1;
            EditorGUILayout.Space();
            ShowProperties(serializedObject, properties, showHierarchy);
            EditorGUILayout.Space();
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();
        }

        public static void ShowPropertiesBoxFoldOut(SerializedObject serializedObject, string bName, string[] properties, ref bool fOut, bool showHierarchy)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel += 1;
            EditorGUILayout.Space();
            if (fOut = EditorGUILayout.Foldout(fOut, bName))
            {
                ShowProperties(serializedObject, properties, showHierarchy);
            }
            EditorGUILayout.Space();
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();
        }

        public static void ShowPropertiesBoxFoldOut(SerializedObject serializedObject, string bName, string[] properties, string sessionStateName, bool showHierarchy)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel += 1;
            EditorGUILayout.Space();
            bool fOut = SessionState.GetBool(sessionStateName, false);
            if (fOut = EditorGUILayout.Foldout(fOut, bName))
            {
                ShowProperties(serializedObject, properties, showHierarchy);
            }
            SessionState.SetBool(sessionStateName, fOut);
            EditorGUILayout.Space();
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();
        }

        public static void ShowPropertiesBoxFoldOut(string bName, ref bool fOut, Action showAction)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel += 1;
            EditorGUILayout.Space();
            if (fOut = EditorGUILayout.Foldout(fOut, bName))
            {
                showAction?.Invoke();
            }
            EditorGUILayout.Space();
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();
        }

        public static void ShowPropertiesBoxFoldOut(string bName, string sessionStateName, Action showAction)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel += 1;
            EditorGUILayout.Space();
            bool fOut = SessionState.GetBool(sessionStateName, false);
            if (fOut = EditorGUILayout.Foldout(fOut, bName))
            {
                showAction?.Invoke();
            }
            SessionState.SetBool(sessionStateName, fOut);
            EditorGUILayout.Space();
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();
        }

        public static void ShowReordListBoxFoldOut(string bName, ReorderableList rList, ref bool fOut)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel += 1;
            EditorGUILayout.Space();
            if (fOut = EditorGUILayout.Foldout(fOut, bName))
            {
                rList.DoLayoutList();
            }
            EditorGUILayout.Space();
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();
        }

        public static void ShowReordListBoxFoldOut(string bName, ReorderableList rList, string sessionStateName)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel += 1;
            EditorGUILayout.Space();
            bool fOut = SessionState.GetBool(sessionStateName, false);
            if (fOut = EditorGUILayout.Foldout(fOut, bName))
            {
                rList.DoLayoutList();
            }
            SessionState.SetBool(sessionStateName, fOut);
            EditorGUILayout.Space();
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();
        }
        #endregion showProperties

        #region array
        public static void ShowList(SerializedProperty list, bool showListSize = true, bool showListLabel = true)
        {
            if (showListLabel)
            {
                EditorGUILayout.PropertyField(list);
                EditorGUI.indentLevel += 1;
            }
            if (!showListLabel || list.isExpanded)
            {
                if (showListSize)
                {
                    EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size"));
                }
                for (int i = 0; i < list.arraySize; i++)
                {
                    EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
                }
            }
            if (showListLabel)
            {
                EditorGUI.indentLevel -= 1;
            }
        }
        #endregion array

        #region showChoise EditorGuiLayOut
        public static void ShowChoiseLO(string[] ordChoises, UnityEngine.Object target, ref int choiseIndex)
        {
            if (ordChoises != null && ordChoises.Length > 0)
            {
                int _choiceIndex = choiseIndex;
                _choiceIndex = EditorGUILayout.Popup(_choiceIndex, ordChoises);
                choiseIndex = _choiceIndex;
                EditorUtility.SetDirty(target);
            }
        }
        #endregion showChoise EditorGuiLayOut

        public static bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
        {
            var position = GUILayoutUtility.GetRect(label, EditorStyles.linkLabel, options);

            Handles.BeginGUI();
            Handles.color = EditorStyles.linkLabel.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
            Handles.color = Color.white;
            Handles.EndGUI();

            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
            return GUI.Button(position, label, EditorStyles.linkLabel);
        }

        public static void ClearConsole()
        {
            var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }
#endif
    }
}
/*
   ReorderableList CreateList(SerializedObject obj, SerializedProperty prop) // https://pastebin.com/WhfRgcdC
        {
            ReorderableList list = new ReorderableList(obj, prop, true, true, true, true);

            list.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "Sprites");
            };

            List<float> heights = new List<float>(prop.arraySize);

            list.drawElementCallback = (rect, index, active, focused) => {
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                Sprite s = (element.objectReferenceValue as Sprite);

                bool foldout = active;
                float height = EditorGUIUtility.singleLineHeight * 1.25f;
                if (foldout)
                {
                    height = EditorGUIUtility.singleLineHeight * 5;
                }

                try
                {
                    heights[index] = height;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Debug.LogWarning(e.Message);
                }
                finally
                {
                    float[] floats = heights.ToArray();
                    Array.Resize(ref floats, prop.arraySize);
                    heights = new List<float> (floats);
                }

                float margin = height / 10;
                rect.y += margin;
                rect.height = (height / 5) * 4;
                rect.width = rect.width / 2 - margin / 2;

                if (foldout)
                {
                    if (s)
                    {
                        EditorGUI.DrawPreviewTexture(rect, s.texture);
                    }
                }
                rect.x += rect.width + margin;
                EditorGUI.ObjectField(rect, element, GUIContent.none);
            };

            list.elementHeightCallback = (index) => {
                Repaint();
                float height = 0;

                try
                {
                    height = heights[index];
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Debug.LogWarning(e.Message);
                }
                finally
                {
                    float[] floats = heights.ToArray();
                    Array.Resize(ref floats, prop.arraySize);
                    heights = new List<float>(floats);
                }

                return height;
            };

            list.drawElementBackgroundCallback = (rect, index, active, focused) => {
                rect.height = heights[index];
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, new Color(0.33f, 0.66f, 1f, 0.66f));
                tex.Apply();
                if (active)
                    GUI.DrawTexture(rect, tex as Texture);
            };

            list.onAddDropdownCallback = (rect, li) => {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Add Element"), false, () => {
                    serializedObject.Update();
                    li.serializedProperty.arraySize++;
                    serializedObject.ApplyModifiedProperties();
                });

                menu.ShowAsContext();

                float[] floats = heights.ToArray();
                Array.Resize(ref floats, prop.arraySize);
                heights = new List<float>(floats);
            };

            return list;
        }
 */

/*
 reordable list
    is active if any element of list is active - gray color
    is focused - blue
 */

/*
* https://catlikecoding.com/unity/tutorials/editor/custom-list/
[CustomEditor(typeof(CustomList))]

public class CustomListEditor : Editor
{

enum displayFieldType { DisplayAsAutomaticFields, DisplayAsCustomizableGUIFields }
displayFieldType DisplayFieldType;

CustomList t;
SerializedObject GetTarget;
SerializedProperty ThisList;
int ListSize;

void OnEnable()
{
    t = (CustomList)target;
    GetTarget = new SerializedObject(t);
    ThisList = GetTarget.FindProperty("MyList"); // Find the List in our script and create a refrence of it
}

public override void OnInspectorGUI()
{
    //Update our list

    GetTarget.Update();

    //Choose how to display the list<> Example purposes only
    EditorGUILayout.Space();
    EditorGUILayout.Space();
    DisplayFieldType = (displayFieldType)EditorGUILayout.EnumPopup("", DisplayFieldType);

    //Resize our list
    EditorGUILayout.Space();
    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Define the list size with a number");
    ListSize = ThisList.arraySize;
    ListSize = EditorGUILayout.IntField("List Size", ListSize);

    if (ListSize != ThisList.arraySize)
    {
        while (ListSize > ThisList.arraySize)
        {
            ThisList.InsertArrayElementAtIndex(ThisList.arraySize);
        }
        while (ListSize < ThisList.arraySize)
        {
            ThisList.DeleteArrayElementAtIndex(ThisList.arraySize - 1);
        }
    }

    EditorGUILayout.Space();
    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Or");
    EditorGUILayout.Space();
    EditorGUILayout.Space();

    //Or add a new item to the List<> with a button
    EditorGUILayout.LabelField("Add a new item with a button");

    if (GUILayout.Button("Add New"))
    {
        t.MyList.Add(new CustomList.MyClass());
    }

    EditorGUILayout.Space();
    EditorGUILayout.Space();

    //Display our list to the inspector window

    for (int i = 0; i < ThisList.arraySize; i++)
    {
        SerializedProperty MyListRef = ThisList.GetArrayElementAtIndex(i);
        SerializedProperty MyInt = MyListRef.FindPropertyRelative("AnInt");
        SerializedProperty MyFloat = MyListRef.FindPropertyRelative("AnFloat");
        SerializedProperty MyVect3 = MyListRef.FindPropertyRelative("AnVector3");
        SerializedProperty MyGO = MyListRef.FindPropertyRelative("AnGO");
        SerializedProperty MyArray = MyListRef.FindPropertyRelative("AnIntArray");


        // Display the property fields in two ways.

        if (DisplayFieldType == 0)
        {// Choose to display automatic or custom field types. This is only for example to help display automatic and custom fields.
            //1. Automatic, No customization <-- Choose me I'm automatic and easy to setup
            EditorGUILayout.LabelField("Automatic Field By Property Type");
            EditorGUILayout.PropertyField(MyGO);
            EditorGUILayout.PropertyField(MyInt);
            EditorGUILayout.PropertyField(MyFloat);
            EditorGUILayout.PropertyField(MyVect3);

            // Array fields with remove at index
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Array Fields");

            if (GUILayout.Button("Add New Index", GUILayout.MaxWidth(130), GUILayout.MaxHeight(20)))
            {
                MyArray.InsertArrayElementAtIndex(MyArray.arraySize);
                MyArray.GetArrayElementAtIndex(MyArray.arraySize - 1).intValue = 0;
            }

            for (int a = 0; a < MyArray.arraySize; a++)
            {
                EditorGUILayout.PropertyField(MyArray.GetArrayElementAtIndex(a));
                if (GUILayout.Button("Remove  (" + a.ToString() + ")", GUILayout.MaxWidth(100), GUILayout.MaxHeight(15)))
                {
                    MyArray.DeleteArrayElementAtIndex(a);
                }
            }
        }
        else
        {
            //Or

            //2 : Full custom GUI Layout <-- Choose me I can be fully customized with GUI options.
            EditorGUILayout.LabelField("Customizable Field With GUI");
            MyGO.objectReferenceValue = EditorGUILayout.ObjectField("My Custom Go", MyGO.objectReferenceValue, typeof(GameObject), true);
            MyInt.intValue = EditorGUILayout.IntField("My Custom Int", MyInt.intValue);
            MyFloat.floatValue = EditorGUILayout.FloatField("My Custom Float", MyFloat.floatValue);
            MyVect3.vector3Value = EditorGUILayout.Vector3Field("My Custom Vector 3", MyVect3.vector3Value);


            // Array fields with remove at index
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Array Fields");

            if (GUILayout.Button("Add New Index", GUILayout.MaxWidth(130), GUILayout.MaxHeight(20)))
            {
                MyArray.InsertArrayElementAtIndex(MyArray.arraySize);
                MyArray.GetArrayElementAtIndex(MyArray.arraySize - 1).intValue = 0;
            }

            for (int a = 0; a < MyArray.arraySize; a++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("My Custom Int (" + a.ToString() + ")", GUILayout.MaxWidth(120));
                MyArray.GetArrayElementAtIndex(a).intValue = EditorGUILayout.IntField("", MyArray.GetArrayElementAtIndex(a).intValue, GUILayout.MaxWidth(100));
                if (GUILayout.Button("-", GUILayout.MaxWidth(15), GUILayout.MaxHeight(15)))
                {
                    MyArray.DeleteArrayElementAtIndex(a);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space();

        //Remove this index from the List
        EditorGUILayout.LabelField("Remove an index from the List<> with a button");
        if (GUILayout.Button("Remove This Index (" + i.ToString() + ")"))
        {
            ThisList.DeleteArrayElementAtIndex(i);
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }

    //Apply the changes to our list
    GetTarget.ApplyModifiedProperties();
}
}
*/
