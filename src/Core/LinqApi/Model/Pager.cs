namespace LinqApi.Model
{
    public class Pager
    {
        private int _pageNumber = 1;
        private int _pageSize = 50;

        public int PageNumber
        {
            get => _pageNumber;
            set
            {
                if (value < 1)
                    _pageNumber = 1;
                else if (value > int.MaxValue)
                    _pageNumber = int.MaxValue;
                else
                    _pageNumber = value;
            }
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (value < 1)
                    _pageSize = 1;
                else if (value > 500)
                    _pageSize = 500;
                else
                    _pageSize = value;
            }
        }
    }
}
