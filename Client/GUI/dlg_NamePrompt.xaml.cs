using Eudora.Net.Core;
using Eudora.Net.Data;
using System.Windows;
using System.Windows.Input;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for dlg_NamePrompt.xaml
    /// </summary>
    public partial class dlg_NamePrompt : Window
    {
        ///////////////////////////////////////////////////////////
        #region Dependency Properties
        /////////////////////////////

        public static readonly DependencyProperty ItemNameProperty =
            DependencyProperty.Register("ItemName", typeof(string), typeof(dlg_NamePrompt),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ItemTypeNameProperty =
            DependencyProperty.Register("ItemTypeName", typeof(string), typeof(dlg_NamePrompt),
                new PropertyMetadata(string.Empty));

        public string ItemName
        {
            get { return (string)GetValue(ItemNameProperty); }
            set { SetValue(ItemNameProperty, value); }
        }

        public string ItemTypeName
        {
            get { return (string)GetValue(ItemTypeNameProperty); }
            set { SetValue(ItemTypeNameProperty, value); }
        }

        /////////////////////////////
        #endregion Dependency Properties
        ///////////////////////////////////////////////////////////

        public dlg_NamePrompt(string itemType)
        {
            InitializeComponent();
            DataContext = this;
            ItemTypeName = itemType;
            tb_Name.Focus();
        }

        private void SetErrorState(bool set)
        {
            tb_Name.BorderBrush = set ? GHelpers.ErrorBrush : SystemColors.HotTrackBrush;
        }

        private bool ValidateName()
        {
            SetErrorState(false);
            if (!DataValidation.IsValidPath(ItemName))
            {
                SetErrorState(true);
                return false;
            }
            return true;
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            if(!ValidateName()) return;
            DialogResult = true;
            Close();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void tb_Name_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                ItemName = tb_Name.Text;
                btn_OK_Click(sender, new());
            }
        }
    }
}
