using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFVAnimationsEditor.WpfUtil
{
    public interface IDialogService
    {
        void ShowError(string message, string title);
        void ShowError(Exception error, string title);
        void ShowMessage(string message, string title = "");
        bool ShowMessageWithResult(string message, string title);
    }
}
