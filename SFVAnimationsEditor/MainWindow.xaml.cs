using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using SFVAnimationsEditor.Resources;
using SFVAnimationsEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using WK.Libraries.BetterFolderBrowserNS;

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
        //    @"\originals\DA_NCL_AnimSeqWithIdContainer.uasset";
        //    @"\originals\DA_KRN_PSListContainer.uasset";
        //    @"\originals\DA_RYU_PSListContainer.uasset";
        //    @"\originals\DA_RYU_TrailList.uasset";
        //    @"DA_RYU_AnimSeqWithIdContainer.uasset";
            @"\originals\DA_Z26_AnimSeqWithIdContainer.uasset";
        //    @"\originals\DA_Z41_AnimSeqWithIdContainer.uasset";
        //    @"\originals\DA_Z41_Prop_01.uasset";
#endif

        public MainViewModel mainVM;
        public string filePath = "";

        private IMessenger _messenger;


        public MainWindow(string path = "")
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();

            _messenger = Messenger.Default;
            
            // Register listeners for dialog display requests here, instead of in the VM, because dialog boxes are part of the View
            _messenger.Register<NotificationMessage<string>>(this, Constants.REQUEST_DIALOG, DialogRequestHandler);

#if DEBUG
            filePath = path == "" && TEMP_FILEPATH != "" ? (executableLocation + TEMP_FILEPATH) : path;
            //declarationSplitter.Visibility = Visibility.Visible;
            //declarationViewer.Visibility = Visibility.Visible;
            //rowDefDebugInfo.Height = new GridLength(1, GridUnitType.Star);
#else
            if (path != string.Empty)
                filePath = path;
#endif
            Start();
        }

        private void Start()
        {
            //var tbOutput = (TextBox)FindName("tbOutput");
            //tbOutput.Clear();
            //Console.SetOut(new WpfUtil.OutputWriter(tbOutput));
            //Console.SetOut(new UassetReader.WpfUtil.MultiOutTextWriter(new WpfUtil.OutputWriter(tbOutput), Console.Out));
            //Console.WriteLine("Console ready.\n");
            Console.WriteLine("Console disabled.\n");

            try { mainVM = (MainViewModel)DataContext; }
            catch { System.Diagnostics.Debug.WriteLine("Could not set Main VM. Data context may not have been set."); }

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
            //var binding = new System.Windows.Data.Binding
            //{
            //    Source = mainVM,
            //    Path = new PropertyPath("DeclarationItems"),
            //    Mode = System.Windows.Data.BindingMode.OneWay
            //};

            //System.Windows.Data.BindingOperations.SetBinding(declarationViewer, DataGrid.ItemsSourceProperty, binding);
#endif
        }

        /// <summary>
        /// Handles requests for dialog boxes received via the messenger.
        /// The message's Notification property tells which type of dialog box is being requested.
        /// If a "Save As" dialog box is requested, the message's Content property tells the file path of the original save file.
        /// </summary>
        /// <param name="message"></param>
        private void DialogRequestHandler(NotificationMessage<string> message)
        {
            string str = message.Notification;
            switch (str) {
                case Constants.REQUESTTYPE_FOLDER:
                    DisplayFolderBrowserDialog();
                    break;
                case Constants.REQUESTTYPE_SAVEAS:
                    DisplaySaveAsDialog(message.Content);
                    break;
                default: break;
            } 
        }

        private void DisplayFolderBrowserDialog()
        {
            // display
            var dialog = new BetterFolderBrowser() {
                    Title = Constants.TITLE_FOLDERSELECTION,
                    RootFolder = System.IO.Path.GetDirectoryName(filePath),
                    Multiselect = false
                };

            // return result via messenger
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                _messenger.Send<string>(token:   Constants.RESPONSETYPE_FOLDER, 
                                        message: dialog.SelectedFolder);
        }

        private void DisplaySaveAsDialog(string path)
        {
            var fileInfo = new FileInfo(path);

            var saveDialog = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "UAsset files (*.uasset)|*.uasset|All files (*.*)|*.*",
                FileName = fileInfo.Name
            };
            
            if (saveDialog.ShowDialog() == false) return;

            // send message with result
            _messenger.Send<string>(token:   Constants.RESPONSETYPE_SAVEAS,
                                    message: saveDialog.FileName);
        }
    }
}