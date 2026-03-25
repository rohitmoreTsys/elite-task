using Elite.Auth.Token.Lib.Command;
using Elite.Auth.Token.Lib.Common;
using Elite.Auth.Token.Lib.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Elite.Auth.Token.Lib.Services
{
    public interface IAuthTokenServices
    {

        Task<AuthReponse> PostAuthToken(AuthTokenCommand token);
        Task<AuthTokenCommand> GetToken(Guid guid);
        Task<bool> IsRequestProcessing(Guid guid);
        Task<bool> GetReviewStatus(string bundleId, string version);
    }
}
