using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver.V1;

namespace DataImport
{
    /// <summary>
    /// A simple container class for DataFileObject that 
    /// contains all of the Neo4j data and import methods.
    /// </summary>
    public class NodeData
    {
        protected DataFileObject dataObject;

        /// <summary>
        /// Constructs NodeGraph object based on the given g
        /// </summary>
        /// <param name="data">The DataFileObject to use for the import</param>
        public NodeData(DataFileObject data)
        {
            dataObject = data;
        }

        /// <summary>
        /// Generates the full query string for the data file and executes it.
        /// </summary>
        public void RunMergeQuery()
        {
            // Get the merge queries for the data file
            string mediaStatements = CypherQueryUtils.CreateMergeQuery(dataObject.GetMediaCypherStrings());
            string peopleStatements = CypherQueryUtils.CreateMergeQuery(dataObject.GetPeopleCypherStrings());
            string companyStatements = CypherQueryUtils.CreateMergeQuery(dataObject.GetCompaniesCypherStrings());
            string relationshipStatements = CypherQueryUtils.CreateMergeQuery(dataObject.GetRelationshipStrings());
            string fullQuery = $"{mediaStatements}{peopleStatements}{companyStatements}{relationshipStatements}";
            Console.WriteLine($"Full Query:\n{fullQuery}");
            Console.WriteLine("Executing query...");
            IStatementResult result = CypherQueryUtils.ExecuteQuery(fullQuery);
            Console.WriteLine("Query execution complete!");
        }
    }
}
