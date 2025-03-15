public static class EnvironmentVariables
{
    public static bool IsDevelopment { get; set; }
    public static string ApiDomainUrl { get; set; }
    public static string FrontendUrl { get; set; }
    public static string DbConnectionString { get; set; }
    public static string JwtSecret { get; set; }
    public static string JwtIssuer { get; set; }
    public static string JwtAudience { get; set; }
}