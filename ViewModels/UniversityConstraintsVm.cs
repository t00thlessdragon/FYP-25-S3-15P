using System;
using System.Collections.Generic;

namespace FYP_25_S3_15P.ViewModels
{
    public class UniversityConstraintsVm
    {
        public string UniCode { get; set; } = "";          
        public string UniversityName { get; set; } = "";  
        public string Abbrev { get; set; } = "";           

        // Constraints
        public int Year { get; set; } = DateTime.UtcNow.Year;
        public int NumSessions { get; set; } = 4;
        public int TeamSizeMin { get; set; } = 4;
        public int TeamSizeMax { get; set; } = 6;
        public int SupervisorLoadCap { get; set; } = 1;
        public int AssessorLoadCap { get; set; } = 1;

        public string S1Label { get; set; } = "S1";
        public string S2Label { get; set; } = "S2";
        public string S3Label { get; set; } = "S3";
        public string S4Label { get; set; } = "S4";

        public string S1Range { get; set; } = "";
        public string S2Range { get; set; } = "";
        public string S3Range { get; set; } = "";
        public string S4Range { get; set; } = "";

        public int PreferencesRankLimit { get; set; } = 3;

        public List<int> Years { get; set; } = new();
        public List<int> SessionCounts { get; set; } = new() { 1, 2, 3, 4 };
        public List<int> TeamMinChoices { get; set; } = new() { 2, 3, 4, 5, 6 };
        public List<int> TeamMaxChoices { get; set; } = new() { 3, 4, 5, 6, 7, 8 };
        public List<int> LoadCaps { get; set; } = new() { 1, 2, 3, 4, 5 };
        public List<int> PrefLimits { get; set; } = new() { 3, 4, 5, 6, 7, 8 };
    }
}
