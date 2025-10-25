using System.Collections.Generic;

namespace FYP_25_S3_15P.ViewModels
{
    public class CatalogVm
    {
        public List<ProgramVm> Programs { get; set; } = new();
        public List<CourseVm>  Courses  { get; set; } = new();
        public List<ModuleVm>  Modules  { get; set; } = new();
    }

    public class ProgramVm
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class CourseVm
    {
        public string Code        { get; set; } = "";
        public string Name        { get; set; } = "";
        public string ProgramCode { get; set; } = "";
        public int    CourseId    { get; set; }
    }

    public class ModuleVm
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";

        public string? CourseCode { get; set; } = "";

        public int ModuleId { get; set; }
        public List<string> CourseCodes { get; set; } = new();
    }
}
