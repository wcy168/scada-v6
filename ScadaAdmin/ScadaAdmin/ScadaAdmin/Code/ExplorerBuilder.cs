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
 * Module   : Administrator
 * Summary  : Manipulates the explorer tree
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2018
 * Modified : 2021
 */

using Scada.Admin.Project;
using Scada.Data.Entities;
using Scada.Data.Tables;
using Scada.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Scada.Admin.App.Code
{
    /// <summary>
    /// Manipulates the explorer tree.
    /// <para>Манипулирует деревом проводника.</para>
    /// </summary>
    internal class ExplorerBuilder
    {
        private readonly AppData appData;           // the common data of the application
        //private readonly ServerShell serverShell;   // the shell to edit Server settings
        //private readonly CommShell commShell;       // the shell to edit Communicator settings
        private readonly TreeView treeView;         // the manipulated tree view 
        private readonly ContextMenus contextMenus; // references to the context menus
        private ScadaProject project;               // the current project to build tree


        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ExplorerBuilder(AppData appData, /*ServerShell serverShell, CommShell commShell, */
            TreeView treeView, ContextMenus contextMenus)
        {
            this.appData = appData ?? throw new ArgumentNullException(nameof(appData));
            //this.serverShell = serverShell ?? throw new ArgumentNullException("serverShell");
            //this.commShell = commShell ?? throw new ArgumentNullException("commShell");
            this.treeView = treeView ?? throw new ArgumentNullException(nameof(treeView));
            this.contextMenus = contextMenus ?? throw new ArgumentNullException(nameof(contextMenus));
            project = null;

            ProjectNode = null;
            BaseNode = null;
            BaseTableNodes = null;
            ViewsNode = null;
            InstancesNode = null;
        }


        /// <summary>
        /// Gets the project node.
        /// </summary>
        public TreeNode ProjectNode { get; private set; }

        /// <summary>
        /// Gets the configuration database node.
        /// </summary>
        public TreeNode BaseNode { get; private set; }

        /// <summary>
        /// Gets the configuration database table nodes by names.
        /// </summary>
        public Dictionary<string, TreeNode> BaseTableNodes { get; private set; }

        /// <summary>
        /// Gets the views node.
        /// </summary>
        public TreeNode ViewsNode { get; private set; }

        /// <summary>
        /// Gets the instances node.
        /// </summary>
        public TreeNode InstancesNode { get; private set; }


        /// <summary>
        /// Creates a node that represents the configuration database.
        /// </summary>
        private TreeNode CreateBaseNode(ConfigBase configBase)
        {
            TreeNode baseNode = TreeViewUtils.CreateNode(AppPhrases.BaseNode, "database.png");
            baseNode.Tag = new TreeNodeTag(project.ConfigBase, AppNodeType.Base);

            // primary tables sorted in the order they are configured
            TreeNode primaryNode = TreeViewUtils.CreateNode(AppPhrases.PrimaryTablesNode, "folder_closed.png");
            primaryNode.Nodes.Add(CreateBaseTableNode(configBase.ObjTable));
            primaryNode.Nodes.Add(CreateBaseTableNode(configBase.CommLineTable));
            primaryNode.Nodes.Add(CreateBaseTableNode(configBase.DeviceTable));

            TreeNode inCnlTableNode = CreateBaseTableNode(configBase.InCnlTable);
            TreeNode outCnlTableNode = CreateBaseTableNode(configBase.OutCnlTable);
            inCnlTableNode.ContextMenuStrip = contextMenus.CnlTableMenu;
            outCnlTableNode.ContextMenuStrip = contextMenus.CnlTableMenu;
            primaryNode.Nodes.Add(inCnlTableNode);
            primaryNode.Nodes.Add(outCnlTableNode);
            FillCnlTableNodes(inCnlTableNode, outCnlTableNode, configBase);

            primaryNode.Nodes.Add(CreateBaseTableNode(configBase.LimTable));
            primaryNode.Nodes.Add(CreateBaseTableNode(configBase.ViewTable));
            primaryNode.Nodes.Add(CreateBaseTableNode(configBase.RoleTable));
            primaryNode.Nodes.Add(CreateBaseTableNode(configBase.RoleRefTable));
            primaryNode.Nodes.Add(CreateBaseTableNode(configBase.ObjRightTable));
            primaryNode.Nodes.Add(CreateBaseTableNode(configBase.UserTable));
            baseNode.Nodes.Add(primaryNode);

            // secondary tables in alphabetical order
            TreeNode secondaryNode = TreeViewUtils.CreateNode(AppPhrases.SecondaryTablesNode, "folder_closed.png");
            SortedList<string, TreeNode> secondaryNodes = new()
            {
                { configBase.ArchiveTable.Title, CreateBaseTableNode(configBase.ArchiveTable) },
                { configBase.CnlStatusTable.Title, CreateBaseTableNode(configBase.CnlStatusTable) },
                { configBase.CnlTypeTable.Title, CreateBaseTableNode(configBase.CnlTypeTable) },
                { configBase.CmdTypeTable.Title, CreateBaseTableNode(configBase.CmdTypeTable) },
                { configBase.DataTypeTable.Title, CreateBaseTableNode(configBase.DataTypeTable) },
                { configBase.DevTypeTable.Title, CreateBaseTableNode(configBase.DevTypeTable) },
                { configBase.FormatTable.Title, CreateBaseTableNode(configBase.FormatTable) },
                { configBase.QuantityTable.Title, CreateBaseTableNode(configBase.QuantityTable) },
                { configBase.ScriptTable.Title, CreateBaseTableNode(configBase.ScriptTable) },
                { configBase.UnitTable.Title, CreateBaseTableNode(configBase.UnitTable) },
                { configBase.ViewTypeTable.Title, CreateBaseTableNode(configBase.ViewTypeTable) }
            };

            foreach (TreeNode tableNode in secondaryNodes.Values)
            {
                secondaryNode.Nodes.Add(tableNode);
            }

            baseNode.Nodes.Add(secondaryNode);
            return baseNode;
        }

        /// <summary>
        /// Creates a node that represents the table of the configuration database.
        /// </summary>
        private TreeNode CreateBaseTableNode(IBaseTable baseTable)
        {
            TreeNode baseTableNode = TreeViewUtils.CreateNode(baseTable.Title, "table.png");
            baseTableNode.Tag = CreateBaseTableTag(baseTable);
            BaseTableNodes.Add(baseTable.Name, baseTableNode);
            return baseTableNode;
        }

        /// <summary>
        /// Creates a tag to associate with a tree node representing a table.
        /// </summary>
        private TreeNodeTag CreateBaseTableTag(IBaseTable baseTable, TableFilter tableFilter = null)
        {
            return new TreeNodeTag
            {
                FormType = typeof(FrmBaseTable),
                FormArgs = new object[] { baseTable, tableFilter, project, appData },
                RelatedObject = new BaseTableItem(baseTable, tableFilter),
                NodeType = AppNodeType.BaseTable
            };
        }

        /// <summary>
        /// Fills the channel table nodes.
        /// </summary>
        private void FillCnlTableNodes(TreeNode inCnlTableNode, TreeNode outCnlTableNode, ConfigBase configBase)
        {
            foreach (Device device in configBase.DeviceTable.EnumerateItems())
            {
                string nodeText = string.Format(AppPhrases.TableByDeviceNode, device.DeviceNum, device.Name);

                TreeNode inCnlsByDeviceNode = TreeViewUtils.CreateNode(nodeText, "table.png");
                inCnlsByDeviceNode.ContextMenuStrip = contextMenus.CnlTableMenu;
                inCnlsByDeviceNode.Tag = CreateBaseTableTag(configBase.InCnlTable, CreateFilterByDevice(device));
                inCnlTableNode.Nodes.Add(inCnlsByDeviceNode);

                TreeNode outCnlsByDeviceNode = TreeViewUtils.CreateNode(nodeText, "table.png");
                outCnlsByDeviceNode.ContextMenuStrip = contextMenus.CnlTableMenu;
                outCnlsByDeviceNode.Tag = CreateBaseTableTag(configBase.OutCnlTable, CreateFilterByDevice(device));
                outCnlTableNode.Nodes.Add(outCnlsByDeviceNode);
            }

            TreeNode inCnlsEmptyDeviceNode = TreeViewUtils.CreateNode(AppPhrases.EmptyDeviceNode, "table.png");
            inCnlsEmptyDeviceNode.ContextMenuStrip = contextMenus.CnlTableMenu;
            inCnlsEmptyDeviceNode.Tag = CreateBaseTableTag(configBase.InCnlTable, CreateFilterByDevice(null));
            inCnlTableNode.Nodes.Add(inCnlsEmptyDeviceNode);

            TreeNode outCnlsEmptyDeviceNode = TreeViewUtils.CreateNode(AppPhrases.EmptyDeviceNode, "table.png");
            outCnlsEmptyDeviceNode.ContextMenuStrip = contextMenus.CnlTableMenu;
            outCnlsEmptyDeviceNode.Tag = CreateBaseTableTag(configBase.OutCnlTable, CreateFilterByDevice(null));
            outCnlTableNode.Nodes.Add(outCnlsEmptyDeviceNode);
        }

        /// <summary>
        /// Creates a node that represents the specified directory.
        /// </summary>
        private TreeNode CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            TreeNode directoryNode = TreeViewUtils.CreateNode(directoryInfo.Name, "folder_closed.png");
            directoryNode.ContextMenuStrip = contextMenus.DirectoryMenu;
            directoryNode.Tag = new TreeNodeTag(new FileItem(directoryInfo), AppNodeType.Directory);
            return directoryNode;
        }

        /// <summary>
        /// Creates a node that represents the specified file.
        /// </summary>
        private TreeNode CreateFileNode(FileInfo fileInfo)
        {
            TreeNode fileNode = TreeViewUtils.CreateNode(fileInfo.Name, "file.png");
            fileNode.ContextMenuStrip = contextMenus.FileItemMenu;
            fileNode.Tag = new TreeNodeTag(new FileItem(fileInfo), AppNodeType.File);
            return fileNode;
        }

        /// <summary>
        /// Fills the tree node according to the file system.
        /// </summary>
        private void FillFileNode(TreeNode treeNode, DirectoryInfo directoryInfo)
        {
            foreach (DirectoryInfo subdirInfo in directoryInfo.EnumerateDirectories())
            {
                TreeNode subdirNode = CreateDirectoryNode(subdirInfo);
                FillFileNode(subdirNode, subdirInfo);
                treeNode.Nodes.Add(subdirNode);
            }

            foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles())
            {
                TreeNode fileNode = CreateFileNode(fileInfo);
                treeNode.Nodes.Add(fileNode);
            }
        }

        /// <summary>
        /// Creates an empty node.
        /// </summary>
        private static TreeNode CreateEmptyNode()
        {
            return TreeViewUtils.CreateNode(AppPhrases.EmptyNode, "empty.png");
        }

        /// <summary>
        /// Creates a table filter for filtering by device.
        /// </summary>
        private static TableFilter CreateFilterByDevice(Device device)
        {
            return device == null
                ? new TableFilter("DeviceNum", null)
                {
                    Title = AppPhrases.EmptyDeviceFilter
                }
                : new TableFilter("DeviceNum", device.DeviceNum)
                {
                    Title = string.Format(AppPhrases.DeviceFilter, device.DeviceNum)
                };
        }


        /// <summary>
        /// Creates tree nodes according to the project structure.
        /// </summary>
        public void CreateNodes(ScadaProject project)
        {
            this.project = project ?? throw new ArgumentNullException(nameof(project));
            ProjectNode = null;
            BaseNode = null;
            BaseTableNodes = new Dictionary<string, TreeNode>();
            ViewsNode = null;
            InstancesNode = null;

            try
            {
                treeView.BeginUpdate();
                treeView.Nodes.Clear();

                // project node
                ProjectNode = TreeViewUtils.CreateNode(project.Name, "project.png", true);
                ProjectNode.ContextMenuStrip = contextMenus.ProjectMenu;
                ProjectNode.Tag = new TreeNodeTag(project, AppNodeType.Project);
                treeView.Nodes.Add(ProjectNode);

                // configuration database node
                BaseNode = CreateBaseNode(project.ConfigBase);
                ProjectNode.Nodes.Add(BaseNode);

                // views node
                ViewsNode = TreeViewUtils.CreateNode(AppPhrases.ViewsNode, "views.png");
                ViewsNode.ContextMenuStrip = contextMenus.DirectoryMenu;
                ViewsNode.Tag = new TreeNodeTag(project.Views, AppNodeType.Views);
                ViewsNode.Nodes.Add(CreateEmptyNode());
                ProjectNode.Nodes.Add(ViewsNode);

                // instances node
                InstancesNode = TreeViewUtils.CreateNode(AppPhrases.InstancesNode, "instances.png");
                InstancesNode.ContextMenuStrip = contextMenus.InstanceMenu;
                InstancesNode.Tag = new TreeNodeTag(project.Instances, AppNodeType.Instances);
                project.Instances.ForEach(i => InstancesNode.Nodes.Add(CreateInstanceNode(i)));
                ProjectNode.Nodes.Add(InstancesNode);

                ProjectNode.Expand();
                InstancesNode.Expand();
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Creates a node that represents the specified instance.
        /// </summary>
        public TreeNode CreateInstanceNode(ProjectInstance projectInstance)
        {
            TreeNode instanceNode = TreeViewUtils.CreateNode(projectInstance.Name, "instance.png");
            instanceNode.ContextMenuStrip = contextMenus.InstanceMenu;
            instanceNode.Tag = new TreeNodeTag(new LiveInstance(projectInstance), AppNodeType.Instance);
            instanceNode.Nodes.Add(CreateEmptyNode());
            return instanceNode;
        }

        /// <summary>
        /// Fills the channel table nodes, creating child nodes.
        /// </summary>
        public void FillChannelTableNodes(TreeNode inCnlTableNode, TreeNode outCnlTableNode, ConfigBase configBase)
        {
            try
            {
                treeView.BeginUpdate();
                inCnlTableNode.Nodes.Clear();
                outCnlTableNode.Nodes.Clear();
                FillCnlTableNodes(inCnlTableNode, outCnlTableNode, configBase);
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Fills the views node, creating child nodes.
        /// </summary>
        public void FillViewsNode(TreeNode viewsNode)
        {
            if (TreeViewUtils.GetRelatedObject(viewsNode) is ProjectViews projectViews)
                FillFileNode(viewsNode, projectViews.ViewDir);
        }

        /// <summary>
        /// Fills the instances node without creating child nodes.
        /// </summary>
        public void FillInstancesNode()
        {
            try
            {
                treeView.BeginUpdate();
                InstancesNode.Nodes.Clear();
                project.Instances.ForEach(i => InstancesNode.Nodes.Add(CreateInstanceNode(i)));
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Fills the instance node, creating child nodes.
        /// </summary>
        public void FillInstanceNode(TreeNode instanceNode)
        {
            if (TreeViewUtils.GetRelatedObject(instanceNode) is not LiveInstance liveInstance)
                return;

            try
            {
                treeView.BeginUpdate();
                instanceNode.Nodes.Clear();
                ProjectInstance projectInstance = liveInstance.ProjectInstance;

                // create Server nodes
                if (projectInstance.ServerApp.Enabled)
                {
                    TreeNode serverNode = TreeViewUtils.CreateNode(AppPhrases.ServerNode, "server.png");
                    serverNode.ContextMenuStrip = contextMenus.ServerMenu;
                    serverNode.Tag = new TreeNodeTag(projectInstance.ServerApp, AppNodeType.ServerApp);
                    //serverNode.Nodes.AddRange(serverShell.GetTreeNodes(
                    //    projectInstance.ServerApp.Config, liveInstance.ServerEnvironment));
                    instanceNode.Nodes.Add(serverNode);
                }

                // create Communicator nodes
                if (projectInstance.CommApp.Enabled)
                {
                    TreeNode commNode = TreeViewUtils.CreateNode(AppPhrases.CommNode, "comm.png");
                    commNode.ContextMenuStrip = contextMenus.CommMenu;
                    commNode.Tag = new TreeNodeTag(projectInstance.CommApp, AppNodeType.CommApp);
                    //commNode.Nodes.AddRange(commShell.GetTreeNodes(
                    //    projectInstance.CommApp.Config, liveInstance.CommEnvironment));
                    //SetContextMenus(commNode);
                    instanceNode.Nodes.Add(commNode);
                }

                // create Webstation nodes
                if (projectInstance.WebApp.Enabled)
                {
                    TreeNode webNode = TreeViewUtils.CreateNode(AppPhrases.WebNode, "chrome.png");
                    webNode.ContextMenuStrip = contextMenus.DirectoryMenu;
                    webNode.Tag = new TreeNodeTag(projectInstance.WebApp, AppNodeType.WebApp);
                    webNode.Nodes.Add(CreateEmptyNode());
                    instanceNode.Nodes.Add(webNode);
                }
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Fills the web application node, creating child nodes.
        /// </summary>
        public void FillWebstationNode(TreeNode webNode)
        {
            if (TreeViewUtils.GetRelatedObject(webNode) is WebApp webApp)
                FillFileNode(webNode, webApp.AppDir);
        }

        /// <summary>
        /// Fills the tree node according to the file system.
        /// </summary>
        public void FillFileNode(TreeNode treeNode, string directory)
        {
            try
            {
                treeView.BeginUpdate();
                treeNode.Nodes.Clear();
                FillFileNode(treeNode, new DirectoryInfo(directory));
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Inserts a newly created directory node.
        /// </summary>
        public void InsertDirectoryNode(TreeNode parentNode, string directory)
        {
            try
            {
                treeView.BeginUpdate();
                DirectoryInfo directoryInfo = new(directory);
                string name = directoryInfo.Name;
                int index = 0;

                foreach (TreeNode treeNode in parentNode.Nodes)
                {
                    if (treeNode.TagIs(AppNodeType.Directory))
                    {
                        if (string.Compare(name, treeNode.Text, StringComparison.Ordinal) < 0)
                            break;
                        else
                            index = treeNode.Index + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                TreeNode directoryNode = CreateDirectoryNode(directoryInfo);
                parentNode.Nodes.Insert(index, directoryNode);
                treeView.SelectedNode = directoryNode;
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Inserts a newly created file node.
        /// </summary>
        public void InsertFileNode(TreeNode parentNode, string fileName)
        {
            try
            {
                treeView.BeginUpdate();
                FileInfo fileInfo = new(fileName);
                string name = fileInfo.Name;
                int index = 0;

                foreach (TreeNode treeNode in parentNode.Nodes)
                {
                    if (treeNode.TagIs(AppNodeType.Directory))
                    {
                        index = treeNode.Index + 1;
                    }
                    else if (treeNode.TagIs(AppNodeType.File))
                    {
                        if (string.Compare(name, treeNode.Text, StringComparison.Ordinal) < 0)
                            break;
                        else
                            index = treeNode.Index + 1;
                    }
                    else
                    {
                        index = treeNode.Index;
                        break;
                    }
                }

                TreeNode fileNode = CreateFileNode(fileInfo);
                parentNode.Nodes.Insert(index, fileNode);
                treeView.SelectedNode = fileNode;
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Defines context menus of the node and its child nodes.
        /// </summary>
        /// <remarks>The method works for communication line.</remarks>
        public void SetContextMenus(TreeNode parentNode)
        {
            /*foreach (TreeNode treeNode in TreeViewUtils.IterateNodes(parentNode))
            {
                if (treeNode.Tag is TreeNodeTag tag)
                {
                    if (tag.NodeType == CommNodeType.CommLines || tag.NodeType == CommNodeType.CommLine)
                        treeNode.ContextMenuStrip = contextMenus.CommLineMenu;
                    else if (tag.NodeType == CommNodeType.Device)
                        treeNode.ContextMenuStrip = contextMenus.DeviceMenu;
                }
            }*/
        }

        /// <summary>
        /// Sets the node image as open or closed folder.
        /// </summary>
        public static void SetFolderImage(TreeNode treeNode)
        {
            if (treeNode.ImageKey.StartsWith("folder_"))
                treeNode.SetImageKey(treeNode.IsExpanded ? "folder_open.png" : "folder_closed.png");
        }
    }
}
