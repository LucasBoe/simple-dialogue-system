using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Simple.DialogueTree.Nodes
{
    /// <summary>
    /// Base class for all nodes in the Behavior tree.
    /// </summary>
    [System.Serializable]
    public abstract class DialogueNode : ScriptableObject
    {
        /// <value>
        /// The Position in the Behavior Tree View that this Node is at.
        /// </value>
        [HideInInspector] public Vector2 nodeGraphPosition;

        /// <value>
        /// Does this node have more then one parent.
        /// </value>
        [HideInInspector] public bool hasMultipleParents;

        [HideInInspector] public string guid;

        public void GenerateGUID()
        {
            guid = UnityEditor.GUID.Generate().ToString();
        }

        public abstract void Create(string name);

        #region Abstract Methods

        /// <summary>
        /// Add the child node to this node.
        /// </summary>
        /// <param name="nextNode">The Node to add as a Child.</param>
        public abstract void SetNextNode(int optionIndex, DialogueNode nextNode);

        /// <summary>
        /// Remove a Child from the Node.
        /// </summary>
        /// <param name="nextNode">The Child to remove.</param>
        public abstract void RemoveAsNextNode(DialogueNode nextNode);

        /// <summary>
        /// Get a list of children the node contains.
        /// </summary>
        /// <returns>A list of children Nodes.</returns>
        public abstract List<DialogueNode> GetNextNodes();

        #endregion

        /// <summary>
        /// Clone the Node.
        /// </summary>
        /// <returns>A Clone of the Node.</returns>
        public DialogueNode Clone()
        {
            return Instantiate(this);
        }

        public abstract Dictionary<int, DialogueNode> GetNextNodeInfos();
        public virtual List<ScriptableObject> GetContainedObjects() => new List<ScriptableObject>();
    }
}
