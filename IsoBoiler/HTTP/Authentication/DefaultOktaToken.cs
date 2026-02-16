namespace IsoBoiler.HTTP.Authentication
{
    public class DefaultOktaToken : IAuthToken
    {
        public string token_type { get; set; } = string.Empty;
        public int expires_in { get; set; }
        public string access_token { get; set; } = string.Empty;
        public string scope { get; set; } = string.Empty;

        public string GetToken()
        {
            return access_token;
        }
    }
}
