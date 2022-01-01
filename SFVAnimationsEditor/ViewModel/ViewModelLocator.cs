/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:SFVAnimationsEditor.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using SFVAnimationsEditor.Model;
using SFVAnimationsEditor.WpfUtil;

namespace SFVAnimationsEditor.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<IDataService, Design.DesignDataService>();
            }
            else
            {
                SimpleIoc.Default.Register<IDataService, DataService>();
            }

            SimpleIoc.Default.Register<IDialogService, WpfUtil.DialogService>(createInstanceImmediately: true);

            SimpleIoc.Default.Register<AnimationsEditorViewModel>();
            SimpleIoc.Default.Register<VfxEditorViewModel>();
            SimpleIoc.Default.Register<TrailEditorViewModel>();
            SimpleIoc.Default.Register<StringEditorViewModel>();
            SimpleIoc.Default.Register<MainViewModel>(true);
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        /// <summary>
        /// Gets the StringEditor property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public StringEditorViewModel StringEditor => ServiceLocator.Current.GetInstance<StringEditorViewModel>();

        /// <summary>
        /// Gets the AnimationsEditor property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public AnimationsEditorViewModel AnimationsEditor => ServiceLocator.Current.GetInstance<AnimationsEditorViewModel>();
        
        /// <summary>
        /// Gets the VfxEditor property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public VfxEditorViewModel VfxEditor => ServiceLocator.Current.GetInstance<VfxEditorViewModel>();

        /// <summary>
        /// Gets the TrailEditor property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public TrailEditorViewModel TrailEditor => ServiceLocator.Current.GetInstance<TrailEditorViewModel>();


        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
        }
    }
}