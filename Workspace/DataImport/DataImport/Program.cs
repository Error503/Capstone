using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver.V1;

namespace DataImport
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Try and deserialize the example file
            Console.OutputEncoding = Encoding.UTF8;
            string filePath = @"D:\Discord Daily\Datafiles\dat4.xml";
            DataFileObject dataObject = DataFileReader.ReadDataFile(filePath);
            NodeData nodeData = new NodeData(dataObject);
            nodeData.RunMergeQuery();
        }

        private static string ListToString(List<string> list)
        {
            StringBuilder builder = new StringBuilder();

            for(int i = 0; i < list.Count; i++)
            {
                builder.AppendLine(i == list.Count - 1 ? $"{list[i]}" : $"{list[i]},");
            }

            return builder.ToString();
        }
    }
}
