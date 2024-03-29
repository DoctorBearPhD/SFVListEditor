﻿using GalaSoft.MvvmLight.Messaging;
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
            "";
        //    @"/originals/DA_NCL_AnimSeqWithIdContainer.uasset";
        //    @"/originals/DA_KRN_PSListContainer.uasset";
        //    @"/originals/DA_RYU_PSListContainer.uasset";
        //    @"/originals/DA_RYU_TrailList.uasset";
        //    @"DA_RYU_AnimSeqWithIdContainer.uasset";
        //    @"/originals/DA_Z26_AnimSeqWithIdContainer.uasset";
        //    @"/originals/DA_Z41_AnimSeqWithIdContainer.uasset";
        //    @"/originals/DA_Z41_Prop_01.uasset";
#endif

        public MainViewModel mainVM;
        public string filePath = "";

        private IMessenger _messenger;


        public MainWindow(string path = "")
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();

            _messenger = Messenger.Default;
            _messenger.Register<NotificationMessage>(this, "ERROR MESSAGE", DisplayMessageBox);

#if DEBUG
            filePath = path == "" && TEMP_FILEPATH != "" ? (executableLocation + TEMP_FILEPATH) : path;
            declarationSplitter.Visibility = Visibility.Visible;
            declarationViewer.Visibility = Visibility.Visible;
            rowDefDebugInfo.Height = new GridLength(1, GridUnitType.Star);
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
            Console.SetOut(new UassetReader.WpfUtil.MultiOutTextWriter(new WpfUtil.OutputWriter(tbOutput), Console.Out));
            Console.WriteLine("Console ready.\n");

            mainVM = new MainViewModel();
            DataContext = mainVM;

            if (filePath != "")
            {
                mainVM.FilePath = filePath;
                mainVM.OpenFile(isFilePreselected: true);
            }
            else
                Console.WriteLine("TIP:  You can drag a compatible file onto SFVListEditor.exe to immediately open it for editing!\n" +
                    "\tCompatible Files:\n" +
                    "\t\t\"DA_***_AnimSeqWithIdContainer.uasset\"\n" +
                    "\t\t\"DA_***_PSListContainer.uasset\"\n" +
                    "\t\t\"DA_***_TrailList.uasset\"");

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

        private void DisplayMessageBox(NotificationMessage message)
        {
            MessageBox.Show(message.Notification);
        }
    }
}