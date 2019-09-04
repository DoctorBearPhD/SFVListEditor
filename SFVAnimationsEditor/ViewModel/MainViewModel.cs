using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using SFVAnimationsEditor.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

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
        private const string ANIMATION_NAME_TYPE = "AnimSequence";
        private const string ANIMATION_PATH_TYPE = "Package";
        private const string ANIMATION_NAME_NAMESPACE = "/Script/Engine";
        private const string ANIMATION_PATH_NAMESPACE = "/Script/CoreUObject";

        private readonly IDataService _dataService;

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


        public AnimationsEditorViewModel AnimationsEditor { get; set; }
        //public StringEditorViewModel StringEditor { get; set; }


        public RelayCommand OpenFileCommand => new RelayCommand( () => OpenFile() );
        public RelayCommand SaveAsCommand => new RelayCommand(SaveAs, () => FilePath != "");



        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService = null)
        {
            _dataService = dataService;
            //_dataService.GetData(
            //    (item, error) =>
            //    {
            //        if (error != null)
            //        {
            //            // Report error here
            //            return;
            //        }
            //    });

            AnimationsEditor = SimpleIoc.Default.GetInstance<AnimationsEditorViewModel>();
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
            var strPropComparer = new StringPropertyComparer();

            _UFile = new UassetFile();

            try
            {
                _UFile.ReadUasset(ref br);
                UFile = _UFile;
                ////StringEditor.UFileStringList = UFile.StringList;
                OriginalStringList = UFile.StringList;
                OriginalDeclareBlock = UFile.Declaration;

                AnimationsEditor.GetAnimationList(UFile.ContentStruct, UFile.Declaration);

                // Remove animation names/paths from Original List, leaving only the strings that won't be changed.
                UnchangedStringList = new List<StringProperty>(OriginalStringList.Except(AnimationsEditor.AnimationStrings, strPropComparer));

                // do the same for the Declare Block
                //  remove entries whose name exists in the AnimationStrings list
                UnchangedDeclareBlock = new DeclarationBlock();
                
                //  comparing DeclarationBlock.Items to List<StringProperty>
                //  comparing DeclarationItem Name (string) to StringProperty Value (string)
                var animationStrings = AnimationsEditor.AnimationStrings.Select(s => s.Value);

                foreach (var item in OriginalDeclareBlock.Items)
                {
                    if (!animationStrings.Contains(item.Name))
                        UnchangedDeclareBlock.Items.Add(item);
                }
            }
            catch
            {
                throw;
            }
        }

        private void SaveAs()
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "UAsset files (*.uasset)|*.uasset|All files (*.*)|*.*"
            };

            if (saveDialog.ShowDialog() == false) return;

            Console.WriteLine($"\n({DateTime.Now}) Saving..." );

            // Update Strings List
            //   Get all strings used into a list (order alphabetically for bonus points)
            UpdateStringsList();

            // Update Declare Block
            UpdateDeclareBlock();

            // Convert Animations List into Uasset Form (ArrayProperty, etc)
            UpdateContent();

            // Update Imports List
            UpdateImports();

            try
            {
                // make temp file
                FileInfo fileInfo = new FileInfo(FilePath);
                var tempName = fileInfo.FullName + ".tmp";
                File.Move(fileInfo.FullName, tempName); // NOTE: will crash if .tmp file already exists

                // save
                var br = new BinaryReader(File.OpenRead(tempName));
                var bw = new BinaryWriter(File.Create(saveDialog.FileName));
                UFile.WriteUasset(ref br, ref bw);
                Console.WriteLine($"\n({DateTime.Now}) Save complete.");

                // delete temp file
                br.Dispose(); // release resources used by reader so the temp file can be deleted
                File.Delete(tempName);
            }
            catch (Exception)
            {
                throw;
            }
            
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

            // make backup
            File.Copy(FilePath, FilePath + ".bak", true);

            // restart main viewmodel
            ReadFile();
        }

        private void UpdateStringsList()
        {
            ModifiedStringList = new List<StringProperty>(UnchangedStringList);

            ModifiedStringList.AddRange(AnimationsEditor.GetAnimationStrings());
            ModifiedStringList = ModifiedStringList.Distinct(new StringPropertyComparer()).ToList();
            ModifiedStringList.Sort();
            UFile.StringList = new ObservableCollection<StringProperty>(ModifiedStringList);
        }

        private void UpdateDeclareBlock()
        {
            // original order is actually based on path name; see notes for more

            var comparer = new DeclarationComparer();
            var pathsDeclareBlock = new DeclarationBlock();

            // note: can't directly add items because the original values would be overwritten
            foreach (var item in UnchangedDeclareBlock.Items)
            {
                if (item.Type == ANIMATION_PATH_TYPE) // same type for all paths
                {
                    pathsDeclareBlock.Items.Add(new DeclarationItem() {
                        Id = item.Id,
                        Name = item.Name,
                        Namespace = item.Namespace,
                        Type = item.Type,
                        Depends = item.Depends,
                        Item6 = item.Item6,
                        Items = item.Items
                    });
                }
            }

            DeclarationItem path;

            // Get all path items
            foreach(var animSeqList in AnimationsEditor.AnimSeqLists)
            {
                foreach(var animItem in animSeqList.Items)
                {
                    if (animItem.Name == "" || animItem.Path == "")
                        continue;
                    
                    // add declaration items for each path
                    
                    //animItem.Path;
                    path = new DeclarationItem()
                    {
                        Name = animItem.Path,
                        Namespace = ANIMATION_PATH_NAMESPACE,
                        Type = ANIMATION_PATH_TYPE,
                        Item6 = animItem.Item6
                    };

                    pathsDeclareBlock.Items.Add(path);
                }
            }

            pathsDeclareBlock.Items = pathsDeclareBlock.Items
                .Distinct(comparer)
                .OrderBy(item => item.Name)
                .ThenBy(item => item.Item6)
                .ToList();

            // for each item in path list, find the path in the animseqlists 
            //   and add the item's name property (as a declaration item) to a new list (in order)

            ModifiedDeclareBlock = new DeclarationBlock();

            DeclarationItem animNameDeclareItem;
            AnimationListItem animationItem;
            var foundItemName = false;

            for (var i = 0; i < pathsDeclareBlock.Count; i++)
            {
                foundItemName = false;

                foreach(var animSeqList in AnimationsEditor.AnimSeqLists)
                {
                    animationItem = animSeqList.Items
                        .FirstOrDefault(item => item.Path == pathsDeclareBlock.Items[i].Name && item.Item6 == pathsDeclareBlock.Items[i].Item6);
                    
                    if (animationItem?.Name == null)
                        continue;

                    //animationItem.Name;
                    animNameDeclareItem = new DeclarationItem()
                    {
                        Name = animationItem.Name,
                        Namespace = ANIMATION_NAME_NAMESPACE,
                        Type = ANIMATION_NAME_TYPE,
                        Item6 = animationItem.Item6
                    };

                    ModifiedDeclareBlock.Items.Add(animNameDeclareItem);
                    foundItemName = true;
                    break;
                }

                if (foundItemName)
                    continue;
            }

            // add non-animation, non-package items to list
            //  note: can't directly add items because the original values would be overwritten
            foreach (var item in UnchangedDeclareBlock.Items)
            {
                if (item.Type != ANIMATION_PATH_TYPE) // same type for all paths
                {
                    ModifiedDeclareBlock.Items.Add(new DeclarationItem()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Namespace = item.Namespace,
                        Type = item.Type,
                        Depends = item.Depends,
                        Item6 = item.Item6,
                        Items = item.Items
                    });
                }
            }
            
            // add paths to end of the names list
            ModifiedDeclareBlock.Items.AddRange(pathsDeclareBlock.Items);

            // sort items and re-assign ids based on index
            ModifiedDeclareBlock.Items = ModifiedDeclareBlock.Items
                .Distinct(comparer)
                .ToList();

            for (var i = 0; i < ModifiedDeclareBlock.Count; i++)
            {
                ModifiedDeclareBlock.Items[i].Id = i;
            }

            // assign Depends where needed
            DeclarationItem pathItem;

            foreach (var animSeqList in AnimationsEditor.AnimSeqLists)
            {
                foreach (var animItem in animSeqList.Items)
                {
                    if (animItem.Name == "" || animItem.Path == "")
                        continue;

                    pathItem = ModifiedDeclareBlock.Items
                        .Find(item => item.Name == animItem.Path && item.Item6 == animItem.Item6); // find animation path in declaration

                    ModifiedDeclareBlock.Items
                        .Find(item => item.Name == animItem.Name && item.Item6 == pathItem.Item6) // find animation name in declaration 
                        .Depends = -pathItem.Id - 1; // and set dependency
                }
            }

            // Update Depends of non-animation, non-package items
            foreach (var unchangedPackageItem in UnchangedDeclareBlock.Items.Where(item => item.Type == ANIMATION_PATH_TYPE))
            {
                // find the new id of the package item
                var newId = ModifiedDeclareBlock.Items
                    .Find(packageItem => comparer.Equals(packageItem, unchangedPackageItem)).Id;
                // find the item and update its depends id to the new id
                // get the name of the item whose depends id is this package item's id. this is the item to update.
                var dependentItemName = UnchangedDeclareBlock.Items.Find(item => item.Depends == (-unchangedPackageItem.Id - 1)).Name;
                // use the name to find the [item to update] in the ModifiedDeclareBlock
                ModifiedDeclareBlock.Items.Find(item => item.Name == dependentItemName).Depends = -newId - 1;
            }

            UFile.Declaration = ModifiedDeclareBlock;
        }

        private void UpdateContent()
        {
            UFile.ContentStruct = AnimationsEditor.GetModifiedContent(ModifiedDeclareBlock);
        }

        private void UpdateImports()
        {
            ModifiedImportBlock = new ImportBlock();

            foreach (var item in ModifiedDeclareBlock.Items)
            {
                if (item.Type == "AnimSequence")
                    ModifiedImportBlock.Items.Add(item.Id);
            }
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