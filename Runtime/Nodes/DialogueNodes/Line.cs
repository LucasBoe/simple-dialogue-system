using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Simple.DialogueTree.Nodes
{
    /// <summary>
    /// <see cref="DialogueNode"/> that contains a single line of text.
    /// </summary>
    [System.Serializable]
    public class Line : DialogueNode, ILocalizeableText
    {
        /// <value>
        /// The <see cref="DialogueNode"/> to Augment.
        /// </value>
        [SerializeField, ReadOnly] protected DialogueNode next;
        public DialogueNode Next => next;

        [SerializeField, ReadOnly] public string Text;
        string ILocalizeableText.TextValue => Text;

        [SerializeField, ReadOnly] private bool isLocalized = false;
        public bool IsLocalized { get => isLocalized; }
        public SerializedProperty TextProperty { get => new SerializedObject(this).FindProperty("Text"); }
        public string Guid => base.guid;
        public string GetValue() => Text;
        public override void Create(string name)
        {
            this.name = name;
            GenerateGUID();
            ILocalizableEvents.OnCreateAction?.Invoke(guid);

            if (DialogueTreeTextProcessor.FindIsValidKey(guid))
            {
                SetLocalized(true, guid);
            }
        }
        public void SetLocalized(bool localized, string key = "")
        {
            isLocalized = localized;
            if (localized) Text = key;
        }
        private void OnDestroy() => ILocalizableEvents.OnDestroyAction?.Invoke(guid);

        #region Overrides of Node

        /// <inheritdoc />
        public override void SetNextNode(int optionIndex, DialogueNode nextNode)
        {
            next = nextNode;
        }

        /// <inheritdoc />
        public override void RemoveAsNextNode(DialogueNode childNode)
        {
            if (next == childNode)
                next = null;
        }

        /// <inheritdoc />
        public override List<DialogueNode> GetNextNodes()
        {
            return new List<DialogueNode> { next };
        }

        public override Dictionary<int, DialogueNode> GetNextNodeInfos()
        {
            Dictionary<int, DialogueNode> nodeInfos = new Dictionary<int, DialogueNode>();
            nodeInfos.Add(0, next);
            return nodeInfos;
        }

        #endregion
    }
}