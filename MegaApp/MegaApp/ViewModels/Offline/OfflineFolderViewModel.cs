﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;
using MegaApp.Classes;
using Windows.UI.Xaml;

namespace MegaApp.ViewModels.Offline
{
    /// <summary>
    /// Class that handles all process and operations of a section that contains Offline nodes
    /// </summary>
    public class OfflineFolderViewModel : BaseFolderViewModel
    {
        public OfflineFolderViewModel() : base(SdkService.MegaSdk, ContainerType.Offline)
        {
            this.FolderRootNode = null;
        }

        #region Properties

        public new IOfflineNode FolderRootNode
        {
            get { return base.FolderRootNode as IOfflineNode; }
            set { base.FolderRootNode = value; }
        }

        #endregion

        #region Methods

        public void ClearChildNodes() => this.ItemCollection.Clear();
        public void SelectAll() => this.ItemCollection.SelectAll(true);
        public void DeselectAll() => this.ItemCollection.SelectAll(false);        

        /// <summary>
        /// Load the nodes for this specific folder
        /// </summary>
        public override void LoadChildNodes()
        {
            // First cancel any other loading task that is busy
            CancelLoad();

            // FolderRootNode should not be null
            if (FolderRootNode == null)
            {
                OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_LoadNodesFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_LoadNodesFailed"));
                });
                return;
            }

            SetProgressIndication(true);

            // Process is started so we can set the empty content template to loading already
            //SetEmptyContentTemplate(true);

            // Clear the child nodes to make a fresh start
            ClearChildNodes();

            // Set the correct view for the main drive. Do this after the childs are cleared to speed things up
            SetViewOnLoad();

            // Build the bread crumbs. Do this before loading the nodes so that the user can click on home
            //OnUiThread(this.BreadCrumb.Create(this));

            // Create the option to cancel
            CreateLoadCancelOption();

            // Load and create the childnodes for the folder
            Task.Factory.StartNew(() =>
            {
                try
                {
                    // We will not add nodes one by one in the dispatcher but in groups
                    List<IOfflineNode> helperList;
                    try { helperList = new List<IOfflineNode>(1024); }
                    catch (ArgumentOutOfRangeException) { helperList = new List<IOfflineNode>(); }

                    this.ItemCollection.DisableCollectionChangedDetection();

                    string[] childFolders = Directory.GetDirectories(FolderRootNode.NodePath);
                    foreach (var folder in childFolders)
                    {
                        var childNode = new OfflineFolderNodeViewModel(new DirectoryInfo(folder), this.ItemCollection.Items);
                        if (childNode == null) continue;

                        if (FolderService.IsEmptyFolder(childNode.NodePath))
                        {
                            FolderService.DeleteFolder(childNode.NodePath, true);
                            continue;
                        }

                        OnUiThread(() => this.ItemCollection.Items.Add(childNode));
                    }

                    string[] childFiles = Directory.GetFiles(FolderRootNode.NodePath);
                    foreach (var file in childFiles)
                    {
                        var fileInfo = new FileInfo(file);

                        if (FileService.IsPendingTransferFile(fileInfo.Name))
                        {
                            if (!(TransferService.MegaTransfers.Downloads.Count > 0))
                                FileService.DeleteFile(fileInfo.FullName);
                            continue;
                        }

                        var childNode = new OfflineFileNodeViewModel(fileInfo, this.ItemCollection.Items);
                        if (childNode == null) continue;

                        OnUiThread(() => this.ItemCollection.Items.Add(childNode));
                    }

                    this.ItemCollection.EnableCollectionChangedDetection();

                    //OrderChildNodes(tempChildNodes);

                    // Show the user that processing the childnodes is done
                    SetProgressIndication(false);

                    // Set empty content to folder instead of loading view
                    //SetEmptyContentTemplate(false);
                }
                catch (OperationCanceledException)
                {
                    // Do nothing. Just exit this background process because a cancellation exception has been thrown
                }

            }, LoadingCancelToken, TaskCreationOptions.PreferFairness, TaskScheduler.Current);
        }

        /// <summary>
        /// Sets the default view mode for the folder content.
        /// </summary>
        protected override void SetViewDefaults()
        {
            OnUiThread(() =>
            {
                this.NodeTemplateSelector = new OfflineNodeTemplateSelector()
                {
                    FileItemTemplate = (DataTemplate)Application.Current.Resources["OfflineNodeListViewFileItemContent"],
                    FolderItemTemplate = (DataTemplate)Application.Current.Resources["OfflineNodeListViewFolderItemContent"]
                };
            });

            base.SetViewDefaults();
        }

        public override void OnChildNodeTapped(IBaseNode node)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
