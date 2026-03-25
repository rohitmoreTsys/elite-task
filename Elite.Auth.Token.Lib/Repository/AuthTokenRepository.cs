using Elite.Auth.Token.Lib.Entities;
using Elite.Auth.Token.Lib.Models;
using Elite.Auth.Token.Lib.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Elite.Auth.Token.Lib.Repository
{
    public class AuthTokenRepository : BaseDataAccess, IAuthTokenRepository
    {
        public AuthTokenRepository(EliteAuthTokenContext context) : base(context)
        {
        }

        public void Add(EliteAuthToken token)
        {
            _context.EliteAuthToken.Add(token);
        }

        public async Task<EliteAuthToken> GetById(Guid guid) =>
         await this.GetDBSet<EliteAuthToken>().FindAsync(guid);

        public async Task<List<EliteAuthToken>> GetByUid(string uid) =>
         await this.GetDBSet<EliteAuthToken>().Where(p => p.Uid.ToUpper().Equals(uid.ToUpper())).ToListAsync();


        public void Update(EliteAuthToken token)
        {
            _context.Entry(token).State = EntityState.Modified;
        }

        public void Delete(EliteAuthToken tokens)
        {
            _context.Entry(tokens).State = EntityState.Deleted;
        }

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        public async  Task<bool> IsRequestProcessing(Guid guid)
        {
            await semaphoreSlim.WaitAsync();
            try
            { 

                var reqId = await this._context.TrackRequestSessions.FindAsync(guid);
                if (reqId != null)
                    return true;
                else
                {
                    this._context.TrackRequestSessions.Add(new TrackRequestSessions { RequestId = guid, CreateDate = DateTime.Now });
                    this._context.SaveChanges();
                    return false;
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<bool> GetReviewStatus(string bundleId, string version)
        {
            try
            {
                var isInReview = await _context.LegalPropertyContent.Where(c => c.BundleId == bundleId && c.Version == version).FirstOrDefaultAsync();
                return (isInReview != null) ?  isInReview.IsInReview :  false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
 