using System;
using System.Collections.Generic;
using System.Text;

namespace Zema.DTO
{
    public class GetByPageResponse<TItem>
    {
        public IEnumerable<TItem> List { get; set; }
        public int Total { get; set; }
    }
}