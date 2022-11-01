using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Simple.DialogueTree.Nodes;
using UnityEditor.Experimental.GraphView;
using DialogueNode = Simple.DialogueTree.Nodes.DialogueNode;

namespace Simple.DialogueTree.Editor.Views
{
    public class DialogueTreeNodeView : UnityEditor.Experimental.GraphView.Node
    {

        private DialogueNode node;
        public DialogueNode Node => node;
        protected DialogueTreeView Tree;

        public Action<DialogueNode> OnNodeSelectedAction;
        public Action<DialogueNode> OnSetRootNodeAction;

        public Port Input;
        protected virtual List<Port> GetAllOutputs() => new List<Port>();
        public List<Port> AllOutputs => GetAllOutputs();

        public DialogueTreeNodeView(DialogueNode node, DialogueTreeView tree, string uiFile) : base(uiFile)
        {
            this.Tree = tree;
            this.node = node;

            if (this.node == null) return;

            base.title = this.node.GetType().Name;
            viewDataKey = this.node.guid;
            style.left = this.node.nodeGraphPosition.x;
            style.top = this.node.nodeGraphPosition.y;

            CreateInputPorts();
            SetupClasses();
        }

        private void SetupClasses()
        {
            if (node as Choice != null)
            {
                AddToClassList("choice");
            }
            else if (node as Line != null)
            {
                AddToClassList("line");
            }
        }

        private void CreateInputPorts()
        {
            Input = InstantiatePort(Orientation.Horizontal, Direction.Input,
                                    Port.Capacity.Multi, typeof(DialogueNode));
            if (Input == null) return;
            Input.portName = "";
            Input.name = "input-port";
            inputContainer.Add(Input);
        }

        #region Overrides of Node
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Undo.RecordObject(node, "Dialogue Tree (Set Position)");
            node.nodeGraphPosition.x = newPos.xMin;
            node.nodeGraphPosition.y = newPos.yMin;
            EditorUtility.SetDirty(node);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction($"Set as Root Node", _ => OnSetRootNodeAction?.Invoke(node));
            base.BuildContextualMenu(evt);
        }

        #endregion

        #region Overrides of GraphElement

        public override void OnSelected()
        {
            OnNodeSelectedAction?.Invoke(node);
            base.OnSelected();
        }

        #endregion
    }
}