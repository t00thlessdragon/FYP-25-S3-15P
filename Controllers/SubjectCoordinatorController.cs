// Controllers/SubjectCoordinatorController.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using FYP_25_S3_15P.Data;       // SmartDbContext
using FYP_25_S3_15P.Models;
using FYP_25_S3_15P.ViewModels;
using FYP_25_S3_15P.Services.Allocation;
namespace FYP_25_S3_15P.Controllers
{


    // public class SaveCourseConstraintDto
    // {
    //     public string CourseCode { get; set; } = "";
    //     public int Year { get; set; }
    //     public int SessionNo { get; set; }
    //     public bool IsOverride { get; set; }
    //     public int SLoadCap { get; set; }
    //     public int ALoadCap { get; set; }
    //     public int PrefRankLimit { get; set; }
    //     public int SMin { get; set; }
    //     public int SMax { get; set; }
    // }
    
    public class SaveProgramConstraintDto
    {
        public int programId { get; set; }
        public int Year { get; set; }
        public int SessionNo { get; set; }
        public bool IsOverride { get; set; }
        public int SLoadCap { get; set; }
        public int ALoadCap { get; set; }
        public int PrefRankLimit { get; set; }
        public int SMin { get; set; }
        public int SMax { get; set; }

        public List<PreferenceWeightDto>? Weights { get; set; }
    }
    public class PreferenceWeightDto
    {
        public int RankNo { get; set; }
        public int WeightValue { get; set; }
    }

    public class DeleteConstraintsListDto
    {
        public List<DeleteConstraintsDto> deleteConstraintsDtos { get; set; } = new();
    }

    public class DeleteConstraintsDto
    {
        public int Year { get; set; }
        public string? ProgramCode { get; set; }
        public int? SessionNo { get; set; }
    }


    public class CreateProjectsRequest
    {
        public string Programme { get; set; } = "";
        public string Course { get; set; } = "";
        public int Year { get; set; }
        public int Session { get; set; }        // 1..3
        public List<string> TemplateIds { get; set; } = new();
        public string? PrefCloseAt { get; set; }
    }

    public class EditProjectDto
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Programme { get; set; } = "";
        public List<string> Modules { get; set; } = new();
        public string Desc { get; set; } = "";
    }

    public class PublishProjectsRequest
    {
        public List<string> Ids { get; set; } = new();
    }

    public class SubjectCoordinatorController : Controller
    {
        private readonly SmartDbContext _db;
        private readonly IAllocationEngine _engine;
        public SubjectCoordinatorController(SmartDbContext db, IAllocationEngine engine) {
        _db = db;
        _engine = engine;
        }

        [HttpGet]
        public IActionResult GroupDetails(string groupId)
        {
            var vm = new ScGroupDetailsVm
            {
                GroupId = groupId ?? "FYP-25-S3-15P",
                ProjectTitle = "A Smart Project Allocation System",
                Modules = new[] { "AI301 Machine Learning", "DATA201 Data Mining" },
                Students = new()
                {
                    new("123456","Tan","sdf@gmail.com"),
                    new("324562","Tan","eew@gmail.com"),
                    new("345678","Tan","erw@gmail.com"),
                },
                Supervisor = new ScPerson("Mr Prem", "aaa@gmail.com"),
                Assessor   = new ScPerson("Mr Tian", "sss@gmail.com"),
            };
            return View(vm);
        }

        [HttpGet]
        public IActionResult GroupList(string projectId = "CSIT-25-S1-01")
        {
            var groups = new[]
            {
                new { GroupId = "G-01", Members = 3, Supervisor = "Dr. Kuan", Assessor = "Mr Tian" },
                new { GroupId = "G-02", Members = 5, Supervisor = "Mr Tan",  Assessor = "Mr Prem" }
            };

            ViewBag.ProjectId = projectId;
            return View(groups);
        }

        // ===== Manage page =====
        [HttpGet]
        public async Task<IActionResult> Projects(string tab = "constraints")
        {
            var uniId = await _db.Universities
                                 .Select(u => u.UniID)
                                 .FirstAsync();

            var curYear = DateTime.UtcNow.Year;
            var year = DateTime.UtcNow.Year;
            //Get all year selection base on university constaint
            var years = await _db.UniversityConstraints
                                 .Select(u => u.Year)
                                 .ToListAsync();

            // list rows from CourseConstraints (joined to Course)
            // var rows = await _db.CourseConstraints
            //     .Where(cc => cc.UniID == uniId && cc.Year == year)
            //     .Join(_db.Courses,
            //           cc => cc.CourseId,
            //           c  => c.ID,
            //           (cc, c) => new ScConstraintRowVm
            //           {
            //               Code    = c.CourseCode,
            //               Name    = c.CourseName,
            //               Session = $"S1–S{cc.SessionNo}",
            //               Status  = cc.IsOverride ? "Override" : "Default",
            //               Updated = cc.UpdatedAt ?? cc.CreatedAt
            //           })
            //     .OrderBy(r => r.Code)
            //     .ToListAsync();

            // list rows from ProgramConstraint (joined to Program)
            var rows = await _db.ProgramConstraints
                .Where(cc => cc.UniID == uniId && cc.Year >= curYear)
                .Join(_db.Programs,
                      cc => cc.ProgramId,
                      c => c.ID,
                      (cc, c) => new ScConstraintRowVm
                      {
                          Code = c.ProgramCode,
                          Name = c.ProgramName,
                          Session = cc.SessionNo,
                          Status = cc.IsOverride ? "Override" : "Default",
                          Year = cc.Year,
                          Updated = cc.UpdatedAt ?? cc.CreatedAt
                      })
                .OrderBy(r => r.Code)
                .ToListAsync();
                
            var projectRows = await _db.Projects
            .OrderBy(p => p.ProjectId)
            .Select(p => new ScProjectsPageVm.ProjectRow
            {
                Id          = p.ProjectId,
                Title       = p.Title,
                Programme   = p.ProgramCode,
                Description = p.Description ?? "",
                IsPublish = p.IsPublish
                // Modules     = _db.ProjectModules
                //             .Where(pm => pm.ProjectId == p.ProjectId)
                //             .Join(_db.Modules, pm => pm.ModuleCode, m => m.ModuleCode, (pm, m) => m.ModuleName)
                //             .ToList(),
                // ModuleCodes = _db.ProjectModules
                //             .Where(pm => pm.ProjectId == p.ProjectId)
                //             .Select(pm => pm.ModuleCode)
                //             .ToList()
            })
            .ToListAsync();

                var vm = new ScProjectsPageVm
                {
                    ActiveTab = tab,
                    curYear = curYear,
                    Constraints = rows,
                    Projects = projectRows,
                    years = years
                };

                return View(vm);
        }

[HttpPost]
[IgnoreAntiforgeryToken] 
public async Task<IActionResult> DeleteProjects([FromBody] DeleteProjectsReq req)
{
    if (req?.Ids == null || req.Ids.Count == 0)
        return BadRequest("No ids.");

    // normalize & distinct
    var ids = req.Ids
                 .Where(s => !string.IsNullOrWhiteSpace(s))
                 .Select(s => s.Trim())
                 .Distinct(StringComparer.OrdinalIgnoreCase)
                 .ToList();

    // fetch projects (include children)
    var projects = await _db.Projects
                            .Include(p => p.ProjectModules)
                            .Where(p => ids.Contains(p.ProjectId))
                            .ToListAsync();

    if (projects.Count == 0)
        return NotFound("No matching projects.");

    var links = projects.SelectMany(p => p.ProjectModules).ToList();
    if (links.Count > 0)
        _db.ProjectModules.RemoveRange(links);

    _db.Projects.RemoveRange(projects);

    await _db.SaveChangesAsync();
    return Ok(new { deleted = projects.Count });
}

public class DeleteProjectsReq
{
    public List<string> Ids { get; set; } = new();
}



[HttpGet]
public IActionResult GetAllModules()
{
    var rows = _db.Set<Module>()
                  .OrderBy(m => m.ModuleCode)
                  .Select(m => new { code = m.ModuleCode, name = m.ModuleName })
                  .ToList();
    return Json(rows);
}


[HttpGet]
public IActionResult GetConstraintScaffold(int year)
{
    var uc = _db.UniversityConstraints.FirstOrDefault(x => x.Year == year);
    if (uc == null) return NotFound();

    return Json(new
    {
        sessionNo     = uc.SessionNo,
        sLoadCap      = uc.SLoadCap,
        aLoadCap      = uc.ALoadCap,
        prefRankLimit = uc.PrefRankLimit,
        sMin          = uc.SMin,
        sMax          = uc.SMax
    });
}

    [HttpGet]
    public IActionResult GetCourses()
    {
        var rows = _db.Courses
                        .OrderBy(c => c.CourseCode)
                        .Select(c => new { code = c.CourseCode, name = c.CourseName })
                        .ToList();
        return Json(rows);
    }

    [HttpGet]
    public IActionResult GetProgram()
    {
        var rows = _db.Programs
                    .OrderBy(c => c.ProgramCode)
                    .Select(c => new { id = c.ID,code = c.ProgramCode, name = c.ProgramName })
                    .ToList();
        return Json(rows);
    }
    [HttpGet]
    public async Task<IActionResult> GetAvailableYears()
    {
        var years = await _db.UniversityConstraints
            .Select(u => u.Year)
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync();

        return Json(years);
    }




        // Create / Update a per-course constraint row
        // [HttpPost]
        // public async Task<IActionResult> SaveCourseConstraint([FromBody] SaveCourseConstraintDto dto)
        // {
        //     if (dto == null) return BadRequest("Invalid payload.");

        //     var uniId = await _db.Universities.Select(u => u.UniID).FirstAsync();

        //     var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseCode == dto.CourseCode);
        //     if (course == null) return BadRequest("Course not found.");

        //     var cc = await _db.CourseConstraints
        //         .FirstOrDefaultAsync(x => x.UniID == uniId && x.Year == dto.Year && x.CourseId == course.ID);

        //     if (cc == null)
        //     {
        //         cc = new CourseConstraint
        //         {
        //             UniID = uniId,
        //             Year = dto.Year,
        //             CourseId = course.ID,
        //             SessionNo = dto.SessionNo,
        //             SLoadCap = dto.SLoadCap,
        //             ALoadCap = dto.ALoadCap,
        //             PrefRankLimit = dto.PrefRankLimit,
        //             SMin = dto.SMin,
        //             SMax = dto.SMax,
        //             IsOverride = dto.IsOverride,
        //             CreatedAt = DateTime.UtcNow
        //         };
        //         _db.CourseConstraints.Add(cc);
        //     }
        //     else
        //     {
        //         cc.SessionNo = dto.SessionNo;
        //         cc.SLoadCap = dto.SLoadCap;
        //         cc.ALoadCap = dto.ALoadCap;
        //         cc.PrefRankLimit = dto.PrefRankLimit;
        //         cc.SMin = dto.SMin;
        //         cc.SMax = dto.SMax;
        //         cc.IsOverride = dto.IsOverride;
        //         cc.UpdatedAt = DateTime.UtcNow;
        //     }

        //     await _db.SaveChangesAsync();
        //     return Ok(new { ok = true });
        // }
        
        // Create / Update a per-program constraint row
        [HttpPost]
        public async Task<IActionResult> SaveProgramConstraint([FromBody] SaveProgramConstraintDto dto)
        {
            if (dto == null) return BadRequest("Invalid payload.");

            var uniId = await _db.Universities.Select(u => u.UniID).FirstAsync();

            var program = await _db.Programs.FirstOrDefaultAsync(c => c.ID == dto.programId);
            if (program == null) return BadRequest("Program not found.");

            var pc = await _db.ProgramConstraints
                .FirstOrDefaultAsync(x => x.UniID == uniId && x.Year == dto.Year && x.ProgramId == program.ID);

            if (pc == null)
            {
                pc = new ProgramConstraint
                {
                    UniID        = uniId,
                    Year         = dto.Year,
                    ProgramId    = program.ID,
                    SessionNo    = dto.SessionNo,
                    SLoadCap     = dto.SLoadCap,
                    ALoadCap     = dto.ALoadCap,
                    PrefRankLimit= dto.PrefRankLimit,
                    SMin         = dto.SMin,
                    SMax         = dto.SMax,
                    IsOverride   = dto.IsOverride,
                    CreatedAt    = DateTime.UtcNow
                };
                _db.ProgramConstraints.Add(pc);
            }
            else
            {
                pc.SessionNo     = dto.SessionNo;
                pc.SLoadCap      = dto.SLoadCap;
                pc.ALoadCap      = dto.ALoadCap;
                pc.PrefRankLimit = dto.PrefRankLimit;
                pc.SMin          = dto.SMin;
                pc.SMax          = dto.SMax;
                pc.IsOverride    = dto.IsOverride;
                pc.UpdatedAt     = DateTime.UtcNow;
            }

            // Save the ProgramConstraint first to get pc.Id (for FK usage)
            await _db.SaveChangesAsync();

            // ✅ Step 2: Check if PreferenceWeights exist for this ProgramConstraint
            var existingWeights = await _db.PreferenceWeights
                .Where(w => w.ProgramConstraintID == pc.Id)
                .ToListAsync();

            if (existingWeights.Any())
            {
                // --- Update existing weights ---
                foreach (var w in dto.Weights)
                {
                    var match = existingWeights.FirstOrDefault(x => x.RankNo == w.RankNo);
                    if (match != null)
                    {
                        match.WeightValue = w.WeightValue;
                        match.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // New rank (added beyond previous limit)
                        _db.PreferenceWeights.Add(new PreferenceWeight
                        {
                            ProgramConstraintID = pc.Id,
                            RankNo = w.RankNo,
                            WeightValue = w.WeightValue,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
            else
            {
                // --- Create new weights for this ProgramConstraint ---
                if (dto.Weights != null && dto.Weights.Count > 0)
                {
                    var newWeights = dto.Weights.Select(w => new PreferenceWeight
                    {
                        ProgramConstraintID = pc.Id,
                        RankNo = w.RankNo,
                        WeightValue = w.WeightValue,
                        CreatedAt = DateTime.UtcNow
                    });
                    _db.PreferenceWeights.AddRange(newWeights);
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { ok = true });
        }


// [HttpGet]
// public async Task<IActionResult> GetConstraintDetails(string courseCode, int year)
// {
//     if (string.IsNullOrWhiteSpace(courseCode))
//         return BadRequest("courseCode is required.");

//     // Your single-university lookup (same as elsewhere)
//     var uniId = await _db.Universities.Select(u => u.UniID).FirstAsync();

//     // Find the course by code
//     var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
//     if (course == null) return NotFound("Course not found.");

//     // Find the constraint row for that course & year
//     var cc = await _db.CourseConstraints
//                       .FirstOrDefaultAsync(x => x.UniID == uniId
//                                              && x.Year == year
//                                              && x.CourseId == course.ID);

//     if (cc == null)
//         return NotFound("No constraints found for that course/year.");

//     return Json(new
//     {
//         courseName = course.CourseName,
//         sessionNo = cc.SessionNo,
//         status    = cc.IsOverride ? "override" : "default",
//         sup       = cc.SLoadCap,
//         ass       = cc.ALoadCap,
//         rank      = cc.PrefRankLimit,
//         teamMin   = cc.SMin,
//         teamMax   = cc.SMax
//     });
// }
[HttpGet]
public async Task<IActionResult> GetConstraintDetails(string programCode, int year)
{
    if (string.IsNullOrWhiteSpace(programCode))
        return BadRequest("programCode is required.");

    // Your single-university lookup (same as elsewhere)
    var uniId = await _db.Universities.Select(u => u.UniID).FirstAsync();

    // Find the Program by code
    var program = await _db.Programs.FirstOrDefaultAsync(c => c.ProgramCode == programCode);
    if (program == null) return NotFound("Program not found.");

    // Find the constraint row for that course & year
    var cc = await _db.ProgramConstraints
                      .FirstOrDefaultAsync(x => x.UniID == uniId
                                             && x.Year == year
                                             && x.ProgramId == program.ID);

    if (cc == null)
        return NotFound("No constraints found for that program/year.");

    return Json(new
    {
        programName = program.ProgramName,
        sessionNo = cc.SessionNo,
        status    = cc.IsOverride ? "override" : "default",
        sup       = cc.SLoadCap,
        ass       = cc.ALoadCap,
        rank      = cc.PrefRankLimit,
        teamMin   = cc.SMin,
        teamMax   = cc.SMax
    });
}

// [HttpPost]
// [IgnoreAntiforgeryToken]
// public async Task<IActionResult> DeleteProgramConstraints([FromBody] DeleteConstraintsListDto dto)
// {
//     if (dto == null || dto.Year <= 0 || dto.ProgramCodes == null || dto.ProgramCodes.Count == 0)
//         return BadRequest("Invalid payload.");

//     // normalize codes
//     var codes = dto.ProgramCodes
//         .Where(c => !string.IsNullOrWhiteSpace(c))
//         .Select(c => c.Trim().ToUpperInvariant())
//         .Distinct()
//         .ToList();

//     try
//     {
//         // scope to current university (same pattern you used elsewhere)
//         var uniId = await _db.Universities.Select(u => u.UniID).FirstAsync();

//         // translate ProgramCode -> ProgramId
//         var programIds = await _db.Programs
//             .Where(p => codes.Contains(p.ProgramCode.ToUpper()))
//             .Select(p => p.ID)
//             .ToListAsync();

//         if (programIds.Count == 0)
//             return NotFound("No matching courses.");

//         // build delete query on CourseConstraints
//         var q = _db.ProgramConstraints
//                    .Where(x => x.UniID == uniId
//                             && x.Year == dto.Year
//                             && programIds.Contains(x.ProgramId));

//         if (dto.SessionNo.HasValue)
//             q = q.Where(x => x.SessionNo == dto.SessionNo.Value);

//         var rows = await q.ToListAsync();
//         if (rows.Count == 0)
//             return NotFound("No matching constraints.");

//         _db.ProgramConstraints.RemoveRange(rows);
//         await _db.SaveChangesAsync();

//         return Ok(new { deleted = rows.Count });
//     }
//     catch (DbUpdateException ex)
//     {
//         return StatusCode(500, $"Cannot delete because of related data: {ex.GetBaseException().Message}");
//     }
//     catch (Exception ex)
//     {
//         return StatusCode(500, ex.GetBaseException().Message);
//     }
// }
[HttpPost]
public async Task<IActionResult> DeleteProgramConstraints([FromBody] DeleteConstraintsListDto dto)
{
    if (dto?.deleteConstraintsDtos == null || !dto.deleteConstraintsDtos.Any())
        return BadRequest("Invalid request payload.");

    try
    {
        // current uni
        var uniId = await _db.Universities.Select(u => u.UniID).FirstAsync();

        // Normalize input
        var normalized = dto.deleteConstraintsDtos
            .Where(i =>
                !string.IsNullOrWhiteSpace(i.ProgramCode) &&
                i.Year > 0 &&
                i.SessionNo > 0)
            .Select(i => new {
                ProgramCode = i.ProgramCode.Trim().ToUpperInvariant(),
                i.Year,
                i.SessionNo
            })
            .ToList();

        if (!normalized.Any())
            return BadRequest("No valid items found.");

        // Get program info once
        var allProgramCodes = normalized.Select(i => i.ProgramCode).Distinct().ToList();
        var programMap = await _db.Programs
            .Where(p => allProgramCodes.Contains(p.ProgramCode.ToUpper()))
            .ToDictionaryAsync(p => p.ProgramCode.ToUpper(), p => p.ID);

        var toDelete = new List<ProgramConstraint>();

        foreach (var item in normalized)
        {
            if (!programMap.TryGetValue(item.ProgramCode, out var programId))
                continue;

            var pc = await _db.ProgramConstraints.FirstOrDefaultAsync(x =>
                x.ProgramId == programId &&
                x.Year == item.Year &&
                x.SessionNo == item.SessionNo);

            if (pc != null)
                toDelete.Add(pc);
        }

        if (!toDelete.Any())
            return NotFound("No matching constraints found.");

        _db.ProgramConstraints.RemoveRange(toDelete);
        await _db.SaveChangesAsync();

        return Ok(new { deleted = toDelete.Count });
    }
    catch (DbUpdateException ex)
    {
        return StatusCode(500, $"Cannot delete due to linked data: {ex.GetBaseException().Message}");
    }
    catch (Exception ex)
    {
        return StatusCode(500, ex.GetBaseException().Message);
    }
}
// GET: load templates into the modal (filters + pagination)
[HttpGet]
public async Task<IActionResult> GetProjectTemplates(
    string programme = "",
    // string course = "",
    int year = 0,
    int session = 0,
    // string module = "",
    string q = "",
    int page = 1,
    int pageSize = 10)          // keep 10 default
{
    // Only ACTIVE templates
    var query = _db.ProjectTemplates
        .Include(t => t.TemplateModules)
        .Where(t => t.Status == "Active")   // << filter here
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(programme))
        query = query.Where(t => t.ProgramCode == programme);

    if (!string.IsNullOrWhiteSpace(q))
        query = query.Where(t => t.Title.Contains(q));

    // if (!string.IsNullOrWhiteSpace(module))
    //     query = query.Where(t => t.TemplateModules.Any(m => m.ModuleCode == module));

    var total = await query.CountAsync();   // total ACTIVE rows after filters

    var items = await query
        .OrderBy(t => t.TemplateCode)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)                     // << 10 per page
        .Select(t => new ProjectTemplateRowVm
        {
            TemplateId  = t.TemplateCode,
            Title       = t.Title,
            Programme   = t.ProgramCode,
            // Modules     = t.TemplateModules.Select(m => m.ModuleCode).ToArray(),
            Status      = t.Status,
            Description = t.Description
        })
        .ToListAsync();

    return Json(new PagedResult<ProjectTemplateRowVm> { Total = total, Items = items });
}


// SubjectCoordinatorController.cs
[HttpGet]
public IActionResult GetPrograms()
{
    var rows = _db.Set<FYP_25_S3_15P.Models.Program>()
                  .OrderBy(p => p.ProgramCode)
                  .Select(p => new { code = p.ProgramCode, name = p.ProgramName })
                  .ToList();
    return Json(rows);
}


[HttpGet]
public IActionResult GetSessions()
{
    // Get the active year (latest constraint entry)
    var latest = _db.UniversityConstraints
                    .OrderByDescending(c => c.Year)
                    .FirstOrDefault();

    var count = latest?.SessionNo ?? 2; // default to 2 if null

    // Generate S1..S(count)
    var sessions = Enumerable.Range(1, count)
                             .Select(i => new { value = $"S{i}", text = $"S{i}" })
                             .ToList();

    return Json(sessions);
}


[HttpGet]
public IActionResult GetCoursesByProgram(string programCode)
{
    if (string.IsNullOrWhiteSpace(programCode))
        return Json(Array.Empty<object>());

    // find matching Program row first
    var prog = _db.Programs.FirstOrDefault(p => p.ProgramCode == programCode);
    if (prog == null)
        return Json(Array.Empty<object>());

    // fetch all courses under that Program
    var rows = _db.Courses
                  .Where(c => c.ProgramID == prog.ID)
                  .OrderBy(c => c.CourseCode)
                  .Select(c => new
                  {
                      code = c.CourseCode,
                      name = c.CourseName
                  })
                  .ToList();

    return Json(rows);
}

[HttpPost]
[IgnoreAntiforgeryToken]
public async Task<IActionResult> PublishProjects([FromBody] PublishProjectsRequest req)
{
    if (req?.Ids == null || req.Ids.Count == 0)
        return BadRequest("No projects to publish.");

    var defaultCloseAt = DateTime.UtcNow.AddDays(14);

    var updated = await PublishAsync(req.Ids, defaultCloseAt);
    return Json(new { updated });
}

// “Publish” = set PrefCloseAt
private async Task<int> PublishAsync(IEnumerable<string> ids, DateTime? closeAtUtc)
{
    // IMPORTANT: match on ProjectId (not Id)
    var items = await _db.Projects
                         .Where(p => ids.Contains(p.ProjectId))
                         .ToListAsync();

    foreach (var p in items)
    {
        p.PrefCloseAt = closeAtUtc;
        p.IsPublish = true;
    }

    return await _db.SaveChangesAsync();
}



[HttpPost]
[IgnoreAntiforgeryToken]
public async Task<IActionResult> CreateProjectsFromTemplates([FromBody] CreateProjectsRequest req)
{
    if (req.TemplateIds == null || req.TemplateIds.Count == 0)
        return BadRequest("No templates selected.");


    DateTime? prefCloseAt = null;

    if (!string.IsNullOrWhiteSpace(req.PrefCloseAt) &&
        DateTime.TryParse(req.PrefCloseAt, null,
            System.Globalization.DateTimeStyles.AssumeLocal, out var dtLocal))
    {
        prefCloseAt = dtLocal;                   // store as local
        // OR store as UTC:
        // prefCloseAt = dtLocal.ToUniversalTime();
    }


    static int ExtractSeq(string id)
    {
        var last = (id ?? "").Split('-').LastOrDefault();
        return int.TryParse(last, out var n) ? n : 0;
    }
        // All existing IDs for this Course/Year/Session
    var existingIds = await _db.Projects
        .Where(p => p.CourseCode == req.Course && p.Year == req.Year && p.SessionNo == req.Session)
        .Select(p => p.ProjectId)
        .ToListAsync();

    // Build a set of used sequence numbers: 1,2,3,...
    var usedSeqs = existingIds
        .Select(ExtractSeq)
        .Where(n => n > 0)
        .ToHashSet();

    // Return the smallest positive integer not in usedSeqs
    int NextSeq()
    {
        var s = 1;
        while (usedSeqs.Contains(s)) s++;
        usedSeqs.Add(s);  // reserve it for this request
        return s;
    }

    var templates = await _db.ProjectTemplates
        .Include(t => t.TemplateModules)
        .Where(t => req.TemplateIds.Contains(t.TemplateCode))
        .ToListAsync();

    var createdIds = new List<string>();

    foreach (var t in templates.OrderBy(t => t.TemplateCode))
    {
        var seq = NextSeq();
        var pid = MakeProjectId(req.Course, req.Year, req.Session, seq);

        var proj = new Project
        {
            ProjectId   = pid,
            Title       = t.Title,
            ProgramCode = t.ProgramCode,
            CourseCode  = req.Course,
            Year        = req.Year,
            SessionNo   = req.Session,
            Description = t.Description,
            PrefCloseAt = prefCloseAt
        };

        foreach (var tm in t.TemplateModules)
        {
            proj.ProjectModules.Add(new ProjectModule
            {
                ProjectId  = pid,
                ModuleCode = tm.ModuleCode
            });
        }

        _db.Projects.Add(proj);
        createdIds.Add(pid);
    }

    await _db.SaveChangesAsync();
    return Json(new { created = createdIds.Count, projectIds = createdIds });
}

[HttpGet]
public async Task<IActionResult> PreviewProjectIds(string program,int year, int session, int count = 1)
{
    if (year <= 0 || session <= 0 || count <= 0)
        return BadRequest("Missing/invalid course, year, session, or count.");

    // existing IDs for that Course/Year/Session
    var existingIds = await _db.Projects
        .Where(p => p.Year == year && p.SessionNo == session)
        .Select(p => p.ProjectId)
        .ToListAsync();

    // extract used sequences
    static int SeqOf(string id)
    {
        var last = (id ?? "").Split('-').LastOrDefault();
        return int.TryParse(last, out var n) ? n : 0;
    }
    var used = existingIds.Select(SeqOf).Where(n => n > 0).ToHashSet();

    // gap-filling allocator
    int NextSeq()
    {
        int s = 1;
        while (used.Contains(s)) s++;
        used.Add(s);
        return s;
    }

    var ids = new List<string>(capacity: count);
    for (int i = 0; i < count; i++)
    {
        var seq = NextSeq();
        ids.Add(MakeProjectId(program,year, session, seq));
    }

    return Json(new { ids });
}

[HttpPost]
[IgnoreAntiforgeryToken]
public async Task<IActionResult> EditProject([FromBody] EditProjectDto dto)
{
    if (dto == null || string.IsNullOrWhiteSpace(dto.Id))
        return BadRequest("Invalid payload.");

    var proj = await _db.Projects
                        .Include(p => p.ProjectModules)
                        .FirstOrDefaultAsync(p => p.ProjectId == dto.Id);
    if (proj == null) return NotFound("Project not found.");

    // Basic fields
    proj.Title       = dto.Title?.Trim() ?? proj.Title;
    proj.ProgramCode = dto.Programme?.Trim().ToUpperInvariant() ?? proj.ProgramCode;
    proj.Description = dto.Desc ?? "";

    // // MODULES: dto.Modules should be module CODES (e.g., "AI301")
    // var desiredCodes = (dto.Modules ?? new List<string>())
    //                     .Select(s => s.Trim().ToUpperInvariant())
    //                     .Where(s => !string.IsNullOrEmpty(s))
    //                     .Distinct()
    //                     .ToHashSet(StringComparer.OrdinalIgnoreCase);

    // // Validate codes exist
    // var validCodes = await _db.Modules
    //                           .Where(m => desiredCodes.Contains(m.ModuleCode))
    //                           .Select(m => m.ModuleCode)
    //                           .ToListAsync();
    // var validSet = validCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);

    // // Remove links not desired
    // var toRemove = proj.ProjectModules
    //                    .Where(pm => !validSet.Contains(pm.ModuleCode))
    //                    .ToList();
    // if (toRemove.Count > 0) _db.ProjectModules.RemoveRange(toRemove);

    // // Add missing links
    // var existingSet = proj.ProjectModules
    //                       .Select(pm => pm.ModuleCode)
    //                       .ToHashSet(StringComparer.OrdinalIgnoreCase);

    // var toAdd = validSet.Where(code => !existingSet.Contains(code))
    //                     .Select(code => new ProjectModule
    //                     {
    //                         ProjectId  = proj.ProjectId,
    //                         ModuleCode = code
    //                     })
    //                     .ToList();
    // if (toAdd.Count > 0) await _db.ProjectModules.AddRangeAsync(toAdd);

    await _db.SaveChangesAsync();
    return Ok(new { ok = true });
}

        // ===== Optional landing -> Templates =====
        public IActionResult Dashboard() => RedirectToAction(nameof(Templates));

        // ===================== LIST (feeds Templates.cshtml) =====================
        [HttpGet]
        public async Task<IActionResult> Templates(string q = "", string prog = "", string status = "")
        {
            // Programme dropdown (value=code, text="CODE Name")
            ViewBag.ProgramList = await _db.Set<FYP_25_S3_15P.Models.Program>()
                .OrderBy(x => x.ProgramCode)
                .Select(x => new SelectListItem($"{x.ProgramCode} - {x.ProgramName}", x.ProgramCode))
                .ToListAsync();

            // Module dropdown (value=code, text="CODE Name")
            ViewBag.ModuleList = await _db.Set<Module>()
                .OrderBy(x => x.ModuleCode)
                .Select(x => new SelectListItem($"{x.ModuleCode} {x.ModuleName}", x.ModuleCode))
                .ToListAsync();

            var query = _db.ProjectTemplates
                .Include(t => t.TemplateModules)
                .OrderBy(t => t.TemplateCode)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(t => t.TemplateCode.Contains(q) || t.Title.Contains(q));
            if (!string.IsNullOrWhiteSpace(prog))
                query = query.Where(t => t.ProgramCode == prog);

            var statusCanon = CanonicalStatus(status);
            if (!string.IsNullOrEmpty(statusCanon))
                query = query.Where(t => t.Status == statusCanon);

            var rows = await query.Select(t => new TemplateRowVm
            {
                TemplateId = t.TemplateCode,
                Title = t.Title,
                Programme = t.ProgramCode,
                Modules = t.TemplateModules.Select(m => m.ModuleCode).ToList(),
                Status = t.Status,
                Description = t.Description ?? ""
            }).ToListAsync();

            return View(rows);
        }

        // ===================== Sample CSV =====================
        [HttpGet]
        public IActionResult TemplateCsv()
        {
            var sb = new StringBuilder();
            sb.AppendLine("TemplateId,Title,ProgramCode,ModuleCode,Status,Description");
            sb.AppendLine("# Note: Put multiple ModuleCode values in the ModuleCode cell separated by ';' e.g. AI301;DATA201");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "project_templates_template.csv");
        }

        private bool IsAjax()
        {
            var xrw = Request.Headers["X-Requested-With"].ToString();
            var accept = Request.Headers["Accept"].ToString();
            return string.Equals(xrw, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
                || (accept?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? false);
        }

        // ===================== Upload CSV =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadTemplatesCsv(IFormFile file)
        {
            bool allowUpdates = true;
            var errors = new List<string>();
            var warnings = new List<string>();
            int inserted = 0, updated = 0;

            if (file == null || file.Length == 0)
                return BadRequest(new { ok = false, errors = new[] { "No file selected." } });

            var fname = file.FileName ?? "(uploaded file)";

            // --- lookups ---
            var validProgCodes = await _db.Set<FYP_25_S3_15P.Models.Program>()
                                          .Select(p => p.ProgramCode.ToUpper())
                                          .ToListAsync();

            var validModuleSet = new HashSet<string>(
                await _db.Set<Module>().Select(m => m.ModuleCode.ToUpper()).ToListAsync(),
                StringComparer.OrdinalIgnoreCase);

            // preload templates (+ modules for update sync)
            var templates = await _db.ProjectTemplates
                                     .Include(t => t.TemplateModules)
                                     .ToListAsync();

            var byId = templates.ToDictionary(
                t => (t.TemplateCode ?? "").Trim().ToUpperInvariant(),
                t => t,
                StringComparer.OrdinalIgnoreCase);

            var titleOwner = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in templates)
            {
                var key = NormTitle(t.Title);
                if (key.Length > 0)
                    titleOwner[key] = (t.TemplateCode ?? "").Trim().ToUpperInvariant();
            }

            var idFirstLine = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var titleFirstLine = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            using var sr = new StreamReader(file.OpenReadStream(), Encoding.UTF8, true);
            var header = SplitCsvLine(await sr.ReadLineAsync() ?? "");

            if (!HeaderIs(header, "TemplateId", "Title", "ProgramCode", "ModuleCode", "Status", "Description"))
                return BadRequest(new
                {
                    ok = false,
                    errors = new[] { $"{fname}: unknown header. Expected: TemplateId,Title,ProgramCode,ModuleCode,Status,Description" }
                });

            int lineNo = 1;
            await using var tx = await _db.Database.BeginTransactionAsync();

            while (!sr.EndOfStream)
            {
                var raw = await sr.ReadLineAsync();
                lineNo++;
                if (string.IsNullOrWhiteSpace(raw) || IsComment(raw)) continue;

                var cols = SplitCsvLine(raw);
                Require(cols, 6, fname, lineNo, errors);
                if (cols.Length < 6) continue;

                var rowErrSet = new HashSet<string>();

                // ===== Required column validation =====
                var id = MustValue(cols[0], "TemplateId", fname, lineNo, errors).Trim().ToUpperInvariant();
                var title = MustValue(cols[1], "Title", fname, lineNo, errors).Trim();
                var prog = MustValue(cols[2], "ProgramCode", fname, lineNo, errors).Trim().ToUpperInvariant();
                var mods = MustValue(cols[3], "ModuleCode", fname, lineNo, errors).Trim();
                var status = MustValue(cols[4], "Status", fname, lineNo, errors).Trim();
                var desc = MustValue(cols[5], "Description", fname, lineNo, errors).Trim();

                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(title) ||
                    string.IsNullOrEmpty(prog) || string.IsNullOrEmpty(mods) ||
                    string.IsNullOrEmpty(status) || string.IsNullOrEmpty(desc))
                {
                    rowErrSet.Add($"{fname} line {lineNo}: All columns (TemplateId, Title, ProgramCode, ModuleCode, Status, Description) are required and cannot be empty.");
                    errors.AddRange(rowErrSet);
                    continue;
                }

                var titleKey = NormTitle(title);

                if (idFirstLine.TryGetValue(id, out var firstLine))
                    rowErrSet.Add($"{fname} line {lineNo}: duplicate TemplateId '{id}' in uploaded file (also at line {firstLine}).");
                else
                    idFirstLine[id] = lineNo;

                if (titleFirstLine.TryGetValue(titleKey, out var firstLine2))
                    rowErrSet.Add($"{fname} line {lineNo}: Project Title '{title}' duplicated in uploaded file (also at line {firstLine2}).");
                else
                    titleFirstLine[titleKey] = lineNo;

                // ---- Conflict matrix ----
                string decision = "";
                string decisionError = "";

                var idExists = !string.IsNullOrEmpty(id) && byId.TryGetValue(id, out var existingTemplate);
                string ownerId = "";
                var titleExists = !string.IsNullOrEmpty(titleKey) && titleOwner.TryGetValue(titleKey, out ownerId);

                if (!idExists && !titleExists)
                    decision = "Insert";
                else if (idExists && titleExists && string.Equals(ownerId, id, StringComparison.OrdinalIgnoreCase))
                    decision = "Update";
                else if (idExists && !titleExists)
                    decision = "Update";
                else
                {
                    decision = "Error";
                    decisionError = $"{fname} line {lineNo}: Title '{title}' already used by {ownerId}.";
                }

                if (decision == "Error")
                {
                    rowErrSet.Add(decisionError);
                    errors.AddRange(rowErrSet);
                    continue;
                }

                // ---- Program and Module check ----
                if (!validProgCodes.Contains(prog))
                    rowErrSet.Add($"{fname} line {lineNo}: ProgramCode '{prog}' not found.");

                var desired = mods.Split(new[] { ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(s => s.Trim().ToUpperInvariant())
                                  .Distinct(StringComparer.OrdinalIgnoreCase)
                                  .ToList();
                if (desired.Count == 0)
                    rowErrSet.Add($"{fname} line {lineNo}: ModuleCode is required.");
                else
                    foreach (var m in desired.Where(m => !validModuleSet.Contains(m)))
                        rowErrSet.Add($"{fname} line {lineNo}: ModuleCode '{m}' not found.");

                // AFTER
var statusCanon = CanonicalStatus(status);
if (string.IsNullOrEmpty(statusCanon))
{
    rowErrSet.Add($"{fname} line {lineNo}: Status '{status}' must be one of: Active, Inactive, Draft.");
}



                if (rowErrSet.Count > 0)
                {
                    errors.AddRange(rowErrSet);
                    continue;
                }

                // ---- Insert / Update logic ----
                if (!allowUpdates)
                {
                    if (decision == "Update")
                    {
                        rowErrSet.Add($"{fname} line {lineNo}: Row requires update, but updates not allowed.");
                        errors.AddRange(rowErrSet);
                        continue;
                    }

                    var tNew = new ProjectTemplate
                    {
                        TemplateCode = id,
                        Title = title,
                        ProgramCode = prog,
                        Status       = statusCanon,
                        Description = desc
                    };
                    _db.ProjectTemplates.Add(tNew);
                    await _db.SaveChangesAsync();

                    foreach (var mc in desired)
                        _db.ProjectTemplateModules.Add(new ProjectTemplateModule
                        {
                            ProjectTemplateId = tNew.Id,
                            ModuleCode = mc
                        });

                    inserted++;
                    titleOwner[titleKey] = id;
                    byId[id] = tNew;
                    continue;
                }

                if (decision == "Insert")
                {
                    var tNew = new ProjectTemplate
                    {
                        TemplateCode = id,
                        Title = title,
                        ProgramCode = prog,
                        Status       = statusCanon,
                        Description = desc
                    };
                    _db.ProjectTemplates.Add(tNew);
                    await _db.SaveChangesAsync();

                    foreach (var mc in desired)
                        _db.ProjectTemplateModules.Add(new ProjectTemplateModule
                        {
                            ProjectTemplateId = tNew.Id,
                            ModuleCode = mc
                        });

                    inserted++;
                    titleOwner[titleKey] = id;
                    byId[id] = tNew;
                }
                else if (decision == "Update")
                {
                    var tExisting = byId[id];
                    var oldTitleKey = NormTitle(tExisting.Title);

                    tExisting.Title = title;
                    tExisting.ProgramCode = prog;
                    tExisting.Status = statusCanon;
                    tExisting.Description = desc;

                    var desiredSet = desired.ToHashSet(StringComparer.OrdinalIgnoreCase);
                    foreach (var link in tExisting.TemplateModules.Where(tm => !desiredSet.Contains(tm.ModuleCode)).ToList())
                        _db.ProjectTemplateModules.Remove(link);

                    foreach (var code in desiredSet.Where(c => !tExisting.TemplateModules.Any(tm => tm.ModuleCode.Equals(c, StringComparison.OrdinalIgnoreCase))))
                        _db.ProjectTemplateModules.Add(new ProjectTemplateModule
                        {
                            ProjectTemplateId = tExisting.Id,
                            ModuleCode = code
                        });

                    if (!string.Equals(oldTitleKey, titleKey, StringComparison.OrdinalIgnoreCase))
                    {
                        if (oldTitleKey.Length > 0 && titleOwner.TryGetValue(oldTitleKey, out var ownedBy)
                            && string.Equals(ownedBy, id, StringComparison.OrdinalIgnoreCase))
                            titleOwner.Remove(oldTitleKey);

                        titleOwner[titleKey] = id;
                    }

                    updated++;
                }
            }

            if (errors.Count > 0)
            {
                await tx.RollbackAsync();
                return BadRequest(new { ok = false, errors, warnings });
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new { ok = true, summary = new { inserted, updated }, warnings });
        }

        // Live check used by the Create modal
[HttpGet]
public async Task<IActionResult> CheckTemplateExists(string id = "", string title = "")
{
    id    = (id ?? "").Trim().ToUpperInvariant();
    var t = (title ?? "").Trim();
    var titleKey = NormTitle(t);

    var idExists    = !string.IsNullOrEmpty(id)    && await _db.ProjectTemplates.AnyAsync(x => x.TemplateCode == id);
    var titleExists = !string.IsNullOrEmpty(title) && await _db.ProjectTemplates.AnyAsync(x => x.Title.ToUpper() == titleKey);

    return Json(new { idExists, titleExists });
}

// Live check used by the Edit modal (title may stay the same for the same ID)
[HttpGet]
public async Task<IActionResult> CheckTemplateUpdateAllowed(string id = "", string title = "")
{
    id    = (id ?? "").Trim().ToUpperInvariant();
    var t = (title ?? "").Trim();
    var titleKey = NormTitle(t);

    var clash = await _db.ProjectTemplates
        .AnyAsync(x => x.TemplateCode != id && x.Title.ToUpper() == titleKey);

    return Json(new { titleClash = clash }); // true = another template already has this title
}


        // ===================== Create / Edit / Delete =====================
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateTemplate([FromForm] TemplateRowVm vm)
{
    // Requireds
    vm.TemplateId = (vm.TemplateId ?? "").Trim().ToUpperInvariant();
    var title     = (vm.Title ?? "").Trim();
    var prog      = (vm.Programme ?? "").Trim().ToUpperInvariant();
    var status    = string.IsNullOrWhiteSpace(vm.Status) ? "" : vm.Status.Trim();
    var desc      = (vm.Description ?? "").Trim();

    var statusCanon = CanonicalStatus(vm.Status);

    if (string.IsNullOrWhiteSpace(vm.TemplateId)) return BadRequest("Template ID is required.");
    if (string.IsNullOrWhiteSpace(title))         return BadRequest("Title is required.");
    if (string.IsNullOrWhiteSpace(prog))          return BadRequest("Programme is required.");
    if (string.IsNullOrWhiteSpace(status))        return BadRequest("Status is required.");
    if (string.IsNullOrWhiteSpace(desc))          return BadRequest("Description is required.");
    if (vm.Modules == null || !vm.Modules.Any())  return BadRequest("At least one module is required.");
    
    // Uniqueness
    if (await _db.ProjectTemplates.AnyAsync(x => x.TemplateCode == vm.TemplateId))
        return BadRequest("Template ID already exists.");
    var titleKey = NormTitle(title);
    if (await _db.ProjectTemplates.AnyAsync(x => x.Title.ToUpper() == titleKey))
        return BadRequest("Title already exists.");

    // Status whitelist
    var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Active", "Inactive", "Draft" };
    if (!allowed.Contains(status)) return BadRequest("Status must be one of: Active, Inactive, Draft.");

    var t = new ProjectTemplate
    {
        TemplateCode = vm.TemplateId,
        Title        = title,
        ProgramCode  = prog,
        Status       = status,
        Description  = desc
    };
    _db.ProjectTemplates.Add(t);
    await _db.SaveChangesAsync();

    foreach (var mc in vm.Modules.Distinct(StringComparer.OrdinalIgnoreCase))
    {
        _db.ProjectTemplateModules.Add(new ProjectTemplateModule
        {
            ProjectTemplateId = t.Id,
            ModuleCode = mc.Trim().ToUpperInvariant()
        });
    }

    await _db.SaveChangesAsync();
    return Ok();
}


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTemplate([FromForm] TemplateRowVm vm)
    {
        var id     = (vm.TemplateId ?? "").Trim().ToUpperInvariant();
        var title  = (vm.Title ?? "").Trim();
        var prog   = (vm.Programme ?? "").Trim().ToUpperInvariant();
        var status = string.IsNullOrWhiteSpace(vm.Status) ? "" : vm.Status.Trim();
        var desc   = (vm.Description ?? "").Trim();

        if (string.IsNullOrWhiteSpace(id))     return BadRequest("Template ID is required.");
        if (string.IsNullOrWhiteSpace(title))  return BadRequest("Title is required.");
        if (string.IsNullOrWhiteSpace(prog))   return BadRequest("Programme is required.");
        if (string.IsNullOrWhiteSpace(status)) return BadRequest("Status is required.");
        if (string.IsNullOrWhiteSpace(desc))   return BadRequest("Description is required.");
        if (vm.Modules == null || !vm.Modules.Any()) return BadRequest("At least one module is required.");

        var t = await _db.ProjectTemplates.Include(x => x.TemplateModules)
                                        .FirstOrDefaultAsync(x => x.TemplateCode == id);
        if (t == null) return NotFound();

        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Active", "Inactive", "Draft" };
        if (!allowed.Contains(status)) return BadRequest("Status must be one of: Active, Inactive, Draft.");

        // Title must be unique across OTHER templates
        var titleKey = NormTitle(title);
        var titleClash = await _db.ProjectTemplates
            .AnyAsync(x => x.TemplateCode != id && x.Title.ToUpper() == titleKey);
        if (titleClash) return BadRequest("Title already exists for another template.");

        // Update
        t.Title        = title;
        t.ProgramCode  = prog;
        t.Status       = status;
        t.Description  = desc;

        // Replace modules (kept as your approach)
        _db.ProjectTemplateModules.RemoveRange(t.TemplateModules);
        foreach (var mc in vm.Modules.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            _db.ProjectTemplateModules.Add(new ProjectTemplateModule
            {
                ProjectTemplateId = t.Id,
                ModuleCode = mc.Trim().ToUpperInvariant()
            });
        }

        await _db.SaveChangesAsync();
        return Ok();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMultipleTemplates([FromBody] List<string> templateIds)
    {
        if (templateIds == null || templateIds.Count == 0)
            return BadRequest("No template IDs provided.");

        var upperIds = templateIds.Select(x => x.Trim().ToUpperInvariant()).ToList();

        var templates = await _db.ProjectTemplates
            .Include(t => t.TemplateModules)
            .Where(t => upperIds.Contains(t.TemplateCode.ToUpper()))
            .ToListAsync();

        if (templates.Count == 0)
            return NotFound("No matching templates found.");

        foreach (var t in templates)
        {
            if (t.TemplateModules?.Count > 0)
                _db.ProjectTemplateModules.RemoveRange(t.TemplateModules);

            _db.ProjectTemplates.Remove(t);
        }

        await _db.SaveChangesAsync();
        return Ok(new { ok = true, deleted = templates.Count });
    }


        // ===================== CSV helpers =====================

    private static string CanonicalStatus(string? raw)
    {
        var s = (raw ?? "").Trim();
        if (s.Equals("Active",   StringComparison.OrdinalIgnoreCase)) return "Active";
        if (s.Equals("Inactive", StringComparison.OrdinalIgnoreCase)) return "Inactive";
        if (s.Equals("Draft",    StringComparison.OrdinalIgnoreCase)) return "Draft";
        return ""; // invalid/unknown
    }


        private static string MakeProjectId(string program, int year, int session, int seq)
        {
            var yy = (year % 100).ToString("00");
            if(program != null && program.Length != 0)
            {
                return $"{program.ToUpper()}-{yy}-S{session}-{seq:00}";
            }
            else
            {
                program = "FYP";
                return $"{program.ToUpper()}-{yy}-S{session}-{seq:00}";
            }
        }
        static string NormTitle(string? s) => (s ?? "").Trim().ToUpperInvariant();

        private static bool IsComment(string? line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            var s = line.TrimStart();
            return s.StartsWith("#") || s.StartsWith("//");
        }

        private static bool HeaderIs(string[] hdr, params string[] expected)
        {
            if (hdr.Length < expected.Length) return false;
            for (int i = 0; i < expected.Length; i++)
                if (!string.Equals(hdr[i].Trim(), expected[i], StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        private static void Require(string[] cols, int count, string file, int line, List<string> errors)
        {
            if (cols.Length < count)
                errors.Add($"{file} line {line}: expected {count} columns, got {cols.Length}.");
        }

        private static string MustValue(string? v, string name, string file, int line, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(v))
            {
                errors.Add($"{file} line {line}: {name} is required.");
                return "";
            }
            return v.Trim();
        }

        private static string[] SplitCsvLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return Array.Empty<string>();
            var res = new List<string>();
            var sb = new StringBuilder();
            bool inQ = false;
            for (int i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (ch == '"')
                {
                    if (inQ && i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                    else inQ = !inQ;
                }
                else if (ch == ',' && !inQ)
                {
                    res.Add(sb.ToString());
                    sb.Clear();
                }
                else sb.Append(ch);
            }
            res.Add(sb.ToString());
            return res.ToArray();
        }

        [HttpGet]
        public async Task<IActionResult> GetPreferenceWeights(int programId, int year, int sessionNo)
        {
            //Check for existing program constraints
            var constraint = await _db.ProgramConstraints
                .FirstOrDefaultAsync(x => x.ProgramId == programId && x.Year == year && x.SessionNo == sessionNo);
            
            //Check send back default value if no constraint found
            if (constraint == null)
            {
                // return no weights + prefLimit from University constraint defaults
                var uni = await _db.UniversityConstraints
                    .FirstOrDefaultAsync(u => u.Year == year);

                if (uni == null)
                {
                    return NotFound();
                }
                else
                {  
                    return Json(new
                    {
                        sessionNo = uni.SessionNo,
                        sLoadCap = uni.SLoadCap,
                        aLoadCap = uni.ALoadCap,
                        prefLimit = uni.PrefRankLimit,
                        sMin = uni.SMin,
                        sMax = uni.SMax,
                        isOverride = false,
                        existingWeights = new List<object>()
                    });
                }
            }
            else
            {
                // 3. Retrieve weights
                var weights = await _db.PreferenceWeights
                    .Where(w => w.ProgramConstraintID == constraint.Id)
                    .OrderBy(w => w.RankNo)
                    .Select(w => new { w.RankNo, w.WeightValue })
                    .ToListAsync();
                 // Return combined info
                return Json(new
                {
                    sessionNo = constraint.SessionNo,
                    sLoadCap = constraint.SLoadCap,
                    aLoadCap = constraint.ALoadCap,
                    prefLimit = constraint.PrefRankLimit,
                    sMin = constraint.SMin,
                    sMax = constraint.SMax,
                    isOverride = constraint.IsOverride,
                    existingWeights = weights
                });

            }
        }


        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Allocation(string tab = "summary")
        {
            ViewBag.ActiveTab = tab;

            // --- Load dropdown data ---
            var programs = await _db.Programs
                .Select(p => new { p.ID, p.ProgramName })
                .ToListAsync();

            var years = await _db.ProgramConstraints
                .Select(c => c.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            // By default, load all sessions (you’ll filter them in the UI with JS)
            var sessions = await _db.ProgramConstraints
                .Select(c => new { c.Id, c.ProgramId, c.Year, c.SessionNo })
                .OrderBy(c => c.SessionNo)
                .ToListAsync();

            ViewBag.Programs = programs;
            ViewBag.Years = years;
            ViewBag.Sessions = sessions;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RunAllocation(int programConstraintID)
        {
            //Retrieve constraints base on programConstraint ID
            var programConstraints = await _db.ProgramConstraints
                .Where(x => x.Id == programConstraintID)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            //Retrieve Role records
            var targetRoles = new[] { "Assessor", "Supervisor", "Student" };

            var roles = await _db.Roles
                .Where(r => targetRoles.Contains(r.Name))
                .ToDictionaryAsync(r => r.Name, r => r.RoleId);

            var preferenceWeights = await _db.PreferenceWeights
                .Where(w => w.ProgramConstraintID == programConstraintID)
                .Select(w => new PreferenceWeightDto
                {
                    RankNo = w.RankNo,
                    WeightValue = w.WeightValue
                })
                .OrderBy(w => w.RankNo)
                .ToListAsync();

            var p = new AllocationParameters(
                ProgramConstraintID: programConstraintID,
                MaxGroupSize: programConstraints?.SMax ?? 5,
                MinGroupSize: programConstraints?.SMin ?? 3,
                SupervisorLoadCap: programConstraints?.SLoadCap ?? 4,
                AssessorLoadCap: programConstraints?.ALoadCap ?? 4,
                PreferenceLimit: programConstraints?.PrefRankLimit ?? 3,
                WModMatchSup: 10,
                WModMatchAss: 10,
                // Role IDs: adjust to your system constants
                RoleIdStudent: roles["Student"],
                RoleIdSupervisor: roles["Supervisor"],
                RoleIdAssessor: roles["Assessor"],    // <-- set correctly for your data
                Year: programConstraints?.Year ?? 2025,
                SessionNo: programConstraints.SessionNo,
                PreferenceWeightDtos: preferenceWeights
            );

            var result = await _engine.RunAsync(p);

            TempData["AllocationMessage"] = result.Conflicts.Count == 0
                ? $"Allocation completed. Run #{result.RunId}. AvgPrefScore: {result.AvgPreferenceScore:F1}"
                : $"Allocation partial. Run #{result.RunId}. Conflicts: {string.Join("; ", result.Conflicts.Select(c => c.Detail))}";

            return RedirectToAction("Allocation", new { tab = "summary" });
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAllocationRuns()
        {
            var runs = await _db.AllocationRuns
                .OrderByDescending(r => r.StartedAt)
                .Select(r => new
                {
                    r.Id,
                    r.ProgramConstraintID,

                    // Join with ProgramConstraints
                    Constraint = _db.ProgramConstraints
                        .Where(pc => pc.Id == r.ProgramConstraintID)
                        .Select(pc => new
                        {
                            pc.Year,
                            pc.SessionNo,
                            pc.ProgramId
                        })
                        .FirstOrDefault(),

                    r.StartedAt,
                    r.FinishedAt,
                    r.Status,
                    AvgScore = r.AvgPreferenceScore,
                    r.UnassignedStudents
                })
                .ToListAsync();

            // Post-process to include Program Name
            var output = new List<object>();

            foreach (var run in runs)
            {
                string programName = "-";

                if (run.Constraint != null)
                {
                    var program = await _db.Programs
                        .Where(p => p.ID == run.Constraint.ProgramId)
                        .Select(p => p.ProgramName)
                        .FirstOrDefaultAsync();

                    programName = program ?? "-";
                }

                output.Add(new
                {
                    run.Id,
                    Year = run.Constraint?.Year ?? 0,
                    ProgramName = programName,
                    Session = run.Constraint?.SessionNo ?? 0,
                    StartedAt = run.StartedAt.ToString("dd/MM/yyyy h:mmtt"),
                    FinishedAt = run.FinishedAt == null ? "-" :
                                run.FinishedAt.Value.ToString("dd/MM/yyyy h:mmtt"),
                    run.Status,
                    run.AvgScore,
                    Unassigned = run.UnassignedStudents
                });
            }

            return Json(output);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllocationRunDetails(int runId)
        {
            var run = await _db.AllocationRuns.FirstOrDefaultAsync(r => r.Id == runId);
            if (run == null)
                return NotFound("Run not found.");

            // Load details directly from AllocationRunDetails
            var details = await _db.AllocationRunDetails
                .Where(d => d.RunId == runId)
                .Select(d => new
                {
                    d.UserId,
                    d.RoleId,
                    d.GroupId,
                    d.ProjectId,
                    d.PreferenceRank,
                    d.Score,
                    d.MatchedModules,

                    UserName = d.User.Name,
                    UserEmail = d.User.Email,

                    ProjectTitle = _db.Projects
                        .Where(p => p.ProjectId == d.ProjectId)
                        .Select(p => p.Title)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var grouped = details
                .GroupBy(p => new { p.ProjectId, p.ProjectTitle })
                .Select(pg => new
                {
                    ProjectId = pg.Key.ProjectId,
                    ProjectTitle = pg.Key.ProjectTitle,

                    Groups = pg
                        .GroupBy(g => g.GroupId)
                        .Select(gx => new
                        {
                            GroupId = gx.Key,
                            Members = gx.Select(x => new
                            {
                                x.UserId,
                                x.UserName,
                                x.UserEmail,
                                x.RoleId,
                                x.PreferenceRank,
                                x.Score,
                                x.MatchedModules
                            })
                        })
                });

            return Json(grouped);
        }

        // [HttpPost]
        // public async Task<IActionResult> ConfirmAllocationRun([FromBody] dynamic body)
        // {
        //     int runId = Convert.ToInt32(body.runId);

        //     var runDetails = await _db.AllocationRunDetails
        //         .Where(x => x.RunId == runId)
        //         .ToListAsync();

        //     if (!runDetails.Any())
        //         return BadRequest("No allocation details found for this run.");

        //     // Group personnel by GroupId
        //     var grouped = runDetails
        //         .GroupBy(d => d.GroupId)
        //         .ToList();

        //     foreach (var g in grouped)
        //     {
        //         // Find the project from the first record in this group
        //         string projectId = g.First().ProjectId;

        //         // Supervisor / Assessor
        //         var supervisor = g.FirstOrDefault(x => x.RoleId == 5); // YOUR DB: RoleId 5 = Supervisor
        //         var assessor = g.FirstOrDefault(x => x.RoleId == 4);   // RoleId 4 = Assessor

        //         // Create or update Groups table
        //         var existingGroup = await _db.Groups.FirstOrDefaultAsync(x => x.Id == g.Key);

        //         if (existingGroup == null)
        //         {
        //             existingGroup = new Group
        //             {
        //                 Id = g.Key,
        //                 ProjectId = projectId,
        //                 CreatedAt = DateTime.UtcNow,
        //                 CreatedBy = "System"
        //             };
        //             _db.Groups.Add(existingGroup);
        //         }

        //         if (supervisor != null)
        //             existingGroup. = supervisor.UserId;

        //         if (assessor != null)
        //             existingGroup.AssessorId = assessor.UserId;

        //         existingGroup.UpdatedAt = DateTime.UtcNow;
        //     }

        //     await _db.SaveChangesAsync();
        //     return Ok(new { success = true });
        // }


    }
}
