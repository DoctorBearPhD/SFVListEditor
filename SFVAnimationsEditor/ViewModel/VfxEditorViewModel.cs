using GalaSoft.MvvmLight;
using SFVAnimationsEditor.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SFVAnimationsEditor.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class VfxEditorViewModel : BaseEditorViewModel
    {
        public const string CONTAINER_KEY = "Container";
        ////public const string ARRAY_KEY = "Container";

        public override string ITEM_NAME_TYPE => "KWParticleSystemListDataAsset";
        public override string ITEM_PATH_TYPE => "Package";
        public override string ITEM_NAME_NAMESPACE => "/Script/KiwiVfx";
        public override string ITEM_PATH_NAMESPACE => "/Script/CoreUObject";

        private VfxList _VfxList;
        public  VfxList VfxList
        {
            get => _VfxList;
            set => Set(ref _VfxList, value);
        }
        
        public List<StringProperty> VfxStrings;


        public void GetVfxList(StructProperty content, DeclarationBlock declare)
        {
            if (!content.Value.ContainsKey(CONTAINER_KEY))
            {
                Console.WriteLine("WARNING!!! - No readable content found! (Is this a PSListContainer UAsset file?)");
                return;
            }

            VfxList = new VfxList( items: new ObservableCollection<VfxListItem>() );

            VfxListItem vfxItemVm;
            ObjectProperty item;

            DeclarationItem declareItem;

            int vfxNameId;
            int vfxPathId;
            int vfxItem6;

            string vfxName;
            string vfxPath;

            var vfxContainer = (ArrayProperty)content.Value[CONTAINER_KEY];

            // iterate through Container
            for (var i = 0; i < vfxContainer.Count; i++)
            {
                //var vfxListItemsArrayProperty = (ArrayProperty)vfxContainer.Items[i];

                item = (ObjectProperty)vfxContainer.Items[i];

                // if it's null, add a blank item and continue
                if (item.Id == -1)
                {
                    vfxItemVm = new VfxListItem(i, "", "");
                    VfxList.Items.Add(vfxItemVm);
                    continue;
                }

                #region Get VFX Name and Path (and Item6)

                #region Get VFX Name

                vfxName = item.Name;
                vfxNameId = item.Id; // get Id of VFX Name

                declareItem = declare.Items
                    .First(di => di.Id == vfxNameId); // use Id to find item in the Declare Block

                vfxItem6 = declareItem.Item6;

                #endregion

                #region Get VFX Path

                vfxPathId = -declareItem.Depends - 1; // use Declare Block item to get Depends Id (positive form)

                declareItem = declare.Items
                    .First(di => di.Id == vfxPathId); // use Depends Id to find the VFX Path's Declare Block item

                vfxPath = declareItem.Name; // get Path from Declare Block item

                if (declareItem.Item6 != vfxItem6)
                    System.Diagnostics.Debug.WriteLine("Something went wrong. Item6 was not the same for declaration items of vfx name and path.");

                #endregion

                #endregion

                // add strings to VFX Strings List (List of Modifiable Strings, for later)
                VfxStrings.AddRange(new StringProperty[] { new StringProperty(vfxName), new StringProperty(vfxPath) });
                // add Item to VfxList
                vfxItemVm = new VfxListItem(i, vfxName, vfxPath, vfxItem6);
                VfxList.Items.Add(vfxItemVm);
            }
        }

        public override IList<StringProperty> GetStrings()
        {
            var result = new List<StringProperty>();
            VfxListItem item;

            for (var i = 0; i < VfxList.Items.Count; i++)
            {
                item = VfxList.Items[i];

                if (item.Name == "" || item.Path == "")
                    continue;

                result.Add(new StringProperty(item.Name));
                result.Add(new StringProperty(item.Path));
            }
            
            return result;
        }

        public override StructProperty GetModifiedContent(DeclarationBlock declare)
        {
            var contentStruct = new StructProperty();
            var vfxContainer = new ArrayProperty() { PropertyType = "ObjectProperty" };

            ObjectProperty vfxListItem;

            // iterate through VfxList
            for (var i = 0; i < VfxList.Items.Count; i++)
            {
                // convert Items in VfxList into ObjectProperty`s

                vfxListItem = new ObjectProperty { Name = VfxList.Items[i].Name };

                if (vfxListItem.Name == "") vfxListItem.Id = -1;
                else
                    vfxListItem.Id = declare.Items
                        .FirstOrDefault(item => item.Name == vfxListItem.Name && item.Item6 == VfxList.Items[i].Item6).Id;

                vfxContainer.Items.Add(vfxListItem);
            }

            contentStruct.Value[CONTAINER_KEY] = vfxContainer;

            return contentStruct;
        }

        internal override IList<DeclarationItem> GetModifiablePathDeclarationItems()
        {
            DeclarationBlock modifiablePathsDeclareBlock = new DeclarationBlock();
            DeclarationItem path;

            // Get all modifiable path items
            foreach (var vfxItem in VfxList.Items)
            {
                if (vfxItem.Name == "" || vfxItem.Path == "")
                    continue;

                // add declaration items for each path

                //vfxItem.Path;
                path = new DeclarationItem()
                {
                    Name = vfxItem.Path,
                    Namespace = ITEM_PATH_NAMESPACE,
                    Type = ITEM_PATH_TYPE,
                    Item6 = vfxItem.Item6
                };

                modifiablePathsDeclareBlock.Items.Add(path);
            }
            

            return modifiablePathsDeclareBlock.Items;
        }

        internal override IList<DeclarationItem> GetNameDeclarationItems(DeclarationBlock pathsDeclareBlock)
        {
            var modifiedDeclareBlock = new DeclarationBlock();

            DeclarationItem vfxNameDeclareItem; // declaration item representing the vfx name
            VfxListItem vfxListItem;            // vm form of the ^
            bool foundItemName;

            // for each path item, search for a name item that corresponds to that path item
            foreach (var pathItem in pathsDeclareBlock.Items)
            {
                foundItemName = false;

                foreach (var vfxItem in VfxList.Items)
                {
                    vfxListItem = VfxList.Items
                        .FirstOrDefault( item => 
                            item.Path  == pathItem.Name  && 
                            item.Item6 == pathItem.Item6
                        );

                    if (vfxListItem?.Name == null)
                        continue;

                    vfxNameDeclareItem = new DeclarationItem()
                    {
                        Name = vfxListItem.Name,
                        Namespace = ITEM_NAME_NAMESPACE,
                        Type = ITEM_NAME_TYPE,
                        Item6 = vfxListItem.Item6
                    };

                    modifiedDeclareBlock.Items.Add(vfxNameDeclareItem);
                    foundItemName = true;
                    break;
                }

                if (foundItemName)
                    continue;
            }

            return modifiedDeclareBlock.Items;
        }

        internal override void UpdateDepends(ref DeclarationBlock modifiedDeclareBlock)
        {
            DeclarationItem pathItem;
            
            foreach (var vfxItem in VfxList.Items)
            {
                if (vfxItem.Name == "" || vfxItem.Path == "")
                    continue;

                pathItem = modifiedDeclareBlock.Items
                    .Find(item => item.Name == vfxItem.Path && item.Item6 == vfxItem.Item6); // find vfx path in declaration

                modifiedDeclareBlock.Items
                    .Find(item => item.Name == vfxItem.Name && item.Item6 == pathItem.Item6) // find vfx name in declaration 
                    .Depends = -pathItem.Id - 1; // and set dependency
            }
        }

        public override void Initialize()
        {
            VfxList = new VfxList();
            VfxStrings = new List<StringProperty>();
        }
    }

    public class VfxList
    {
        public ObservableCollection<VfxListItem> Items { get; set; }


        public VfxList() { }

        public VfxList(ObservableCollection<VfxListItem> items)
        {
            Items = items;
        }

        public void UpdateIndices()
        {
            for (var i = 0; i < Items.Count; i++)
                Items[i].UpdateIndex(i);
        }
    }

    public class VfxListItem
    {
        public int Index { get; private set; } = -1;

        public string Name { get; set; }
        public string Path { get; set; }
        public int Item6 { get; set; }


        public VfxListItem()
        {
            Name = "Name";
            Path = "Path";
            Item6 = 0;
        }

        public VfxListItem(string name, string path, int item6 = 0)
        {
            Name = name;
            Path = path;
            Item6 = item6;
        }

        public VfxListItem(int index, string name, string path, int item6 = 0) : this(name, path, item6)
        {
            Index = index;
        }


        public void UpdateIndex(int index)
        {
            Index = index;
        }


        public override string ToString()
        {
            return $"{this.GetType().Name}; {Index}: {Name}, {Path}";
        }
    }
}