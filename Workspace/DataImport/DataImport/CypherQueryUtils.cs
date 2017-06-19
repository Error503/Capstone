using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver.V1;

namespace DataImport
{
    public static class CypherQueryUtils
    {
        private static readonly Dictionary<RelationshipType, string> kRelationshipMap = new Dictionary<RelationshipType, string>
        {
            { RelationshipType.Created, "CREATED" },
            { RelationshipType.Cast, "CAST_IN" },
            { RelationshipType.Sequel, "SEQUEL_OF" },
            { RelationshipType.Prequel, "PREQUEL_OF" },
            { RelationshipType.Spinoff, "SPINOFF_OF" },
            { RelationshipType.Adaptation, "ADAPTATION_OF" },
            { RelationshipType.Remake, "REMAKE_OF" },
            { RelationshipType.Series, "SERIES" }
        };

        /// <summary>
        /// Static method that generates the string representation of
        /// the given array for Cypher queries.
        /// </summary>
        /// <param name="stringList">The list for which to generate the Cypher string</param>
        /// <returns>The generated Cypher array string</returns>
        public static string GetCypherStringArray(List<string> stringList)
        {
            StringBuilder builder = new StringBuilder("[");

            // Loop through the given array
            for (int i = 0; i < stringList.Count; i++)
            {
                builder.Append(i == stringList.Count - 1 ? $"\"{stringList[i]}\"" : $"\"{stringList[i]}\",");
            }
            // Append the ending ']'
            builder.Append("]");

            return builder.ToString();
        }

        /// <summary>
        /// Gets the string representation of the given relationship type.
        /// </summary>
        /// <param name="type">The RelationshipType for which to get the 
        /// string representation</param>
        /// <returns>The Cypher string representation of the relationship type</returns>
        public static string GetCypherRelationshipType(RelationshipType type)
        {
            return kRelationshipMap[type];
        }

        public static string CreateMergeQuery(List<string> nodeStrings)
        {
            StringBuilder builder = new StringBuilder();

            for(int i = 0; i < nodeStrings.Count; i++)
            {
                builder.AppendLine($"MERGE {nodeStrings[i]}");
            }

            return builder.ToString();
        }

        public static IStatementResult ExecuteQuery(string cypherQuery)
        {
            const string url = @"bolt://localhost";
            const string login = "neo4j";
            const string pass = "meaninglessPassword";
            IStatementResult queryResult = null;

            // Establish a connection
            using (IDriver driver = GraphDatabase.Driver(url, AuthTokens.Basic(login, pass)))
            {
                using (ISession session = driver.Session())
                {
                    queryResult = session.Run(cypherQuery);
                }
            }

            return queryResult;
        }
    }
}
