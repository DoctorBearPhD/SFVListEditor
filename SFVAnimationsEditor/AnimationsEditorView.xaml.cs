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
            if (e.NewItem is ViewModel.AnimationListItem item)
                item.UpdateIndex(((DataGrid)sender).Items.Count - 2);
        }
    }
}