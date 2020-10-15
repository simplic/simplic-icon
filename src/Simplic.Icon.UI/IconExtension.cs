using CommonServiceLocator;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace Simplic.Icon.UI
{
    /// <summary>
    /// A markup extension to show simplic icons 
    /// </summary>
    public class IconExtension : System.Windows.Markup.MarkupExtension
    {
        private IIconService iconService;

        public IconExtension()
        {
            iconService = null;
            try
            {
                iconService = ServiceLocator.Current.GetInstance<IIconService>();
            }
            catch { /* design time */ }
        }

        /// <summary>
        /// Icon name to be searched in the db
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Returns a <see cref="BitmapImage"/> of a given icons name
        /// </summary>        
        /// <returns><see cref="BitmapImage"/></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                return null;

            if (string.IsNullOrWhiteSpace(Name))
                return null;

            var iconBytes = iconService.GetByName(Name);
            if (iconBytes == null || iconBytes.Length <= 0) return null;

            var img = new BitmapImage();
            using (var ms = new MemoryStream(iconBytes))
            {
                ms.Position = 0;
                img.BeginInit();
                img.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.UriSource = null;
                img.StreamSource = ms;
                img.EndInit();                
            }

            img.Freeze();
            return img;
        }
    }
}
