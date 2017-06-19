using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediaGraph.Models.Util
{
    public enum RelationshipType
    {
        Cast_In = 0,
        Company = 1,
        Series = 2,
        Franchise = 3,
        Adaptation_Of = 4,
        Multiple = 5
    }
}