﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace HunterPie.UI.Architecture.Converters
{
    public class MonsterEmToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string monsterEm = (string)value;

            if (monsterEm is null || monsterEm.Length == 0)
                return null;

            bool isRise = monsterEm.StartsWith("Rise");

            if (!isRise)
                monsterEm += "_ID";

            return new ImageSourceConverter().ConvertFromString($"pack://siteoforigin:,,,/Assets/Monsters/Icons/{monsterEm}.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
