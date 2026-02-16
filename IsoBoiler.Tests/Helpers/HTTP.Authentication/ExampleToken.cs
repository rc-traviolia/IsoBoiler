using IsoBoiler.HTTP.Authentication;

namespace IsoBoiler.UnitTests.Helpers.HTTP.Authentication
{
    internal class ExampleToken : IAuthToken
    {
        public string Access_token { get; set; } = string.Empty;
        public int Expires_in { get; set; }
        public string Token_type { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;

        public string GetToken()
        {
            return Access_token;
        }
    }
}
