using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using AATS.API.Controllers;
using AATS.Application.Common.Interfaces;
using AATS.Domain.Entities;
using AATS.Infrastructure.Persistence;
using AATS.Infrastructure.Services;
using Xunit;

namespace AATS.Tests
{
    public class AuthAndUsersTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new ApplicationDbContext(options);
            dbContext.Database.EnsureCreated();
            return dbContext;
        }

        private IConfiguration GetTestConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Jwt:Key", "SuperSecretTestKeyThatIsVeryLongAndSecure12345!"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"}
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsSuccessAndToken()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var config = GetTestConfiguration();
            var authService = new AuthService(config, db);
            var emailServiceMock = new Mock<IEmailService>();
            var controller = new AuthController(authService, db, emailServiceMock.Object);

            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Pass@123");
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "testuser@example.com",
                PasswordHash = passwordHash,
                Role = UserRole.Staff,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var loginRequest = new LoginDto
            {
                Email = "testuser@example.com",
                Password = "Pass@123"
            };

            // Act
            var result = await controller.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var config = GetTestConfiguration();
            var authService = new AuthService(config, db);
            var emailServiceMock = new Mock<IEmailService>();
            var controller = new AuthController(authService, db, emailServiceMock.Object);

            var loginRequest = new LoginDto
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword"
            };

            // Act
            var result = await controller.Login(loginRequest);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorizedResult.Value);
        }

        [Fact]
        public async Task Register_WithNewUser_CreatesUserAndReturnsToken()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var config = GetTestConfiguration();
            var authService = new AuthService(config, db);
            var emailServiceMock = new Mock<IEmailService>();
            var controller = new AuthController(authService, db, emailServiceMock.Object);

            var registerDto = new RegisterRequest
            {
                Username = "newadmin",
                Email = "newadmin@example.com",
                Password = "AdminPassword123!",
                Role = 1 // Admin
            };

            // Act
            var result = await controller.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var createdUser = await db.Users.FirstOrDefaultAsync(u => u.Username == "newadmin");
            Assert.NotNull(createdUser);
            Assert.Equal(UserRole.Admin, createdUser.Role);
        }

        [Fact]
        public async Task UsersController_UpdateUser_UpdatesRoleAndBranch()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "johndoe",
                Email = "john@example.com",
                PasswordHash = "hash",
                Role = UserRole.Staff,
                IsActive = true
            };
            var branch = new Branch { Id = Guid.NewGuid(), Name = "Central Branch" };
            db.Users.Add(user);
            db.Branches.Add(branch);
            await db.SaveChangesAsync();

            var repoMock = new Mock<IRepository<User>>();
            repoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            repoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var controller = new UsersController(repoMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = new ServiceCollection()
                        .AddSingleton(db)
                        .BuildServiceProvider()
                }
            };

            user.Role = UserRole.Manager;
            user.BranchId = branch.Id;

            // Act
            var result = await controller.Update(user.Id, user);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
            Assert.Equal(UserRole.Manager, user.Role);
            Assert.Equal(branch.Id, user.BranchId);
        }
    }
}
