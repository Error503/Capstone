using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace DataImport
{
    /// <summary>
    /// Reads data files 
    /// </summary>
    public static class DataFileReader
    {

        private static XmlSerializer xmlReader = new XmlSerializer(typeof(DataFileObject));

        /// <summary>
        /// Creates a DataFileObject from the XML file at the given file path.
        /// </summary>
        /// <param name="filePath">The file path of </param>
        /// <returns>The DataFileObject that was created</returns>
        public static DataFileObject ReadDataFile(string filePath)
        {
            return xmlReader.Deserialize(File.Open(filePath, FileMode.Open)) as DataFileObject; 
        }
    }
}
