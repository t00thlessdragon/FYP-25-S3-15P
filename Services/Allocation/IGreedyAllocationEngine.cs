// Services/Allocation/GreedyAllocationEngine.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;
using FYP_25_S3_15P.Controllers;

namespace FYP_25_S3_15P.Services.Allocation
{
    public sealed class GreedyAllocationEngine : IAllocationEngine
    {
        private readonly SmartDbContext _db;
        private readonly Random _rng = new();

        public GreedyAllocationEngine(SmartDbContext db) => _db = db;

        public async Task<AllocationRunResult> RunAsync(AllocationParameters ap)
        {
            
            Console.WriteLine("=== GreedyAllocationEngine Test Run ===");
            Console.WriteLine($"ProgramConstraintID: {ap.ProgramConstraintID}");
            Console.WriteLine($"Max Group Size: {ap.MaxGroupSize}");
            Console.WriteLine($"Min Group Size: {ap.MinGroupSize}");
            Console.WriteLine($"SupervisorLoadCap: {ap.SupervisorLoadCap}");
            Console.WriteLine($"AssessorLoadCap: {ap.AssessorLoadCap}");
            Console.WriteLine($"Weight Module Match Supervisor: {ap.WModMatchSup}");
            Console.WriteLine($"Weight Module Match Accessor: {ap.WModMatchAss}");
            Console.WriteLine($"Sudent Role Id: {ap.RoleIdStudent}");
            Console.WriteLine($"Supervisor Role Id: {ap.RoleIdSupervisor}");
            Console.WriteLine($"Assessor Role Id: {ap.RoleIdAssessor}");
            Console.WriteLine($"Year: {ap.Year}");
            Console.WriteLine($"Session: {ap.SessionNo}");
            Console.WriteLine($"Preference Weight: {ap.PreferenceWeightDtos}");

                var result = new AllocationRunResult();

    // 1️⃣ Create allocation run record
    var run = new AllocationRun
    {
        ProgramConstraintID = ap.ProgramConstraintID,
        StartedAt = DateTime.UtcNow,
        Strategy = "GreedyWeighted",
        Status = "Running",
        Notes = "Auto allocation run"
    };
    _db.AllocationRuns.Add(run);
    await _db.SaveChangesAsync();

    try
    {
        // 2️⃣ Retrieve students for this session/year
        var students = await _db.StudentProfiles
            .Where(sp => sp.YearOfStudy == ap.Year && sp.SessionID == ap.SessionNo)
            .Select(sp => new
            {
                sp.UserID,
                sp.StudentID,
                Preferences = _db.StudentPreferences
                    .Where(pref => pref.StudentId == sp.UserID)
                    .OrderBy(pref => pref.Rank)
                    .Select(pref => new { pref.ProjectId, pref.Rank })
                    .ToList()
            })
            .ToListAsync();

        if (students.Count == 0)
        {
            result.Conflicts.Add(new("Students", "No students found for this session/year."));
            run.Status = "Failed";
            await _db.SaveChangesAsync();
            return result;
        }

        // 3️⃣ Retrieve all project topics for this session
        var topics = await _db.Projects
            .Where(p => p.Year == ap.Year && p.SessionNo == ap.SessionNo)
            .Select(p => new { p.ProjectId, p.Title })
            .ToListAsync();

        if (topics.Count == 0)
        {
            result.Conflicts.Add(new("Projects", "No project topics found."));
            run.Status = "Failed";
            await _db.SaveChangesAsync();
            return result;
        }

        // 4️⃣ Retrieve supervisors and assessors based on Role IDs
        var supervisors = await _db.Users
            .Where(u => u.RoleId == ap.RoleIdSupervisor)
            .Select(u => new { u.Id, u.Name })
            .ToListAsync();

        var assessors = await _db.Users
            .Where(u => u.RoleId == ap.RoleIdAssessor)
            .Select(u => new { u.Id, u.Name })
            .ToListAsync();

        if (!supervisors.Any() || !assessors.Any())
        {
            result.Conflicts.Add(new("Staff", "Supervisors or Assessors not found."));
            run.Status = "Failed";
            await _db.SaveChangesAsync();
            return result;
        }

        // 5️⃣ Build lookup tables
        var weightLookup = ap.PreferenceWeightDtos.ToDictionary(w => w.RankNo, w => w.WeightValue);
        var supervisorLoad = supervisors.ToDictionary(s => s.Id, _ => 0);
        var assessorLoad = assessors.ToDictionary(a => a.Id, _ => 0);

        var totalScore = 0;
        var placedStudents = 0;

        // For each project, pre-load its existing groups
        var projectGroups = topics.ToDictionary(t => t.ProjectId, t =>
            _db.Groups.Where(g => g.ProjectId == t.ProjectId).ToList());

        // 6️⃣ Start assigning students greedily
        foreach (var student in students)
        {
            bool assigned = false;

            foreach (var pref in student.Preferences)
            {
                if (!projectGroups.ContainsKey(pref.ProjectId))
                    continue;

                // Find an available group for this project
                var groups = projectGroups[pref.ProjectId];
                Group? targetGroup = groups
                    .FirstOrDefault(g => _db.AllocationRunDetails
                        .Count(d => d.GroupId == g.Id && d.RoleId == ap.RoleIdStudent && d.RunId == run.Id) < ap.MaxGroupSize);

                // Create new group if needed (up to 7)
                if (targetGroup == null && groups.Count < 7)
                {
                    targetGroup = new Group
                    {
                        GroupName = $"{pref.ProjectId}-G{groups.Count + 1}",
                        ProjectId = pref.ProjectId,
                        IsFullTime = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = 1
                    };
                    _db.Groups.Add(targetGroup);
                    await _db.SaveChangesAsync();
                    groups.Add(targetGroup);
                }

                // Skip if all 7 groups are full
                if (targetGroup == null)
                    continue;

                // Assign student to this group
                var score = weightLookup.TryGetValue(pref.Rank, out var s) ? s : 0;
                totalScore += score;
                placedStudents++;

                _db.AllocationRunDetails.Add(new AllocationRunDetail
                {
                    RunId = run.Id,
                    UserId = student.UserID,
                    RoleId = ap.RoleIdStudent,
                    GroupId = targetGroup.Id,
                    ProjectId = pref.ProjectId,
                    Score = score,
                    PreferenceRank = pref.Rank,
                    MatchedModules = 0,
                    CreatedAt = DateTime.UtcNow
                });

                assigned = true;
                break;
            }

            // Fallback: assign to least-filled project
            if (!assigned)
            {
                var leastProject = projectGroups.OrderBy(pg =>
                        pg.Value.Sum(g => _db.AllocationRunDetails
                            .Count(d => d.GroupId == g.Id && d.RoleId == ap.RoleIdStudent && d.RunId == run.Id)))
                    .First().Key;

                var groups = projectGroups[leastProject];
                Group? targetGroup = groups
                    .FirstOrDefault(g => _db.AllocationRunDetails
                        .Count(d => d.GroupId == g.Id && d.RoleId == ap.RoleIdStudent && d.RunId == run.Id) < ap.MaxGroupSize);

                if (targetGroup == null && groups.Count < 7)
                {
                    targetGroup = new Group
                    {
                        GroupName = $"{leastProject}-G{groups.Count + 1}",
                        ProjectId = leastProject,
                        IsFullTime = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = 1
                    };
                    _db.Groups.Add(targetGroup);
                    await _db.SaveChangesAsync();
                    groups.Add(targetGroup);
                }

                if (targetGroup != null)
                {
                    _db.AllocationRunDetails.Add(new AllocationRunDetail
                    {
                        RunId = run.Id,
                        UserId = student.UserID,
                        RoleId = ap.RoleIdStudent,
                        GroupId = targetGroup.Id,
                        ProjectId = leastProject,
                        Score = 0,
                        PreferenceRank = 0,
                        MatchedModules = 0,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                placedStudents++;
            }

            await _db.SaveChangesAsync();
        }

        // 7️⃣ Assign supervisors and assessors
        var allGroups = await _db.Groups
            .Where(g => topics.Select(t => t.ProjectId).Contains(g.ProjectId))
            .ToListAsync();

        foreach (var group in allGroups)
        {
            // Assign supervisor with lowest current load (under cap)
            var availableSup = supervisorLoad
                .Where(s => s.Value < ap.SupervisorLoadCap)
                .OrderBy(s => s.Value)
                .FirstOrDefault();

            if (availableSup.Key != 0)
            {
                supervisorLoad[availableSup.Key]++;
                _db.AllocationRunDetails.Add(new AllocationRunDetail
                {
                    RunId = run.Id,
                    UserId = availableSup.Key,
                    RoleId = ap.RoleIdSupervisor,
                    GroupId = group.Id,
                    ProjectId = group.ProjectId,
                    Score = 0,
                    PreferenceRank = 0,
                    MatchedModules = 0,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Assign assessor with lowest current load (under cap)
            var availableAss = assessorLoad
                .Where(a => a.Value < ap.AssessorLoadCap)
                .OrderBy(a => a.Value)
                .FirstOrDefault();

            if (availableAss.Key != 0)
            {
                assessorLoad[availableAss.Key]++;
                _db.AllocationRunDetails.Add(new AllocationRunDetail
                {
                    RunId = run.Id,
                    UserId = availableAss.Key,
                    RoleId = ap.RoleIdAssessor,
                    GroupId = group.Id,
                    ProjectId = group.ProjectId,
                    Score = 0,
                    PreferenceRank = 0,
                    MatchedModules = 0,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _db.SaveChangesAsync();

        // 8️⃣ Update run metrics
        run.FinishedAt = DateTime.UtcNow;
        run.Status = "Completed";
        run.AvgPreferenceScore = placedStudents > 0 ? (double)totalScore / placedStudents : 0;
        run.AvgSupervisorLoad = supervisorLoad.Values.Average();
        run.AvgAssessorLoad = assessorLoad.Values.Average();
        run.UnassignedStudents = students.Count - placedStudents;

        await _db.SaveChangesAsync();

        result.Success = true;
        result.Allocations = new List<AllocationResultDto>();
    }
    catch (Exception ex)
    {
        run.Status = "Error";
        run.Notes = ex.Message;
        await _db.SaveChangesAsync();
        result.Conflicts.Add(new("Exception", ex.Message));
    }

    return result;
        }
    }
}
