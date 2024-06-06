using Eudora.Net.Core;
using Eudora.Net.Data;
using System.Collections.ObjectModel;

//using RtfPipe;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;

// Not in namespace Core despite physical location
namespace Eudora.Net.Core
{
    /// <summary>
    /// Yes there is a GridLengthConverter built into System.Windows. 
    /// However, it is not a ValueConverter but a TypeConverter
    /// </summary>
    public class GridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val = (double)value;
            GridLength gridLength = new(val);

            return gridLength;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GridLength val = (GridLength)value;

            return val.Value;
        }
    }

    /// <summary>
    /// Strictly speaking, this one doesn't actually convert. According to the MSDN docs,
    /// this is the way to allow enabled & disabled items within a ComboBox
    /// </summary>
    public class ComboBoxItemDisabler : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is null) return value;

            //if (value == "CollectionItem2" || value == "CollectionItem3")
            //{
            //    return true;
            //}

            return false;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// For binding a radiobuttun group to a property with an enum of options for the value.
    /// If the parameter matches, return bool true. Thus in xaml: 
    /// IsChecked="{Binding SelectedOption, Converter={StaticResource EnumBooleanConverter}, ConverterParameter={x:Static local:TheEnum.FirstValue}}"
    /// ... and so on, one button for each entry in the enum.
    /// </summary>
    public class EnumBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? parameter : Binding.DoNothing;
        }
    }


    /// <summary>
    /// Custom converter for supporting List<string> to single-string, and back.
    /// This is used in the email address textboxes in the mail message window;
    /// internally these are List<string> but to display and edit that in a 
    /// single-lined textbox, ala old Eudora, we must use two-way fancy conversion.
    /// </summary>
    [ValueConversion(typeof(ObservableCollection<EmailAddress>), typeof(string))]
    public class EmailListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //return String.Join(";", ((List<Core.EmailAddress>)value).ToArray());
            string result = string.Empty;
            if(value is not ObservableCollection<EmailAddress> collection)
            {
                return result;
            }
            int count = 0;
            foreach(var address in collection)
            {
                if(count > 1)
                {
                    result += ";";
                }
                result += (address.Address);
                count++;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<EmailAddress> addresses = [];
            string? s = value as string;
            if (s is not null)
            {
                //In: Some Name <address@domain.ext>;//Some otheName <address2@domain.ext>;
                var strings1 = s.Split(";").ToList<string>();
                foreach(var item in strings1)
                {
                    addresses.Add(new() { Address = item });
                }
            }
            return addresses;
        }
    }

    [ValueConversion(typeof(ObservableCollection<string>), typeof(string))]
    public class StringCollectionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return String.Join(";", ((ObservableCollection<string>)value).ToArray());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<string> ocstring = [];
            string? s = value as string;
            if (s != null)
            {
                var list = s.Split(";").ToList<string>();
                foreach(var token in list)
                {
                    ocstring.Add(token);
                }
            }
            return ocstring;
        }
    }


    /// <summary>
    /// Data converter to transmute a WPF/.Net FlowDocument into an HTML document.
    /// </summary>
    [ValueConversion(typeof(FlowDocument), typeof(string))]
    public class FlowDocumentToHTMLConverter : IValueConverter
    {
        /// <summary>
        /// IN: FlowDocument
        /// OUT: HTML document (string)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string output = "";

            try
            {
                var document = value as FlowDocument;
                if (document != null)
                {
                    var content = new TextRange(document.ContentStart, document.ContentEnd);
                    if (content.CanSave(DataFormats.Html))
                    {
                        using (var stream = new MemoryStream())
                        {
                            content.Save(stream, DataFormats.Html);
                            stream.Position = 0;
                            //output = Rtf.ToHtml(stream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            return output;
        }

        /// <summary>
        /// IN: HTML document (string)
        /// OUT: FlowDocument compatible with RichEditControl
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string output = "";

            return output;
        }
    }
}

/// <summary>
/// Data converter to transmute a WPF/.Net FlowDocument into an RTF document.
/// </summary>
[ValueConversion(typeof(FlowDocument), typeof(string))]
public class FlowDocumentToRTFConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string output = "";

        try
        {
            var document = value as FlowDocument;
            if (document != null)
            {
                var content = new TextRange(document.ContentStart, document.ContentEnd);
                if (content.CanSave(DataFormats.Rtf))
                {
                    using (var stream = new MemoryStream())
                    {
                        content.Save(stream, DataFormats.Rtf);
                        stream.Position = 0;
                        
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogException(ex);
        }

        return output;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
