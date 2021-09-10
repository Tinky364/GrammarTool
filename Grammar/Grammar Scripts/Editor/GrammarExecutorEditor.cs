using Grammar.Core;
using UnityEditor;
using UnityEngine;

namespace Grammar.Editor
{
    [CustomEditor(typeof(GrammarExecutor))]
    public class GrammarExecutorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GrammarExecutor current = (GrammarExecutor)target;
            if (GUILayout.Button("Execute"))
            {
                current.Execute();
            }
            DrawDefaultInspector();
        }
    }
}
