using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AuthServer.Models;
using AuthServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Apis.v1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class TokenController
    {
        private readonly IConfiguration _configuration;
        private ApiProvider ApiProvider;

        public TokenController(IConfiguration configuration, ApiProvider apiProvider)
        {
            _configuration = configuration;
            ApiProvider = apiProvider;
        }

        [HttpPost]
        public async Task<IActionResult> Token(AuthModel model)
        {
            var isValid = await ApiProvider.AuthorizeAsync(model, CancellationToken.None);

            if (!isValid)
                return new UnauthorizedResult();

            var jwtSettings = new JwtSettings(DateTime.UtcNow);
            _configuration.GetSection("JwtSettings").Bind(jwtSettings);
            jwtSettings.Claims = GetClaims(model);

            var jwt = new JwtSecurityToken(jwtSettings.Issuer, jwtSettings.Audience, notBefore: jwtSettings.Now,
                claims: jwtSettings.Claims,
                expires: jwtSettings.Expires,
                signingCredentials: new SigningCredentials(jwtSettings.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
            );

            var result = new TokenModel
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                Expires = jwtSettings.Expires,
                Login = model.Login,
                Type = model.AuthType
            };
            
            return new JsonResult(result);
        }

        private Claim[] GetClaims(AuthModel model)
        {
            return new[]
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, model.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, model.AuthType.ToString())
            };
        }
    }
}