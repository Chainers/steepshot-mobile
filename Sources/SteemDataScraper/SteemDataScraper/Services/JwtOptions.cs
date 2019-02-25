using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace SteemDataScraper.Services
{
    public class JwtOptions
    {
        private readonly IConfiguration _configuration;

        public byte[] Key => Encoding.ASCII.GetBytes(_configuration["JWT:Key"]);
        public string Issuer => _configuration["JWT:Issuer"];
        public string Audience => _configuration["JWT:Audience"];
        public TimeSpan ExpireTime => TimeSpan.Parse(_configuration["JWT:ExpireTime"]);
        public int MaxIncorrectPasswordCount => int.Parse(_configuration["JWT:MaxIncorrectPasswordCount"]);
        public TimeSpan IncorrectPasswordLockTime => TimeSpan.Parse(_configuration["JWT:IncorrectPasswordLockTime"]);

        public SymmetricSecurityKey SymmetricSecurityKey => new SymmetricSecurityKey(Key);
        public SigningCredentials SigningCredentials => new SigningCredentials(SymmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        public JwtOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<string> JtiGenerator()
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }
    }
}