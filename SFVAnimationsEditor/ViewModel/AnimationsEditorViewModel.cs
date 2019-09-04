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
    public class AnimationsEditorViewModel : ViewModelBase
    {
        public const string CONTAINER_KEY = "AnimSeqWithIdListContainer";
        public const string CATEGORY_KEY  = "Description";
        public const string ARRAY_KEY     = "AnimSeqList";

        private ObservableCollection<AnimationList> _AnimSeqLists;
        public ObservableCollection<AnimationList> AnimSeqLists // (Tabs)
        {
            get => _AnimSeqLists;
            set => Set(ref _AnimSeqLists, value);
        }

        public List<StringProperty> AnimationStrings;

        /// <summary>
        /// Initializes a new instance of the AnimationsEditorViewModel class.
        /// </summary>
        public AnimationsEditorViewModel()
        {
            AnimSeqLists = new ObservableCollection<AnimationList>();
            AnimationStrings = new List<StringProperty>();
        }


        public void GetAnimationList(StructProperty content, DeclarationBlock declare)
        {
            if (!content.Value.ContainsKey(CONTAINER_KEY))
            {
                Console.WriteLine("WARNING!!! - No readable content found! (Is this an AnimSeqWithIdList UAsset file?)");
                return;
            }

            AnimSeqLists = new ObservableCollection<AnimationList>();
            
            AnimationList animSeqListVm;
            ObjectProperty item;

            DeclarationItem declareItem;

            int animNameId;
            int animPathId;
            int animItem6;

            string animName;
            string animPath;

            var animContainer = (ArrayProperty)content.Value[CONTAINER_KEY];
            
            // iterate through AnimSeqListWithIdContainer
            for (var i = 0; i < animContainer.Count; i++)
            {
                animSeqListVm = new AnimationList
                {
                    Header = animContainer.Items[i].Value[CATEGORY_KEY],
                    Items = new List<AnimationListItem>()
                };

                var animSeqListItemsArrayProperty = (ArrayProperty)animContainer.Items[i].Value[ARRAY_KEY];

                // iterate through each AnimSeqList
                for (var j = 0; j < animSeqListItemsArrayProperty.Count; j++)
                {
                    item = (ObjectProperty)animSeqListItemsArrayProperty.Items[j];
                    
                    // if it's null, add a blank item and continue
                    if (item.Id == -1)
                    {
                        animSeqListVm.Items.Add(new AnimationListItem("", ""));
                        continue;
                    }

                    #region Get Animation Name and Path (and Item6)

                    #region Get Animation Name

                    animName = item.Name;
                    animNameId = item.Id; // get Id of Animation Name

                    declareItem = declare.Items
                        .First(di => di.Id == animNameId); // use Id to find item in the Declare Block

                    animItem6 = declareItem.Item6;

                    #endregion

                    #region Get Animation Path

                    animPathId = -declareItem.Depends - 1; // use Declare Block item to get Depends Id (positive form)

                    declareItem = declare.Items
                        .First(di => di.Id == animPathId); // use Depends Id to find the Animation Path's Declare Block item

                    animPath = declareItem.Name; // get Path from Declare Block item

                    if (declareItem.Item6 != animItem6)
                        System.Diagnostics.Debug.WriteLine("Something went wrong. Item6 was not the same for declaration items of animation name and path.");

                    #endregion

                    #endregion

                    // add strings to Animation Strings List (List of Modifiable Strings, for later)
                    AnimationStrings.AddRange(new StringProperty[] { new StringProperty(animName), new StringProperty(animPath) });
                    // add Items to AnimationList
                    animSeqListVm.Items.Add(new AnimationListItem(animName, animPath, animItem6));
                }

                // add AnimationList to Container
                AnimSeqLists.Add(animSeqListVm);
            }
        }

        public IList<StringProperty> GetAnimationStrings()
        {
            var result = new List<StringProperty>();
            AnimationListItem item;

            foreach(var list in AnimSeqLists)
            {
                for (var i = 0; i < list.Items.Count; i++)
                {
                    item = list.Items[i];

                    if (item.Name == "" || item.Path == "")
                        continue;

                    result.Add(new StringProperty(item.Name));
                    result.Add(new StringProperty(item.Path));
                }
            }

            return result;
        }

        public StructProperty GetModifiedContent(DeclarationBlock declare)
        {
            var contentStruct = new StructProperty();
            var animContainer = new ArrayProperty() { PropertyType = "StructProperty" };

            StructProperty animSeqListStructProperty;
            ArrayProperty animSeqListItemsArrayProperty;
            ObjectProperty animSeqListItem;
            AnimationList animSeqListVm;

            // iterate through list of AnimSeqList`s
            for (var i = 0; i < AnimSeqLists.Count; i++)
            {
                animSeqListVm = AnimSeqLists[i];

                animSeqListItemsArrayProperty = new ArrayProperty() { PropertyType = "ObjectProperty" };

                // iterate through each AnimSeqList
                for (var j = 0; j < animSeqListVm.Items.Count; j++)
                {
                    // convert Items in AnimationList into ObjectProperty`s

                    animSeqListItem = new ObjectProperty { Name = animSeqListVm.Items[j].Name };

                    if (animSeqListItem.Name == "") animSeqListItem.Id = -1;
                    else
                        animSeqListItem.Id = declare.Items
                            .FirstOrDefault(item => item.Name == animSeqListItem.Name && item.Item6 == animSeqListVm.Items[j].Item6).Id;

                    animSeqListItemsArrayProperty.Items.Add(animSeqListItem);
                }

                animSeqListStructProperty = new StructProperty();
                animSeqListStructProperty.Value[CATEGORY_KEY] = new StringProperty(animSeqListVm.Header);
                animSeqListStructProperty.Value[ARRAY_KEY] = animSeqListItemsArrayProperty;
                animContainer.Items.Add(animSeqListStructProperty);
            }
            
            contentStruct.Value[CONTAINER_KEY] = animContainer;

            return contentStruct;
        }
    }

    public class AnimationList
    {
        public string Header { get; set; } // (Tab Item)
        public IList<AnimationListItem> Items { get; set; } // (Tab Content)

        public AnimationList() { }
        public AnimationList(IList<AnimationListItem> items) { Items = items; }
    }

    public class AnimationListItem
    {
        // name -- dependency -> path
        // path

        public string Name { get; set; }
        public string Path { get; set; }
        public int Item6 { get; set; }

        public AnimationListItem()
        {
            Name = "Name";
            Path = "Path";
            Item6 = 0;
        }

        public AnimationListItem(string name, string path, int item6 = 0)
        {
            Name = name;
            Path = path;
            Item6 = item6;
        }
    }

    public class StringPropertyComparer : IEqualityComparer<StringProperty>
    {
        public bool Equals(StringProperty x, StringProperty y)
        {
            return x.Value == y.Value;
        }

        public int GetHashCode(StringProperty obj)
        {
            return obj.Value.GetHashCode();
        }
    }

    public class DeclarationToStringPropertyComparer : System.Collections.IEqualityComparer
    {
        public new bool Equals(object x, object y)
        {
            if (x is StringProperty && y is DeclarationItem)
                return ((StringProperty)x).Value == ((DeclarationItem)y).Name;
            else if (x is DeclarationItem && y is StringProperty)
                return ((DeclarationItem)x).Name == ((StringProperty)y).Value;
            else
                return EqualityComparer<object>.Default.Equals(x, y);
        }

        public int GetHashCode(object obj)
        {
            return EqualityComparer<object>.Default.GetHashCode(obj);
        }
    }
}