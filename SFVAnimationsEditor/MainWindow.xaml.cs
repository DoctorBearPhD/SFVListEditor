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
        public const string TEMP_FILEPATH =
            @"E:\Program Files (x86)\Fluffy's Mod Manager\Mod Tools\AnimationsEditor\DA_RYU_AnimSeqWithIdContainer.uasset";
        // DA_RYU_AnimSeqWithIdContainer.uasset
#endif

        public MainViewModel mainVM;
        public string filePath = "";


        public MainWindow(string path = "")
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();

#if DEBUG
            filePath = path == "" ? TEMP_FILEPATH : path;
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
            if (filePath != "") mainVM.FilePath = filePath;
            mainVM.ReadFile();
        }


        // WPF Stuff

    }
}