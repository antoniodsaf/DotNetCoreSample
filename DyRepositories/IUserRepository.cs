﻿using SmartSql.DyRepository;
using SmartSql.DyRepository.Annotations;
using Zema.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartSql;

namespace Zema.DyRepositories
{
    public interface IUserRepository
    {
        ISqlMapper SqlMapper { get; }
        long Insert(User entity);
        Task<long> InsertAsync(User entity);
        [Statement(Id = "GetEntity")]
        User GetById([Param("Id")]long id);
        IEnumerable<User> Query([Param("Taken")]int taken);
        [Statement(Id = "QueryByPage")]
        TPageResult GetByPage<TPageResult>(object request);
        Task<IEnumerable<User>> QueryAsync([Param("Taken")]int taken);
        int Update(User entity);
    }
}
