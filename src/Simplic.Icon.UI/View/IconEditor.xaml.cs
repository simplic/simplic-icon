using Simplic.Framework.UI;
using System;
using System.Windows.Data;
using Telerik.Windows.Controls;

namespace Simplic.Icon.UI
{
    /// <summary>
    /// Interaction logic for IconEditor.xaml
    /// </summary>
    public partial class IconEditor : RibbonBasedWindow
    {
        private IIconService iconService;

        public IconEditor(string searchText)
        {
            iconService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IIconService>();

            this.WindowMode = WindowMode.Edit;
            this.AllowSave = true;
            this.AllowDrop = false;
            this.AllowPaging = false;

            InitializeComponent();
            this.DataContext = new IconEditorViewModel(searchText);

            AddRibbonUserControls();
        }

        public override void OnSave(WindowSaveEventArg e)
        {
            var result = (this.DataContext as IconEditorViewModel).Save();

            if (result)
            {
                e.IsSaved = true;
            }
            else
                return;
        }

        /// <summary>
        /// Deletes selected icon.
        /// </summary>
        public override void OnDelete(WindowDeleteEventArg e)
        {
            if (SelectedIcon != null && SelectedIcon.Id != Guid.Empty)
            {
                (this.DataContext as IconEditorViewModel).DeleteIcon(SelectedIcon.Id);
                e.IsDeleted = true;
            }

            base.OnDelete(e);
        }

        private void AddRibbonUserControls()
        {
            var group = new RadRibbonGroup
            {
                Header = "New Icon"
            };

            var btn = new RadRibbonButton
            {
                CollapseToSmall = CollapseThreshold.WhenGroupIsMedium,
                LargeImage = iconService.GetByNameAsImage("action_add_16xLG"),
                Size = Telerik.Windows.Controls.RibbonView.ButtonSize.Large,
                Text = "Add new Icon"
            };

            btn.SetBinding(RadRibbonButton.CommandProperty, new Binding("AddNewIconCommand"));
            group.Items.Add(btn);

            this.RadRibbonHomeTab.Items.Add(group);
        }

        public IconViewModel SelectedIcon
        {
            get
            {
                return (this.DataContext as IconEditorViewModel).SelectedIcon;
            }
        }

        private void ListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SelectedIcon != null)
            {
                this.WindowMode = WindowMode.View;
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}
