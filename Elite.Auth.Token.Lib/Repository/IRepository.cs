using Elite.Auth.Token.Lib.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Auth.Token.Lib.Repository
{
    public interface IRepository<T> where T : IEntity
    {
        IUnitOfWork UnitOfWork { get; }
    }
}
