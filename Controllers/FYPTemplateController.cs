using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;
using System.Security.Claims;
using System.Text;
namespace FYP_25_S3_15P.Controllers;

public class FYPTemplateController : Controller
{
    private readonly SmartDbContext _db;
    public FYPTemplateController(SmartDbContext db) => _db = db;

    // PUBLIC pricing page (unchanged)
    public async Task<IActionResult> Index()
    {
        var plans = await _db.SubscriptionPlans
            .AsNoTracking()
            .Include(p => p.PlanFeatures).ThenInclude(pf => pf.Feature)
            .OrderBy(p => p.Price)
            .ToListAsync();

        return View(plans);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ProjectName,Description,ProgID")] FYPTemplates template)
    {
        if (!ModelState.IsValid)
        {
            // Redirect back to your SC dashboard with correct tab if validation fails
            return RedirectToAction("FYPTemplateMaster", "SCDashboard", new { tab = "templates" });
        }

        // Automatically fill system-managed fields
        template.CreatedAt = DateTime.UtcNow;
        // Optionally set CreatedBy if your app has user identity tracking
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var UserData = await _db.Users.FindAsync(int.Parse(userIdClaim ?? "0"));
        if(UserData == null)
        {
            return RedirectToAction("FYPTemplateMaster", "SCDashboard", new { tab = "templates" });
        }
        else
        {
            template.UniID = UserData.UniID ?? 0;
        }
        template.CreatedBy = int.TryParse(userIdClaim, out var uid) ? uid : 0;
        //template.CreatedBy = User?.Identity?.Name ?? "System";

        _db.FYPTemplates.Add(template);
        await _db.SaveChangesAsync();

        // Redirect back to the FYP Template Dashboard tab
        return RedirectToAction("FYPTemplateMaster", "SCDashboard", new { tab = "templates" });
    }

    // NOTE: no separate id param anymore (prevents 404 when id isn't posted)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind("ID,ProjectName,Description,ProgID")] FYPTemplates template)
    {
        if (!ModelState.IsValid)
            return RedirectToAction("FYPTemplateMaster", "SCDashboard", new { tab = "templates" });

        var existing = await _db.FYPTemplates.FindAsync(template.ID);
        if (existing == null)
            return NotFound();
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        existing.ProjectName = template.ProjectName;
        existing.Description = template.Description;
        existing.ProgID = template.ProgID;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.CreatedBy = int.TryParse(userIdClaim, out var uid) ? uid : 0;

        _db.Update(existing);
        await _db.SaveChangesAsync();

        return RedirectToAction("FYPTemplateMaster", "SCDashboard", new { tab = "templates" });
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var templates = await _db.FYPTemplates.FindAsync(id);
        if (templates != null)
        {
            _db.FYPTemplates.Remove(templates);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("FYPTemplateMaster", "SCDashboard", new { tab = "templates" });
    }

    // =========================================================
        // DOWNLOAD CSV TEMPLATE
        // =========================================================
        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            var csvHeader = "Program Name,Project Name,Description\n";
            var bytes = Encoding.UTF8.GetBytes(csvHeader);
            return File(bytes, "text/csv", "FYP_Template.csv");
        }

        // =========================================================
        // UPLOAD CSV PREVIEW
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a CSV file.";
                return RedirectToAction("FYPTemplateMaster", "SCDashboard", new { tab = "uploadtemplates" });
            }

            var results = new List<UploadPreviewRow>();
            using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);

            // Read header
            if (reader.EndOfStream)
            {
                TempData["Error"] = "CSV file is empty.";
                return RedirectToAction("FYPTemplateMaster", "SCDashboard", new { tab = "uploadtemplates" });
            }

            var headerLine = await reader.ReadLineAsync();
            if (headerLine == null)
            {
                TempData["Error"] = "Invalid CSV format.";
                return RedirectToAction("FYPTemplateMaster", "SCDashboard", new { tab = "uploadtemplates" });
            }

            var headers = headerLine.Split(',').Select(h => h.Trim().ToLower()).ToList();
            int colProgram = headers.IndexOf("program name");
            int colProject = headers.IndexOf("project name");
            int colDesc    = headers.IndexOf("description");

            if (colProgram == -1 || colProject == -1 || colDesc == -1)
            {
                TempData["Error"] = "Missing required columns: Program Name, Project Name, Description.";
                return RedirectToAction("FYPTemplateMaster", "SCDashboard", new { tab = "uploadtemplates" });
            }

            var programs = await _db.Programs.ToListAsync();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var cells = line.Split(',').Select(c => c.Trim()).ToArray();

                string progName = colProgram < cells.Length ? cells[colProgram] : "";
                string projName = colProject < cells.Length ? cells[colProject] : "";
                string desc     = colDesc    < cells.Length ? cells[colDesc]    : "";

                string remark = "";

                if (string.IsNullOrEmpty(progName))
                    remark += "Missing Program Name; ";
                if (string.IsNullOrEmpty(projName))
                    remark += "Missing Project Name; ";

                var matchedProg = programs.FirstOrDefault(p =>
                    p.ProgramName.Equals(progName, StringComparison.OrdinalIgnoreCase));

                if (matchedProg == null)
                    remark += "Program not found;";

                results.Add(new UploadPreviewRow
                {
                    ProgramName = progName,
                    ProjectName = projName,
                    Description = desc,
                    Remarks = string.IsNullOrEmpty(remark) ? "OK" : remark.Trim()
                });
            }

            TempData["UploadPreview"] = System.Text.Json.JsonSerializer.Serialize(results);
            return RedirectToAction("FYPTemplateMaster", "SCDashboard", new { tab = "uploadtemplates" });
        }

        // =========================================================
        // CLEAR PREVIEW
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearPreview()
        {
            TempData.Remove("UploadPreview");
            return RedirectToAction("FYPTemplateMaster", "SCDashboard", new { tab = "uploadtemplates" });
        }

        // =========================================================
        // CONFIRM UPLOAD
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmUpload()
        {
            if (!TempData.TryGetValue("UploadPreview", out var previewData))
                return RedirectToAction("FYPTemplateMaster", "SCDashboard", new { tab = "uploadtemplates" });

            var rows = System.Text.Json.JsonSerializer.Deserialize<List<UploadPreviewRow>>(previewData.ToString() ?? "[]");
            var validRows = rows.Where(r => r.Remarks == "OK").ToList();

            if (!validRows.Any())
            {
                TempData["Error"] = "No valid records to import.";
                return RedirectToAction("FYPTemplateMaster", "SCDashboard", new { tab = "uploadtemplates" });
            }

            var programs = await _db.Programs.ToListAsync();
            var now = DateTime.UtcNow;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var UserData = await _db.Users.FindAsync(int.Parse(userIdClaim ?? "0"));
            var createdById = int.TryParse(userIdClaim, out var uid) ? uid : 0;
            foreach (var row in validRows)
            {
                var prog = programs.FirstOrDefault(p =>
                    p.ProgramName.Equals(row.ProgramName, StringComparison.OrdinalIgnoreCase));

                if (prog != null)
                {
                    _db.FYPTemplates.Add(new FYPTemplates
                    {
                        UniID = UserData?.UniID ?? 0,
                        ProgID = prog.ID,
                        ProjectName = row.ProjectName,
                        Description = row.Description,
                        CreatedAt = now,
                        CreatedBy = createdById // TODO: replace with logged user ID
                    });
                }
            }

            await _db.SaveChangesAsync();
            TempData.Remove("UploadPreview");
            TempData["Flash"] = "Templates successfully imported.";
            return RedirectToAction("FYPTemplateMaster", "SCDashboard", new { tab = "uploadtemplates" });
        }

        // --- Helper model ---
        public class UploadPreviewRow
        {
            public string ProgramName { get; set; } = "";
            public string ProjectName { get; set; } = "";
            public string Description { get; set; } = "";
            public string Remarks { get; set; } = "";
        }
}
