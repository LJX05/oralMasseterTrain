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

    public class SimpleResult
    {
        public string message { get; set; } = string.Empty;

        public string code { get; set; } = string.Empty;

        public object data { get; set; } = string.Empty;
    }

    public class PageQuery
    {
        public string search { get; set; } = string.Empty;
        
        public int pageIndex { get; set; } = 1;
        
        public int pageSize { get; set; } = 15;
    }
    public class PageResult
    {
        public IEnumerable list { get; set; } = Enumerable.Empty<object>();

        public int count { get; set; } = 15;

    }
}