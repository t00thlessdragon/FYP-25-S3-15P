using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FYP_25_S3_15P.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "FAQs",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Question = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FAQs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Features",
                schema: "dbo",
                columns: table => new
                {
                    FeatureID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShowOnHome = table.Column<bool>(type: "bit", nullable: false),
                    HomeOrder = table.Column<int>(type: "int", nullable: true),
                    HomeTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.FeatureID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "dbo",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "subscriptionPlans",
                schema: "dbo",
                columns: table => new
                {
                    PlanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptionPlans", x => x.PlanID);
                });

            migrationBuilder.CreateTable(
                name: "Universities",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UniName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UnivCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Universities", x => x.ID);
                    table.UniqueConstraint("AK_Universities_UniID", x => x.UniID);
                });

            migrationBuilder.CreateTable(
                name: "PlanFeatures",
                schema: "dbo",
                columns: table => new
                {
                    PlanID = table.Column<int>(type: "int", nullable: false),
                    FeatureID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanFeatures", x => new { x.PlanID, x.FeatureID });
                    table.ForeignKey(
                        name: "FK_PlanFeatures_Features_FeatureID",
                        column: x => x.FeatureID,
                        principalSchema: "dbo",
                        principalTable: "Features",
                        principalColumn: "FeatureID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanFeatures_subscriptionPlans_PlanID",
                        column: x => x.PlanID,
                        principalSchema: "dbo",
                        principalTable: "subscriptionPlans",
                        principalColumn: "PlanID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationForm",
                schema: "dbo",
                columns: table => new
                {
                    AppId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicantName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PlanID = table.Column<int>(type: "int", nullable: false),
                    UniID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationForm", x => x.AppId);
                    table.ForeignKey(
                        name: "FK_ApplicationForm_Universities_UniID",
                        column: x => x.UniID,
                        principalSchema: "dbo",
                        principalTable: "Universities",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationForm_subscriptionPlans_PlanID",
                        column: x => x.PlanID,
                        principalSchema: "dbo",
                        principalTable: "subscriptionPlans",
                        principalColumn: "PlanID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GlobalUniConstraints",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniID = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    PTeamSize = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SLoadCap = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ALoadCap = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PrefRankLimit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalUniConstraints", x => x.ID);
                    table.ForeignKey(
                        name: "FK_GlobalUniConstraints_Universities_UniID",
                        column: x => x.UniID,
                        principalSchema: "dbo",
                        principalTable: "Universities",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Program",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniID = table.Column<int>(type: "int", nullable: false),
                    ProgramID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProgramCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProgramName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Program", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Program_Universities_UniID",
                        column: x => x.UniID,
                        principalSchema: "dbo",
                        principalTable: "Universities",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Year = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SessionNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Dte_fr = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Dte_to = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UniversityID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Sessions_Universities_UniID",
                        column: x => x.UniID,
                        principalSchema: "dbo",
                        principalTable: "Universities",
                        principalColumn: "UniID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Universities_UniversityID",
                        column: x => x.UniversityID,
                        principalSchema: "dbo",
                        principalTable: "Universities",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniID = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    EmailNormalized = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MustChangePassword = table.Column<bool>(type: "bit", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    UsersID = table.Column<int>(type: "int", nullable: true),
                    EmailDomain = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleID",
                        column: x => x.RoleID,
                        principalSchema: "dbo",
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Universities_UniID",
                        column: x => x.UniID,
                        principalSchema: "dbo",
                        principalTable: "Universities",
                        principalColumn: "UniID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Users_UsersID",
                        column: x => x.UsersID,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "UniSession",
                schema: "dbo",
                columns: table => new
                {
                    UniID = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    SessionID = table.Column<int>(type: "int", nullable: false),
                    Date_Frm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Date_To = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniSession", x => new { x.UniID, x.Year, x.SessionID });
                    table.ForeignKey(
                        name: "FK_UniSession_Sessions_SessionID",
                        column: x => x.SessionID,
                        principalSchema: "dbo",
                        principalTable: "Sessions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UniSession_Universities_UniID",
                        column: x => x.UniID,
                        principalSchema: "dbo",
                        principalTable: "Universities",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffProfiles",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffProfiles", x => x.ID);
                    table.UniqueConstraint("AK_StaffProfiles_StaffID", x => x.StaffID);
                    table.ForeignKey(
                        name: "FK_StaffProfiles_Users_UserID",
                        column: x => x.UserID,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    RoleID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleID",
                        column: x => x.RoleID,
                        principalSchema: "dbo",
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserID",
                        column: x => x.UserID,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Assessments",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarkAwarded = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GroupID = table.Column<int>(type: "int", nullable: false),
                    FYPTopicID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CourseName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CourseCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProgramID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModuleID = table.Column<int>(type: "int", nullable: true),
                    UniversityProgramID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.ID);
                    table.UniqueConstraint("AK_Courses_CourseID", x => x.CourseID);
                    table.ForeignKey(
                        name: "FK_Courses_Program_UniversityProgramID",
                        column: x => x.UniversityProgramID,
                        principalSchema: "dbo",
                        principalTable: "Program",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModuleName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ModuleCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CourseID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.ID);
                    table.UniqueConstraint("AK_Modules_ModuleID", x => x.ModuleID);
                    table.ForeignKey(
                        name: "FK_Modules_Courses_CourseID",
                        column: x => x.CourseID,
                        principalSchema: "dbo",
                        principalTable: "Courses",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Programs",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProgramName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ProgramCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UniID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CourseID = table.Column<int>(type: "int", nullable: true),
                    UniversityID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programs", x => x.ID);
                    table.UniqueConstraint("AK_Programs_ProgramID", x => x.ProgramID);
                    table.ForeignKey(
                        name: "FK_Programs_Courses_CourseID",
                        column: x => x.CourseID,
                        principalSchema: "dbo",
                        principalTable: "Courses",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Programs_Universities_UniID",
                        column: x => x.UniID,
                        principalSchema: "dbo",
                        principalTable: "Universities",
                        principalColumn: "UniID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Programs_Universities_UniversityID",
                        column: x => x.UniversityID,
                        principalSchema: "dbo",
                        principalTable: "Universities",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "StudentProfiles",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    UserID = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    CourseID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PhoneNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SessionID = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    isFullTime = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProfiles", x => x.ID);
                    table.UniqueConstraint("AK_StudentProfiles_StudentID", x => x.StudentID);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_Courses_CourseID",
                        column: x => x.CourseID,
                        principalSchema: "dbo",
                        principalTable: "Courses",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_Sessions_SessionID",
                        column: x => x.SessionID,
                        principalSchema: "dbo",
                        principalTable: "Sessions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_Users_UserID",
                        column: x => x.UserID,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffModules",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffID = table.Column<int>(type: "int", nullable: false),
                    ModuleID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffModules", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StaffModules_Modules_ModuleID",
                        column: x => x.ModuleID,
                        principalSchema: "dbo",
                        principalTable: "Modules",
                        principalColumn: "ModuleID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffModules_StaffProfiles_StaffID",
                        column: x => x.StaffID,
                        principalSchema: "dbo",
                        principalTable: "StaffProfiles",
                        principalColumn: "StaffID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FYPTopics",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TopicID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Program_Abbrev_Year_Session_IndexNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TopicTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TopicDesc = table.Column<string>(type: "nvarchar(max)", maxLength: 500, nullable: true),
                    Tag = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SessionID = table.Column<int>(type: "int", nullable: true),
                    ProgramID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UniversityID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FYPTopics", x => x.ID);
                    table.UniqueConstraint("AK_FYPTopics_TopicID", x => x.TopicID);
                    table.ForeignKey(
                        name: "FK_FYPTopics_Programs_ProgramID",
                        column: x => x.ProgramID,
                        principalSchema: "dbo",
                        principalTable: "Programs",
                        principalColumn: "ProgramID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FYPTopics_Sessions_SessionID",
                        column: x => x.SessionID,
                        principalSchema: "dbo",
                        principalTable: "Sessions",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_FYPTopics_Universities_UniversityID",
                        column: x => x.UniversityID,
                        principalSchema: "dbo",
                        principalTable: "Universities",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "StudentModules",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    ModuleID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentModules", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StudentModules_Modules_ModuleID",
                        column: x => x.ModuleID,
                        principalSchema: "dbo",
                        principalTable: "Modules",
                        principalColumn: "ModuleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentModules_StudentProfiles_StudentID",
                        column: x => x.StudentID,
                        principalSchema: "dbo",
                        principalTable: "StudentProfiles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FYPTemplates",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProgramID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FYPTopicID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FYPTemplates", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FYPTemplates_FYPTopics_FYPTopicID",
                        column: x => x.FYPTopicID,
                        principalSchema: "dbo",
                        principalTable: "FYPTopics",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_FYPTemplates_Programs_ProgramID",
                        column: x => x.ProgramID,
                        principalSchema: "dbo",
                        principalTable: "Programs",
                        principalColumn: "ProgramID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FYPTemplates_Universities_UniID",
                        column: x => x.UniID,
                        principalSchema: "dbo",
                        principalTable: "Universities",
                        principalColumn: "UniID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsFullTime = table.Column<bool>(type: "bit", nullable: true),
                    TopicID = table.Column<int>(type: "int", nullable: false),
                    UniversityID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Groups_FYPTopics_TopicID",
                        column: x => x.TopicID,
                        principalSchema: "dbo",
                        principalTable: "FYPTopics",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Groups_Universities_UniversityID",
                        column: x => x.UniversityID,
                        principalSchema: "dbo",
                        principalTable: "Universities",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Preferences",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    FYPTopics = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Rank = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preferences", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Preferences_FYPTopics_FYPTopics",
                        column: x => x.FYPTopics,
                        principalSchema: "dbo",
                        principalTable: "FYPTopics",
                        principalColumn: "TopicID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Preferences_Users_UserID",
                        column: x => x.UserID,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupID = table.Column<int>(type: "int", nullable: false),
                    TaskTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TaskDesc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DueAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tasks_Groups_GroupID",
                        column: x => x.GroupID,
                        principalSchema: "dbo",
                        principalTable: "Groups",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserGroups",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    GroupID = table.Column<int>(type: "int", nullable: true),
                    RoleID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroups", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserGroups_Groups_GroupID",
                        column: x => x.GroupID,
                        principalSchema: "dbo",
                        principalTable: "Groups",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserGroups_Roles_RoleID",
                        column: x => x.RoleID,
                        principalSchema: "dbo",
                        principalTable: "Roles",
                        principalColumn: "RoleId");
                    table.ForeignKey(
                        name: "FK_UserGroups_Users_UserID",
                        column: x => x.UserID,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskEvaluations",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CriterionLabel = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    Weighted = table.Column<double>(type: "float", nullable: false),
                    DeliverableScore = table.Column<double>(type: "float", nullable: false),
                    FinalContribution = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SAvedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TaskID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskEvaluations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TaskEvaluations_Tasks_TaskID",
                        column: x => x.TaskID,
                        principalSchema: "dbo",
                        principalTable: "Tasks",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationForm_PlanID",
                schema: "dbo",
                table: "ApplicationForm",
                column: "PlanID");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationForm_UniID",
                schema: "dbo",
                table: "ApplicationForm",
                column: "UniID");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_FYPTopicID",
                schema: "dbo",
                table: "Assessments",
                column: "FYPTopicID");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_GroupID",
                schema: "dbo",
                table: "Assessments",
                column: "GroupID");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ID",
                schema: "dbo",
                table: "Courses",
                column: "ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ModuleID",
                schema: "dbo",
                table: "Courses",
                column: "ModuleID");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ProgramID",
                schema: "dbo",
                table: "Courses",
                column: "ProgramID");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_UniversityProgramID",
                schema: "dbo",
                table: "Courses",
                column: "UniversityProgramID");

            migrationBuilder.CreateIndex(
                name: "IX_FYPTemplates_FYPTopicID",
                schema: "dbo",
                table: "FYPTemplates",
                column: "FYPTopicID");

            migrationBuilder.CreateIndex(
                name: "IX_FYPTemplates_ProgramID",
                schema: "dbo",
                table: "FYPTemplates",
                column: "ProgramID");

            migrationBuilder.CreateIndex(
                name: "IX_FYPTemplates_UniID",
                schema: "dbo",
                table: "FYPTemplates",
                column: "UniID");

            migrationBuilder.CreateIndex(
                name: "IX_FYPTopics_ProgramID",
                schema: "dbo",
                table: "FYPTopics",
                column: "ProgramID");

            migrationBuilder.CreateIndex(
                name: "IX_FYPTopics_SessionID",
                schema: "dbo",
                table: "FYPTopics",
                column: "SessionID");

            migrationBuilder.CreateIndex(
                name: "IX_FYPTopics_UniversityID",
                schema: "dbo",
                table: "FYPTopics",
                column: "UniversityID");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalUniConstraints_UniID",
                schema: "dbo",
                table: "GlobalUniConstraints",
                column: "UniID");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_ID",
                schema: "dbo",
                table: "Groups",
                column: "ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_TopicID",
                schema: "dbo",
                table: "Groups",
                column: "TopicID");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_UniversityID",
                schema: "dbo",
                table: "Groups",
                column: "UniversityID");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_CourseID",
                schema: "dbo",
                table: "Modules",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_ID",
                schema: "dbo",
                table: "Modules",
                column: "ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanFeatures_FeatureID",
                schema: "dbo",
                table: "PlanFeatures",
                column: "FeatureID");

            migrationBuilder.CreateIndex(
                name: "IX_Preferences_FYPTopics",
                schema: "dbo",
                table: "Preferences",
                column: "FYPTopics");

            migrationBuilder.CreateIndex(
                name: "IX_Preferences_UserID",
                schema: "dbo",
                table: "Preferences",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Program_UniID",
                schema: "dbo",
                table: "Program",
                column: "UniID");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_CourseID",
                schema: "dbo",
                table: "Programs",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_ID",
                schema: "dbo",
                table: "Programs",
                column: "ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Programs_UniID",
                schema: "dbo",
                table: "Programs",
                column: "UniID");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_UniversityID",
                schema: "dbo",
                table: "Programs",
                column: "UniversityID");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                schema: "dbo",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ID",
                schema: "dbo",
                table: "Sessions",
                column: "ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UniID",
                schema: "dbo",
                table: "Sessions",
                column: "UniID");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UniversityID",
                schema: "dbo",
                table: "Sessions",
                column: "UniversityID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffModules_ModuleID",
                schema: "dbo",
                table: "StaffModules",
                column: "ModuleID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffModules_StaffID",
                schema: "dbo",
                table: "StaffModules",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffProfiles_UserID",
                schema: "dbo",
                table: "StaffProfiles",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentModules_ModuleID",
                schema: "dbo",
                table: "StudentModules",
                column: "ModuleID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentModules_StudentID",
                schema: "dbo",
                table: "StudentModules",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_CourseID",
                schema: "dbo",
                table: "StudentProfiles",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_SessionID",
                schema: "dbo",
                table: "StudentProfiles",
                column: "SessionID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_UserID",
                schema: "dbo",
                table: "StudentProfiles",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskEvaluations_TaskID",
                schema: "dbo",
                table: "TaskEvaluations",
                column: "TaskID");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_GroupID",
                schema: "dbo",
                table: "Tasks",
                column: "GroupID");

            migrationBuilder.CreateIndex(
                name: "IX_UniSession_SessionID",
                schema: "dbo",
                table: "UniSession",
                column: "SessionID");

            migrationBuilder.CreateIndex(
                name: "IX_Universities_UniID",
                schema: "dbo",
                table: "Universities",
                column: "UniID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_GroupID",
                schema: "dbo",
                table: "UserGroups",
                column: "GroupID");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_RoleID",
                schema: "dbo",
                table: "UserGroups",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_UserID",
                schema: "dbo",
                table: "UserGroups",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleID",
                schema: "dbo",
                table: "UserRoles",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserID",
                schema: "dbo",
                table: "UserRoles",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailNormalized",
                schema: "dbo",
                table: "Users",
                column: "EmailNormalized",
                unique: true,
                filter: "[EmailNormalized] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleID",
                schema: "dbo",
                table: "Users",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UniID",
                schema: "dbo",
                table: "Users",
                column: "UniID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UsersID",
                schema: "dbo",
                table: "Users",
                column: "UsersID");

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_FYPTopics_FYPTopicID",
                schema: "dbo",
                table: "Assessments",
                column: "FYPTopicID",
                principalSchema: "dbo",
                principalTable: "FYPTopics",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_Groups_GroupID",
                schema: "dbo",
                table: "Assessments",
                column: "GroupID",
                principalSchema: "dbo",
                principalTable: "Groups",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Modules_ModuleID",
                schema: "dbo",
                table: "Courses",
                column: "ModuleID",
                principalSchema: "dbo",
                principalTable: "Modules",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Programs_ProgramID",
                schema: "dbo",
                table: "Courses",
                column: "ProgramID",
                principalSchema: "dbo",
                principalTable: "Programs",
                principalColumn: "ProgramID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Program_Universities_UniID",
                schema: "dbo",
                table: "Program");

            migrationBuilder.DropForeignKey(
                name: "FK_Programs_Universities_UniID",
                schema: "dbo",
                table: "Programs");

            migrationBuilder.DropForeignKey(
                name: "FK_Programs_Universities_UniversityID",
                schema: "dbo",
                table: "Programs");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Modules_ModuleID",
                schema: "dbo",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Program_UniversityProgramID",
                schema: "dbo",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Programs_ProgramID",
                schema: "dbo",
                table: "Courses");

            migrationBuilder.DropTable(
                name: "ApplicationForm",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Assessments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "FAQs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "FYPTemplates",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "GlobalUniConstraints",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PlanFeatures",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Preferences",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "StaffModules",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "StudentModules",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TaskEvaluations",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UniSession",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserGroups",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Features",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "subscriptionPlans",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "StaffProfiles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "StudentProfiles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Tasks",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Groups",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "FYPTopics",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Sessions",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Universities",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Modules",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Program",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Programs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Courses",
                schema: "dbo");
        }
    }
}
