using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using SFVAnimationsEditor.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UassetLib;
using SFVAnimationsEditor.Resources;
using GalaSoft.MvvmLight.Messaging;
using System.Text.RegularExpressions;
using SFVAnimationsEditor.WpfUtil;

namespace SFVAnimationsEditor.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        ////private readonly IDataService _dataService;
        private readonly IDialogService _dialogService;

        public string FilePath = "";

        private UassetFile _UFile;
        public UassetFile UFile {
            get => _UFile;
            set => Set(ref _UFile, value);
        }

        private ObservableCollection<StringProperty> _OriginalStringList;
        private ObservableCollection<StringProperty> OriginalStringList
        {
            get => _OriginalStringList;
            set => Set(ref _OriginalStringList, value);
        }

        private List<StringProperty> _ModifiedStringList;
        private List<StringProperty> ModifiedStringList
        {
            get => _ModifiedStringList;
            set => Set(ref _ModifiedStringList, value);
        }

        private List<StringProperty> _UnchangedStringList;
        private List<StringProperty> UnchangedStringList
        {
            get => _UnchangedStringList;
            set => Set(ref _UnchangedStringList, value);
        }

        private DeclarationBlock _OriginalDeclareBlock;
        private DeclarationBlock OriginalDeclareBlock
        {
            get => _OriginalDeclareBlock;
            set => Set(ref _OriginalDeclareBlock, value);
        }

        private DeclarationBlock _ModifiedDeclareBlock;
        private DeclarationBlock ModifiedDeclareBlock
        {
            get => _ModifiedDeclareBlock;
            set => Set(ref _ModifiedDeclareBlock, value);
        }

        private DeclarationBlock _UnchangedDeclareBlock;
        private DeclarationBlock UnchangedDeclareBlock
        {
            get => _UnchangedDeclareBlock;
            set => Set(ref _UnchangedDeclareBlock, value);
        }

        private ImportBlock _ModifiedImportBlock;
        private ImportBlock ModifiedImportBlock
        {
            get => _ModifiedImportBlock;
            set => Set(ref _ModifiedImportBlock, value);
        }


        private BaseEditorViewModel _CurrentEditor;
        public  BaseEditorViewModel CurrentEditor
        {
            get => _CurrentEditor;
            set => Set(ref _CurrentEditor, value);
        }

        public AnimationsEditorViewModel AnimationsEditor { get; set; }
        public VfxEditorViewModel VfxEditor { get; set; }
        public TrailEditorViewModel TrailEditor { get; set; }
        //public StringEditorViewModel StringEditor { get; set; }


        public RelayCommand OpenFileCommand => new RelayCommand( () => OpenFile() );
        public RelayCommand SaveAsCommand   => new RelayCommand(RequestSaveAs, () => FilePath != "");
        public RelayCommand SaveAllCommand  => new RelayCommand(RequestSaveAll, CanExecuteSaveAll);
        public RelayCommand ExitCommand     => new RelayCommand(Exit);


#if DEBUG
        private ObservableCollection<DeclarationItem> _DeclarationItems;
        public ObservableCollection<DeclarationItem> DeclarationItems { get => _DeclarationItems; set => Set(ref _DeclarationItems, value); }
#endif


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDialogService dialogService)
        {
            ////_dataService = dataService;
            ////_dataService.GetData(
            ////    (item, error) =>
            ////    {
            ////        if (error != null)
            ////        {
            ////            // Report error here
            ////            return;
            ////        }
            ////    });

            _dialogService = dialogService;

            AnimationsEditor = SimpleIoc.Default.GetInstance<AnimationsEditorViewModel>();
            VfxEditor = SimpleIoc.Default.GetInstance<VfxEditorViewModel>();
            TrailEditor = SimpleIoc.Default.GetInstance<TrailEditorViewModel>();
            //StringEditor = SimpleIoc.Default.GetInstance<StringEditorViewModel>();

            MessengerInstance.Register<string>(recipient: this,
                                               token: Constants.RESPONSETYPE_SAVEAS,
                                               action: SaveAs);

            MessengerInstance.Register<string>(recipient: this,
                                               token: Constants.RESPONSETYPE_FOLDER,
                                               action: SaveAll);
        }


        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
        
        public void ReadFile()
        {
            var br = new BinaryReader(File.OpenRead(FilePath));
            
            _UFile = new UassetFile();

            try
            {
                UFile.ReadUasset(ref br);
                ////StringEditor.UFileStringList = UFile.StringList;
                OriginalStringList = UFile.StringList;
                OriginalDeclareBlock = UFile.Declaration;

#if DEBUG
                DeclarationItems = new ObservableCollection<DeclarationItem>(OriginalDeclareBlock.Items);
#endif

                if (UFile.ContentStruct.Value.ContainsKey(AnimationsEditorViewModel.CONTAINER_KEY))
                {
                    CurrentEditor = AnimationsEditor;
                    CurrentEditor.Initialize(); // reset to default
                    AnimationsEditor.GetAnimationList(UFile.ContentStruct, UFile.Declaration);
                }
                else if (UFile.ContentStruct.Value.ContainsKey(VfxEditorViewModel.CONTAINER_KEY))
                {
                    CurrentEditor = VfxEditor;
                    CurrentEditor.Initialize(); // reset to default
                    VfxEditor.GetVfxList(UFile.ContentStruct, UFile.Declaration);
                }
                else if (UFile.ContentStruct.Value.ContainsKey(TrailEditorViewModel.CONTAINER_KEY))
                {
                    CurrentEditor = TrailEditor;
                    CurrentEditor.Initialize();
                    TrailEditor.GetTrailList(UFile.ContentStruct, UFile.Declaration);
                }
                else
                { // TODO: Display using DialogService
                    Console.WriteLine("WARNING!!! - Could not display the file contents! Only the following types of UAsset files are allowed:\n\t" +
                        "1) AnimSeqWithIdList\n\t" +
                        "2) PSListContainer\n\t" +
                        "3) TrailList\n");
                    return;
                }

                // Remove modifiable names/paths from Original List, leaving only the strings that won't be changed.
                SetUnchangedStringList(CurrentEditor.Strings);

                // do the same for the Declare Block
                //  remove entries whose name exists in the strings list
                SetUnchangedDeclareBlock(CurrentEditor.Strings);
            }
            catch
            {
                throw;
            }
        }

        public void SetUnchangedStringList(IList<StringProperty> strings)
        {
            UnchangedStringList = new List<StringProperty>(OriginalStringList.Except(strings, new StringPropertyComparer()));
        }

        public void SetUnchangedDeclareBlock(IList<StringProperty> stringList)
        {
            UnchangedDeclareBlock = new DeclarationBlock();

            //  comparing DeclarationBlock.Items to List<StringProperty>
            //  comparing DeclarationItem Name (string) to StringProperty Value (string)
            var strings = stringList.Select(s => s.Value);

            foreach (var item in OriginalDeclareBlock.Items)
            {
                if (!strings.Contains(item.Name))
                    UnchangedDeclareBlock.Items.Add(item);
            }
        }

        private void RequestSaveAs()
        {
            if (!UFile.ContentStruct.Value.ContainsKey(AnimationsEditorViewModel.CONTAINER_KEY) &&
                !UFile.ContentStruct.Value.ContainsKey(VfxEditorViewModel.CONTAINER_KEY) &&
                !UFile.ContentStruct.Value.ContainsKey(TrailEditorViewModel.CONTAINER_KEY))
            {
                // How did we get here? Probably unnecessary...
                Console.WriteLine("!!!WARNING!!!  Unexpected error occurred while saving! File type could not be determined.");
                return;
            }

            MessengerInstance.Send<NotificationMessage<string>>(
                token:   Constants.REQUEST_DIALOG,
                message: new NotificationMessage<string>(FilePath, Constants.REQUESTTYPE_SAVEAS));
        }

        private void SaveAs(string savePath)
        {
            Console.WriteLine($"\n({DateTime.Now}) Saving..." );

            CommitData(); // set Uasset object's data to be the modified data
            
            try
            {
                var tempPath = CreateTempFile();

                // save
                var br = new BinaryReader(File.OpenRead(tempPath));
                var bw = new BinaryWriter(File.Create(savePath));
                UFile.WriteUasset(ref br, ref bw);
                
                // finish, delete temp file
                br.Dispose(); // release resources used by reader so the temp file can be deleted/renamed
                DeleteTempFile(tempPath, savePath);

                FilePath = savePath; // Update FilePath to the new path

                Console.WriteLine($"\n({DateTime.Now}) Save complete.");
            }
            catch (Exception e)
            {
                _dialogService.ShowError(e, "An error occurred while saving");
            }

#if DEBUG
            //DeclarationItems = new ObservableCollection<DeclarationItem>(UFile.Declaration.Items);
#endif
        }

        private void CommitData()
        {
            // Update Strings List
            UpdateStringsList();

            // Update Declare Block
            UpdateDeclareBlock();

            // Convert Editor List VM into Uasset Form (ArrayProperty, etc)
            UFile.ContentStruct = CurrentEditor.GetModifiedContent(ModifiedDeclareBlock);

            // Update Imports List
            UpdateImports();
        }

        private string CreateTempFile()
        {
            // make temp file
            var fileInfo = new FileInfo(FilePath);
            var tempName = fileInfo.FullName + ".tmp";

            // delete any accidentally leftover temp file
            if (File.Exists(tempName)) File.Delete(tempName);

            File.Move(fileInfo.FullName, tempName); // rename original file to temp name, so we can overwrite the original file, if needed
            File.SetAttributes(tempName, FileAttributes.Temporary);

            return tempName;
        }

        private void DeleteTempFile(string tempPath, string savePath)
        {
            if (FilePath != savePath)          // if user saved the file under a different name...
                File.Move(tempPath, FilePath); // rename tmp back to original name since it wasn't 
            else
                File.Delete(tempPath);         // or just delete tmp if original was overwritten
        }

        private void RequestSaveAll()
        {
            // Send warning to make sure user knows what they're doing!
            bool shouldContinue = _dialogService.ShowMessageWithResult(message: Constants.WARNING_SAVE_ALL, "Warning!");

            // Request folder selection
            if (shouldContinue)
                MessengerInstance.Send<NotificationMessage<string>>(
                    token: Constants.REQUEST_DIALOG, 
                    message: new NotificationMessage<string>("", Constants.REQUESTTYPE_FOLDER)); // for folder selection, we just tell the request type
            
            // if user selects a folder, this VM will receive a message which initiates the callback action, SaveAll

            // ? - Let Animations Editor VM handle the actual saving? Spaghetti code? Separation of duties? Who knows?
        }

        private void SaveAll(string selectedFolder)
        {
            // generate a list of files that would be overridden
            Regex fileNamePatternRegex = new Regex(@"(...)\\DataAsset\\DA_\1_AnimSeqWithIdContainer\.uasset"); // regex to use in search
            const string fileNamePattern = @"DA_???_AnimSeqWithIdContainer.uasset"; // pattern (with wildcards) to use in search

            List<string> matchingFiles;
            string matchingFilesString = "";

            try
            {
                matchingFiles = new List<string>(Directory.EnumerateFiles(
                    selectedFolder, fileNamePattern, SearchOption.AllDirectories)
                    .Where(path => fileNamePatternRegex.IsMatch(path, selectedFolder.Length + 1)));

                foreach (string s in matchingFiles)
                {
                    matchingFilesString += s.Remove(0, selectedFolder.Length + 1) + "\n";
                }

                if (matchingFiles.Count == 0) 
                    throw new Exception("Could not find any files to change. (Did you select a Chara folder?)");

                // display list, return if cancelled
                if (!_dialogService.ShowMessageWithResult(matchingFilesString, "These files will be changed")) 
                    return;
            }
            catch (DirectoryNotFoundException badDirEx)
            {
                _dialogService.ShowError(badDirEx.Message, "Could not find directory!");
                return;
            }
            catch (Exception ex) // misc exceptions
            {
                _dialogService.ShowError(ex.Message, "Something went wrong...");
                return;
            }

            // save backups for all files to be changed

            // create backup folder using selectedFolder
            List<string> backupFiles = new List<string>(matchingFiles);
            
            string backupFolderName = selectedFolder + "Backup";
            string newFilePath = "";
            string newFileDir  = "";

            for (var i = 0; i < backupFiles.Count; i++)
            {
                string path = backupFiles[i];
                newFilePath = path.Replace(selectedFolder, backupFolderName); // replaces "Chara" with "CharaBackup"
                newFileDir  = new FileInfo(newFilePath).DirectoryName;        // gets the path of the file without the file name
                Directory.CreateDirectory(newFileDir);                        // makes new (sub)directories as needed

                File.Copy(matchingFiles[i], newFilePath, true);
            }
        }

        // What is isFilePreselected actually doing? Why did I put that there? Rename it so it makes sense >:(
        public void OpenFile(bool isFilePreselected = false)
        {
            if (!isFilePreselected)
            {
                // create open file dialog
                var dialog = new Microsoft.Win32.OpenFileDialog()
                {
                    Filter = "UAsset files (*.uasset)|*.uasset|All files (*.*)|*.*"
                };

                if (dialog.ShowDialog() == false) return;

                // set file path
                FilePath = dialog.FileName;
            }

            if (FilePath == "") return;

            // ***WARNING: Shortcutting my design here by referencing the view from inside the viewmodel... D:
            ((System.Windows.Controls.TextBox)App.Current.MainWindow.FindName("tbOutput"))?.Clear(); // clears the output panel's text

            // make backup
            try
            {
                /* NOTE: Using try/catch and setting file attributes because 
                 *  UnauthorizedAccessException can happen when using shared files.
                 */
                var bakFilePath = FilePath + ".bak";

                if (File.Exists(bakFilePath) && File.GetAttributes(bakFilePath) != FileAttributes.Normal)
                    File.SetAttributes(bakFilePath, FileAttributes.Normal);

                File.Copy(FilePath, bakFilePath, true);
                File.SetAttributes(bakFilePath, FileAttributes.Normal);
            }
            catch (Exception e)
            {
                MessengerInstance.Send(
                    token: Constants.DISPLAY_MESSAGE, 
                    message: new NotificationMessage(e.Message)
                );
            }

            // restart main viewmodel
            ReadFile();
        }

        private void UpdateDeclareBlock()
        {
            ModifiedDeclareBlock = CurrentEditor.UpdateDeclareBlock(UnchangedDeclareBlock);
            UFile.Declaration = ModifiedDeclareBlock;
        }
        
        /// <summary>
        /// Puts all strings used into a list (ordered alphabetically) 
        /// and assigns it as the Uasset file's StringsList.
        /// </summary>
        private void UpdateStringsList()
        {
            ModifiedStringList = new List<StringProperty>(UnchangedStringList);

            ModifiedStringList.AddRange(CurrentEditor.GetStrings());
            ModifiedStringList = ModifiedStringList.Distinct(new StringPropertyComparer()).ToList();
            ModifiedStringList.Sort();
            UFile.StringList = new ObservableCollection<StringProperty>(ModifiedStringList);
        }

        private void UpdateImports()
        {
            ModifiedImportBlock = new ImportBlock();

            foreach (var item in ModifiedDeclareBlock.Items)
            {
                if (item.Type == CurrentEditor.ITEM_NAME_TYPE)
                    ModifiedImportBlock.Items.Add(item.Id);
            }

            UFile.Imports = ModifiedImportBlock;
        }

        private bool CanExecuteSaveAll()
        {
            return (CurrentEditor is AnimationsEditorViewModel);
        }


        private void Exit()
        {
            App.Current.MainWindow.Close();
        }
    }

    public class StringListVMItem
    {
        public int Id { get; private set; }
        public string Value { get; set; }
        public int Length => Value.Length;


        public StringListVMItem()
        {
            Id = 0;
            Value = "String List Item";
        }

        public void UpdateId(ref IList<StringListVMItem> list)
        {
            Id = list.IndexOf(this);
        }
    }

    public class DeclarationComparer : IEqualityComparer<DeclarationItem>
    {
        public bool Equals(DeclarationItem x, DeclarationItem y)
        {
            return x.Name == y.Name && x.Item6 == y.Item6;
        }

        public int GetHashCode(DeclarationItem obj)
        {
            return obj.Name.GetHashCode() + obj.Item6.GetHashCode();
        }
    }
}