using System.Collections.Generic;

namespace FYP_25_S3_15P.ViewModels
{
    public class PagedResult<T>
    {
        public int Total { get; set; }
        public List<T> Items { get; set; } = new();
    }
}
