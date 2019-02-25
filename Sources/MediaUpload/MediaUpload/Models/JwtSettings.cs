using System;
using System.Collections.Generic;
using System.Security.Claims;
using Cryptography.ECDSA;
using Microsoft.IdentityModel.Tokens;

namespace MediaUpload.Models
{
    public class JwtSettings
    {
        public string Issuer { get; set; }

        public string Key { get; set; }

        public string Audience { get; set; }

        public TimeSpan LifeTime { get; set; }

        public IEnumerable<Claim> Claims { get; set; }


        public SecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Hex.HexToBytes(Key));
        }
    }
}