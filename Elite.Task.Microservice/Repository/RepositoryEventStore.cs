using Elite.Task.Microservice.Models.Entities;
using Elite.Task.Microservice.Repository.Contracts;
using Elite_Task.Microservice.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Repository
{
    public class RepositoryEventStore : BaseDataAccess, IRepositoryEventStore
    {
        private readonly IConfiguration _configuration;
        public RepositoryEventStore(IConfiguration configuration) : base(null)
        {
            _configuration = configuration;
            SetDbContext(configuration);
        }

        public void Add(NoticationEventStore data)
        {
            _context.NoticationEventStore.Add(data);

        }

        public async Task<List<long>> GetByIds()
        {
            return await (from p in this.GetDBSet<NoticationEventStore>()
                          where p.IsProcessed == false && p.IsFailed == false
                          orderby p.CreatedDate
                          select p.Id).Take(Convert.ToInt32(_configuration.GetSection("BackGroundThreadCount").Value)).ToListAsync();

        }

        public async Task<List<long>> GetByTaskId(long taskId)
        {
            return await (from p in this.GetDBSet<NoticationEventStore>()
                          where (p.IsProcessed == true && p.IsFailed == false) && p.Sourcetypeid == taskId
                          select p.Id).ToListAsync();

        }


        public async Task<NoticationEventStore> GetById(long id)
        {
            return await (from p in this.GetDBSet<NoticationEventStore>()
                          where p.Id.Equals(id)
                          select p).SingleOrDefaultAsync();
        }

        public void Update(NoticationEventStore data)
        {
            _context.Entry(data).State = EntityState.Modified;
        }
    }
}
