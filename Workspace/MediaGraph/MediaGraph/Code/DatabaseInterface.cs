using MediaGraph.ViewModels.Edit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediaGraph.Code
{
    /// <summary>
    /// Defines the interface used to interact with the various parses of the database
    /// </summary>
    public interface DatabaseInterface
    {
        /// <summary>
        /// Inserts the given node into the database system. This method controls inserting
        /// the node into the Neo4j database and inserting the names into the 
        /// </summary>
        /// <param name="node">The node description to add to the database system</param>
        /// <returns>Returns true if the node was inserted into the database system successfully</returns>
        bool InsertNode(BasicNodeViewModel node);

        /// <summary>
        /// Inserts the given relationship into the database system. This method controls
        /// inserting the relationship into the applicable databases.
        /// </summary>
        /// <param name="rel">The relationship to insert into the database</param>
        /// <returns>Returns true if the relationship was added to the database system successfully</returns>
        //bool InsertRelationship(RelationshipDescription rel);

        /// <summary>
        /// Removes the node with the specified GUID from the database system.
        /// </summary>
        /// <param name="id">The GUID of the node to remove from the database system</param>
        /// <returns></returns>
        bool RemoveNodeFromSystem(Guid id);

        /// <summary>
        /// Removes the specified relationship from the database system.
        /// </summary>
        /// <param name="rel">The description of the relationship to rmeove</param>
        /// <returns>Returns true if the relationship was removed successfully</returns>
        //bool RemoveRelationshipFromSystem(RelationshipDescription rel);
    }
}