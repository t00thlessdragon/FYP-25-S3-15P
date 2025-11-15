// Services/Allocation/IAllocationEngine.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using FYP_25_S3_15P.Controllers;
using FYP_25_S3_15P.Models;

namespace FYP_25_S3_15P.Services.Allocation
{
    public interface IAllocationEngine
    {
        Task<AllocationRunResult> RunAsync(AllocationParameters p);
    }
    public class AllocationResultDto
    {
        public string StudentId { get; set; } = "";
        public string ProjectId { get; set; } = "";
        public int PreferenceScore { get; set; }
    }

    public sealed record AllocationParameters(
        int ProgramConstraintID,
        int MaxGroupSize,
        int MinGroupSize,
        int SupervisorLoadCap,
        int AssessorLoadCap,
        int PreferenceLimit,
        int WModMatchSup, int WModMatchAss,
        // role ids (configurable)
        int RoleIdStudent,
        int RoleIdSupervisor,
        int RoleIdAssessor,
        int Year,
        int SessionNo,
        List<PreferenceWeightDto> PreferenceWeightDtos
    );

    public sealed class AllocationRunResult
    {
        public int RunId { get; set; }
        public List<StudentAssignment> StudentAssignments { get; } = new();
        public List<StaffAssignment> StaffAssignments { get; } = new();
        public List<Conflict> Conflicts { get; } = new();
        public double AvgPreferenceScore { get; set; }
        public int UnassignedStudents { get; set; }
        public bool Success { get; set; }
         public List<AllocationResultDto> Allocations { get; set; } = new();
    }

    public sealed record StudentAssignment(int StudentId, int GroupId, int TopicId, int PreferenceRank, double Score);
    public sealed record StaffAssignment(int StaffId, int GroupId, string Role, double Score, int MatchedModules);
    public sealed record Conflict(string Type, string Detail);
}
