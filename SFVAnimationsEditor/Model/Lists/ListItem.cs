using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFVAnimationsEditor.Model.Lists
{
    public class ListItem
    {
        /// <summary>
        /// Used to prevent editing entries in the COMMON_OBJECT tab if they already had a value.
        /// </summary>
        public bool IsReadOnly = false;

        public int  Index      { get; private set; } = -1;

        public string Name  { get; set; }
        public string Path  { get; set; }
        public int    Item6 { get; set; }


        public ListItem()
        {
            Name = "Name";
            Path = "Path";
            Item6 = 0;
        }

        public ListItem(string name, string path, int item6 = 0)
        {
            Name = name;
            Path = path;
            Item6 = item6;
        }

        public ListItem(int index, string name, string path, int item6 = 0) : this(name, path, item6)
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
