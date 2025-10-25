public class AllocationSummaryVm
{
    public List<StudentAllocationVm> StudentAllocations { get; set; } = new();
    public List<ProjectAllocationVm> ProjectAllocations { get; set; } = new();
}

public class StudentAllocationVm
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = "";
    public string? CurrentProject { get; set; }
    public string AllocationStatus { get; set; } = "";
}

public class ProjectAllocationVm
{
    public string ProjectId { get; set; } = "";
    public string Title { get; set; } = "";
    public string GroupId { get; set; } = "";
    public string Supervisor { get; set; } = "";
    public string Assessor { get; set; } = "";
    public List<string> Students { get; set; } = new();
    public string Status { get; set; } = "";
}
