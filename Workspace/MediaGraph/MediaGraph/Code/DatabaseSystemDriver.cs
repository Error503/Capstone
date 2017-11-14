using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MediaGraph.Models.Component;
using System.Threading.Tasks;

namespace MediaGraph.Code
{
    public class DatabaseSystemDriver
    {
        /// <summary>
        /// Adds the given node into the database system.
        /// </summary>
        /// <param name="node">The node to add to the system</param>
        public void AddNode(BasicNodeModel node)
        {
            // Add the node into the autocomplete database
            using (AutocompleteDatabaseDriver autocompleteDriver = new AutocompleteDatabaseDriver())
            {
                autocompleteDriver.AddNode(node);
            }
            // Add the node to the graph
            using (Neo4jGraphDatabaseDriver graphDriver = new Neo4jGraphDatabaseDriver())
            {
                graphDriver.AddNode(node);
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
            using (AutocompleteDatabaseDriver autocompleteDriver = new AutocompleteDatabaseDriver())
            {
                autocompleteDriver.UpdateNode(node);
            }
            // Update the node in the graph database
            using (Neo4jGraphDatabaseDriver graphDriver = new Neo4jGraphDatabaseDriver())
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
            using (AutocompleteDatabaseDriver autocompleteDriver = new AutocompleteDatabaseDriver())
            {
                autocompleteDriver.DeleteNode(id);
            }
            // Delete the node from the graph database
            using (Neo4jGraphDatabaseDriver graphDriver = new Neo4jGraphDatabaseDriver())
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