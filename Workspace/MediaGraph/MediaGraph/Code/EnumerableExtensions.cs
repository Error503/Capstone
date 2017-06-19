using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MediaGraph.Code
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// An extension method for IEnumerables that returns a 
        /// string representation of the collection
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection</typeparam>
        /// <param name="list">The list for which to get the string representation</param>
        /// <returns>A string represenation of the collection</returns>
        public static string AsString<T>(this IEnumerable<T> list)
        {
            StringBuilder builder = new StringBuilder();

            foreach(T element in list)
            {
                builder.AppendFormat("{0}, ", element.ToString());
            }

            return builder.ToString().TrimEnd(' ', ',');
        }
    }
}