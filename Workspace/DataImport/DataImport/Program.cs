using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataImport.JsonModel;
using DataImport.Drivers;

namespace DataImport
{
    public class Program
    {
        private static List<AutocompleteRecord> ExistingRecords;

        public static void Main(string[] args)
        {
            PushData(@"D:\Courses\Q9\Capstone\ignore\blue-reflection-data.json");
        }

        private static void PushData(string filePath)
        {
            // Try and deserialize the example file
            Console.OutputEncoding = Encoding.UTF8;
            // Read the file
            List<BasicNodeModel> models = DataFileReader.ReadJson(filePath);
            // Map the data
            MapData(models);
            LogInColor("\n===== Mapping Complete =====\n", ConsoleColor.Yellow);
            // Push the data
            //PushData(models);
        }
         
        private static void PushData(List<BasicNodeModel> list)
        {
            // The data has been mapped and 
            DatabaseDriver driver = new DatabaseDriver();
            // Add each of the nodes
            foreach (BasicNodeModel model in list)
            {
                // Add the node
                driver.AddNode(model);
            }
        }

        private static List<BasicNodeModel> MapData(List<BasicNodeModel> list)
        {
            List<BasicNodeModel> resultingList = new List<BasicNodeModel>();
            Dictionary<string, Guid> nodeMap = new Dictionary<string, Guid>();

            // Get the currently existing data values
            using (CassandraDriver driver = new CassandraDriver())
            {
                ExistingRecords = driver.AggregateExistingRecords();
            }
            foreach (AutocompleteRecord record in ExistingRecords)
            {
                nodeMap.Add(record.CommonName.ToLowerInvariant(), Guid.Parse(record.Id));
            }

            // For each of the nodes in the node list
            foreach (BasicNodeModel model in list)
            {
                // Map the node
                if(MapNode(model, ref nodeMap))
                {
                    resultingList.Add(model);
                }
            }

            LogInColor("\n===== Mapping Complete =====\n", ConsoleColor.Yellow);
            Console.WriteLine("The following nodes will be added or referenced:");
            foreach(BasicNodeModel n in resultingList)
            {
                LogInColor($"{n.CommonName}", ConsoleColor.Blue);
                foreach(RelationshipModel m in n.Relationships)
                {
                    LogInColor($"\t{m.TargetName}", ConsoleColor.DarkMagenta);
                }
            }
            LogInColor($"\nA total of {resultingList.Count} nodes will be added.\n", ConsoleColor.Cyan);

            return resultingList;
        }

        private static bool MapNode(BasicNodeModel node, ref Dictionary<string, Guid> nodeMap)
        {
            // Guard clauses 
            // If the node exists in the database already,
            if(ExistingRecords.Find(x => x.CommonName.ToLowerInvariant() == node.CommonName.ToLowerInvariant()) != null)
            {
                LogInColor($"Skipping [{node.CommonName}] - It already exists in the database", ConsoleColor.Red);
                return false;
            }
            // If the node exists in the node map,
            if(nodeMap.ContainsKey(node.CommonName.ToLowerInvariant()))
            {
                LogInColor($"Skipping [{node.CommonName}] - It is duplicated within the provided file", ConsoleColor.Red);
                return false;
            }

            // If we reach this point then the node is a node to be added
            node.Id = Guid.NewGuid();
            nodeMap.Add(node.CommonName.ToLowerInvariant(), node.Id);
            LogInColor($"Mapped [{node.CommonName}] with id {node.Id.ToString()}", ConsoleColor.Yellow);
            // Check each of the referenced nodes
            foreach(RelationshipModel relModel in node.Relationships)
            {
                // If the node map contains the node's name
                if (nodeMap.ContainsKey(relModel.TargetName.ToLowerInvariant()))
                {
                    // Give the relationship the existing id
                    relModel.TargetId = nodeMap[relModel.TargetName.ToLowerInvariant()];
                    LogInColor($"\tMaped related node [{relModel.TargetName}] to existing id {relModel.TargetId}", ConsoleColor.DarkCyan);
                }
                else
                {
                    // Give the relationship a new id
                    relModel.TargetId = Guid.NewGuid();
                    relModel.IsNewAddition = true;
                    nodeMap.Add(relModel.TargetName.ToLowerInvariant(), relModel.TargetId.Value);
                    LogInColor($"\tMapped related node [{relModel.TargetName}] with id {relModel.TargetId}", ConsoleColor.DarkGreen);
                }
            }

            Console.WriteLine();

            return true;
        }

        public static void LogInColor(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
    }
}
