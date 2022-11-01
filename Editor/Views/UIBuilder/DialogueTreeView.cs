using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using DialogueNode = Simple.DialogueTree.Nodes.DialogueNode;
using System.Linq;

namespace Simple.DialogueTree.Editor.Views
{
    public class DialogueTreeView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<DialogueTreeView, UxmlTraits> { }
        public Action<DialogueNode> OnNodeSelectedEvent;

        private DialogueTree dialogueTree;
        private bool hasTree => dialogueTree != null;

        public DialogueTreeView()
        {
            style.flexGrow = 1;
            Insert(0, new GridBackground() { name = "grid_background" });
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        public void PopulateViewFromTree(DialogueTree tree)
        {
            dialogueTree = tree;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            if (!hasTree) return;

            dialogueTree.GetNodes().ForEach(n =>
            {
                CreateViewForNode(n, this);
            });

            var edgesQuery = from singleNode in dialogueTree.GetNodes()
                         from nexts in singleNode.GetNextNodeInfos()
                         where nexts.Value != null
                         let nextNodeVisuals = GetNodeByGuid(nexts.Value.guid) as DialogueTreeNodeView
                         let nodeVisual = GetNodeByGuid(singleNode.guid) as DialogueTreeNodeView
                         select nextNodeVisuals.Input.ConnectTo(nodeVisual.AllOutputs[nexts.Key]);

            foreach (Edge edge in edgesQuery)
            {
                AddElement(edge);
            }
        }

        internal void ForceVisualUpdate()
        {
            if (hasTree) PopulateViewFromTree(dialogueTree);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                foreach (GraphElement element in graphViewChange.elementsToRemove)
                {
                    switch (element)
                    {
                        case DialogueTreeNodeView nodeView:
                            DeleteNode(nodeView.Node);
                            break;
                        case Edge edge:
                            {
                                DialogueTreeNodeView parentView = edge.output.node as DialogueTreeNodeView;
                                DialogueTreeNodeView childView = edge.input.node as DialogueTreeNodeView;
                                dialogueTree.RemoveChild(parentView.Node, childView.Node);
                                int count = (childView.Input.connections ?? Array.Empty<Edge>()).Count();
                                childView.Node.hasMultipleParents = count > 2;

                                break;
                            }
                    }
                }
            }

            if (graphViewChange.edgesToCreate != null && hasTree)
            {
                foreach (Edge edge in graphViewChange.edgesToCreate)
                {
                    DialogueTreeNodeView from = edge.output.node as DialogueTreeNodeView;
                    DialogueTreeNodeView to = edge.input.node as DialogueTreeNodeView;

                    int choiceIndex = GetChoiceIndexFromEdge(edge);

                    dialogueTree.AddChild(from.Node, to.Node, choiceIndex);

                    int count = (to.Input.connections ?? Array.Empty<Edge>()).Count();
                    to.Node.hasMultipleParents = count > 0;
                }
            }

            return graphViewChange;
        }

        private int GetChoiceIndexFromEdge(Edge edge)
        {
            int index = -1;

            int.TryParse(edge.output.parent.parent.name, out index);

            Debug.Log(index);

            return index - 1;
        }

        private void CreateViewForNode(DialogueNode node, DialogueTreeView treeView)
        {
            DialogueTreeNodeView nodeView = null;

            if (node as Simple.DialogueTree.Nodes.Choice != null)
            {
                nodeView = new DialogueTreeChoiceNodeView(node as Simple.DialogueTree.Nodes.Choice, treeView)
                {
                    OnNodeSelectedAction = OnNodeSelectedEvent
                };
            } else if (node as Simple.DialogueTree.Nodes.Line != null)
            {
                nodeView = new DialogueTreeLineNodeView(node as Simple.DialogueTree.Nodes.Line, treeView)
                {
                    OnNodeSelectedAction = OnNodeSelectedEvent
                };
            }

            if (nodeView == null) return;

            if (hasTree)
                nodeView.OnSetRootNodeAction = _ => dialogueTree.rootNode = node;

            AddElement(nodeView);
        }

        private void CreateNodeWithViewFromType(Type type)
        {
            if (!hasTree) return;

            DialogueNode node = dialogueTree.CreateNode(type);
            CreateViewForNode(node, this);
            Undo.RecordObject(dialogueTree, "Dialogue Tree (Create Node)");

            if (Application.isPlaying) return;

            AssetDatabase.AddObjectToAsset(node, dialogueTree);
            AssetDatabase.SaveAssets();

            Undo.RegisterCreatedObjectUndo(node, "Dialogue Tree (Create Node)");
            EditorUtility.SetDirty(node);
        }

        private void DeleteNode(DialogueNode node)
        {
            if (!hasTree) return;

            dialogueTree.DeleteNode(node);
            Undo.RecordObject(dialogueTree, "Dialogue Tree (Delete Node)");
            foreach (ScriptableObject child in node.GetContainedObjects())
            {
                Undo.DestroyObjectImmediate(child);
            }
            Undo.DestroyObjectImmediate(node);
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(dialogueTree);
        }
        private void UndoRedoPerformed()
        {
            PopulateViewFromTree(dialogueTree);
            AssetDatabase.SaveAssets();
        }

        #region Overrides of GraphView

        /// <summary>
        /// Override <a href="https://docs.unity3d.com/ScriptReference/Experimental.GraphView.GraphView.BuildContextualMenu.html" rel="external">UnityEditor.Experimental.GraphView.GraphView.BuildContextualMenu</a>
        /// Add menu items to the contextual menu.
        /// </summary>
        /// <param name="evt">The (<a href="https://docs.unity3d.com/2021.3/Documentation/ScriptReference/UIElements.ContextualMenuPopulateEvent.html" rel="external">UnityEngine.UIElements.ContextualMenuPopulateEvent</a>) event holding the menu to populate.</param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Add Choice", _ => CreateNodeWithViewFromType(typeof(Simple.DialogueTree.Nodes.Choice)));
            evt.menu.AppendAction("Add Line", _ => CreateNodeWithViewFromType(typeof(Simple.DialogueTree.Nodes.Line)));

            //TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<Node>();
            //foreach (Type type in types)
            //{
            //    if (type.IsAbstract) continue;
            //    evt.menu.AppendAction($"{type.BaseType.Name}/{type.Name}",
            //                          _ => CreateNode(type));
            //}

            base.BuildContextualMenu(evt);
        }

        /// <summary>
        /// Override <a href="https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Experimental.GraphView.GraphView.GetCompatiblePorts.html" rel="external">UnityEditor.Experimental.GraphView.GraphView.GetCompatiblePorts</a>
        /// Get all ports compatible with given port.
        /// </summary>
        /// <param name="startPort">
        /// <a href="https://docs.unity3d.com/ScriptReference/Experimental.GraphView.Port.html" rel="external">UnityEditor.Experimental.GraphView.Port</a>
        /// Start port to validate against.
        /// </param>
        /// <param name="nodeAdapter">
        /// <a href="https://docs.unity3d.com/ScriptReference/Experimental.GraphView.Port.html" rel="external">UnityEditor.Experimental.GraphView.Port</a>
        /// Node adapter.
        /// </param>
        /// <returns>List of <a href="https://docs.unity3d.com/ScriptReference/Experimental.GraphView.NodeAdapter.html" rel="external">UnityEditor.Experimental.GraphView.NodeAdapter</a> List of compatible ports.</returns>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList()!.Where(endPort =>
                                             endPort.direction != startPort.direction &&
                                             endPort.node != startPort.node &&
                                             endPort.portType == startPort.portType).ToList();
        }

        #endregion
    }
}