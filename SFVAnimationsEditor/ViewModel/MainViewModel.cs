using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using SFVAnimationsEditor.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

// TODO: Move all animation-specific code to animation-specific handler / AnimationEditorViewModel

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
        public RelayCommand SaveAsCommand => new RelayCommand(SaveAs, () => FilePath != "");
        public RelayCommand ExitCommand => new RelayCommand(Exit);


#if DEBUG
        private ObservableCollection<DeclarationItem> _DeclarationItems;
        public ObservableCollection<DeclarationItem> DeclarationItems { get => _DeclarationItems; set => Set(ref _DeclarationItems, value); }
#endif


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService = null)
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

            AnimationsEditor = SimpleIoc.Default.GetInstance<AnimationsEditorViewModel>();
            VfxEditor = SimpleIoc.Default.GetInstance<VfxEditorViewModel>();
            TrailEditor = SimpleIoc.Default.GetInstance<TrailEditorViewModel>();
            //StringEditor = SimpleIoc.Default.GetInstance<StringEditorViewModel>();
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
                _UFile.ReadUasset(ref br);
                UFile = _UFile;
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
                {
                    Console.WriteLine("WARNING!!! - No readable content found! Only the following types of UAsset files are allowed:\n\t" +
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

        private void SaveAs()
        {
            if (!UFile.ContentStruct.Value.ContainsKey(AnimationsEditorViewModel.CONTAINER_KEY) &&
                !UFile.ContentStruct.Value.ContainsKey(VfxEditorViewModel.CONTAINER_KEY) &&
                !UFile.ContentStruct.Value.ContainsKey(TrailEditorViewModel.CONTAINER_KEY))
            {
                // How did we get here? Probably unnecessary...
                Console.WriteLine("!!!WARNING!!!  Unexpected error occurred while saving! File type could not be determined.");
                return;
            }

            var fileInfo = new FileInfo(FilePath);

            var saveDialog = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "UAsset files (*.uasset)|*.uasset|All files (*.*)|*.*",
                FileName = fileInfo.Name
            };

            if (saveDialog.ShowDialog() == false) return;

            Console.WriteLine($"\n({DateTime.Now}) Saving..." );

            // Update Strings List
            UpdateStringsList();

            // Update Declare Block
            UpdateDeclareBlock();

            // Convert Editor List VM into Uasset Form (ArrayProperty, etc)
            UFile.ContentStruct = CurrentEditor.GetModifiedContent(ModifiedDeclareBlock);
            
            // Update Imports List
            UpdateImports();
            
            try
            {
                // make temp file
                fileInfo = new FileInfo(FilePath);
                var tempName = fileInfo.FullName + ".tmp";

                // delete any accidentally leftover temp file
                if (File.Exists(tempName))
                {
                    if (File.GetAttributes(tempName) != FileAttributes.Temporary)
                        File.SetAttributes(tempName, FileAttributes.Temporary);
                    File.Delete(tempName);
                }
                File.Move(fileInfo.FullName, tempName);
                File.SetAttributes(tempName, FileAttributes.Temporary);

                // save
                var br = new BinaryReader(File.OpenRead(tempName));
                var bw = new BinaryWriter(File.Create(saveDialog.FileName));
                UFile.WriteUasset(ref br, ref bw);
                Console.WriteLine($"\n({DateTime.Now}) Save complete.");

                // delete temp file
                br.Dispose(); // release resources used by reader so the temp file can be deleted/renamed
                if (FilePath != saveDialog.FileName)
                {
                    File.Move(tempName, FilePath); // rename tmp back to original if it wasn't overwritten
                    FilePath = saveDialog.FileName; // Update file path to new path
                }
                else
                {
                    File.Delete(tempName); // delete tmp if original was overwritten
                }
            }
            catch (Exception)
            {
                throw;
            }

#if DEBUG
            DeclarationItems = new ObservableCollection<DeclarationItem>(UFile.Declaration.Items);
#endif
        }

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
            ((System.Windows.Controls.TextBox)App.Current.MainWindow.FindName("tbOutput"))?.Clear();

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
                    token: "ERROR MESSAGE", 
                    message: new GalaSoft.MvvmLight.Messaging.NotificationMessage(e.Message)
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