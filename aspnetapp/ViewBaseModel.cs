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
        /// <summary>
        /// 大于0为成功
        /// </summary>
        public int code { get; set; }

        public object data { get; set; } = string.Empty;
    }
    public class SimpleItem
    {

        /// <summary>
        /// 项名
        /// </summary>
        public virtual string text { get; set; }

        /// <summary>
        /// 项值
        /// </summary>
        public virtual string value { get; set; }
        /// <summary>
        /// 项对象
        /// </summary>
        public virtual object tag { get; set; }
    }
    public class PageQuery
    {
        public string search { get; set; } = string.Empty;
        
        public DateTime date1 { get; set; }
        public DateTime date2 { get; set; }

        public int pageIndex { get; set; } = 1;
        
        public int pageSize { get; set; } = 15;
    }
    public class PageResult
    {
        public IEnumerable list { get; set; } = Enumerable.Empty<object>();

        public int count { get; set; } = 15;

    }
}