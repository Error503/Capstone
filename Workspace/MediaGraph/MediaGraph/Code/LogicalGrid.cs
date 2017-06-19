using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Neo4j.Driver.V1;
using System.Text;
using MediaGraph.Properties;
using MediaGraph.Models.Util;

namespace MediaGraph.Code
{
    /// <summary>
    /// LogicalGrid is a logical graph used as representation
    /// of the SVG image that will be displayed to the user. 
    /// LogicalGrid is used as a staging board to get the 
    /// coordinates and information for all.
    /// TODO: Mr.Krebs says that is best to get the client to do the work. Use JavaScript instead
    /// </summary>
    public class LogicalGrid
    {
        // The LogicalGrid is an empty grid on to which nodes can be positioned by release date.
        // The very center of the graph is the root node.
        // The logical grid is 
        protected Dictionary<long, GridNode> nodes;
        protected Dictionary<Tuple<long, long>, GridRelationship> relationships;
        protected GridNode rootNode;

        private LogicalGrid(GridNode root, Dictionary<long, GridNode> otherNodes, Dictionary<Tuple<long, long>, GridRelationship> rels)
        {
            rootNode = root;
            nodes = otherNodes;
            relationships = rels;
        }

        #region SVG Generation Methods
        public HtmlString GenerateSVG()
        {
            StringBuilder builder = new StringBuilder();
            float relativeCenterX = Settings.Default.mediaGraphWidth / 2.0f;
            float relativeCenterY = Settings.Default.mediaGraphHeight / 2.0f;
            // TODO: need a better translation offset calculation - none exists at the moment
            // Generate the SVG for all nodes
            builder.AppendLine("<g id='node-layer'>");
            foreach(KeyValuePair<long, GridNode> nodePair in nodes)
            {
                builder.Append(GenerateNodeSVG(nodePair.Value, relativeCenterX, relativeCenterY));
            }
            builder.AppendLine("</g>");

            // Add relationships
            builder.AppendLine("<g id='relationship-layer'>");
            foreach(KeyValuePair<Tuple<long, long>, GridRelationship> rel in relationships)
            {
                builder.Append(GenerateRelationshipSVG(rel.Key, rel.Value));
            }
            builder.AppendLine("</g>");

            return new HtmlString(builder.ToString());
        }

        /// <summary>
        /// Generates the SVG for the given node
        /// </summary>
        /// <param name="node">The svg string for the given node</param>
        private string GenerateNodeSVG(GridNode node, float relativeCenterX, float relativeCenterY)
        {
            float nodeX = relativeCenterX + (node.GridX * (Settings.Default.mediaGraphWidth + Settings.Default.mediaGraphColSpacing));
            float nodeY = relativeCenterY + (node.GridY * ((Settings.Default.mediaNodeHeight / 2) + Settings.Default.mediaGraphRowSpacing));
            string baseFormat = $"<g onclick='handleNodeClick(this)' transform='translate({nodeX},{nodeY})' class='graph-node-group'>";
            string nodeRect = $"<rect x='0' y='0' rx='10' ry='10' width='{Settings.Default.mediaNodeTextInset}' height='{Settings.Default.mediaNodeLineHeight}' class='relationship-node' />";
            string nameText = $"<text x='{Settings.Default.mediaNodeTextInset}' y='{Settings.Default.mediaNodeLineHeight}' lengthAdjust='spacingAndGlyphs' textLength='85'>{node.Names[0]}</text>";
            string releaseText = $"<text x='{Settings.Default.mediaNodeTextInset}' y='{(2 * Settings.Default.mediaNodeLineHeight)}' lengthAdjust='spacingAndGlyphs' textLength='85'>{node.ReleaseDate.ToShortDateString()}</text>";
            return string.Format("{0}{1}{2}{3}</g>", baseFormat, nodeRect, nameText, releaseText);
        }

        /// <summary>
        /// Generates the SVG for the given relationship.
        /// </summary>
        /// <param name="rel">The relationship for which to generate the SVG string</param>
        /// <returns>The SVG string for the given relationship</returns>
        private string GenerateRelationshipSVG(Tuple<long, long> startAndEndIds, GridRelationship rel)
        {
            float halfCanWidth = Settings.Default.mediaGraphWidth / 2.0f;
            float halfCanHeight = Settings.Default.mediaGraphHeight / 2.0f;
            float widthOffset = Settings.Default.mediaNodeWidth * Settings.Default.mediaGraphColSpacing;
            float heightOffset = Settings.Default.mediaNodeHeight * Settings.Default.mediaGraphRowSpacing;

            float startX = halfCanWidth + (nodes[startAndEndIds.Item1].GridX * widthOffset);
            float startY = halfCanHeight + (nodes[startAndEndIds.Item1].GridY * heightOffset);
            float endX = halfCanWidth + (nodes[startAndEndIds.Item2].GridX * widthOffset);
            float endY = halfCanHeight + (nodes[startAndEndIds.Item2].GridY * heightOffset);

            // TODO: Position lines to anchoring points

            // Rotate by angle between start and end for text
            string relationshipLine = $"<line x1='{startX}' y1='{startY}' x2='{endX}' y2='{endY}' />";
            // Arrow head? - make a triangle and rotate it?
            return relationshipLine; 
        }
        #endregion

        #region Static Generation Methods
        public static LogicalGrid CreateRelationshipGraph(IStatementResult queryResult)
        {
            Dictionary<long, GridNode> nodes = new Dictionary<long, GridNode>();
            Dictionary<Tuple<long, long>, GridRelationship> relationships = new Dictionary<Tuple<long, long>, GridRelationship>();
            GridNode rootNode = null;

            // The query result here should be a collection of paths between nodes
            foreach(IRecord record in queryResult)
            {
                IPath nodePath = record[0].As<IPath>();

                if(rootNode == null)
                {
                    // Set the starting node
                    rootNode = new GridNode(0, 0, nodePath.Start);
                }
                
                // Add all of the nodes
                foreach(INode node in nodePath.Nodes)
                {
                    if (node.Labels[0] == "Media" && !nodes.ContainsKey(node.Id))
                    {
                        Tuple<int, int> offsets = GetRelativeGridPosition(rootNode.ReleaseDate, DateTime.Parse(node.Properties["releaseDate"].As<string>()));
                        nodes.Add(node.Id, new GridNode(offsets.Item1, offsets.Item2, node));
                    }
                }

                Tuple<long, long> key = new Tuple<long, long>(nodePath.Start.Id, nodePath.End.Id);
                // Add all of the relationships
                foreach(IRelationship relationship in nodePath.Relationships)
                {
                    if(!relationships.ContainsKey(key))
                    {
                        // Add the relationshpi
                        relationships.Add(key, new GridRelationship(relationship));
                    }
                }
            }

            return new LogicalGrid(rootNode, nodes, relationships);
        }

        private static Tuple<int, int> GetRelativeGridPosition(DateTime rootReleaseDate, DateTime currentReleaseDate)
        {
            // Get the time span between the two release dates
            TimeSpan span = currentReleaseDate.Subtract(rootReleaseDate);
            // Create the tuple
            return new Tuple<int, int>(currentReleaseDate.Year - rootReleaseDate.Year, (span.Days / 30) % 12);
        }
        #endregion
    }

    public class GridNode
    {
        protected int gridX;
        public int GridX { get { return gridX; } }

        protected int gridY;
        public int GridY { get { return gridY; }  }

        protected NodeType labels;
        public NodeType Labels { get { return labels; } }

        protected List<string> names;
        public List<string> Names { get { return names; } }

        protected DateTime releaseDate;
        public DateTime ReleaseDate { get { return releaseDate; } }

        protected string franchise;
        public string Franchise { get { return franchise; } }

        protected List<string> genres;
        public List<string> Genres { get { return genres; } }

        public GridNode(int xPos, int yPos, INode dataNode)
        {
            gridX = xPos;
            gridY = yPos;
            // Get node data
            // TODO: Handle labels
            names = dataNode["names"].As<List<string>>();
            if (dataNode.Labels.Contains("Media"))
            {
                releaseDate = DateTime.Parse(dataNode["releaseDate"].As<string>());
                franchise = dataNode["franchise"].As<string>();
                genres = dataNode["genres"].As<List<string>>();
            }
        }
    }

    public class GridRelationship
    {
        //protected RelationshipLabel labels;
        //public RelationshipLabel Labels { get { return labels; } }


        public GridRelationship(IRelationship relationshipData)
        {
            // TODO: Handle labels
        }
    }
}