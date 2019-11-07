using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFVAnimationsEditor.Model;
using SFVAnimationsEditor.Model.Lists;
using SFVAnimationsEditor.ViewModel.Lists;

namespace SFVAnimationsEditor.ViewModel
{
    public class TrailEditorViewModel : BaseEditorViewModel
    {
        public const string CONTAINER_KEY = "TrailDataList";
        public const string ITEM_OBJECT_KEY = "TrailDataAsset";
        public const string ITEM_ASSET_ID_KEY = "AssetId";
        public const string ITEM_SHARE_NUM_KEY = "ShareNum";

        public override string ITEM_NAME_TYPE => "KWTrailDataAsset";
        public override string ITEM_PATH_TYPE => "Package";
        public override string ITEM_NAME_NAMESPACE => "/Script/KiwiChara";
        public override string ITEM_PATH_NAMESPACE => "/Script/CoreUObject";

        private ListVm<TrailListItem> _TrailList;
        public ListVm<TrailListItem> TrailList
        {
            get => _TrailList;
            set => Set(ref _TrailList, value);
        }


        public void GetTrailList(StructProperty content, DeclarationBlock declare)
        {
            if (!content.Value.ContainsKey(CONTAINER_KEY))
            {
                Console.WriteLine("WARNING!!! - No readable content found! (Is this a PSListContainer UAsset file?)");
                return;
            }

            TrailList = new ListVm<TrailListItem>(items: new ObservableCollection<TrailListItem>());

            TrailListItem trailItemVm;
            StructProperty itemStruct;
            ObjectProperty itemObject;

            DeclarationItem declareItem;

            int trailNameId;
            int trailPathId;
            int trailItem6;

            int trailAssetId;
            int trailShareNum;

            string trailName;
            string trailPath;

            var trailContainer = (ArrayProperty)content.Value[CONTAINER_KEY];

            // iterate through Container
            for (var i = 0; i < trailContainer.Count; i++)
            {
                //var trailListItemsArrayProperty = (ArrayProperty)trailContainer.Items[i];

                itemStruct = (StructProperty)trailContainer.Items[i];
                itemObject = (ObjectProperty)itemStruct.Value[ITEM_OBJECT_KEY];

                #region Get Trail Data

                #region Get Trail Name, AssetId, ShareNum, Item6

                trailName = itemObject.Name;
                trailNameId = itemObject.Id; // get Id of Trail Name

                trailAssetId = ((IntProperty)itemStruct.Value[ITEM_ASSET_ID_KEY]).Value;
                trailShareNum = ((IntProperty)itemStruct.Value[ITEM_SHARE_NUM_KEY]).Value;

                declareItem = declare.Items
                    .First(di => di.Id == trailNameId); // use Id to find item in the Declare Block

                trailItem6 = declareItem.Item6;

                #endregion

                #region Get Trail Path

                trailPathId = -declareItem.Depends - 1; // use Declare Block item to get Depends Id (positive form)

                declareItem = declare.Items
                    .First(di => di.Id == trailPathId); // use Depends Id to find the Trail Path's Declare Block item

                trailPath = declareItem.Name; // get Path from Declare Block item

                if (declareItem.Item6 != trailItem6)
                    System.Diagnostics.Debug.WriteLine("Something went wrong. Item6 was not the same for declaration items of trail name and path.");

                #endregion

                #endregion

                // add strings to Trail Strings List (List of Modifiable Strings, for later)
                Strings.AddRange(new StringProperty[] { new StringProperty(trailName), new StringProperty(trailPath) });
                // add Item to TrailList
                trailItemVm = new TrailListItem(i, trailName, trailPath, trailAssetId, trailShareNum, trailItem6);
                TrailList.Items.Add(trailItemVm);
            }
        }


        public override StructProperty GetModifiedContent(DeclarationBlock declare)
        {
            var contentStruct = new StructProperty();
            var trailContainer = new ArrayProperty() { PropertyType = "StructProperty" };

            StructProperty trailListItem;
            ObjectProperty trailListItemObject;


            // iterate through TrailList
            for (var i = 0; i < TrailList.Items.Count; i++)
            {
                // convert Items in TrailList into StructProperty`s

                trailListItem = new StructProperty();

                #region Obtain ObjectProperty form of TrailListItem

                trailListItemObject = new ObjectProperty { Name = TrailList.Items[i].Name };

                if (trailListItemObject.Name == "") trailListItemObject.Id = -1;
                else
                    trailListItemObject.Id = declare.Items
                        .FirstOrDefault(item => item.Name == trailListItemObject.Name && item.Item6 == TrailList.Items[i].Item6).Id;

                #endregion

                trailListItem.Value[ITEM_ASSET_ID_KEY]  = new IntProperty(TrailList.Items[i].AssetId);
                trailListItem.Value[ITEM_SHARE_NUM_KEY] = new IntProperty(TrailList.Items[i].ShareNum);
                trailListItem.Value[ITEM_OBJECT_KEY] = trailListItemObject;

                trailContainer.Items.Add(trailListItem);
            }

            contentStruct.Value[CONTAINER_KEY] = trailContainer;

            return contentStruct;
        }

        public override IList<StringProperty> GetStrings()
        {
            var result = new List<StringProperty>();
            ListItem item;

            for (var i = 0; i < TrailList.Items.Count; i++)
            {
                item = TrailList.Items[i];

                if (item.Name == "" || item.Path == "")
                    continue;

                result.Add(new StringProperty(item.Name));
                result.Add(new StringProperty(item.Path));
            }

            return result;
        }

        public override void Initialize()
        {
            TrailList = new ListVm<TrailListItem>();
            Strings = new List<StringProperty>();
        }

        internal override IList<DeclarationItem> GetModifiablePathDeclarationItems()
        {
            DeclarationBlock modifiablePathsDeclareBlock = new DeclarationBlock();
            DeclarationItem path;

            // Get all modifiable path items
            foreach (var trailItem in TrailList.Items)
            {
                if (trailItem.Name == "" || trailItem.Path == "")
                    continue;

                // add declaration items for each path

                //trailItem.Path;
                path = new DeclarationItem()
                {
                    Name = trailItem.Path,
                    Namespace = ITEM_PATH_NAMESPACE,
                    Type = ITEM_PATH_TYPE,
                    Item6 = trailItem.Item6
                };

                modifiablePathsDeclareBlock.Items.Add(path);
            }


            return modifiablePathsDeclareBlock.Items;
        }

        internal override IList<DeclarationItem> GetNameDeclarationItems(DeclarationBlock pathsDeclareBlock)
        {
            var modifiedDeclareBlock = new DeclarationBlock();

            DeclarationItem trailNameDeclareItem; // declaration item representing the trail name
            ListItem trailListItem;            // vm form of the ^
            bool foundItemName;

            // for each path item, search for a name item that corresponds to that path item
            foreach (var pathItem in pathsDeclareBlock.Items)
            {
                foundItemName = false;

                foreach (var trailItem in TrailList.Items)
                {
                    trailListItem = TrailList.Items
                        .FirstOrDefault(item =>
                           item.Path == pathItem.Name &&
                           item.Item6 == pathItem.Item6
                        );

                    if (trailListItem?.Name == null)
                        continue;

                    trailNameDeclareItem = new DeclarationItem()
                    {
                        Name = trailListItem.Name,
                        Namespace = ITEM_NAME_NAMESPACE,
                        Type = ITEM_NAME_TYPE,
                        Item6 = trailListItem.Item6
                    };

                    modifiedDeclareBlock.Items.Add(trailNameDeclareItem);
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

            foreach (var trailItem in TrailList.Items)
            {
                if (trailItem.Name == "" || trailItem.Path == "")
                    continue;

                pathItem = modifiedDeclareBlock.Items
                    .Find(item => item.Name == trailItem.Path && item.Item6 == trailItem.Item6); // find trail path in declaration

                modifiedDeclareBlock.Items
                    .Find(item => item.Name == trailItem.Name && item.Item6 == pathItem.Item6) // find trail name in declaration 
                    .Depends = -pathItem.Id - 1; // and set dependency
            }
        }
    }
}
