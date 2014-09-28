using System;

namespace softWrench.sW4.Data.Pagination
{
    public class PaginationUtils
    {
        public static Tuple<int,int> GetPaginationBounds(int pageNumber, int pageCount)
        {
            int upperLimit = pageNumber <= 6 ? Math.Min(10,pageCount) : Math.Min(pageNumber + 4, pageCount);
            int lowerLimit = pageNumber <= 6 ? 1 : pageNumber - Math.Max(5,10 - (upperLimit - pageNumber) -1);
            return Tuple.Create(lowerLimit, upperLimit);
        }

    }
}
