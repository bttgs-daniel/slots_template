using System;
using System.Globalization;
/*
   08.07.2020 - first
 */
namespace Mkey
{
    public class CustomProvider : IFormatProvider
    {
        private string cultureName;

        public CustomProvider(string cultureName)
        {
            this.cultureName = cultureName;
        }

        public CustomProvider(CultureInfo cInfo)
        {
            cultureName = cInfo.Name;
        }

        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(DateTimeFormatInfo))
            {
                return new CultureInfo(cultureName).GetFormat(formatType);
            }
            else
            {
                return null;
            }
        }
    }
}
