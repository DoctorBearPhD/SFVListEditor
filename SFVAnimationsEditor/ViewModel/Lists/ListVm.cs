using SFVAnimationsEditor.Model.Lists;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFVAnimationsEditor.ViewModel.Lists
{
    public class ListVm
    {
        public ObservableCollection<ListItem> Items { get; set; }


        public ListVm() { }

        public ListVm(ObservableCollection<ListItem> items)
        {
            Items = items;
        }

        public void UpdateIndices()
        {
            for (var i = 0; i < Items.Count; i++)
                Items[i].UpdateIndex(i);
        }
    }
}
