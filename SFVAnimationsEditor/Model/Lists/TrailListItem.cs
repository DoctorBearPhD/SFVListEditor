using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFVAnimationsEditor.Model.Lists
{
    public class TrailListItem : ListItem
    {
        public int AssetId  { get; set; } = -1;
        public int ShareNum { get; set; } =  1;


        public TrailListItem() : base() { }
        public TrailListItem(string name, string path, int item6 = 0) : base(name, path, item6) { }
        public TrailListItem(int index, string name, string path, int item6 = 0) : base(index, name, path, item6) { }

        public TrailListItem(string name, string path, int assetId = -1, int shareNum = 1, int item6 = 0) : base(name, path, item6)
        {
            AssetId  =  assetId;
            ShareNum = shareNum;
        }

        public TrailListItem(int index, string name, string path, int assetId = -1, int shareNum = 1, int item6 = 0) : this(name, path, assetId, shareNum, item6)
        {
            UpdateIndex(index);
        }


        public override string ToString()
        {
            return $"{this.GetType().Name}; {Index}: ({AssetId}) {Name}, {Path}";
        }
    }
}
