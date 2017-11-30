using MediaGraph.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace MediaGraph.Models.Component
{
    public static class HashFactory
    {
        public static string CalculatePartialHash(BasicNodeModel model)
        {
            byte[] hash = Hash(GetPartialByteBuffer(model));
            return hash != null ? ConvertToHexString(hash) : null;
        }

        public static string CalculateFullHash(BasicNodeModel model)
        {
            byte[] hash = Hash(GetFullByteBuffer(model));
            return hash != null ? ConvertToHexString(hash) : null;
        }

        private static byte[] Hash(byte[] inputBytes)
        {
            byte[] outputBytes = null;

            using (MD5 hasher = MD5.Create())
            {
                outputBytes = hasher.ComputeHash(inputBytes);
            }

            return outputBytes;
        }

        #region Byte Buffer Methods
        private static byte[] GetPartialByteBuffer(BasicNodeModel model)
        {
            List<byte> buffer = new List<byte>();

            // Add the common name of the node
            buffer.AddRange(Encoding.UTF8.GetBytes(model.CommonName));
            // Add the content type
            buffer.Add((byte)model.ContentType);
            // Add the release date
            if (model.ReleaseDate.HasValue)
                buffer.AddRange(Encoding.UTF8.GetBytes(DateValueConverter.ToDateTime(model.ReleaseDate.Value).ToString("yyyy-MM-dd")));
            else
                buffer.AddRange(Encoding.UTF8.GetBytes("0000-00-00"));
            // Finally, add the id
            buffer.AddRange(model.Id.ToByteArray());

            return buffer.ToArray();
        }

        private static byte[] GetFullByteBuffer(BasicNodeModel model)
        {
            List<byte> buffer = new List<byte>();
            // Add the contentType
            buffer.Add((byte)model.ContentType);
            // Add the common name
            buffer.AddRange(Encoding.UTF8.GetBytes(model.CommonName));
            // Add each of the other names
            foreach(string name in model.OtherNames.OrderBy(x => x))
            {
                buffer.AddRange(Encoding.UTF8.GetBytes(name));
            }
            // Add the release date
            if (model.ReleaseDate.HasValue)
                buffer.AddRange(Encoding.UTF8.GetBytes(DateValueConverter.ToDateTime(model.ReleaseDate.Value).ToString("yyyy-MM-dd")));
            else
                buffer.AddRange(Encoding.UTF8.GetBytes("0000-00-00"));
            // Add the death date
            if (model.DeathDate.HasValue)
                buffer.AddRange(Encoding.UTF8.GetBytes(DateValueConverter.ToDateTime(model.DeathDate.Value).ToString("yyyy-MM-dd")));
            else
                buffer.AddRange(Encoding.UTF8.GetBytes("0000-00-00"));
            // Add the id
            buffer.AddRange(model.Id.ToByteArray());

            // Add node type content
            if (model.ContentType == NodeContentType.Company)
                AddCompanyInformationToBuffer(((CompanyNodeModel)model), ref buffer);
            else if(model.ContentType == NodeContentType.Media) 
                AddMediaInformationToBuffer(((MediaNodeModel)model), ref buffer);
            else if (model.ContentType == NodeContentType.Person)
                AddPersonInformationToBuffer(((PersonNodeModel)model), ref buffer);

            return buffer.ToArray();
        }

        private static void AddCompanyInformationToBuffer(CompanyNodeModel model, ref List<byte> buffer)
        {
            // No specific information here 
        }

        private static void AddMediaInformationToBuffer(MediaNodeModel model, ref List<byte> buffer)
        {
            // Add media type
            buffer.Add((byte)model.MediaType);
            // Add Franchise
            buffer.AddRange(Encoding.UTF8.GetBytes(model.Franchise));
            // Add the genres
            foreach(string genre in model.Genres.OrderBy(x => x))
            {
                buffer.AddRange(Encoding.UTF8.GetBytes(genre));
            }
        }

        private static void AddPersonInformationToBuffer(PersonNodeModel model, ref List<byte> buffer)
        {
            // Add first name 
            buffer.AddRange(Encoding.UTF8.GetBytes(model.GivenName));
            // Add last name
            buffer.AddRange(Encoding.UTF8.GetBytes(model.FamilyName));
            // Add status
            buffer.Add((byte)model.Status);
        }
        #endregion

        /// <summary>
        /// Converts the given byte array to a hexidecimal string.
        /// </summary>
        /// <param name="values">The array of bytes to </param>
        /// <returns></returns>
        private static string ConvertToHexString(byte[] values)
        {
            StringBuilder builder = new StringBuilder(values.Length * 2);
            // For each of the bytes...
            foreach (byte b in values)
            {
                // Append the hexadecimal string value
                builder.AppendFormat("{0:x2}", b);
            }

            return builder.ToString();
        }
    }
}