using MediaGraph.Models.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediaGraph.Code
{
    /// <summary>
    /// Defines the interface used to interact with the database system
    /// as a whole.
    /// </summary>
    public interface DatabaseInterface
    {
        /// <summary>
        /// Inserts the given node into the database system. This method controls inserting
        /// the node into the Neo4j database and inserting the names into the 
        /// </summary>
        /// <param name="node">The node description to add to the database system</param>
        /// <returns>Returns true if the node was inserted into the database system successfully</returns>
        bool AddNode(BasicNodeModel node);

        /// <summary>
        /// Removes the node with the specified GUID from the database system.
        /// </summary>
        /// <param name="id">The GUID of the node to remove from the database system</param>
        /// <returns></returns>
        bool DeleteNode(Guid id);

        /// <summary>
        /// Updates the given node's information within the database system.
        /// </summary>
        /// <param name="node">The node to update</param>
        /// <returns>Returns true if the node's information was updated successfully</returns>
        bool UpdateNode(BasicNodeModel node);

        /// <summary>
        /// Searches for nodes in the database that match the given text.
        /// </summary>
        /// <param name="text">The text for which to search</param>
        /// <returns>A dictionary of key-value pairs representing the nodes that match
        /// the given text</returns>
        Dictionary<string, string> AutocompleteSearch(string text);
    }
}