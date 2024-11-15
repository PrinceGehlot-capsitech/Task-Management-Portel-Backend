namespace SignInPage.Models
{
    public class SignInSettings
    {
        public string? ConnectionString { get; set; }
        public string? DatabaseName { get; set; }
        public string SignInCollectionName { get; set; } = string.Empty;
        public string JwtKey {  get; set; }
    }
}
