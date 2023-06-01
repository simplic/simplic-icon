using Simplic.Framework.UI;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Telerik.Windows.Controls;
using System.Runtime.Remoting.Channels;

namespace Simplic.Icon.UI
{
    /// <summary>
    /// Interaction logic for IconEditor.xaml
    /// </summary>
    public partial class IconEditor : RibbonBasedWindow
    {
        private IIconService iconService;

        /// <summary>
        /// The constructor of the class IconEditor.
        /// </summary>
        /// <param name="searchText">The given searched string.</param>
        public IconEditor(string searchText)
        {
            iconService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IIconService>();

            this.WindowMode = WindowMode.View;
            this.AllowSave = true;
            this.AllowDrop = false;
            this.AllowPaging = false;

            InitializeComponent();
            this.DataContext = new IconEditorViewModel(searchText);

            AddRibbonUserControls();
        }

        /// <summary>
        /// Saves all icons.
        /// </summary>
        /// <param name="e">WindowSaveEventArg.</param>
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
                (this.DataContext as IconEditorViewModel).OnDeleteIconCommand(SelectedIcon.Id);
                e.IsDeleted = true;
            }

            base.OnDelete(e);
        }

        /// <summary>
        /// Adds RadRibbonButtons into the RadRibbonGroup.
        /// </summary>
        private void AddRibbonUserControls()
        {
            // Creats a new tab.
            var group = new RadRibbonGroup
            {
                Header = "Icon"
            };

            // Creats the Add button.
            var addButton = new RadRibbonButton
            {
                CollapseToSmall = CollapseThreshold.WhenGroupIsMedium,
                LargeImage = iconService.GetByNameAsImage("action_add_16xLG"),
                Size = Telerik.Windows.Controls.RibbonView.ButtonSize.Large,
                Text = "Hochladen",
                Name = "addNewIcon"
            };

            addButton.SetBinding(RadRibbonButton.CommandProperty, new Binding("AddNewIconCommand"));
            addButton.Click += (sender, e) =>
            {
                scrollListViewer.ScrollIntoView(scrollListViewer.ItemContainerGenerator.Items[scrollListViewer.ItemContainerGenerator.Items.Count - 1]);
            };
            group.Items.Add(addButton);

            // Creates the delete button.
            var deleteButton = new RadRibbonButton
            {
                CollapseToSmall = CollapseThreshold.WhenGroupIsMedium,
                LargeImage = iconService.GetByNameAsImage("delete_32x"),
                Size = Telerik.Windows.Controls.RibbonView.ButtonSize.Large,
                Text = "Löschen",
                IsHitTestVisible = false
            };

            deleteButton.SetBinding(RadRibbonButton.CommandProperty, new Binding("DeleteIconCommand"));

            deleteButton.SetBinding(RadRibbonButton.IsHitTestVisibleProperty, new Binding("VisibleIfIconSelected")
            {
                Converter = new VisibilityToBooleanConverter()
            });

            // Creates a trigger to grey out the button when no icon is selected.
            Trigger opacityTrigger = new Trigger()
            {
                Property = RadRibbonButton.IsHitTestVisibleProperty,
                Value = false
            };
            opacityTrigger.Setters.Add(new Setter(RadRibbonButton.OpacityProperty, 0.5));

            Style deleteButtonStyle = new Style(typeof(RadRibbonButton));
            deleteButtonStyle.Triggers.Add(opacityTrigger);

            deleteButton.Style = deleteButtonStyle;

            group.Items.Add(deleteButton);

            this.RadRibbonHomeTab.Items.Add(group);
        }

        /// <summary>
        /// Gets the selected icon.
        /// </summary>
        public IconViewModel SelectedIcon => (this.DataContext as IconEditorViewModel).SelectedIcon;

        /// <summary>
        /// Selects an icon.
        /// </summary>
        /// <param name="sender">object.</param>
        /// <param name="e">MouseButtonEventArgs.</param>
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
