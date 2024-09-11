namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_OptionsView.xaml
    /// </summary>
    public partial class uc_OptionsView : ChildWindowBase
    {
        public uc_OptionsView()
        {
            InitializeComponent();
        }

        public void ActivateTab(int index)
        {
            SettingsTabs.SelectedIndex = index;
        }
    }
}
