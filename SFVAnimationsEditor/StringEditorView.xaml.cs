using System.Windows;
using System.Windows.Controls;

namespace SFVAnimationsEditor
{
    /// <summary>
    /// Description for StringEditorView.
    /// </summary>
    public partial class StringEditorView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the StringEditorView class.
        /// </summary>
        public StringEditorView()
        {
            InitializeComponent();
        }


        // WPF Stuff

        private void OnLoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex().ToString() + $" (0x{e.Row.GetIndex():X2})";
        }
    }
}