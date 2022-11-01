using Simple.DialogueTree.Editor.Views;
using Simple.DialogueTree.Nodes;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Simple.DialogueTree.Editor
{
    public class DialogueTreeEditor : EditorWindow
    {
        private DialogueTreeView treeView;
        private IMGUIContainer imguiContainer;
        private UnityEditor.Editor editor;

        [MenuItem("Tools/Dialogue Tree")]
        public static void OpenTreeEditor() => GetWindow<DialogueTreeEditor>("Dialogue Tree Editor");

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            if (Selection.activeObject as DialogueTree == null) return false;
            OpenTreeEditor();
            return true;
        }

        private void CreateGUI()
        {
            Debug.Log("CreateGUI");
            VisualTreeAsset vt = Resources.Load<VisualTreeAsset>("DialogueTreeEditor");
            vt.CloneTree(rootVisualElement);

            treeView = rootVisualElement.Q<DialogueTreeView>();
            imguiContainer = rootVisualElement.Q<IMGUIContainer>("InspectorView");
            treeView.OnNodeSelectedEvent = OnNodeSelectionChange;

            OnSelectionChange();
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnplayModeStateChanged;
            EditorApplication.playModeStateChanged += OnplayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnplayModeStateChanged;
        }
        private void OnSelectionChange()
        {
            DialogueTree tree = Selection.activeObject as DialogueTree;
            if (tree == null)
            {
                if (Selection.activeGameObject)
                {
                    DialogueTreeRunner treeRunner = Selection.activeGameObject.GetComponent<DialogueTreeRunner>();
                    if (treeRunner)
                    {
                        tree = treeRunner.tree;
                    }
                }
            }

            if (tree != null)
            {
                //CanOpenAssetInEditor
                if (Application.isPlaying || AssetDatabase.OpenAsset(tree.GetInstanceID()))
                {
                    SerializedObject so = new SerializedObject(tree);
                    rootVisualElement.Bind(so);
                    if (treeView != null)
                        treeView.PopulateViewFromTree(tree);

                    return;
                }
            }

            rootVisualElement.Unbind();

            TextField textField = rootVisualElement.Q<TextField>("DialogueTreeName");
            if (textField != null)
            {
                textField.value = string.Empty;
            }
        }
        private void OnplayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                // Occurs during the next update of the Editor application if it is in edit mode and was previously in play mode.
                case PlayModeStateChange.EnteredEditMode:
                    OnSelectionChange();
                    break;
                // Occurs when exiting edit mode, before the Editor is in play mode.
                case PlayModeStateChange.ExitingEditMode:
                    break;
                // Occurs during the next update of the Editor application if it is in play mode and was previously in edit mode.
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
                // Occurs when exiting play mode, before the Editor is in edit mode.
                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }
        private void OnNodeSelectionChange(DialogueNode node)
        {
            imguiContainer.Clear();
            DestroyImmediate(editor);
            editor = UnityEditor.Editor.CreateEditor(node);
            imguiContainer.onGUIHandler = () =>
            {
                if (editor.target)
                    editor.OnInspectorGUI();
            };
        }
    }
}