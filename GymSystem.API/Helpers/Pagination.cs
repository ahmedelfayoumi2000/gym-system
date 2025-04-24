namespace GymSystem.API.Helpers
{
    public class Pagination<T>
    {

        public Pagination(int pageIndex, int pageSize, int count, IEnumerable<T> data)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = count;
            Data = data;
        }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IEnumerable<T> Data { get; set; }

    }
}
