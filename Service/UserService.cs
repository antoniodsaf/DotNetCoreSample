using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartSql.AOP;
using Zema.DyRepositories;
using Zema.Entities;

namespace Zema.Service
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        [Transaction]
        public virtual long AddWithTranWrap(User user)
        {
            return AddWithTran(user);
        }
        [Transaction]
        public virtual long AddWithTran(User user)
        {
            return _userRepository.Insert(user);
        }
    }
}
