using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediaGraph.Models.Component
{
    public enum NodeContentType : byte
    {
        Company = 1,
        Media = 2,
        Person = 3
    }

    public enum NodeMediaType : byte
    {
        Audio = 1,
        Book = 2,
        Video = 4,
        Game = 8
    }

    public enum PersonStatus : byte
    {
        Active = 1,
        Inactive = 2,
        Retired = 3,
        Deceased = 4
    }
}