using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TrabalhoSocketsEngine;

namespace TrabalhoSocketsUI.Converters
{
    public class CurrentTeamPlayToBoolConverter : BaseConverter, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var wrapper = values[0] as GameBoardElementWrapper;
            var currentTeam = (eTeam)Enum.Parse(typeof(eTeam), values[1].ToString());

            if (wrapper.Element == null)
                return false;

            return wrapper.Element.Team == currentTeam;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
    }
}
