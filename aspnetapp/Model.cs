using System.Collections;

namespace aspnetapp
{
    public class Counter
    {
        public int id { get; set; }
        public int count { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }

    public class Result
    {
        public string message { get; set; } = string.Empty;

        public string code { get; set; } = string.Empty;

        public object data { get; set; } = string.Empty;
    }

    public class PageQuery
    {
        public string search { get; set; } = string.Empty;

        public int pageSize { get; set; } = 15;

        public int pageNum { get; set; } = 0;
    }
    public class PageResult
    {
        public IEnumerable data { get; set; } = Enumerable.Empty<object>();

        public int pageCount { get; set; } = 15;

         
    }
}