using System;
using System.Linq;
using Assets.Interaction;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Editor
{
    [CustomEditor(typeof(Hotkey))]
    class HotkeyEditor : UnityEditor.Editor
    {
        private RtsInputActions mInputActions;
        private SerializedProperty mActionNameProperty;
        private void OnEnable()
        {
            mInputActions = new RtsInputActions();
            mActionNameProperty = serializedObject.FindProperty(nameof(Hotkey.ActionName));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var actionName = mActionNameProperty.stringValue;

            var array = mInputActions.Bindings.Get().actions
                .Select(a => a.name)
                .ToArray();
            var newIndex = EditorGUILayout.Popup("Action", Array.IndexOf(array, actionName), array);
            if (newIndex >= 0 && newIndex < array.Length)
                mActionNameProperty.stringValue = array[newIndex];

            serializedObject.ApplyModifiedProperties();
        }
    }
}