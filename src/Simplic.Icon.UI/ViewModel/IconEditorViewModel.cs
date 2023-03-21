using CommonServiceLocator;
using Microsoft.Win32;
using Simplic.UI.MVC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Simplic.Icon.UI
{
    /// <summary>
    /// Interaction logic for Icon Editor.
    /// </summary>
    public class IconEditorViewModel : ExtendableViewModelBase
    {
        private string searchIconName;
        private IconViewModel selectedIcon;
        private Visibility visibleIfIconSelected;
        private ObservableCollection<IconViewModel> icons;
        private IList<Guid> iconsToDelete;
        private CollectionViewSource filteredIcons;
        private ICommand exportIconCommand;
        private IIconService iconService;

        /// <summary>
        /// The constructor for the class IconEditorViewModell.
        /// </summary>
        /// <param name="searchText"></param>
        public IconEditorViewModel(string searchText)
        {
            AddNewIconCommand = new RelayCommand(OnAddNewIconCommand);
            DeleteIconCommand = new RelayCommand(OnDeleteIconCommand);
            ExportIconCommand = new RelayCommand((e) =>
            {
                var dialog = new Framework.SqlD.DCEF.ExportDialog();
                dialog.Fill(selectedIcon.Id, 35);
                dialog.Show();
            }, (e) => { return selectedIcon != null; });

            iconService = ServiceLocator.Current.GetInstance<IIconService>();
            icons = new ObservableCollection<IconViewModel>();
            iconsToDelete = new List<Guid>();

            LoadAllIcons();

            filteredIcons = new CollectionViewSource();
            filteredIcons.Source = icons;
            filteredIcons.Filter += FilteredIcons_Filter;

            SearchIconName = searchText;
            IsDirty = false;

            visibleIfIconSelected = Visibility.Hidden;
        }

        /// <summary>
        /// Gets all icons from db.
        /// </summary>
        private void LoadAllIcons()
        {
            var iconsFromDb = iconService.GetAll();
            if (iconsFromDb != null)
            {
                Icons.Clear();
                foreach (var icon in iconsFromDb)
                {
                    Icons.Add(new IconViewModel(icon));
                }
            }
        }

        /// <summary>
        /// Filter event of the CollectionViewSource to filter out icons.
        /// </summary>
        /// <param name="sender">object.</param>
        /// <param name="e">FilterEventArgs.</param>
        private void FilteredIcons_Filter(object sender, FilterEventArgs e)
        {
            var icon = e.Item as IconViewModel;

            if (icon != null && icon.Name != null && SearchIconName != null)
            {
                e.Accepted = icon.Name.ToLower().Contains(SearchIconName.ToLower());
            }
        }

        /// <summary>
        /// Saves changes to the db. 
        /// </summary>
        /// <returns>True if successfull</returns>
        public bool Save()
        {
            // delete icons
            foreach (var iconId in iconsToDelete)
            {
                var success = DeleteIcon(iconId);
                if (success == false)
                    return false;
            }

            // save only icons that changed
            var iconsToSave = Icons.Where(x => x.IsDirty);
            foreach (var icon in iconsToSave)
            {
                icon.UpdateDateTime = DateTime.Now;
                var success = iconService.Save(new Icon
                {
                    Guid = icon.Id,
                    Name = icon.Name,
                    IconBlob = icon.IconBlob,
                    CreateDateTime = icon.CreateDateTime,
                    UpdateDateTime = DateTime.Now
                });

                if (success == false)
                    return false;
            }

            var totalChecksumHash = iconService.GenerateChecksum(Icons.Select(x => x.IconBlob).ToList());
            iconService.SaveChecksumToDb(totalChecksumHash);
            IsDirty = false;
            return true;
        }

        /// <summary>
        /// Deletes Icon by Id.
        /// </summary>
        /// <returns>True if successful</returns>
        public bool DeleteIcon(Guid iconId)
        {
            return iconService.Delete(iconId);
        }

        /// <summary>
        /// Adds a new icon but does not save it to the db yet.
        /// </summary>
        /// <param name="param">object.</param>
        public void OnAddNewIconCommand(object param)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Bilddateien (*.png)|*.png;";
            var dialogResult = openFileDialog.ShowDialog();
            if (dialogResult.HasValue)
            {
                var file = openFileDialog.FileName;
                if (File.Exists(file))
                {
                    var iconViewModel = new IconViewModel(
                    new Icon
                    {
                        Guid = Guid.NewGuid(),
                        Name = Path.GetFileNameWithoutExtension(file),
                        IconBlob = File.ReadAllBytes(file)
                    })
                    { IsDirty = true };

                    Icons.Add(iconViewModel);
                    SelectedIcon = iconViewModel;
                }
            }
        }

        /// <summary>
        /// Deletes an icon and puts it to the to be deleted icons list.
        /// </summary>
        /// <param name="param"></param>
        public void OnDeleteIconCommand(object param)
        {
            iconsToDelete.Add(SelectedIcon.Id);
            Icons.Remove(SelectedIcon);
        }

        /// <summary>
        /// Gets or sets the command for add new icon.
        /// </summary>
        public ICommand AddNewIconCommand { get; private set; }

        /// <summary>
        /// Gets or sets the comment for delete icon.
        /// </summary>
        public ICommand DeleteIconCommand { get; private set; }

        /// <summary>
        /// Gets or sets the filtered icons.
        /// </summary>
        public CollectionViewSource FilteredIcons
        {
            get => filteredIcons;
            set { filteredIcons = value; }
        }

        /// <summary>
        /// Gets or sets the observable collection of icons.
        /// </summary>
        public ObservableCollection<IconViewModel> Icons
        {
            get => icons;
            set { PropertySetter(value, (newValue) => { icons = newValue; }); }
        }

        /// <summary>
        /// Gets or sets the selected icon.
        /// </summary>
        public IconViewModel SelectedIcon
        {
            get => selectedIcon;
            set
            {
                PropertySetter(value, (newValue) => { selectedIcon = newValue; });
                RaisePropertyChanged(nameof(VisibleIfIconSelected));
            }
        }

        /// <summary>
        /// Gets or sets the visibility depending on wheater an icon is selected or not.
        /// </summary>
        public Visibility VisibleIfIconSelected
        {
            get
            {
                if (selectedIcon != null)
                {
                    visibleIfIconSelected = Visibility.Visible;
                }
                else
                {
                    visibleIfIconSelected = Visibility.Hidden;
                }
                return visibleIfIconSelected;
            }

            set { PropertySetter(value, (newValue) => { visibleIfIconSelected = newValue; }); }
        }

        /// <summary>
        /// Gets or sets the search icon name.
        /// </summary>
        public string SearchIconName
        {
            get => searchIconName;
            set
            {
                PropertySetter(value, (newValue) => { searchIconName = newValue; });
                this.FilteredIcons.View.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the command for exporting the icon.
        /// </summary>
        public ICommand ExportIconCommand
        {
            get
            {
                return exportIconCommand;
            }

            set
            {
                exportIconCommand = value;
            }
        }
    }
}