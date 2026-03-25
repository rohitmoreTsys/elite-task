using Elite.Auth.Token.Lib.Models;
using System;
using System.Collections.Generic;

namespace Elite.Auth.Token.Lib.Entities
{
    public partial class EliteAuthToken : IEntity
    {
        public Guid Id { get; set; }
        public string Uid { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Idtoken { get; set; }
        public DateTime TokenExpireDateTime { get; set; }
        public DateTime CreateDate { get; set; }
        public bool? IsRefreshTokenGenerated { get; set; }
    }
}
