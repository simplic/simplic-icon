using Simplic.Framework.UI;
using System;
using System.Windows;
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
                Header = "Icon Einstellung"
            };

            var btn = new RadRibbonButton
            {
                CollapseToSmall = CollapseThreshold.WhenGroupIsMedium,
                LargeImage = iconService.GetByNameAsImage("action_add_16xLG"),
                Size = Telerik.Windows.Controls.RibbonView.ButtonSize.Large,
                Text = "Hochladen",
                Name = "addNewIcon"
            };

            btn.SetBinding(RadRibbonButton.CommandProperty, new Binding("AddNewIconCommand"));
            btn.Click += (sender, e) =>
            {
                scrollListViewer.ScrollIntoView(scrollListViewer.ItemContainerGenerator.Items[scrollListViewer.ItemContainerGenerator.Items.Count - 1]);
            };
            group.Items.Add(btn);

            btn = new RadRibbonButton
            {
                CollapseToSmall = CollapseThreshold.WhenGroupIsMedium,
                LargeImage = iconService.GetByNameAsImage("delete_32x"),
                Size = Telerik.Windows.Controls.RibbonView.ButtonSize.Large,
                Text = "Löschen",
                IsHitTestVisible = false
            };

            btn.SetBinding(RadRibbonButton.IsHitTestVisibleProperty, new Binding("IsSelectedIcon")
            {
                Converter = new VisibilityToBooleanConverter()
            });

            btn.SetBinding(RadRibbonButton.CommandProperty, new Binding("Delete2IconCommand"));
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

        /// <summary>
        /// Gets if an icon is selected.
        /// </summary>
        public Visibility VisibleIconSelected
        {
            get
            {
                return (this.DataContext as IconEditorViewModel).IsSelectedIcon;
            }
        }

        private void ListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SelectedIcon != null)
            {
                if (this.DialogResult != null)
                {
                    this.WindowMode = WindowMode.View;
                    this.DialogResult = true;
                    this.Close();
                }
            }
        }
    }
}
