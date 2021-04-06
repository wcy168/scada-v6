﻿/*
 * Copyright 2021 Rapid Software LLC
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : ScadaCommon.Forms
 * Summary  : The class provides helper methods for a TreeView control
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2016
 * Modified : 2021
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Scada.Forms
{
    /// <summary>
    /// The class provides helper methods for a TreeView control.
    /// <para>Класс, предоставляющий вспомогательные методы для работы с элементом управления TreeView.</para>
    /// </summary>
    public static class TreeViewUtils
    {
        /// <summary>
        /// Gets the child nodes of the specified parent, or the 1st level child nodes if the parent is null.
        /// </summary>
        private static TreeNodeCollection GetChildNodes(this TreeView treeView, TreeNode parentNode)
        {
            return parentNode == null ? treeView.Nodes : parentNode.Nodes;
        }


        /// <summary>
        /// Creates a new tree node.
        /// </summary>
        public static TreeNode CreateNode(string text, string imageKey, bool expand = false)
        {
            TreeNode node = new(text);
            node.SetImageKey(imageKey);

            if (expand)
                node.Expand();

            return node;
        }

        /// <summary>
        /// Creates a new tree node based on the specified tag.
        /// </summary>
        public static TreeNode CreateNode(object tag, string imageKey, bool expand = false)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            TreeNode node = CreateNode(tag.ToString(), imageKey, expand);
            node.Tag = tag;
            return node;
        }

        /// <summary>
        /// Sets the tree node image key for the selected and unselected state.
        /// </summary>
        public static void SetImageKey(this TreeNode treeNode, string imageKey)
        {
            treeNode.ImageKey = treeNode.SelectedImageKey = imageKey;
        }

        /// <summary>
        /// Recursively iterates over the tree nodes.
        /// </summary>
        public static IEnumerable<TreeNode> IterateNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                yield return node;

                foreach (TreeNode childNode in IterateNodes(node.Nodes))
                {
                    yield return childNode;
                }
            }
        }

        /// <summary>
        /// Recursively iterates over the tree nodes.
        /// </summary>
        public static IEnumerable<TreeNode> IterateNodes(TreeNode treeNode)
        {
            yield return treeNode;

            foreach (TreeNode childNode in IterateNodes(treeNode.Nodes))
            {
                yield return childNode;
            }
        }

        /// <summary>
        /// Gets an object associated with the tree node.
        /// </summary>
        public static object GetRelatedObject(TreeNode treeNode)
        {
            return treeNode?.Tag is TreeNodeTag treeNodeTag
                ? treeNodeTag.RelatedObject
                : treeNode?.Tag;
        }

        /// <summary>
        /// Gets an object associated with the selected tree node.
        /// </summary>
        public static object GetSelectedObject(this TreeView treeView)
        {
            return GetRelatedObject(treeView.SelectedNode);
        }

        /// <summary>
        /// Updates the text of the selected tree node according to the associated object.
        /// </summary>
        public static void UpdateSelectedNodeText(this TreeView treeView)
        {
            if (GetSelectedObject(treeView) is object obj)
                treeView.SelectedNode.Text = obj.ToString();
        }


        /// <summary>
        /// Finds the first tree node of the specified type among all child nodes, including the given node.
        /// </summary>
        public static TreeNode FindFirst(this TreeNode treeNode, Type tagType)
        {
            if (tagType == null)
                throw new ArgumentNullException(nameof(tagType));

            foreach (TreeNode childNode in IterateNodes(treeNode))
            {
                if (childNode.TagIs(tagType))
                    return childNode;
            }

            return null;
        }

        /// <summary>
        /// Finds the first tree node of the specified type among all child nodes, including the given node.
        /// </summary>
        public static TreeNode FindFirst(this TreeNode treeNode, string nodeType)
        {
            foreach (TreeNode childNode in IterateNodes(treeNode))
            {
                if (childNode.TagIs(nodeType))
                    return childNode;
            }

            return null;
        }

        /// <summary>
        /// Finds the closest tree node of the specified type, relative to the given node, including it,
        /// traversing up through its ancestors.
        /// </summary>
        public static TreeNode FindClosest(this TreeNode treeNode, Type tagType)
        {
            if (tagType == null)
                throw new ArgumentNullException(nameof(tagType));

            while (treeNode != null && !treeNode.TagIs(tagType))
            {
                treeNode = treeNode.Parent;
            }

            return treeNode;
        }

        /// <summary>
        /// Finds the closest tree node of the specified type, relative to the given node, including it,
        /// traversing up through its ancestors.
        /// </summary>
        public static TreeNode FindClosest(this TreeNode treeNode, string nodeType)
        {
            while (treeNode != null && !treeNode.TagIs(nodeType))
            {
                treeNode = treeNode.Parent;
            }

            return treeNode;
        }

        /// <summary>
        /// Finds a tree node of the specified type among the nodes of the same level.
        /// </summary>
        public static TreeNode FindSibling(this TreeNode treeNode, Type tagType)
        {
            if (tagType == null)
                throw new ArgumentNullException(nameof(tagType));

            if (treeNode != null && treeNode.Parent != null)
            {
                foreach (TreeNode childNode in treeNode.Parent.Nodes)
                {
                    if (childNode.TagIs(tagType))
                        return childNode;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds a tree node of the specified type among the nodes of the same level.
        /// </summary>
        public static TreeNode FindSibling(this TreeNode treeNode, string nodeType)
        {
            if (treeNode != null && treeNode.Parent != null)
            {
                foreach (TreeNode childNode in treeNode.Parent.Nodes)
                {
                    if (childNode.TagIs(nodeType))
                        return childNode;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if the tree node tag is of the specified type.
        /// </summary>
        public static bool TagIs(this TreeNode treeNode, Type tagType)
        {
            return tagType != null && tagType.IsInstanceOfType(treeNode.Tag);
        }

        /// <summary>
        /// Checks if the tree node tag is of the specified type.
        /// </summary>
        public static bool TagIs(this TreeNode treeNode, string nodeType)
        {
            return treeNode.Tag is TreeNodeTag tag && tag.NodeType == nodeType;
        }

        /// <summary>
        /// Finds a parent node and index to insert a new tree node next to a node of the same type 
        /// based on the selected node.
        /// </summary>
        public static bool FindInsertPosition(this TreeView treeView, Type tagType,
            out TreeNode parentNode, out int index)
        {
            TreeNode node = treeView.SelectedNode?.FindClosest(tagType);

            if (node == null)
            {
                parentNode = null;
                index = -1;
                return false;
            }
            else
            {
                parentNode = node.Parent;
                index = node.Index + 1;
                return true;
            }
        }

        /// <summary>
        /// Finds an index to insert a new tree node under the specified parent node after the selected node.
        /// </summary>
        public static int FindInsertIndex(this TreeView treeView, TreeNode parentNode)
        {
            TreeNode node = treeView.SelectedNode;

            while (node != null && node.Parent != parentNode)
            {
                node = node.Parent;
            }

            return node == null ? treeView.GetChildNodes(parentNode).Count : node.Index + 1;
        }


        /// <summary>
        /// Adds the specified tree node as the last child of the given parent node.
        /// </summary>
        public static void Add(this TreeView treeView, TreeNode parentNode, TreeNode nodeToAdd, 
            IList destList, object objToAdd)
        {
            if (nodeToAdd == null)
                throw new ArgumentNullException(nameof(nodeToAdd));
            if (destList == null)
                throw new ArgumentNullException(nameof(destList));
            if (objToAdd == null)
                throw new ArgumentNullException(nameof(objToAdd));

            TreeNodeCollection nodes = treeView.GetChildNodes(parentNode);
            nodes.Add(nodeToAdd);
            destList.Add(objToAdd);
            treeView.SelectedNode = nodeToAdd;
        }

        /// <summary>
        /// Adds the specified tree node as the last child of the given parent node if the nodes implement ITreeNode.
        /// </summary>
        public static void Add(this TreeView treeView, TreeNode parentNode, TreeNode nodeToAdd)
        {
            if (parentNode == null)
                throw new ArgumentNullException(nameof(parentNode));

            if (GetRelatedObject(parentNode) is ITreeNode parentObj &&
                GetRelatedObject(nodeToAdd) is ITreeNode objToAdd)
            {
                objToAdd.Parent = parentObj;
                treeView.Add(parentNode, nodeToAdd, parentObj.Children, objToAdd);
            }
        }

        /// <summary>
        /// Inserts the specified tree node as a child of the given parent node after the selected node.
        /// </summary>
        public static void Insert(this TreeView treeView, TreeNode parentNode, TreeNode nodeToInsert, 
            IList destList, object objToInsert)
        {
            if (nodeToInsert == null)
                throw new ArgumentNullException(nameof(nodeToInsert));
            if (destList == null)
                throw new ArgumentNullException(nameof(destList));
            if (objToInsert == null)
                throw new ArgumentNullException(nameof(objToInsert));

            int index = treeView.FindInsertIndex(parentNode);
            TreeNodeCollection nodes = treeView.GetChildNodes(parentNode);

            nodes.Insert(index, nodeToInsert);
            destList.Insert(index, objToInsert);
            treeView.SelectedNode = nodeToInsert;
        }

        /// <summary>
        /// Inserts the specified tree node as a child of the given parent node after the selected node 
        /// if the nodes implement ITreeNode.
        /// </summary>
        public static void Insert(this TreeView treeView, TreeNode parentNode, TreeNode nodeToInsert)
        {
            if (parentNode == null)
                throw new ArgumentNullException(nameof(parentNode));

            if (GetRelatedObject(parentNode) is ITreeNode parentObj &&
                GetRelatedObject(nodeToInsert) is ITreeNode objToInsert)
            {
                objToInsert.Parent = parentObj;
                treeView.Insert(parentNode, nodeToInsert, parentObj.Children, objToInsert);
            }
        }

        /// <summary>
        /// Moves up the selected tree node and the associated list item.
        /// </summary>
        /// <remarks>Use this method if list items do not implement ITreeNode.</remarks>
        public static void MoveUpSelectedNode(this TreeView treeView, IList list)
        {
            TreeNode selectedNode = treeView.SelectedNode;

            if (selectedNode == null)
                return;

            try
            {
                treeView.BeginUpdate();

                TreeNodeCollection siblings = treeView.GetChildNodes(selectedNode.Parent);
                int index = selectedNode.Index;
                int newIndex = index - 1;

                if (newIndex >= 0)
                {
                    siblings.RemoveAt(index);
                    siblings.Insert(newIndex, selectedNode);

                    object selectedObj = list[index];
                    list.RemoveAt(index);
                    list.Insert(newIndex, selectedObj);

                    treeView.SelectedNode = selectedNode;
                }
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Moves up the selected tree node and the associated list item.
        /// </summary>
        /// <remarks>Use this method if the underlying list items implement ITreeNode.</remarks>
        public static void MoveUpSelectedNode(this TreeView treeView, TreeNodeBehavior moveBehavior)
        {
            TreeNode selectedNode = treeView.SelectedNode;

            if (GetRelatedObject(selectedNode) is not ITreeNode selectedObj || selectedObj.Parent == null)
                return;

            try
            {
                treeView.BeginUpdate();

                TreeNodeCollection siblings = treeView.GetChildNodes(selectedNode.Parent);
                IList list = selectedObj.Parent.Children;

                int index = selectedNode.Index;
                int newIndex = index - 1;

                if (newIndex >= 0)
                {
                    siblings.RemoveAt(index);
                    siblings.Insert(newIndex, selectedNode);

                    list.RemoveAt(index);
                    list.Insert(newIndex, selectedObj);

                    treeView.SelectedNode = selectedNode;
                }
                else if (moveBehavior == TreeNodeBehavior.ThroughSimilarParents)
                {
                    TreeNode parentNode = selectedNode.Parent;
                    TreeNode parentNodePrev = parentNode?.PrevNode;

                    if (parentNode?.Tag is ITreeNode && 
                        parentNodePrev?.Tag is ITreeNode parentPrevObj &&
                        parentNode.Tag.GetType() == parentNodePrev.Tag.GetType())
                    {
                        // the node being moved gets a new parent
                        siblings.RemoveAt(index);
                        parentNodePrev.Nodes.Add(selectedNode);
                        
                        list.RemoveAt(index);
                        parentPrevObj.Children.Add(selectedObj);
                        selectedObj.Parent = parentPrevObj;

                        treeView.SelectedNode = selectedNode;
                    }
                }
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Moves down the selected tree node and the associated list item.
        /// </summary>
        /// <remarks>Use this method if list items do not implement ITreeNode.</remarks>
        public static void MoveDownSelectedNode(this TreeView treeView, IList list)
        {
            TreeNode selectedNode = treeView.SelectedNode;

            if (selectedNode == null)
                return;

            try
            {
                treeView.BeginUpdate();

                TreeNodeCollection siblings = treeView.GetChildNodes(selectedNode.Parent);
                int index = selectedNode.Index;
                int newIndex = index + 1;

                if (newIndex < siblings.Count)
                {
                    siblings.RemoveAt(index);
                    siblings.Insert(newIndex, selectedNode);

                    object selectedObj = list[index];
                    list.RemoveAt(index);
                    list.Insert(newIndex, selectedObj);

                    treeView.SelectedNode = selectedNode;
                }
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Moves down the selected tree node and the associated list item.
        /// </summary>
        /// <remarks>Use this method if the underlying list items implement ITreeNode.</remarks>
        public static void MoveDownSelectedNode(this TreeView treeView, TreeNodeBehavior moveBehavior)
        {
            TreeNode selectedNode = treeView.SelectedNode;

            if (GetRelatedObject(selectedNode) is not ITreeNode selectedObj || selectedObj.Parent == null)
                return;

            try
            {
                treeView.BeginUpdate();

                TreeNodeCollection siblings = treeView.GetChildNodes(selectedNode.Parent);
                IList list = selectedObj.Parent.Children;

                int index = selectedNode.Index;
                int newIndex = index + 1;

                if (newIndex < siblings.Count)
                {
                    siblings.RemoveAt(index);
                    siblings.Insert(newIndex, selectedNode);

                    list.RemoveAt(index);
                    list.Insert(newIndex, selectedObj);

                    treeView.SelectedNode = selectedNode;
                }
                else if (moveBehavior == TreeNodeBehavior.ThroughSimilarParents)
                {
                    TreeNode parentNode = selectedNode.Parent;
                    TreeNode parentNodeNext = parentNode?.NextNode;

                    if (parentNode?.Tag is ITreeNode && 
                        parentNodeNext?.Tag is ITreeNode nextParentObj &&
                        parentNode.Tag.GetType() == parentNodeNext.Tag.GetType())
                    {
                        // the node being moved gets a new parent
                        siblings.RemoveAt(index);
                        parentNodeNext.Nodes.Insert(0, selectedNode);

                        list.RemoveAt(index);
                        nextParentObj.Children.Insert(0, selectedObj);
                        selectedObj.Parent = nextParentObj;

                        treeView.SelectedNode = selectedNode;
                    }
                }
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Moves the selected tree node and the associated list item to the specified index.
        /// </summary>
        /// <remarks>Use this method if the underlying list items implement ITreeNode.</remarks>
        public static void MoveSelectedNode(this TreeView treeView, int newIndex)
        {
            TreeNode selectedNode = treeView.SelectedNode;

            if (GetRelatedObject(selectedNode) is not ITreeNode selectedObj || selectedObj.Parent == null)
                return;

            try
            {
                treeView.BeginUpdate();

                TreeNodeCollection siblings = treeView.GetChildNodes(selectedNode.Parent);
                IList list = selectedObj.Parent.Children;

                if (0 <= newIndex && newIndex < siblings.Count)
                {
                    int index = selectedNode.Index;

                    siblings.RemoveAt(index);
                    siblings.Insert(newIndex, selectedNode);

                    list.RemoveAt(index);
                    list.Insert(newIndex, selectedObj);

                    treeView.SelectedNode = selectedNode;
                }
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Removes the specified tree node and the associated list item.
        /// </summary>
        /// <remarks>Use this method if list items do not implement ITreeNode.</remarks>
        public static void RemoveNode(this TreeView treeView, TreeNode nodeToRemove, IList list)
        {
            if (nodeToRemove != null)
            {
                TreeNodeCollection siblings = treeView.GetChildNodes(nodeToRemove.Parent);
                int index = nodeToRemove.Index;
                siblings.RemoveAt(index);
                list.RemoveAt(index);
            }
        }

        /// <summary>
        /// Removes the selected tree node and the associated list item.
        /// </summary>
        /// <remarks>Use this method if the underlying list items implement ITreeNode.</remarks>
        public static void RemoveSelectedNode(this TreeView treeView)
        {
            if (GetRelatedObject(treeView.SelectedNode) is ITreeNode treeNodeObj && treeNodeObj.Parent != null)
                RemoveNode(treeView, treeView.SelectedNode, treeNodeObj.Parent.Children);
        }

        /// <summary>
        /// Checks that moving up the selected tree node is possible.
        /// </summary>
        public static bool MoveUpSelectedNodeIsEnabled(this TreeView treeView, TreeNodeBehavior moveBehavior)
        {
            TreeNode selectedNode = treeView.SelectedNode;

            if (selectedNode == null)
            {
                return false;
            }
            else if (selectedNode.PrevNode != null)
            {
                return true;
            }
            else if (moveBehavior == TreeNodeBehavior.ThroughSimilarParents)
            {
                TreeNode parentNode = selectedNode.Parent;
                TreeNode parentNodePrev = parentNode?.PrevNode;

                return parentNode != null && parentNodePrev != null &&
                    (parentNode.Tag is ITreeNode && parentNodePrev.Tag is ITreeNode &&
                    parentNode.Tag.GetType() == parentNodePrev.Tag.GetType() ||
                    parentNode.Tag is TreeNodeTag tag1 && parentNodePrev.Tag is TreeNodeTag tag2 &&
                    tag1.NodeType == tag2.NodeType);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks that moving down the selected tree node is possible.
        /// </summary>
        public static bool MoveDownSelectedNodeIsEnabled(this TreeView treeView, TreeNodeBehavior moveBehavior)
        {
            TreeNode selectedNode = treeView.SelectedNode;

            if (selectedNode == null)
            {
                return false;
            }
            else if (selectedNode.NextNode != null)
            {
                return true;
            }
            else if (moveBehavior == TreeNodeBehavior.ThroughSimilarParents)
            {
                TreeNode parentNode = selectedNode.Parent;
                TreeNode parentNodeNext = parentNode?.NextNode;

                return parentNode != null && parentNodeNext != null &&
                    (parentNode.Tag is ITreeNode && parentNodeNext.Tag is ITreeNode &&
                    parentNode.Tag.GetType() == parentNodeNext.Tag.GetType() ||
                    parentNode.Tag is TreeNodeTag tag1 && parentNodeNext.Tag is TreeNodeTag tag2 &&
                    tag1.NodeType == tag2.NodeType);
            }
            else
            {
                return false;
            }
        }
    }
}
