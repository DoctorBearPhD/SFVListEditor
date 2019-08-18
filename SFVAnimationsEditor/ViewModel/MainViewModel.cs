using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using SFVAnimationsEditor.Model;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace SFVAnimationsEditor.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;

        public string FilePath = "";

        private string _welcomeTitle = string.Empty;

        /// <summary>
        /// Gets the WelcomeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WelcomeTitle
        {
            get => _welcomeTitle;
            set => Set(ref _welcomeTitle, value);
        }

        private UassetFile _UFile;
        public UassetFile UFile {
            get => _UFile;
            set => Set(ref _UFile, value);
        }

        private ObservableCollection<StringProperty> _UFileStringList;
        public ObservableCollection<StringProperty> UFileStringList {
            get => _UFileStringList;
            set => Set(ref _UFileStringList, value);
        }

        private ObservableCollection<StringProperty> _FakeStringList;
        public ObservableCollection<StringProperty> FakeStringList
        {
            get => _FakeStringList;
            set => Set(ref _FakeStringList, value);
        }


        public RelayCommand SaveAsCopyCommand => new RelayCommand(SaveAsCopy);



        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService = null)
        {
            _dataService = dataService;
            //_dataService.GetData(
            //    (item, error) =>
            //    {
            //        if (error != null)
            //        {
            //            // Report error here
            //            return;
            //        }
            //    });

            //_FakeStringList = new ObservableCollection<StringProperty>
            //{
            //    new StringProperty("generate"),
            //    new StringProperty("dimentional"),
            //    new StringProperty("displaying"),
            //    new StringProperty("automatically"),
            //    new StringProperty("attributes")
            //};
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
        
        public void ReadFile()
        {
            if (FilePath == "") return;

            var br = new BinaryReader(File.OpenRead(FilePath));

            _UFile = new UassetFile();

            try
            {
                _UFile.ReadUasset(ref br);
                UFile = _UFile;
                UFileStringList = UFile.StringList;
            }
            catch
            {

            }
        }

        private void SaveAsCopy()
        {
            UFile.StringList = UFileStringList;

            try
            {
                var br = new BinaryReader(File.OpenRead(FilePath));
                var bw = new BinaryWriter(File.Create(FilePath.Replace(".uasset", "-Modified.uasset")));
                UFile.WriteUasset(ref br, ref bw);
            }
            catch (Exception)
            {

                throw;
            }
            
        }
    }
}