using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class UserParams
    {
        private const int MaxPageSize = 50;

        public int PageNumber {get; set;} = 1;
        private int _pageSize = 10;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        public string CurrentUsername { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;

        public int MinAge {get; set;} = 18;

        public int MaxAge {get; set;} = 110;

        public string OrderBy { get; set; } = "lastActive";
    }
}