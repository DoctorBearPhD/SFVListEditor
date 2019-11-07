using System.Windows.Controls;

namespace SFVAnimationsEditor
{
    /// <summary>
    /// Interaction logic for TrailEditorView.xaml
    /// </summary>
    public partial class TrailEditorView : UserControl
    {
        public TrailEditorView()
        {
            InitializeComponent();
        }

        private void DataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            if (e.NewItem is Model.Lists.ListItem item)
                item.UpdateIndex(((DataGrid)sender).Items.Count - 2);
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "Index")
                e.Column.DisplayIndex = 0;
        }
    }
}
