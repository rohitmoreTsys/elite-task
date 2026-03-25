using Elite.Auth.Token.Lib.Models;
using System;
using System.Collections.Generic;
using System.Text;


namespace Elite.Auth.Token.Lib.Repository
{
    public interface ICommonRepository<T> where T : IEntity
    {
        void Add(T token);

        void Update(T token);

        void Delete(T token);
    }
}
