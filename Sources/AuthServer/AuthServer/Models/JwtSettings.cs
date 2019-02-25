using System;
using System.Collections.Generic;
using System.Security.Claims;
using Cryptography.ECDSA;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Models
{
    public class JwtSettings
    {
        public DateTime Now { get; private set; }
        
        public string Issuer { get; set; }

        public string Key { get; set; }

        public string Audience { get; set; }

        public TimeSpan LifeTime { get; set; }
        
        public DateTime Expires => Now.AddMinutes(LifeTime.TotalMinutes);

        public IEnumerable<Claim> Claims { get; set; }


        public JwtSettings(DateTime utcNow)
        {
            Now = utcNow;
        }

        public SecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Hex.HexToBytes(Key));
        }
    }
}