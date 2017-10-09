using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Neo4j.Driver.V1;
using MediaGraph.Models;
using MediaGraph.Models.Component;
using MediaGraph.ViewModels.Edit;

namespace MediaGraph.Code
{
    public interface IGraphDatabaseDriver
    {
        /// <summary>
        /// Gets the node with the specified GUID.
        /// Returns an INode node with all of the node's data
        /// </summary>
        /// <param name="id">The GUID of the node to return</param>
        /// <returns>The GraphNode with the specified GUID or null if
        /// no such node can be found</returns>
        BasicNodeModel GetNode(Guid id);

        /// <summary>
        /// Gets the paths from the node with the given GUID.
        /// </summary>
        /// <param name="id">The GUID for the node from which to start</param>
        /// <returns>The paths starting at the node with the given GUID</returns>
        IEnumerable<IPath> GetPaths(Guid id);

        /// <summary>
        /// Attempts to add the given FullNode to the database.
        /// </summary>
        /// <param name="node">The EditNode to add to the database</param>
        /// <returns>Returns true if the node was added successfully</returns>
        bool AddNode(BasicNodeModel node);

        /// <summary>
        /// Attempts to update a node in the database by using a FullNode.
        /// The GUID of the full node will be used to find the node to update.
        /// </summary>
        /// <param name="node">The updated node data</param>
        /// <returns>Returns true if the node was updated successfully</returns>
        bool UpdateNode(BasicNodeViewModel node);

        /// <summary>
        /// Attempts to delete the node with the specified GUID from the database.
        /// </summary>
        /// <param name="node">The GUID of the node to delete</param>
        /// <returns>Returns true if the node with the specified GUID was deleted</returns>
        bool DeleteNode(Guid node);
    }
}