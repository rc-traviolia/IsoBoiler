using System.Threading.Tasks;

namespace IsoBoiler.HTTP.Authentication
{
    public interface ITokenProvider<TTokenFormat> where TTokenFormat : IAuthToken
    {
        Task<string> GetTokenAsync();
    }
}
