using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications.ClassWithFiltersSpec
{
    public class PaginatedResult<T>
    {
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public IEnumerable<T> Data { get; set; }


        public PaginatedResult(IEnumerable<T> data, int totalCount, SpecPrams specParams)
        {
            Data = data;
            TotalCount = totalCount;
            PageIndex = specParams.PageIndex;
            PageSize = specParams.PageSize;
        }
    }
}
