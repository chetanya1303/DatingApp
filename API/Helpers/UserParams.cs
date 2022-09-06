using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class UserParams :PaginationParams
    {
        public string CurrentUsername { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;

        public int MinAge {get; set;} = 18;

        public int MaxAge {get; set;} = 110;

        public string OrderBy { get; set; } = "lastActive";
    }
}