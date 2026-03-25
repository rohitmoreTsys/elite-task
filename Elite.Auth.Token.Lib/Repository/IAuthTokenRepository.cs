
using Elite.Auth.Token.Lib.Entities;
using Elite.Auth.Token.Lib.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elite.Auth.Token.Lib.Repository
{
    public interface IAuthTokenRepository : ICommonRepository<EliteAuthToken>, IRepository<EliteAuthToken>
    {
        Task<EliteAuthToken> GetById(Guid guid);

        Task<List<EliteAuthToken>> GetByUid(string uid);


        Task<bool> IsRequestProcessing(Guid guid);
        Task<bool> GetReviewStatus(string bundleId, string version);
    }
}
