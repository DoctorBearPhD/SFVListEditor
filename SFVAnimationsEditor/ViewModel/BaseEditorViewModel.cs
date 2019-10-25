using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using SFVAnimationsEditor.Model;

namespace SFVAnimationsEditor.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public abstract class BaseEditorViewModel : ViewModelBase
    {
        abstract public string ITEM_NAME_TYPE { get; }
        abstract public string ITEM_PATH_TYPE { get; }

        abstract public string ITEM_NAME_NAMESPACE { get; }
        abstract public string ITEM_PATH_NAMESPACE { get; }


        /// <summary>
        /// Initializes a new instance of the BaseEditorViewModel class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BaseEditorViewModel()
        {
            Initialize();
        }

        abstract public StructProperty GetModifiedContent(DeclarationBlock declare);
        abstract public IList<StringProperty> GetStrings();


        // Note: Original order is actually based on path name; see my notes for more info (different file).
        public DeclarationBlock UpdateDeclareBlock(DeclarationBlock unchangedDeclareBlock)
        {
            var comparer = new DeclarationComparer();

            // Get all unchanging path items
            var pathsDeclareBlock = GetUnchangingPathDeclarationItems(unchangedDeclareBlock);

            // Get all modifiable path items
            pathsDeclareBlock.Items.AddRange(GetModifiablePathDeclarationItems());

            // Sort path items to mimic the original's sort order
            pathsDeclareBlock.Items = pathsDeclareBlock.Items
                .Distinct(comparer)
                .OrderBy(item => item.Name)
                .ThenBy(item => item.Item6)
                .ToList();

            // for each item in path list, find the path in the VM's list
            //   and add the item's name property (as a declaration item) to a new list (in order)
            //     reminder: logically, unchanging name items are added here as well

            var modifiedDeclareBlock = new DeclarationBlock();

            // For each item path, search for a corresponding item name; create a declaration item for each found name
            modifiedDeclareBlock.Items.AddRange(GetNameDeclarationItems(pathsDeclareBlock));

            // add any other types of items to list (e.g. non-animation, non-package items)
            //  note: can't directly add items because the original values would be overwritten
            foreach (var item in unchangedDeclareBlock.Items)
            {
                if (item.Type != ITEM_PATH_TYPE) // same type for all paths
                {
                    modifiedDeclareBlock.Items.Add(new DeclarationItem()
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
            modifiedDeclareBlock.Items.AddRange(pathsDeclareBlock.Items);

            // remove duplicates
            modifiedDeclareBlock.Items = modifiedDeclareBlock.Items
                .Distinct(comparer)
                .OrderBy(item => item.Type)
                .ToList();

            // re-assign IDs based on index
            for (var i = 0; i < modifiedDeclareBlock.Count; i++)
            {
                modifiedDeclareBlock.Items[i].Id = i;
            }

            // assign Depends where needed
            UpdateDepends(ref modifiedDeclareBlock);

            // Update Depends of unchanging items (e.g. non-animation, non-vfx items)
            foreach (var unchangedPathItem in unchangedDeclareBlock.Items.Where(item => item.Type == ITEM_PATH_TYPE))
            {
                // find the new id of the path item
                var newId = modifiedDeclareBlock.Items
                    .Find(pathItem => comparer.Equals(pathItem, unchangedPathItem)).Id;
                // find the item and update its Depends value to the new ID
                // get the name of the item whose Depends value is this path item's old ID. That is the item to update.
                var dependentItemName = unchangedDeclareBlock.Items.Find(item => item.Depends == (-unchangedPathItem.Id - 1)).Name;
                // use the name to find the [item to update] in the modifiedDeclareBlock
                modifiedDeclareBlock.Items.Find(item => item.Name == dependentItemName).Depends = -newId - 1;
            }

            return modifiedDeclareBlock;
        }

        private DeclarationBlock GetUnchangingPathDeclarationItems(DeclarationBlock unchangedDeclareBlock)
        {
            var result = new DeclarationBlock();

            // Note: Can't directly add items because the original values would be overwritten.
            foreach (var item in unchangedDeclareBlock.Items)
            {
                if (item.Type == ITEM_PATH_TYPE) // same type for all paths
                {
                    // fully copy unchanged declaration items to the updated declaration block
                    result.Items.Add(new DeclarationItem()
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

            return result;
        }

        internal abstract IList<DeclarationItem> GetModifiablePathDeclarationItems();
        internal abstract IList<DeclarationItem> GetNameDeclarationItems(DeclarationBlock pathsDeclareBlock);

        internal abstract void UpdateDepends(ref DeclarationBlock modifiedDeclareBlock);

        abstract public void Initialize();
    }
}