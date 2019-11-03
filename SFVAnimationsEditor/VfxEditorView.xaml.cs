using System.Windows;
using System.Windows.Controls;

namespace SFVAnimationsEditor
{
    /// <summary>
    /// Description for VfxEditorView.
    /// </summary>
    public partial class VfxEditorView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the VfxEditorView class.
        /// </summary>
        public VfxEditorView()
        {
            InitializeComponent();
        }

        private void DataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            if (e.NewItem is Model.Lists.ListItem item)
                item.UpdateIndex(((DataGrid)sender).Items.Count - 2);
        }
    }
}