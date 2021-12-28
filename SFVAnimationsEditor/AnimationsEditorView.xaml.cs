using System.Windows;
using System.Windows.Controls;

namespace SFVAnimationsEditor
{
    /// <summary>
    /// Description for AnimationsEditorView.
    /// </summary>
    public partial class AnimationsEditorView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the AnimationsEditorView class.
        /// </summary>
        public AnimationsEditorView()
        {
            InitializeComponent();
        }

        private void DataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            if (e.NewItem is Model.Lists.ListItem item)
                item.UpdateIndex(((DataGrid)sender).Items.Count - 2);
        }

        // Disable ability to edit an entry if it is read-only. User can still select the entry.
        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Item is Model.Lists.ListItem item)
                if (item.IsReadOnly) 
                    e.Cancel = true;
        }
    }
}