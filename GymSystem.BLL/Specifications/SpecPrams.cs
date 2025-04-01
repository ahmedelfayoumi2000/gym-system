using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications
{
    public class SpecPrams
    {
        private const int MaxPageSize = 50;
        public int PageIndex { get; set; } = 1;

        private int pageSize = 25;
        public int PageSize
        {
            get => pageSize;
            set => pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public string? UserCode { get; set; }
        public string? Sort { get; set; }

        private string? search;
        public string? Search
        {
            get => search;
            set => search = value?.ToLower();
        }
    }
}
