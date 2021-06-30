using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Test))]
public class TestEditor : Editor
{
    SerializedProperty scriptProp;
    
    public void OnEnable()
    {
        scriptProp = serializedObject.FindProperty("script");
    }
    public override void OnInspectorGUI()
    {
        //EditorGUILayout.PropertyField(scriptProp, GUIContent(""), GUILayout.ExpandWidth(true), GUILayout.Height(500));
        ((Test)target).script = EditorGUILayout.TextArea(((Test)target).script, GUILayout.ExpandWidth(true), GUILayout.Height(500));
        if(GUILayout.Button("Run", GUILayout.Width(50)))
        {
            ((Test)target).RunTest1();
        }
    }
}
