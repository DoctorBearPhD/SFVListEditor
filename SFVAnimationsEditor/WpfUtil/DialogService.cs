using System;
using System.Windows;

namespace SFVAnimationsEditor.WpfUtil
{
    public class DialogService : IDialogService
    {
        public void ShowError(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowError(Exception error, string title)
        {
            MessageBox.Show(error.ToString(), title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowMessage(string message, string title = "")
        {
            MessageBox.Show(message, title);
        }

        public bool ShowMessageWithResult(string message, string title)
        {
            return MessageBox.Show(message, title, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        }
    }
}
