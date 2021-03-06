﻿using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class SetSharedFolderPermissionDialogViewModel : BaseContentDialogViewModel
    {
        public SetSharedFolderPermissionDialogViewModel() : base()
        {
            this.SetFolderPermissionCommand = new RelayCommand<MShareType>(SetFolderPermission);

            this.TitleText = ResourceService.UiResources.GetString("UI_SetFolderPermissionLevel");
        }

        #region Commands

        public ICommand SetFolderPermissionCommand { get; }

        #endregion

        #region Methods

        private void SetFolderPermission(MShareType accessLevel) => this.AccessLevel = accessLevel;

        #endregion

        #region Properties

        private MShareType _accessLevel;
        public MShareType AccessLevel
        {
            get { return _accessLevel; }
            set { SetField(ref _accessLevel, value); }
        }

        #endregion

        #region UiResources

        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string FolderPermissionText => ResourceService.UiResources.GetString("UI_FolderPermission");
        public string PermissionReadOnlyText => ResourceService.UiResources.GetString("UI_PermissionReadOnly");
        public string PermissionReadAndWriteText => ResourceService.UiResources.GetString("UI_PermissionReadAndWrite");
        public string PermissionFullAccessText => ResourceService.UiResources.GetString("UI_PermissionFullAccess");
        public string ShareText => ResourceService.UiResources.GetString("UI_Share");

        #endregion
    }
}
