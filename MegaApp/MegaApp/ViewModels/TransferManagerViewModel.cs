﻿using mega;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class TransferManagerViewModel : BasePageViewModel
    {
        public TransferManagerViewModel()
        {
            this.Downloads = new TransferListViewModel(MTransferType.TYPE_DOWNLOAD);
            this.Uploads = new TransferListViewModel(MTransferType.TYPE_UPLOAD);
            this.Completed = new TransferListViewModel();

            this.ActiveViewModel = this.Uploads;
        }

        public override void UpdateNetworkStatus()
        {
            base.UpdateNetworkStatus();
            this.Downloads.UpdateNetworkStatus();
            this.Uploads.UpdateNetworkStatus();
            this.Completed.UpdateNetworkStatus();
        }

        public void Update()
        {
            this.Downloads.UpdateTransfers();
            this.Uploads.UpdateTransfers();
        }

        #region Properties

        public TransferListViewModel Uploads { get; }

        public TransferListViewModel Downloads { get; }

        public TransferListViewModel Completed { get; }

        private TransferListViewModel _activeViewModel;
        public TransferListViewModel ActiveViewModel
        {
            get { return _activeViewModel; }
            set { SetField(ref _activeViewModel, value); }
        }

        #endregion

        #region UiResources

        public string SectionNameText => ResourceService.UiResources.GetString("UI_TransferManager");
        
        #endregion
    }
}
