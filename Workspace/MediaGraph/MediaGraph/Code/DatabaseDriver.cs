using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MediaGraph.Models.Component;

namespace MediaGraph.Code
{
    public static class DatabaseDriver
    {
        /// <summary>
        /// Adds the given node into the database system.
        /// </summary>
        /// <param name="node">The node to add to the system</param>
        /// <returns>Returns true if the given node was added to the database system</returns>
        public static bool AddNode(BasicNodeModel node)
        {
            bool result = false;
            // Add the information to the autocomplete database

            // Add the node to the Neo4j database
            using (Neo4jGraphDatabaseDriver graphDriver = new Neo4jGraphDatabaseDriver())
            {
                result = graphDriver.AddNode(node);
            }

            return result;
        }

        /// <summary>
        /// Removes the given node from the database system.
        /// </summary>
        /// <param name="id">The GUID of the node being deleted</param>
        /// <returns>Returns true if the deletion was successful</returns>
        public static bool DeleteNode(Guid id)
        {
            bool result = false;
            // Delete the information from the autocomplete database

            // Delete the information from the Neo4j database
            using (Neo4jGraphDatabaseDriver graphDriver = new Neo4jGraphDatabaseDriver())
            {
                result = graphDriver.DeleteNode(id);
            }

            return result;
        }

        /// <summary>
        /// Updates the given node in the database system.
        /// </summary>
        /// <param name="node">The node to update</param>
        /// <returns>Returns true if the node is updated successfully.</returns>
        public static bool UpdateNode(BasicNodeModel node)
        {
            bool result = false;
            // Update the autocomplete database

            // Update the Neo4j database
            using (Neo4jGraphDatabaseDriver graphDriver = new Neo4jGraphDatabaseDriver())
            {
                result = graphDriver.UpdateNode(node);
            }

            return result;
        }
    }
}