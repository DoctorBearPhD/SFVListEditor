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

        /// <summary>
        /// Initializes a new instance of the AnimationsEditorViewModel class.
        /// </summary>
        public AnimationsEditorViewModel()
        {
            AnimSeqLists = new ObservableCollection<AnimationList>();
        }

        public void GetAnimationList(Dictionary<string, object> content, DeclarationBlock declare)
        {
            if (!content.ContainsKey(CONTAINER_KEY))
            {
                Console.WriteLine("WARNING!!! - No readable content found! (Is this an AnimSeqWithIdList UAsset file?)");
                return;
            }

            AnimSeqLists = new ObservableCollection<AnimationList>();
            
            AnimationList animSeqListVm;
            ObjectProperty item;

            int animNameId;
            int animPathId;

            string animName;
            string animPath;

            var animContainer = (ArrayProperty)content[CONTAINER_KEY];
            
            // iterate through AnimSeqListWithIdContainer
            for (var i = 0; i < animContainer.Count; i++)
            {
                animSeqListVm = new AnimationList
                {
                    Header = animContainer.Items[i][CATEGORY_KEY],
                    Items = new List<AnimationListItem>()
                };

                var animSeqListItemsArrayProperty = (ArrayProperty)animContainer.Items[i][ARRAY_KEY];

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

                    #region Get Animation Name and Path

                    #region Get Animation Name
                    
                    animName = item.Name;
                    // get Id of Animation Name
                    animNameId = item.Id;

                    #endregion

                    #region Get Animation Path

                    animPathId = declare.Items
                        .Where(declareItem => declareItem.Id == animNameId) // use Id to find item in the Declare Block
                        .ToList()[0].Depends;                               // use Declare Block item to get Depends Id

                    animPathId = -animPathId - 1; // convert to positive id

                    animPath = declare.Items                                // use Depends Id to find the Animation Path's Declare Block item
                        .Where(declareItem => declareItem.Id == animPathId)
                        .ToList()[0].Name;                                  // get Path from Declare Block item

                    #endregion

                    #endregion

                    // add Items to AnimationList
                    animSeqListVm.Items.Add(new AnimationListItem(animName, animPath));
                }

                // add AnimationList to Container
                AnimSeqLists.Add(animSeqListVm);
            }
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

        public AnimationListItem()
        {
            Name = "Name";
            Path = "Path";
        }

        public AnimationListItem(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }
}