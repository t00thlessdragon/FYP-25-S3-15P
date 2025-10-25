namespace FYP_25_S3_15P.Services;

public class SmtpOptions
{
    public string Host { get; set; } = "";
    public int    Port { get; set; } = 587;
    public bool   EnableSsl { get; set; } = true;
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromEmail { get; set; } = "";
    public string FromName  { get; set; } = "SMART Team";

    // Convenience for building links in emails
    public string LoginUrl { get; set; } = "";
}