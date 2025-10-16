using System.Collections.Generic;
using FYP_25_S3_15P.ViewModels;

namespace FYP_25_S3_15P.Models;

public class FYPTemplateMaster
{
    public string ActiveTab { get; set; } = "plans";
    public List<FYPTemplates> FYPTemplates { get; set; } = new();
    public List<Programs> Programs { get; set; } = new();

}