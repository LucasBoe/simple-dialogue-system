using Simple.DialogueTree.Nodes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Simple.DialogueTree
{
    public class DialogueTreeRunner : MonoBehaviour
    {
        [SerializeField] public DialogueTree tree;
        [SerializeField] DialogueTreeTextProcessor textProcessor;

        string uiText = "";
        System.Action nextNodeAction;

        List<TreeRunnerButton> buttons = new List<TreeRunnerButton>();

        private void Start()
        {
            if (tree == null) return;
            HandleNode(tree.rootNode);
        }

        private void HandleNode(DialogueNode node)
        {
            if (node as Line != null)
            {
                buttons.Clear();

                Line line = (Line)node;
                uiText = textProcessor.GetText(line);
                nextNodeAction = () => HandleNode(line.Next);
            }
            else if (node as Choice != null)
            {
                uiText = null;
                nextNodeAction = null;

                Choice choice = (Choice)node;
                buttons = new List<TreeRunnerButton>();

                foreach (ChoiceOption option in choice.Options)
                {
                    buttons.Add(new TreeRunnerButton() { Text = textProcessor.GetText(option), Action = () => HandleNode(option.Next) });
                }
            }
            else
            {
                buttons.Clear();
                uiText = null;
                nextNodeAction = null;
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Box(uiText);
            if (nextNodeAction != null)
            {
                if (GUILayout.Button("next"))
                {
                    nextNodeAction?.Invoke();
                }
            }
            GUILayout.EndHorizontal();
            for (int i = 0; i < buttons.Count; i++)
            {
                TreeRunnerButton button = buttons[i];
                if (GUILayout.Button(button.Text))
                    button.Action?.Invoke();
            }
        }

        private class TreeRunnerButton
        {
            public string Text;
            public System.Action Action;
        }
    }
}