using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MediaGraph.Properties;

namespace MediaGraph.ViewModels
{
    /// <summary>
    /// View model for a single media node on the media graph
    /// </summary>
    public class MediaNodeViewModel
    {
        public string Title { get; }
        public DateTime ReleaseDate { get; }
        public float X { get; }
        public float Y { get; }

        /// <summary>
        /// Constructs a new MediaNodeViewModel with the given information.
        /// </summary>
        /// <param name="title">The title of the work</param>
        /// <param name="release">The release date of the work</param>
        /// <param name="x">The x position for the node</param>
        /// <param name="y">The y position for the node</param>
        public MediaNodeViewModel(string title, DateTime release, float x, float y)
        {
            Title = title;
            ReleaseDate = release;
            X = x;
            Y = y;
        }
    }
}