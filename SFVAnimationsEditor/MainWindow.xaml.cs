using SFVAnimationsEditor.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SFVAnimationsEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
#if DEBUG
        public string executableLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public string TEMP_FILEPATH =>
        //    "";
        //    executableLocation + "/originals/DA_NCL_AnimSeqWithIdContainer.uasset";
        //    executableLocation + "/originals/DA_KRN_PSListContainer.uasset";
            executableLocation + "/originals/DA_RYU_PSListContainer.uasset";
        //    @"DA_RYU_AnimSeqWithIdContainer.uasset";
#endif

        public MainViewModel mainVM;
        public string filePath = "";


        public MainWindow(string path = "")
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();

#if DEBUG
            filePath = path == "" ? TEMP_FILEPATH : path;
            declarationSplitter.Visibility = Visibility.Visible;
            declarationViewer.Visibility = Visibility.Visible;
#else
            if (path != string.Empty)
                filePath = path;
#endif
            Start();
        }

        private void Start()
        {
            var tbOutput = (TextBox)FindName("tbOutput");
            tbOutput.Clear();
            Console.SetOut(new WpfUtil.OutputWriter(tbOutput));
            Console.WriteLine("Console ready.\n");

            mainVM = new MainViewModel();
            DataContext = mainVM;

            if (filePath != "")
                mainVM.FilePath = filePath;
            else
                Console.WriteLine("TIP:  You can drag a \"DA_***_AnimSeqWithIdContainer.uasset\" or \"DA_***_PSListContainer.uasset\" onto SFVAnimationsEditor.exe to immediately open it for editing!");

            mainVM.OpenFile(isFilePreselected: true);

#if DEBUG
            // set declarationViewer.ItemsSource binding
            var binding = new System.Windows.Data.Binding
            {
                Source = mainVM,
                Path = new PropertyPath("DeclarationItems"),
                Mode = System.Windows.Data.BindingMode.OneWay
            };

            System.Windows.Data.BindingOperations.SetBinding(declarationViewer, DataGrid.ItemsSourceProperty, binding);
#endif
        }
    }
}