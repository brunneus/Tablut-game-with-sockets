using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using TrabalhoSocketsEngine;
using TrabalhoSocketsUI.Properties;

namespace TrabalhoSocketsUI.Converters
{
    public class eTeamToImageConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var wrapper = value as GameBoardElementWrapper;

            if (wrapper == null)
                return null;

            if(wrapper.R == 4 && wrapper.C == 4 && wrapper.Element == null)
                return new BitmapImage(new Uri("Icons\\king_empty.png", UriKind.RelativeOrAbsolute));

            if (wrapper.Element is Bodyguard)
                return new BitmapImage(new Uri("Icons\\white.png",UriKind.RelativeOrAbsolute));
            if(wrapper.Element is Mercenary)
                return new BitmapImage(new Uri("Icons\\black.png",UriKind.RelativeOrAbsolute));
            if(wrapper.Element is King)
                return new BitmapImage(new Uri("Icons\\king.png", UriKind.RelativeOrAbsolute));

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
