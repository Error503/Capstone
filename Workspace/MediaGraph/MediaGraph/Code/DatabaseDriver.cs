using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MediaGraph.Models.Component;
using System.Threading.Tasks;

namespace MediaGraph.Code
{
    public class DatabaseDriver
    {

        /// <summary>
        /// Adds the given node into the database system.
        /// </summary>
        /// <param name="node">The node to add to the system</param>
        public void AddNode(BasicNodeModel node)
        {
            // Map the node's information for information marked as "New addition"
            MapNewAdditions(ref node);
            // Add the node into the autocomplete database
            using (CassandraDriver autocompleteDriver = new CassandraDriver())
            {
                autocompleteDriver.AddNode(node);
            }
            // Add the node to the graph
            using (NeoDriver graphDriver = new NeoDriver())
            {
                graphDriver.AddNode(node);
            }
        }

        /// <summary>
        /// Uses the autocomplete database to map node information to existing information.
        /// </summary>
        /// <param name="node">The node to map</param>
        private void MapNewAdditions(ref BasicNodeModel node)
        {
            // Get the referenced nodes that are marked as new additions
            IEnumerable<RelationshipModel> newAdditions = node.Relationships.Where(x => x.IsNewAddition);

            using (CassandraDriver driver = new CassandraDriver())
            {
                // For each of the new additions
                foreach(RelationshipModel m in newAdditions)
                {
                    // Search for an existing record
                    List<AutocompleteRecord> existingRecords = driver.MappingSearch(m.TargetName, m.TargetType);
                    // If there is exactly one record,
                    if(existingRecords.Count == 1)
                    {
                        // Update the id to the existing record 
                        m.TargetId = Guid.Parse(existingRecords.First().Id);
                    }
                }
            }
        }

        /// <summary>
        /// Asynchronously adds the given node to the database system.
        /// </summary>
        /// <param name="node">The node to add to the database system</param>
        public async void AddNodeAsync(BasicNodeModel node)
        {
            await Task.Run(() => { AddNode(node); });
        }

        /// <summary>
        /// Updates the given node within the database system.
        /// </summary>
        /// <param name="node">The node to update</param>
        public void UpdateNode(BasicNodeModel node)
        {
            // Update the node in the autocomplete database
            using (CassandraDriver autocompleteDriver = new CassandraDriver())
            {
                autocompleteDriver.UpdateNode(node);
            }
            // Update the node in the graph database
            using (NeoDriver graphDriver = new NeoDriver())
            {
                graphDriver.UpdateNode(node);
            }
        }

        /// <summary>
        /// Asynchronously updates the given node within the database system.
        /// </summary>
        /// <param name="node">The node to update</param>
        public async void UpdateNodeAsync(BasicNodeModel node)
        {
            await Task.Run(() => { UpdateNode(node); });
        }

        /// <summary>
        /// Deletes the node with the specified id from the database.
        /// </summary>
        /// <param name="id">The id of the node to delete</param>
        public void DeleteNode(Guid id)
        {
            // Delete the node from the autocomplete database
            using (CassandraDriver autocompleteDriver = new CassandraDriver())
            {
                autocompleteDriver.DeleteNode(id.ToString());
            }
            // Delete the node from the graph database
            using (NeoDriver graphDriver = new NeoDriver())
            {
                graphDriver.DeleteNode(id);
            }
        }

        /// <summary>
        /// Asynchronously deletes the node with the given id from the database.
        /// </summary>
        /// <param name="id">The id of the node to delete</param>
        public async void DeleteNodeAsync(Guid id)
        {
            await Task.Run(() => { DeleteNode(id); });
        }
    }
}