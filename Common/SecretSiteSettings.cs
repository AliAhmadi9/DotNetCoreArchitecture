namespace Common
{
    public class SecretSiteSettings
    {
        public Authentication ExternalAuthentication { get; set; }
    }

    public class Authentication
    {
        public GoogleAuth GoogleAuth { get; set; }
    }
    public class GoogleAuth
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
