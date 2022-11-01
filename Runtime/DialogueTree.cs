// DialogueTree.cs
// 05-05-2022
// James LaFritz

using System.Collections.Generic;
using System.Linq;
using Simple.DialogueTree.Nodes;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using System;

namespace Simple.DialogueTree
{
    [CreateAssetMenu(fileName = "DialogueTree", menuName = "Simple Dialogue System/Dialogue Tree")]
    [System.Serializable]
    public class DialogueTree : ScriptableObject, ILocalizeableTextContainer
    {
        [HideInInspector] public DialogueNode rootNode;
        [SerializeField, ReadOnly] private List<DialogueNode> nodes = new List<DialogueNode>();
        public List<DialogueNode> GetNodes() => nodes;
        public string ContainerName => name;
        public DialogueNode CreateNode(System.Type type)
        {
            DialogueNode node = CreateInstance(type) as DialogueNode;
            node.Create(type.Name);

            nodes.Add(node);

            if (rootNode == null)
                rootNode = node;

            return node;
        }

        public void DeleteNode(DialogueNode node)
        {
            nodes.Remove(node);

            if (rootNode == node)
            {
                rootNode = null;

                if (nodes.Count > 0)
                {
                    rootNode = nodes[0];
                }
            }
        }

        public void AddChild(DialogueNode from, DialogueNode to, int optionIndex)
        {
            //node does not exist? return
            if (!nodes.Contains(from)) return;

            //link nodes
            nodes[nodes.IndexOf(from)].SetNextNode(optionIndex, to);

            //target node is new node? add to node list
            if (!nodes.Contains(to))
                nodes.Add(to);
        }

        public void RemoveChild(DialogueNode parent, DialogueNode child)
        {
            if (!nodes.Contains(parent)) return;

            nodes[nodes.IndexOf(parent)].RemoveAsNextNode(child);
        }

        public List<DialogueNode> GetNextNodes(DialogueNode previousNode)
        {
            return !nodes.Contains(previousNode)
                ? new List<DialogueNode>()
                : nodes[nodes.IndexOf(previousNode)].GetNextNodes();
        }

        public ILocalizeableText[] GetAllChilds()
        {
            List<ILocalizeableText> result = new List<ILocalizeableText>();

            foreach (DialogueNode node in nodes)
            {
                if (node as ILocalizeableText != null)
                    result.Add(node as ILocalizeableText);
                else if (node as ILocalizeableTextContainer != null)
                    result.AddRange((node as ILocalizeableTextContainer).GetAllChilds());
            }

            return result.ToArray();
        }
    }
}