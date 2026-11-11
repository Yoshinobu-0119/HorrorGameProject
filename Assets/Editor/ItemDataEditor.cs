using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    private ItemData itemData;
    private List<Type> actionTypes = new();

    private void OnEnable()
    {
        itemData = (ItemData)target;

        // プロジェクト内にある IItemAction を継承したクラスを自動検出
        actionTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IItemAction).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            .ToList();
    }

    public override void OnInspectorGUI()
    {
        // 変更の監視開始
        serializedObject.Update();

        // 基本プロパティの描画（id, itemName, icon, baseSellValue, maxStackSize）
        DrawPropertiesExcluding(serializedObject, "actions");

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Item Actions", EditorStyles.boldLabel);

        // 現在登録されている Actions のリストを描画
        SerializedProperty actionsProp = serializedObject.FindProperty("actions");

        for (int i = 0; i < actionsProp.arraySize; i++)
        {
            SerializedProperty elementProp = actionsProp.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();

            // クラス名を表示
            object managedRef = elementProp.managedReferenceValue;
            string typeName = managedRef != null ? managedRef.GetType().Name : "Null Action";

            elementProp.isExpanded = EditorGUILayout.Foldout(elementProp.isExpanded, $"{i + 1}. {typeName}", true);

            // 削除ボタン
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                actionsProp.DeleteArrayElementAtIndex(i);
                serializedObject.ApplyModifiedProperties();
                break;
            }

            EditorGUILayout.EndHorizontal();

            // アクションの中身（フィールド）を展開表示
            if (elementProp.isExpanded && managedRef != null)
            {
                EditorGUI.indentLevel++;

                // 子要素（Action内のプロパティ）を走査して描画
                SerializedProperty endProperty = elementProp.GetEndProperty();
                SerializedProperty currentProperty = elementProp.Copy();

                bool enterChildren = true;
                while (currentProperty.NextVisible(enterChildren) && !SerializedProperty.EqualContents(currentProperty, endProperty))
                {
                    EditorGUILayout.PropertyField(currentProperty, true);
                    enterChildren = false;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(5);

        // 新しい Action を追加するドロップダウンボタン
        if (GUILayout.Button("Add Action", GUILayout.Height(25)))
        {
            GenericMenu menu = new GenericMenu();

            foreach (Type type in actionTypes)
            {
                menu.AddItem(new GUIContent(type.Name), false, () => AddAction(type));
            }

            menu.ShowAsContext();
        }

        // 変更を保存
        serializedObject.ApplyModifiedProperties();
    }

    // アクションのインスタンス化とリストへの追加
    private void AddAction(Type type)
    {
        serializedObject.Update();

        SerializedProperty actionsProp = serializedObject.FindProperty("actions");
        int newIndex = actionsProp.arraySize;
        actionsProp.InsertArrayElementAtIndex(newIndex);

        // 新しいクラスのインスタンスを作成して割り当て
        SerializedProperty newElement = actionsProp.GetArrayElementAtIndex(newIndex);
        newElement.managedReferenceValue = Activator.CreateInstance(type);

        serializedObject.ApplyModifiedProperties();
    }
}