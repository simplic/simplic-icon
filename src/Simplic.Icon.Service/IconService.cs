using Dapper;
using iAnywhere.Data.SQLAnywhere;
using Simplic.Base;
using Simplic.Cache;
using Simplic.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Simplic.Configuration;
using System.Security.AccessControl;

namespace Simplic.Icon.Service
{
    public class IconService : IIconService
    {
        #region Private Consts
        private const string TableName = "Icon";
        private const string ConfigName = "IconChecksum";
        private const string ChecksumFileName = "checksum.txt";
        #endregion

        #region Private Fields
        private string IconFolderPath = GlobalSettings.AppDataPath + "\\Icons\\";
        #endregion

        #region Private Objects
        private readonly ISqlService sqlService;
        private readonly IConfigurationService configurationService;
        #endregion

        #region Constructor
        public IconService(ISqlService sqlService, IConfigurationService configurationService)
        {
            if (Directory.Exists(IconFolderPath) == false)
                Directory.CreateDirectory(IconFolderPath);

            this.configurationService = configurationService;
            this.sqlService = sqlService;
        }
        #endregion

        #region Private Methods

        #region [SaveChecksumToFile]
        /// <summary>
        /// Saves a checksum to the checksum file
        /// </summary>
        /// <param name="totalChecksum">Checksum hash to be saved</param>
        private void SaveChecksumToFile(string checksum)
        {
            try
            {
                File.WriteAllText(IconFolderPath + ChecksumFileName, checksum);
            }
            catch (Exception ex)
            {
                // debug ex
                throw;
            }
        }
        #endregion

        #region [ReadChecksumFromFile]
        /// <summary>
        /// Reads the checksum hash value from the file in the localappdata
        /// </summary>
        /// <returns>Checksum hash value</returns>
        private string ReadChecksumFromFile()
        {
            if (File.Exists(IconFolderPath + ChecksumFileName) == false)
                return null;

            return File.ReadAllText(IconFolderPath + ChecksumFileName);
        }
        #endregion

        #region [ReadChecksumFromDb]
        /// <summary>
        /// Gets the checksum hash value from the configuration table
        /// </summary>
        /// <returns>Checksum hash value</returns>
        private string ReadChecksumFromDb()
        {
            return configurationService.GetValue<string>(ConfigName, ConfigName, "");
        }
        #endregion

        #region [CheckCacheById]
        /// <summary>
        /// Returns an icons byte[] if it is saved in the cache
        /// </summary>
        /// <param name="id">Icon Id to be found</param>
        /// <returns>Icon's byte[]</returns>
        private byte[] CheckCacheById(Guid id)
        {
            var cache = CacheManager.Singleton.GetObjectNoException<IconCache>(IconCache.CacheKey);
            if (cache != null)
            {
                var icon = cache.Icons.Where(x => x.Guid == id).FirstOrDefault();
                if (icon != null)
                    return icon.IconBlob;
            }

            return null;
        }
        #endregion

        #region [DeleteFromCache]
        /// <summary>
        /// Deletes an icon from cache
        /// </summary>
        /// <param name="icon">Icon to be delete</param>
        /// <returns>True if successfull</returns>
        private bool DeleteFromCache(Icon icon)
        {
            var cache = CacheManager.Singleton.GetObjectNoException<IconCache>(IconCache.CacheKey);
            if (cache != null)
                return cache.Icons.Remove(icon);

            return false;
        }
        #endregion

        #region [CheckCacheByName]
        /// <summary>
        /// Returns an icons byte[] if it is saved in the cache
        /// </summary>
        /// <param name="name">Icon name to be found</param>
        /// <returns>Icon's byte[]</returns>
        private byte[] CheckCacheByName(string name)
        {
            var cache = CacheManager.Singleton.GetObjectNoException<IconCache>(IconCache.CacheKey);
            if (cache != null)
            {
                if (string.IsNullOrEmpty(name) == false)
                {
                    var icon = cache.Icons.Where(x => x.Name == name).FirstOrDefault();
                    if (icon != null)
                        return icon.IconBlob;
                }
            }

            return null;
        }
        #endregion

        #region [AddToCache]
        /// <summary>
        /// Adds an icon to the cache
        /// </summary>
        /// <param name="icon">Icon to be cached</param>
        private void AddToCache(Icon icon)
        {
            var cache = CacheManager.Singleton.GetObjectNoException<IconCache>(IconCache.CacheKey);
            IList<Icon> icons;

            if (cache == null)
            {
                cache = new IconCache();
                CacheManager.Singleton.AddObject(cache);

                icons = cache.Icons;
            }
            else
                icons = cache.Icons;

            icons.Add(icon);

        }
        #endregion

        #endregion Private Methods

        #region Public Methods

        #region [GetFromDbById]
        /// <summary>
        /// Gets an icon object of a given Guid
        /// </summary>
        /// <param name="id">Icon Id</param>
        /// <returns><see cref="Icon"/></returns>
        public Icon GetByIdAsIcon(Guid id)
        {
            return sqlService.OpenConnection((connection) =>
            {
                return connection.Query<Icon>($"SELECT *, Icon as IconBlob FROM {TableName} WHERE Guid = :id", new { id })
                    .FirstOrDefault();
            });
        }
        #endregion

        #region [GetByIdAsImage]
        /// <summary>
        /// Gets a image of an icon of a given Guid
        /// </summary>
        /// <param name="id">Icon Id</param>
        /// <returns>Icon as <see cref="System.Windows.Media.Imaging.BitmapImage"/></returns>
        public System.Windows.Media.Imaging.BitmapImage GetByIdAsImage(Guid id)
        {
            var bytes = GetById(id);

            if (bytes != null && bytes.Length > 0)
            {
                var img = new System.Windows.Media.Imaging.BitmapImage();
                using (var ms = new MemoryStream(bytes))
                {
                    ms.Position = 0;
                    img.BeginInit();
                    img.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat;
                    img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    img.UriSource = null;
                    img.StreamSource = ms;
                    img.EndInit();
                }

                img.Freeze();

                return img;
            }

            return null;
        }
        #endregion

        #region [GetById]
        /// <summary>
        /// Gets a byte array of an icon of a given Guid
        /// </summary>
        /// <param name="id">Icon Id</param>
        /// <returns>Icon as Byte[]</returns>
        public byte[] GetById(Guid id)
        {
            var cache = CheckCacheById(id);
            if (cache != null) return cache;

            var iconFilePath = Directory.GetFiles(IconFolderPath, $"{id}#*.png")
                .FirstOrDefault();
            if (string.IsNullOrEmpty(iconFilePath) == false && File.Exists(iconFilePath))
            {
                // TODO: cache accepts only icon objects, we have an array. find out a way to cache this byte array
                return File.ReadAllBytes(iconFilePath);
            }

            var data = GetByIdAsIcon(id);
            if (data != null)
            {
                AddToCache(data);
                return data.IconBlob;
            }
            else
                return null;

        }
        #endregion        

        #region [GetByName]
        /// <summary>
        /// Gets a byte array of an icon of a given name
        /// </summary>
        /// <param name="name">Icon name</param>
        /// <returns>Icon as Byte[]</returns>
        public byte[] GetByName(string name)
        {
            var cache = CheckCacheByName(name);
            if (cache != null) return cache;

            var iconFilePath = Directory.GetFiles(IconFolderPath, $"*#{name}.png")
                .FirstOrDefault();

            // cache icon if it can be loaded from file system
            if (string.IsNullOrEmpty(iconFilePath) == false && File.Exists(iconFilePath))
            {
                var iconPathArr = Path.GetFileNameWithoutExtension(iconFilePath).Split('#');
                var iconBytes = File.ReadAllBytes(iconFilePath);

                if (iconPathArr != null && iconPathArr.Length > 1)
                {
                    var iconId = Guid.Parse(iconPathArr[0]);
                    var iconName = iconPathArr[1];

                    AddToCache(new Icon { Guid = iconId, Name = iconName, IconBlob = iconBytes });
                }

                return iconBytes;
            }

            return sqlService.OpenConnection((connection) =>
            {
                var data = connection.Query<Icon>($"SELECT *, Icon as IconBlob FROM {TableName} WHERE Name = :name", new { name })
                    .FirstOrDefault();

                if (data != null)
                {
                    AddToCache(data);
                    return data.IconBlob;
                }
                else
                    return null;
            });
        }
        #endregion

        #region [GetByNameAsImage]
        /// <summary>
        /// Returns a <see cref="System.Windows.Media.Imaging.BitmapImage"/> by a given name of an icon
        /// </summary>
        /// <param name="name">Icon name to be searched</param>
        /// <returns><see cref="System.Windows.Media.Imaging.BitmapImage"/> by a given name of an icon</returns>
        public System.Windows.Media.Imaging.BitmapImage GetByNameAsImage(string name)
        {
            var bytes = GetByName(name);

            if (bytes != null && bytes.Length > 0)
            {
                var img = new System.Windows.Media.Imaging.BitmapImage();
                using (var ms = new System.IO.MemoryStream(bytes))
                {
                    ms.Position = 0;
                    img.BeginInit();
                    img.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat;
                    img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    img.UriSource = null;
                    img.StreamSource = ms;
                    img.EndInit();
                }

                img.Freeze();

                return img;
            }

            return null;
        }
        #endregion

        #region [GetSystemIcon]
        /// <summary>
        /// Get system icon by file extension
        /// </summary>
        /// <param name="name">System icon name or file extension</param>
        /// <param name="size">Icon size</param>
        /// <param name="linkOverlay">Using an icon overlay</param>
        /// <returns>Icon system (system drawing)</returns>
        public System.Drawing.Icon GetSystemIcon(string name, SystemIconSize size = SystemIconSize.Large, bool linkOverlay = false)
        {
            Shell32.SHFILEINFO shfi = new Shell32.SHFILEINFO();
            uint flags = Shell32.SHGFI_ICON | Shell32.SHGFI_USEFILEATTRIBUTES;

            if (true == linkOverlay) flags += Shell32.SHGFI_LINKOVERLAY;

            // Set icon size
            if (SystemIconSize.Small == size)
            {
                flags += Shell32.SHGFI_SMALLICON;
            }
            else
            {
                flags += Shell32.SHGFI_LARGEICON;
            }

            Shell32.SHGetFileInfo(name,
                Shell32.FILE_ATTRIBUTE_NORMAL,
                ref shfi,
                (uint)System.Runtime.InteropServices.Marshal.SizeOf(shfi),
                flags);

            // Copy (clone) the returned icon to a new object, thus allowing us to clean-up properly
            System.Drawing.Icon icon = (System.Drawing.Icon)System.Drawing.Icon.FromHandle(shfi.hIcon).Clone();
            User32.DestroyIcon(shfi.hIcon);		// Cleanup
            return icon;
        }
        #endregion

        #region [GetSystemIconAsImage]
        /// <summary>
        /// Get system icon by file extension or system name as image
        /// </summary>
        /// <param name="name">System icon name or file extension</param>
        /// <param name="size">Icon size</param>
        /// <param name="linkOverlay">Using an icon overlay</param>
        /// <returns>Icon as image (system drawing)</returns>
        public Image GetSystemIconAsImage(string name, SystemIconSize size = SystemIconSize.Large, bool linkOverlay = false)
        {
            var icon = GetSystemIcon(name, size, linkOverlay);

            var imageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            var iconImage = new Image();

            if (size == SystemIconSize.Large)
            {
                iconImage.Width = 32;
                iconImage.Height = 32;
            }
            else if (size == SystemIconSize.Small)
            {
                iconImage.Width = 16;
                iconImage.Height = 16;
            }

            iconImage.Source = imageSource;

            return iconImage;
        }
        #endregion

        #region [Search]
        /// <summary>
        /// Gets a list of icons filtered out by their name
        /// </summary>
        /// <param name="searchText"></param>
        /// <returns></returns>
        public IEnumerable<Icon> Search(string searchText)
        {
            return sqlService.OpenConnection((connection) =>
            {
                return connection.Query<Icon>($"SELECT *, Icon as IconBlob FROM {TableName} WHERE Name like :name", new { name = searchText + "%" });
            });
        }
        #endregion

        #region [GetAll]
        /// <summary>
        /// Gets all icons in the database
        /// </summary>
        /// <returns>A list of <see cref="Icon"/></returns>
        public IEnumerable<Icon> GetAll()
        {
            return sqlService.OpenConnection((connection) =>
            {
                // Important: Keep order by in this statement, due to checksum generation
                return connection.Query<Icon>($"SELECT *, Icon as IconBlob FROM {TableName} ORDER BY Guid");
            });
        }
        #endregion

        #region [Save]
        /// <summary>
        /// Inserts or updates an icon
        /// </summary>
        /// <param name="icon">Icon object to save</param>
        /// <returns>true if successful</returns>
        public bool Save(Icon icon)
        {
            return sqlService.OpenConnection((connection) =>
            {
                //TODO: CreateAt -> CreateDateTimeeTime
                var affectedRows = connection.Execute($"INSERT INTO {TableName} (Guid, Name, Icon, CreateDateTime, UpdateDateTime) "
                     + " ON EXISTING UPDATE VALUES (:Guid, :Name, :IconBlob, :CreateDateTime, :UpdateDateTime)", icon);

                return affectedRows > 0;
            });
        }
        #endregion

        #region [Delete]
        /// <summary>
        /// Deletes an icon from db, cache and file system
        /// </summary>
        /// <param name="id">Icon Id</param>
        /// <returns>true if successful</returns>
        public bool Delete(Guid id)
        {
            return Delete(GetByIdAsIcon(id));
        }
        #endregion

        #region [Delete]
        /// <summary>
        /// Deletes an icon from db, cache and file system
        /// </summary>
        /// <param name="id">Icon Id</param>
        /// <returns>true if successful</returns>
        public bool Delete(Icon icon)
        {
            DeleteFromCache(icon);

            var iconFilePath = $"{IconFolderPath}{icon.Guid}#{icon.Name}.png";
            if (string.IsNullOrEmpty(iconFilePath) == false && File.Exists(iconFilePath))
                File.Delete(iconFilePath);
            return sqlService.OpenConnection((connection) =>
            {
                var affectedRows = connection.Execute($"DELETE {TableName} WHERE Guid = :Guid", icon.Guid);

                return affectedRows > 0;
            });
        }
        #endregion

        #region [GenerateChecksum]        
        /// <summary>
        /// Generates a checksum looping through all bytes it has given asa parameter        
        /// </summary>
        /// <param name="icons">List of bytes</param>
        /// <returns>Checksum hash</returns>
        public string GenerateChecksum(IList<byte[]> icons)
        {
            var checksumStringBuilder = new StringBuilder();
            foreach (var iconBlob in icons)
            {
                var checksum = Simplic.Security.Cryptography.CryptographyHelper.HashSHA256(iconBlob);
                checksumStringBuilder.Append(checksum);
            }

            return Simplic.Security.Cryptography.CryptographyHelper.HashSHA256(checksumStringBuilder.ToString());
        }
        #endregion

        #region [SaveChecksumToDb]
        /// <summary>
        /// Saves the checksum value to the database (in the configuration table)
        /// </summary>
        /// <param name="checksum">Checksum hash to be saved</param>
        public void SaveChecksumToDb(string checksum)
        {
            configurationService.SetValue(ConfigName, ConfigName, "", checksum);
        }
        #endregion

        #region [SyncFilesFromDatabase]
        /// <summary>
        /// Compares checksums between db and filesystem. If they differ, 
        /// reads all the icons from the database and writes them down 
        /// and creates a new checksum
        /// </summary>
        /// <returns>true if successful</returns>
        public bool SyncFilesFromDatabase()
        {
            var fileChecksum = ReadChecksumFromFile();
            var dbChecksum = ReadChecksumFromDb();

            if (fileChecksum == null || fileChecksum != dbChecksum && dbChecksum != null)
            {
                var icons = GetAll();

                // The list of icons is already sorted by GetAll
                foreach (var icon in icons)
                {
                    var fileName = $"{IconFolderPath}{icon.Guid}#{icon.Name}.png";
                    File.WriteAllBytes(fileName, icon.IconBlob);
                }

                SaveChecksumToFile(dbChecksum);

                return true;
            }

            return false;
        }
        #endregion

        #endregion
    }
}
