namespace Simplic.Icon.UI
{
    /// <summary>
    /// Represents the class to convert a true bool value to 1 else 0.5.
    /// </summary>
    public class IsHitTestVisibleToOpacityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a true bool value to 1 else 0.5.
        /// </summary>
        /// <param name="value">The inspected bool value.</param>
        /// <param name="targetType">Type.</param>
        /// <param name="parameter">object.</param>
        /// <param name="culture">CultureInfo.</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isHitTestVisible = (bool)value;
            return isHitTestVisible ? 1.0 : 0.5;
        }

        /// <summary>
        /// Converts the opacity back to true or false.
        /// </summary>
        /// <param name="value">The inspected double value.</param>
        /// <param name="targetType">Type.</param>
        /// <param name="parameter">object.</param>
        /// <param name="culture">CultureInfo.</param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double opacity = (double)value;
            return opacity >= 1.0 ? true : false;
        }
    }
}
