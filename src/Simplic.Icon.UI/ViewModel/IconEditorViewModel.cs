using CommonServiceLocator;
using Microsoft.Win32;
using Simplic.UI.MVC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Simplic.Icon.UI
{
    /// <summary>
    /// Interaction logic for Icon Editor
    /// </summary>
    public class IconEditorViewModel : ExtendableViewModelBase
    {
        #region Private Fields
        private string searchIconName;
        private IconViewModel selectedIcon;
        private ObservableCollection<IconViewModel> icons;
        private IList<Guid> iconsToDelete;
        private CollectionViewSource filteredIcons;
        private ICommand exportIconCommand;
        private IIconService iconService;
        #endregion

        #region Constructor
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
        }
        #endregion

        #region Private Methods

        #region [LoadAllIcons]
        /// <summary>
        /// Gets all icons from db
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
        #endregion

        #region [FilteredIcons_Filter]
        /// <summary>
        /// Filter event of the CollectionViewSource to filter out icons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilteredIcons_Filter(object sender, FilterEventArgs e)
        {
            var icon = e.Item as IconViewModel;

            if (icon != null && icon.Name != null && SearchIconName != null)
            {
                e.Accepted = icon.Name.ToLower().Contains(SearchIconName.ToLower());
            }
        }
        #endregion

        #endregion

        #region Public Methods

        #region [Save]
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
        #endregion

        #region [Delete Icon]
        /// <summary>
        /// Deletes Icon by Id.
        /// </summary>
        /// <returns>True if successful</returns>
        public bool DeleteIcon(Guid iconId)
        {
            return iconService.Delete(iconId);
        }
        #endregion

        #region [OnAddNewIconCommand]
        /// <summary>
        /// Adds a new icon but does not save it to the db yet.
        /// </summary>
        /// <param name="param"></param>
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
        #endregion

        #region [OnDeleteIconCommand]
        /// <summary>
        /// Deletes an icon and puts it to the to be deleted icons list
        /// </summary>
        /// <param name="param"></param>
        public void OnDeleteIconCommand(object param)
        {
            iconsToDelete.Add(SelectedIcon.Id);
            Icons.Remove(SelectedIcon);
        } 
        #endregion

        #endregion

        #region Public Properties
        public ICommand AddNewIconCommand { get; private set; }
        public ICommand DeleteIconCommand { get; private set; }

        public CollectionViewSource FilteredIcons
        {
            get { return filteredIcons; }
            set { filteredIcons = value; }
        }

        public ObservableCollection<IconViewModel> Icons
        {
            get { return icons; }
            set { PropertySetter(value, (newValue) => { icons = newValue; }); }
        }

        public IconViewModel SelectedIcon
        {
            get { return selectedIcon; }
            set { PropertySetter(value, (newValue) => { selectedIcon = newValue; }); }
        }

        public string SearchIconName
        {
            get { return searchIconName; }
            set
            {
                PropertySetter(value, (newValue) => { searchIconName = newValue; });
                this.FilteredIcons.View.Refresh();
            }
        }

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
        #endregion
    }
}
