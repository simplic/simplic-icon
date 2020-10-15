using Simplic.UI.MVC;
using System;
using System.Windows.Media.Imaging;

namespace Simplic.Icon.UI
{
    /// <summary>
    /// Interaction logic for Icon
    /// </summary>
    public class IconViewModel : ExtendableViewModelBase
    {
        #region Private Fields
        private Icon model;        
        #endregion

        #region Constructor
        public IconViewModel(Icon icon)
        {
            model = icon;
        } 
        #endregion

        #region Public Properties
        public Guid Id
        {
            get { return model.Guid; }
            set { PropertySetter(value, (newValue) => { model.Guid = newValue; }); this.IsDirty = true; }
        }

        public byte[] IconBlob
        {
            get { return model.IconBlob; }
            set { PropertySetter(value, (newValue) => { model.IconBlob = newValue; }); this.IsDirty = true; }
        }

        public string Name
        {
            get { return model.Name; }
            set { PropertySetter(value, (newValue) => { model.Name = newValue; }); this.IsDirty = true; }
        }

        public DateTime CreateDateTime
        {
            get { return model.CreateDateTime; }
            set { PropertySetter(value, (newValue) => { model.CreateDateTime = newValue; }); this.IsDirty = true; }
        }

        public DateTime? UpdateDateTime
        {
            get { return model.UpdateDateTime; }
            set { PropertySetter(value, (newValue) => { model.UpdateDateTime = newValue; }); this.IsDirty = true; }
        }

        public BitmapImage IconBlobAsImage
        {
            get { return model.IconBlobAsImage; }
        }

        public double Width
        {
            get
            {
                return IconBlobAsImage.Width;
            }
        }

        public double Height
        {
            get
            {
                return IconBlobAsImage.Height;
            }
        }

        public string IconSize
        {
            get { return $"{Convert.ToInt32(Width)}x{Convert.ToInt32(Height)}"; }
        } 
        #endregion
    }
}
