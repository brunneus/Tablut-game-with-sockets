using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using TrabalhoSocketsEngine;

namespace TrabalhoSocketsUI.Converters
{
    public class eTeamToImageConverter : BaseConverter, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var element = values[0] as IGameBoardElement;
            var wrapper = values[1] as GameBoardElementWrapper;

            if (wrapper.R == 4 && wrapper.C == 4 && !(element is King))
                return new BitmapImage(new Uri("Icons\\king_empty.png", UriKind.RelativeOrAbsolute));

            if (element is Bodyguard)
                return new BitmapImage(new Uri("Icons\\white.png", UriKind.RelativeOrAbsolute));
            if (element is Mercenary)
                return new BitmapImage(new Uri("Icons\\black.png", UriKind.RelativeOrAbsolute));
            if (element is King)
                return new BitmapImage(new Uri("Icons\\king.png", UriKind.RelativeOrAbsolute));

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
