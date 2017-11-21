using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Cassandra;
using MediaGraph.Models.Component;
using System.Configuration;
using System.Text.RegularExpressions;

namespace MediaGraph.Code
{
    public class CassandraDriver : IDisposable
    {
        private Cluster connection;

        private string autocompleteTable;
        private string autocompleteKeySpace;
        private string autocompleteMetadataTable;

#if DEBUG
        private const string kContactPoint = "automediadata.southcentralus.cloudapp.azure.com";
#else
        private const string kContactPoint = "10.0.0.5";
#endif

        /*
         * Autocomplete Table Schema:
         * Prefix, Remaining, Id, Name, DataType, CommonName, PRIMARY KEY (Prefix, Remaining, Id)
         * 
         * Autocomplete Metadata Table Schema:
         * Id, PrefixValues (list), RemainingValues (list), PRIMARY KEY (Id)
         */

        public CassandraDriver()
        {
            autocompleteTable = "names_table"; //"simple_names_table";
            autocompleteMetadataTable = "names_meta_table";
            autocompleteKeySpace = "test_space"; //"autocomplete_space";
            connection = Cluster.Builder().WithCredentials("cassandra", "GiLY9v5jVfzy")
                .AddContactPoint(kContactPoint)
                .WithPort(9042).Build();
        }

        public void Dispose()
        {
            connection?.Dispose();
        }

        /// <summary>
        /// Searches the database for records that match the given string.
        /// </summary>
        /// <param name="name">The name for which to search</param>
        /// <returns>A collection of records that match the given string</returns>
        public List<AutocompleteRecord> Search(string name)
        {
            List<AutocompleteRecord> resultList = new List<AutocompleteRecord>();
            using (ISession session = connection.Connect(autocompleteKeySpace))
            {
                string searchValue = name.Length > 3 ? name.Substring(0, 3) : name;
                // Prepare the statement
                BoundStatement statement = session.Prepare($"SELECT * FROM {autocompleteTable} WHERE prefix = ?").Bind(searchValue);
                // Execute the query
                RowSet results = session.Execute(statement);
                // Iterate through the results
                foreach (Row r in results)
                {
                    resultList.Add(new AutocompleteRecord
                    {
                        CommonName = r.GetValue<string>("commonname"),
                        MatchedName = r.GetValue<string>("name"),
                        DataType = (NodeContentType)r.GetValue<int>("datatype"),
                        Id = r.GetValue<string>("id")
                    });
                }
            }

            return resultList;
        }

        /// <summary>
        /// Asynchronously searches the autocomplete database for nodes that match 
        /// the given string.
        /// </summary>
        /// <param name="name">The name for which to search</param>
        /// <returns>A collection of records that match the given name</returns>
        public async Task<List<AutocompleteRecord>> SearchAsync(string name)
        {
            return await Task.Run(() => { return Search(name); });
        }

#region Batch Add Methods
        /// <summary>
        /// Adds the given node to the database using a Apache Cassandra BATCH statement.
        /// </summary>
        /// <param name="node">The node to add to the database</param>
        public void AddNode(BasicNodeModel node)
        {
            using (ISession session = connection.Connect(autocompleteKeySpace))
            {
                // Create the metadata statement
                PreparedStatement metadataStatement = session.Prepare(
                    $"INSERT INTO {autocompleteMetadataTable} (id, prefixValues, remainingValues) VALUES (?, ?, ?)");
                // Create the names table statement
                PreparedStatement mainStatement = session.Prepare(
                    $"INSERT INTO {autocompleteTable} (prefix, remaining, id, name, commonName, dataType) VALUES (?, ?, ?, ?, ?, ?) IF NOT EXISTS");
                // Add the original node
                InsertNode(node, session, metadataStatement, mainStatement);
                // For each of the related nodes...
                foreach (RelationshipModel relationship in node.Relationships)
                {
                    // If the node is a new addition,
                    if (relationship.IsNewAddition)
                    {
                        // Add the related node to the batch
                        InsertNode(new BasicNodeModel
                        {
                            Id = relationship.TargetId.Value,
                            CommonName = relationship.TargetName,
                            ContentType = relationship.TargetType
                        }, session, metadataStatement, mainStatement);
                    }
                }
            }
        }

        /// <summary>
        /// Inserts the given node into the Apache Cassandra autocomplete database.
        /// </summary>
        /// <param name="node">The node to add to the database</param>
        /// <param name="session">The session to use</param>
        /// <param name="metaStatement">The prepared metadata statement</param>
        /// <param name="mainStatement">The prepared main statement</param>
        private void InsertNode(BasicNodeModel node, ISession session, PreparedStatement metaStatement, PreparedStatement mainStatement)
        {
            Dictionary<string, Tuple<string, string>> bindingDictionary = GetBindingDictionary(node);
            // Get the id string once instead of everytime it is needed
            string idString = node.Id.ToString();
            // Execute the meta statement
            Console.WriteLine("ADDING TO META: " + node.CommonName + " WITH ID: " + node.Id.ToString());
            session.Execute(metaStatement.Bind(idString, bindingDictionary.Select(x => x.Value.Item1).ToArray(),
                bindingDictionary.Select(x => x.Value.Item2).ToArray()));
            // For each of the entries in the binding dictionary...
            foreach (KeyValuePair<string, Tuple<string, string>> bindingEntry in bindingDictionary)
            {
                Console.WriteLine("ADDING TO MAIN: " + bindingEntry.Key);
                // Execute the main statement
                session.Execute(mainStatement.Bind(bindingEntry.Value.Item1, bindingEntry.Value.Item2, idString,
                    bindingEntry.Key, node.CommonName, (int)node.ContentType));
            }
        }

        /// <summary>
        /// Creates a dictionary containing the names values that will be bound to the 
        /// main table statement when the node is added to the Apache Cassandra database.
        /// </summary>
        /// <param name="node">The node for which to get the binding values</param>
        /// <returns>A dictionary containing the binding values for each of the given node's names</returns>
        private Dictionary<string, Tuple<string, string>> GetBindingDictionary(BasicNodeModel node)
        {
            // Create the dictionary and add the commonName to the dictionary
            Dictionary<string, Tuple<string, string>> dictionary = new Dictionary<string, Tuple<string, string>>
            {
                { node.CommonName, BindName(node.CommonName) }
            };
            // If there is a collection of other names,
            if (node.OtherNames != null)
            {
                // Add each of the other names to the dictionary
                foreach (string otherName in node.OtherNames)
                {
                    dictionary.Add(otherName, BindName(otherName));
                }
            }

            // Return the dictionary as a list
            return dictionary;
        }

        /// <summary>
        /// Matches the given name to the binding regular expression and
        /// creates the binding tuple from the match.
        /// </summary>
        /// <param name="name">The name for which to get the binding values</param>
        /// <returns>A tuple containing the binding values for the given name</returns>
        private Tuple<string, string> BindName(string name)
        {
            // Match the name with the regular expression
            Match m = Regex.Match(name.ToLowerInvariant(), @"^(.{1,3})(.*)");
            return new Tuple<string, string>(m.Groups[1].Value, m.Groups[2].Value);
        }
#endregion

        /// <summary>
        /// Updates the given node within the autocomplete database.
        /// </summary>
        /// <param name="node">The node to update</param>
        public void UpdateNode(BasicNodeModel node)
        {
            // Remove the node from the database
            DeleteNode(node.Id.ToString());
            // Add the node to the database
            AddNode(node);
        }

        /// <summary>
        /// Uses a batch statement to delete the node from the Apache Cassandra 
        /// autocomplete database.
        /// </summary>
        /// <param name="id">The id of the node to delete</param>
        public void DeleteNode(string id)
        {
            using (ISession session = connection.Connect(autocompleteKeySpace))
            {
                // Get the data from the metadata table
                RowSet metadataResult = session.Execute(session.Prepare($"SELECT * FROM {autocompleteMetadataTable} WHERE id = ?").Bind(id));
                // Get the first record from the result
                Row resultRow = metadataResult.First();
                // Get the prefix and remaining value information
                List<string> prefixValues = resultRow.GetValue<List<string>>("prefixvalues");
                List<string> remainingValues = resultRow.GetValue<List<string>>("remainingvalues");

                // Prepare the delete statement
                PreparedStatement deleteStatement = session.Prepare(
                    $"DELETE FROM {autocompleteTable} WHERE prefix = ? AND remaining = ? AND id = ?");
                // Create the batch statement
                BatchStatement batch = new BatchStatement();

                // For each of the prefix and remaining values
                for (int i = 0; i < prefixValues.Count; i++)
                {
                    // Add a delete statement to the batch
                    batch.Add(deleteStatement.Bind(prefixValues[i], remainingValues[i], id));
                }

                // Execute the statement
                session.Execute(batch);
                // Finally, delete the record from the metadata table
                session.Execute(session.Prepare($"DELETE FROM {autocompleteMetadataTable} WHERE id = ?").Bind(id));
            }
        }
    }

    /// <summary>
    /// Models a single record in the autocomplete database.
    /// </summary>
    public class AutocompleteRecord
    {
        public string CommonName { get; set; }
        public string MatchedName { get; set; }
        public NodeContentType DataType { get; set; }
        public string Id { get; set; }
    }
}