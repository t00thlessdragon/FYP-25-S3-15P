using System;
using System.Collections.Generic;

namespace FYP_25_S3_15P.ViewModels
{
    public class FYPTemplateMasterVm
    {
        public IEnumerable<Row> Template { get; set; } = Array.Empty<Row>();

        public class Row
        {
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
            public string UniName { get; set; } = "";
            public string ProgramCode { get; set; } = "";
        }
    }
}