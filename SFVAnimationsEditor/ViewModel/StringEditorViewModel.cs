using GalaSoft.MvvmLight;
using SFVAnimationsEditor.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SFVAnimationsEditor.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class StringEditorViewModel : ViewModelBase
    {
        private IList<StringProperty> _UFileStringList;
        public IList<StringProperty> UFileStringList
        {
            get => _UFileStringList;
            set => Set(ref _UFileStringList, value);
        }


        //[GalaSoft.MvvmLight.Ioc.PreferredConstructor]
        /// <summary>
        /// Initializes a new instance of the StringEditorViewModel class.
        /// </summary>
        public StringEditorViewModel()
        {
            //System.Diagnostics.Debug.WriteLine($"WARNING!!! Parameterless constructor called for {this.GetType()}!");
            UFileStringList = new ObservableCollection<StringProperty>();
        }

        //public StringEditorViewModel(IList<StringProperty> strings)
        //{
        //    UFileStringList = strings;
        //}
    }
}