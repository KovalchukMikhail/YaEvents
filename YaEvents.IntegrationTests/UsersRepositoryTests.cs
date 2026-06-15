using Domain.Enums;
using Domain.Models;
using Infrastructure.Repositories.UsersRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;

namespace YaEvents.IntegrationTests
{
    [Collection("Database")]
    public class UserRepositoryTests
    {
        private readonly DbWorker _dbWorker;
        public UserRepositoryTests(DbWorker dbWorker)
        {
            _dbWorker = dbWorker;
        }
        private User CreateUser(string login = "User1", string password = "User1", UserRole role = UserRole.User)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            var passworHash = Convert.ToHexString(bytes);
            return new User(Guid.NewGuid(), login, passworHash, role);
        }
        
        [Fact]
        public async Task Add_CorrectParameters_SaveUserToDataBase()
        {
            //Arrange
            await _dbWorker.ResetDatabaseAsync();
            var context = await _dbWorker.CreateContext();
            var userRepository = new UsersRepository(context);
            var user = CreateUser();
        
            //Act
            await userRepository.Add(user);
        
            //Assert
            context = await _dbWorker.CreateContext();
            Assert.NotNull(context.Users.SingleOrDefault(u => u.Id == user.Id));
        }
        
        [Fact]
        public async Task Add_TryAddTwoUsersWithSameLogin_ThrowbUpdateException()
        {
            //Arrange
            await _dbWorker.ResetDatabaseAsync();
            var context = await _dbWorker.CreateContext();
            var userRepository = new UsersRepository(context);
            var user1 = CreateUser(login: "User1");
            var user2 = CreateUser(login: "User1");
        
            //Act
            await userRepository.Add(user1);
        
            //Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () => await userRepository.Add(user2));
        }
        [Fact]
        public async Task Get_CorrectLogin_ReturnsUser()
        {
            //Arrange
            await _dbWorker.ResetDatabaseAsync();
            var context = await _dbWorker.CreateContext();
            var user1 = CreateUser(login: "User1");
            context.Users.Add(user1);
            context.SaveChanges();

            //Act
            context = await _dbWorker.CreateContext();
            var userRepository = new UsersRepository(context);
            var user = userRepository.Get("User1");

            //Assert
            Assert.NotNull(user);
        }
        [Fact]
        public async Task Get_CorrectId_ReturnsUser()
        {
            //Arrange
            await _dbWorker.ResetDatabaseAsync();
            var context = await _dbWorker.CreateContext();
            var user1 = CreateUser();
            context.Users.Add(user1);
            context.SaveChanges();

            //Act
            context = await _dbWorker.CreateContext();
            var userRepository = new UsersRepository(context);
            var user = userRepository.Get(user1.Id);

            //Assert
            Assert.NotNull(user);
        }
    }
}
