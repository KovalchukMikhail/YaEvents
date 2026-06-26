using Application.DTO;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.Interfaces
{
    public interface IUserService
    {
        public Task<UserInfo> RegisterUser(CreateUser createUser, CancellationToken token = default);
        public Task<string> Enter(string login, string passwordHash, CancellationToken token = default);
    }
}
