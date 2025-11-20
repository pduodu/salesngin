namespace salesngin.Models
{
    public class Pager
    {
        //Total no of records
        public int TotalItems { get; private set; }
        //Active page
        public int CurrentPage { get; private set; }
        //Page size
        public int PageSize { get; private set; }
        //Total no of pagers in pager bar
        public int TotalPages { get; private set; }
        //Start page no
        public int StartPage { get; private set; }
        //End page number
        public int EndPage { get; private set; }
        //Search text
        //public int SearchText { get; private set; }

        public Pager()
        { }
        public Pager(int totalItems, int page, int pageSize = 10)
        {
            int totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize);
            int currentPage = page;

            int startPage = currentPage - 5;
            int endPage = currentPage + 4;

            if (startPage <= 0)
            {
                endPage = endPage - (startPage - 1);
                startPage = 1;
            }

            if (endPage > totalPages)
            {
                endPage = totalPages;
                if (endPage > 10)
                {
                    startPage = endPage - 9;
                }
            }

            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages= totalPages;
            StartPage = startPage;
            EndPage = endPage;
        }
    }



}
