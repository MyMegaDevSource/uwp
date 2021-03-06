﻿using System.Windows.Input;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class ShareFolderToDialogViewModel : BaseContentDialogViewModel
    {
        public ShareFolderToDialogViewModel() : base()
        {
            this.ShareButtonCommand = new RelayCommand(Share);
            this.CancelButtonCommand = new RelayCommand(Cancel);
            this.SetFolderPermissionCommand = new RelayCommand<MShareType>(SetFolderPermission);

            this.TitleText = this.ShareFolderToText;
        }

        #region Commands

        public ICommand ShareButtonCommand { get; }
        public ICommand CancelButtonCommand { get; }
        public ICommand SetFolderPermissionCommand { get; }

        #endregion

        #region Methods

        private void Share()
        {
            this.CanClose = false;

            if (!ValidationService.IsValidEmail(this.ContactEmail))
            {
                this.SetWarning(true, ResourceService.AppMessages.GetString("AM_IncorrectEmailFormat"));
                return;
            }

            if (this.ContactEmail.Equals(SdkService.MegaSdk.getMyEmail()))
            {
                this.SetWarning(true, ResourceService.AppMessages.GetString("AM_ShareFolderFailedOwnEmail"));
                return;
            }

            this.CanClose = true;
        }

        private void Cancel() => this.CanClose = true;

        private void SetWarning(bool isVisible, string warningText)
        {
            if (isVisible)
            {
                // First text and then display
                this.WarningText = warningText;
                this.IsWarningVisible = true;
            }
            else
            {
                // First remove and than clean text
                this.IsWarningVisible = false;
                this.WarningText = warningText;
            }
        }

        private void SetState()
        {
            var enabled = !string.IsNullOrWhiteSpace(this.ContactEmail);
            OnUiThread(() => this.ShareButtonState = enabled);
        }

        private void SetFolderPermission(MShareType accessLevel) => this.AccessLevel = accessLevel;

        #endregion

        #region Properties

        private string _folderName;
        public string FolderName
        {
            get { return _folderName; }
            set
            {
                if (!SetField(ref _folderName, value)) return;
                OnPropertyChanged(nameof(this.ShareFolderToText));
                this.TitleText = this.ShareFolderToText;
            }
        }

        private MShareType _accessLevel;
        public MShareType AccessLevel
        {
            get { return _accessLevel; }
            set { SetField(ref _accessLevel, value); }
        }

        private string _contactEmail;
        public string ContactEmail
        {
            get { return _contactEmail; }
            set
            {
                if (!SetField(ref _contactEmail, value)) return;
                SetWarning(false, string.Empty);
                SetState();
            }
        }

        public SolidColorBrush ContactEmailBorderBrush => (this.IsWarningVisible) ?
            (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"] :
            new SolidColorBrush(Colors.Transparent);

        private string _warningText;
        public string WarningText
        {
            get { return _warningText; }
            set { SetField(ref _warningText, value); }
        }

        private bool _isWarningVisible;
        public bool IsWarningVisible
        {
            get { return _isWarningVisible; }
            set
            {
                SetField(ref _isWarningVisible, value);
                OnPropertyChanged(nameof(this.ContactEmailBorderBrush));
            }
        }

        private bool _shareButtonState;
        public bool ShareButtonState
        {
            get { return _shareButtonState; }
            set { SetField(ref _shareButtonState, value); }
        }

        #endregion

        #region UiResources

        public string ShareFolderToText => string.Format(
            ResourceService.UiResources.GetString("UI_ShareFolderTo"), this.FolderName);

        public string CancelText => ResourceService.UiResources.GetString("UI_Cancel");
        public string EmailText => ResourceService.UiResources.GetString("UI_Email");
        public string FolderPermissionText => ResourceService.UiResources.GetString("UI_FolderPermission");
        public string PermissionReadOnlyText => ResourceService.UiResources.GetString("UI_PermissionReadOnly");
        public string PermissionReadAndWriteText => ResourceService.UiResources.GetString("UI_PermissionReadAndWrite");
        public string PermissionFullAccessText => ResourceService.UiResources.GetString("UI_PermissionFullAccess");
        public string ShareText => ResourceService.UiResources.GetString("UI_Share");

        #endregion

        #region VisualResources

        public string WarningIconPathData => ResourceService.VisualResources.GetString("VR_WarningIconPathData");

        #endregion
    }
}
