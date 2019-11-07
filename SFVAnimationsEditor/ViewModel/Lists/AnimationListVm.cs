using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFVAnimationsEditor.ViewModel.Lists
{
    public class AnimationListVm : ListVm<Model.Lists.ListItem>
    {
        public string Header { get; set; } // (Tab Item)
        // (`Items` = Tab Content)
    }
}
