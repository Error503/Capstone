using System;
using System.Collections.Generic;
using System.Text;

namespace MediaGraph.Models.Util
{
    public enum NodeType
    {
        /// <summary>
        /// An actor in a video game, audio, or video.
        /// </summary>
        Actor = 1,
        /// <summary>
        /// A production company.
        /// </summary>
        Company = 2,
        /// <summary>
        /// Characters from videos, games, or audio.
        /// </summary>
        Character = 4,
        /// <summary>
        /// Television shows and movies.
        /// </summary>
        Video = 8,
        /// <summary>
        /// Songs and audio CDs.
        /// </summary>
        Audio = 16,
        /// <summary>
        /// Video games
        /// </summary>
        Game = 32,
        /// <summary>
        /// Any type of book. 
        /// </summary>
        Book = 64,
        /// <summary>
        /// Any form of media
        /// </summary>
        Generic_Media = Video | Audio | Game | Book
    }

    public static class NodeContentTypeExtensions
    {
        /// <summary>
        /// Returns a string representing this content type label.
        /// </summary>
        /// <param name="label">The label for which to get a string form</param>
        public static string ToLabelString(this NodeType label)
        {
            NodeType temp = label;
            bool shouldOrLabels = false;
            StringBuilder builder = new StringBuilder();

            // First check if the value is the generic media label
            if((temp & NodeType.Generic_Media) == NodeType.Generic_Media)
            {
                builder.Append(":Media");
                temp ^= NodeType.Generic_Media;
                shouldOrLabels = true;
            }

            // Handle non-compound values
            foreach (object val in Enum.GetValues(typeof(NodeType)))
            {
                NodeType enumVal = (NodeType)val;
                // If the tag is present,
                if((temp & enumVal) == enumVal)
                {
                    // Add the value to the 
                    builder.Append($"{(shouldOrLabels ? "|" : "")}:{enumVal.ToString()}");
                }

                temp ^= enumVal;
            }

            return builder.ToString();
        }

        /// <summary>
        /// Returns a NodeContentType for the given list of labels.
        /// </summary>
        /// <param name="labels">The labels for which to get the content type</param>
        /// <returns>A NodeContentType representing the labels as a bit field</returns>
        public static NodeType FromNodeLabels(IEnumerable<string> labels)
        {
            NodeType bitField = 0;

            // Loop through the labels
            IEnumerator<string> iter = labels.GetEnumerator();

            while(iter.MoveNext())
            {
                // Do not handle the "Media" label
                if(iter.Current != "Media")
                {
                    NodeType value;
                    // Attempt to parse the value of the label
                    if (Enum.TryParse(iter.Current, out value))
                    {
                        // OR the values together
                        bitField |= value;
                    }
                }
            }

            return bitField;
        }
    }
}