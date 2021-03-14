using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Dto
{
    public class PagingDto<T> where T : class
    {
        public IEnumerable<T> Data { get; set; }
        public int CurrentPage { get; set; }
        public long TotalRowCount { get; set; }
        public int TotalPages { get; set; }
    }
}
