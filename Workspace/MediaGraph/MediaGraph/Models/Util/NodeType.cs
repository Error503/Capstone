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
        //Character = 4,
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
        Book = 64
    }

    public static class NodeTypeExtensions
    {
        public const NodeType kGenericMedia = NodeType.Audio | NodeType.Book | NodeType.Game | NodeType.Video;

        /// <summary>
        /// Returns a string representing this content type label.
        /// </summary>
        /// <param name="label">The label for which to get a string form</param>
        public static string ToLabelString(this NodeType label)
        {
            StringBuilder labelString = new StringBuilder();

            // If this is a media label, 
            if((label & kGenericMedia) == label)
            {
                // Add the media label 
                labelString.Append(":Media");
            }

            // Append the string value of the
            labelString.Append(string.Format(":{0}", Enum.GetName(typeof(NodeType), label)));

            return labelString.ToString();
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