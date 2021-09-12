using System;
using System.Globalization;
using System.Xml;

namespace Gpx.Implementation
{
    internal static class XmlReaderExtension 
    {
        public static void SkipElement(this XmlReader reader)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;
            int depth = reader.Depth;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Depth == depth && reader.Name == elementName)
                        return;
                }
            }

            throw new FormatException(elementName);
        }

      /*  public static string ReadElementContentAsString(this XmlReader reader)
        {
            if (reader.IsEmptyElement)
                throw new FormatException(reader.Name);

            string elementName = reader.Name;
            string result = string.Empty;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Text:
                        result = reader.Value;
                        break;

                    case XmlNodeType.EndElement:
                        return result;

                    case XmlNodeType.Element:
                        throw new FormatException(elementName);
                }
            }

            throw new FormatException(elementName);
        }

        public static int ReadElementContentAsInt(this XmlReader reader)
        {
            string value = reader.ReadElementContentAsString();
            return int.Parse(value, CultureInfo.InvariantCulture);
        }

        public static double ReadElementContentAsDouble(this XmlReader reader)
        {
            string value = reader.ReadElementContentAsString();
            return double.Parse(value, CultureInfo.InvariantCulture);
        }

        public static DateTime ReadElementContentAsDateTime(this XmlReader reader)
        {
            string value = reader.ReadElementContentAsString();
            return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        }*/
    }
}