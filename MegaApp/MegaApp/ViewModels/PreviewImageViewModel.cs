﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class PreviewImageViewModel : BasePageViewModel
    {
        public PreviewImageViewModel()
        {
            DownloadCommand = new RelayCommand(Download);
            GetLinkCommand = new RelayCommand(GetLink);
            RemoveCommand = new RelayCommand(Remove);
            RenameCommand = new RelayCommand(Rename);
        }

        public void Initialize(FolderViewModel parentFolder)
        {
            ParentFolder = parentFolder;

            PreviewItems = new ObservableCollection<ImageNodeViewModel>(
                ParentFolder.ItemCollection.Items.Where(n => n is ImageNodeViewModel).Cast<ImageNodeViewModel>());

            ParentFolder.ItemCollection.Items.CollectionChanged += (sender, args) =>
            {
                if (args.Action != NotifyCollectionChangedAction.Remove) return;

                var removedNode = (NodeViewModel)args.OldItems[0];

                PreviewItems.Remove(PreviewItems.FirstOrDefault(n => 
                    n.OriginalMNode.getBase64Handle() == removedNode.OriginalMNode.getBase64Handle()));
            };

            SelectedPreview = ParentFolder.FocusedNode as ImageNodeViewModel;
        }

        #region Commands

        public ICommand DownloadCommand { get; }
        public ICommand GetLinkCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand RenameCommand { get; }

        #endregion

        #region Private Methods

        private void Download()
        {
            SelectedPreview.Download(TransferService.MegaTransfers);
        }

        private void GetLink()
        {
            SelectedPreview?.GetLinkAsync(true);
        }

        private async void Remove()
        {
            await SelectedPreview?.RemoveAsync();
        }

        private async void Rename()
        {
            await SelectedPreview?.RenameAsync();
        }

        /// <summary>
        /// Set the range of previews to load before and after the current preview.
        /// </summary>
        /// <param name="viewRange">Range from the current item to load the previews.</param>
        /// <param name="initialize">Indicates if is an initialization.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        private void SetViewingRange(int viewRange, bool initialize)
        {
            if (SelectedPreview == null) return;

            int currentIndex = PreviewItems.IndexOf(SelectedPreview);
            int lowIndex = currentIndex - viewRange;
            if (lowIndex < 0) lowIndex = 0;
            int highIndex = currentIndex + viewRange;
            if (highIndex > PreviewItems.Count - 1) highIndex = PreviewItems.Count - 1;

            if (initialize)
            {
                for (int i = currentIndex; i >= lowIndex; i--)
                    PreviewItems[i].InViewingRange = true;
                for (int i = currentIndex; i <= highIndex; i++)
                    PreviewItems[i].InViewingRange = true;
            }
            else
            {
                switch (GalleryDirection)
                {
                    case GalleryDirection.Next:
                    default:
                        PreviewItems[highIndex].InViewingRange = true;
                        break;

                    case GalleryDirection.Previous:
                        PreviewItems[lowIndex].InViewingRange = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Clean up from memory the previews that are not in the view range.
        /// </summary>
        /// <param name="cleanRange">Range from the current item to clean previews.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        private void CleanUpMemory(int cleanRange)
        {
            if (SelectedPreview == null) return;

            int currentIndex = PreviewItems.IndexOf(SelectedPreview);
            int previewItemsCount = PreviewItems.Count - 1;

            switch (GalleryDirection)
            {
                case GalleryDirection.Next:
                default:
                    if ((currentIndex - cleanRange) >= 0)
                    {
                        int cleanIndex = currentIndex - cleanRange;
                        if (PreviewItems[cleanIndex].IsBusy)
                            PreviewItems[cleanIndex].CancelPreviewRequest();
                        PreviewItems[cleanIndex].InViewingRange = false;
                        PreviewItems[cleanIndex].PreviewImageUri = null;
                    }
                    break;

                case GalleryDirection.Previous:
                    if ((currentIndex + cleanRange) <= previewItemsCount)
                    {
                        int cleanIndex = currentIndex + cleanRange;
                        if (PreviewItems[cleanIndex].IsBusy)
                            PreviewItems[cleanIndex].CancelPreviewRequest();
                        PreviewItems[cleanIndex].InViewingRange = false;
                        PreviewItems[cleanIndex].PreviewImageUri = null;
                    }
                    break;
            }
        }

        #endregion

        #region Properties

        public FolderViewModel ParentFolder { get; private set; }

        public ObservableCollection<ImageNodeViewModel> PreviewItems { get; private set; }

        public GalleryDirection GalleryDirection { get; set; }

        private ImageNodeViewModel _selectedPreview;
        public ImageNodeViewModel SelectedPreview
        {
            get { return _selectedPreview; }
            set
            {
                bool initialize = _selectedPreview == null;
                SetField(ref _selectedPreview, value);

                if(_selectedPreview != null)
                {
                    SetViewingRange(3, initialize);
                    CleanUpMemory(4);
                }
            }
        }

        #endregion
       
        #region ProgressMessages

        public string ProgressHeaderText => ResourceService.ProgressMessages.GetString("PM_LoadingPreview");
        public string ProgressText => ResourceService.ProgressMessages.GetString("PM_Patient");

        #endregion

        #region UiResources

        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string DownloadText => ResourceService.UiResources.GetString("UI_Download");
        public string GetLinkText => ResourceService.UiResources.GetString("UI_GetLink");
        public string NextText => ResourceService.UiResources.GetString("UI_Next");
        public string PreviousText => ResourceService.UiResources.GetString("UI_Previous");
        public string RemoveText => ResourceService.UiResources.GetString("UI_Remove");
        public string RenameText => ResourceService.UiResources.GetString("UI_Rename");

        #endregion

        #region VisualResources

        public string DownloadPathData => ResourceService.VisualResources.GetString("VR_DownloadPathData");
        public string LinkPathData => ResourceService.VisualResources.GetString("VR_LinkPathData");
        public string RenamePathData => ResourceService.VisualResources.GetString("VR_RenamePathData");
        public string RubbishBinPathData => ResourceService.VisualResources.GetString("VR_RubbishBinPathData");

        #endregion
    }
}
