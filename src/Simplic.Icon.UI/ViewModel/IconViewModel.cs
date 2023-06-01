using Simplic.UI.MVC;
using System;
using System.Windows.Media.Imaging;

namespace Simplic.Icon.UI
{
    /// <summary>
    /// Interaction logic for Icon.
    /// </summary>
    public class IconViewModel : ExtendableViewModelBase
    {
        private Icon model;

        /// <summary>
        /// Constructor of the class IconViewModel.
        /// </summary>
        /// <param name="icon"></param>
        public IconViewModel(Icon icon)
        {
            model = icon;
        }

        /// <summary>
        /// Gets or sets the id as Guid.
        /// </summary>
        public Guid Id
        {
            get { return model.Guid; }
            set { PropertySetter(value, (newValue) => { model.Guid = newValue; }); this.IsDirty = true; }
        }

        /// <summary>
        /// Gets or sets the icon blob.
        /// </summary>
        public byte[] IconBlob
        {
            get { return model.IconBlob; }
            set { PropertySetter(value, (newValue) => { model.IconBlob = newValue; }); this.IsDirty = true; }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name
        {
            get { return model.Name; }
            set { PropertySetter(value, (newValue) => { model.Name = newValue; }); this.IsDirty = true; }
        }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        public DateTime CreateDateTime
        {
            get { return model.CreateDateTime; }
            set { PropertySetter(value, (newValue) => { model.CreateDateTime = newValue; }); this.IsDirty = true; }
        }

        /// <summary>
        /// Gets or sets the date time of the upgradet date time.
        /// </summary>
        public DateTime? UpdateDateTime
        {
            get => model.UpdateDateTime;
            set { PropertySetter(value, (newValue) => { model.UpdateDateTime = newValue; }); this.IsDirty = true; }
        }

        /// <summary>
        /// Gets icon blob as image.
        /// </summary>
        public BitmapImage IconBlobAsImage => model.IconBlobAsImage;

        /// <summary>
        /// Gets the width of the icon blob as image.
        /// </summary>
        public double Width => IconBlobAsImage.Width;

        /// <summary>
        /// Gets the height of the icon blob as image.
        /// </summary>
        public double Height => IconBlobAsImage.Height;

        /// <summary>
        /// Gets the icon size.
        /// </summary>
        public string IconSize => $"{Convert.ToInt32(Width)}x{Convert.ToInt32(Height)}";
    }
}
