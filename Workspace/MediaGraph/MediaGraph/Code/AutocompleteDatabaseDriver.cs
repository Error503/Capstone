using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Cassandra;
using MediaGraph.Models.Component;
using System.Configuration;

namespace MediaGraph.Code
{
    public class AutocompleteDatabaseDriver : IDisposable
    {
        private Cluster connection;

        private string autocompleteFamilyName; 
        private string autocompleteKeySpace;

        public AutocompleteDatabaseDriver()
        {
            autocompleteFamilyName = "autocomplete_family";
            autocompleteKeySpace = "namekeyspace";
            connection = Cluster.Builder().WithCredentials("cassandra", "GiLY9v5jVfzy")
                .AddContactPoint("automediadata.southcentralus.cloudapp.azure.com")
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
                // Prepare the statement
                BoundStatement statement = session.Prepare("SELECT * FROM Autocomplete WHERE Name > ? ALLOW FILTERING").Bind(name);
                // Execute the query
                RowSet results = session.Execute(statement);
                // Iterate through the results
                foreach(var r in results)
                {
                    resultList.Add(new AutocompleteRecord
                    {
                        MatchedName = r.GetValue<string>("name"),
                        DataType = (NodeContentType)r.GetValue<int>("dataType"),
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
            return await Task.Run<List<AutocompleteRecord>>(() => { return Search(name); });
        }

        /// <summary>
        /// Adds the given node to the autocomplete database
        /// </summary>
        /// <param name="node">The node to add to the database</param>
        public void AddNode(BasicNodeModel node)
        {
            bool success = false;
            using(ISession session = connection.Connect(autocompleteKeySpace))
            {
                // Pepare the statement
                PreparedStatement statementBody = session.Prepare($"INSERT INTO {autocompleteFamilyName} (name, id, dataType) VALUES (?, {node.Id.ToString()}, {(int)node.ContentType})");
                // Execute the statement for the common name
                session.Execute(statementBody.Bind(node.CommonName));
                // Execute the statement for each of the other names
                foreach(string other in node.OtherNames)
                {
                    session.Execute(statementBody.Bind(other));
                }
            }
        }

        /// <summary>
        /// Updates the given node within the autocomplete database.
        /// </summary>
        /// <param name="node">The node to update</param>
        public void UpdateNode(BasicNodeModel node)
        {
            // Remove the node from the database
            DeleteNode(node.Id);
            // Add the node to the database
            AddNode(node);
        }

        /// <summary>
        /// Removes the given information from the database.
        /// </summary>
        /// <param name="id">The Guid to remove from the database</param>
        public void DeleteNode(Guid id)
        {
            using (ISession session = connection.Connect(autocompleteKeySpace))
            {
                // Prepare the statement
                PreparedStatement statementBody = session.Prepare($"DELETE FROM {autocompleteFamilyName} WHERE id = ?");
                // Run the statement
                session.Execute(statementBody.Bind(id.ToString()));
            }
        }
    }

    /// <summary>
    /// Models a single record in the autocomplete database.
    /// </summary>
    public class AutocompleteRecord
    {
        public string MatchedName { get; set; }
        public NodeContentType DataType { get; set; }
        public string Id { get; set; }
    }
}