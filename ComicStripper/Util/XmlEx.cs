using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace Util
{
    public static class XmlEx
    {
        public static void ToXmlFile<T>(this T source, string filePath)
        {
            XmlSerializer s = new XmlSerializer(typeof(T));
            using (FileStream fs = File.Create(filePath))
            {
                s.Serialize(fs, source);
            }
        }

        public static T FromXmlFile<T>(string filePath)
        {
            XmlSerializer s = new XmlSerializer(typeof(T));
            using (FileStream fs = File.OpenRead(filePath))
            {
                T source = (T)s.Deserialize(fs);
                return source;
            }
        }
    }
}
