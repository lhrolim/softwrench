namespace softWrench.sW4.Data.Pagination
{
    public class PageToShow
    {
        private readonly bool _active;
        private readonly int _pageNumber;

        public PageToShow(bool active, int pageNumber)
        {
            _active = active;
            _pageNumber = pageNumber;
        }

        public bool Active
        {
            get { return _active; }
        }

        public int PageNumber
        {
            get { return _pageNumber; }
        }
    }
}
