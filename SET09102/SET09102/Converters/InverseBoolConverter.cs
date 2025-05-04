namespace SET09102.Converters
{
    /// <summary>
    /// Converts a boolean value to its inverse.
    /// </summary>
    /// <remarks>
    /// This converter is useful in XAML when you need to bind a visibility property
    /// to the inverse of a boolean property, such as showing a "no selection" message
    /// when no item is selected.
    /// </remarks>
    public class InverseBoolConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to its inverse.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">Optional parameter (not used).</param>
        /// <param name="culture">The culture to use for the conversion.</param>
        /// <returns>The inverse of the input boolean value.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        /// <summary>
        /// Converts a boolean value back to its inverse for two-way binding.
        /// </summary>
        /// <param name="value">The boolean value to convert back.</param>
        /// <param name="targetType">The type of the binding source property.</param>
        /// <param name="parameter">Optional parameter (not used).</param>
        /// <param name="culture">The culture to use for the conversion.</param>
        /// <returns>The inverse of the input boolean value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}