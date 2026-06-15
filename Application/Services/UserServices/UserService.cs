using Application.DTO;
using Application.Repositories;
using Application.Services.Interfaces;
using Application.Services.SecurityServices;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly IUsersRepository _usersRepository;
        private readonly ISecurityServise _securityServise;
        private const string WRONG_USER_DATA = "Пользователь с такой комбинацией логин + пароль не существует";
        public UserService(IUsersRepository usersRepository, ISecurityServise securityServise)
        {
            _usersRepository = usersRepository;
            _securityServise = securityServise;
        }
        public async Task<string> Enter(string login, string password, CancellationToken token = default)
        {
            var user = await _usersRepository.Get(login, token);
            if (user == null)
                throw new NotFoundException(WRONG_USER_DATA);

            var passwordHash = _securityServise.CreatePasswordHash(password);
            if (user.PasswordHash != passwordHash)
                throw new NotFoundException(WRONG_USER_DATA);

            return _securityServise.CreateToken(user);
        }

        public async Task<UserInfo> RegisterUser(CreateUser createUser, CancellationToken token = default)
        {
            var user = await _usersRepository.Get(createUser.Login, token);
            if (user != null)
                throw new UserExistsException($"Пользователь {createUser.Login} уже существует");

            var passwordHash = _securityServise.CreatePasswordHash(createUser.Password);
            var role = createUser.Role ?? UserRole.User;

            user = new User(Guid.NewGuid(), createUser.Login, passwordHash, role);
            await _usersRepository.Add(user, token);

            return new UserInfo(user.Id, user.Login, user.Role);
        }
    }
}
