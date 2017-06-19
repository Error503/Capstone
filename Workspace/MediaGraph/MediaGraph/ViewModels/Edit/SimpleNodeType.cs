using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediaGraph.ViewModels.Edit
{
    public enum SimpleNodeType : byte
    {
        Actor = 1,
        Company = 2,
        // Media
        Audio = 4,
        Book = 8,
        Game = 16,
        Video = 32
    }
}