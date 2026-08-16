using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AATS.API.Controllers;
using AATS.Domain.Entities;
using AATS.Infrastructure.Persistence;
using AATS.Application.Common.Interfaces;
using Moq;
using Xunit;

namespace AATS.Tests
{
    public class SystemAndUploadTests
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

        [Fact]
        public async Task ActivityLogs_CreateAndGetAll_PersistsLogEntries()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var controller = new ActivityLogsController(db);

            var log = new ActivityLog
            {
                Action = "Create",
                Module = "Clients",
                BranchName = "Central",
                Description = "Created client CL-7007",
                UserName = "Admin User",
                CreatedAt = DateTime.UtcNow
            };

            // Act - Create
            var createResult = await controller.Create(log);
            Assert.IsType<OkObjectResult>(createResult);

            // Act - Get
            var getResult = await controller.GetAll();
            Assert.IsType<OkObjectResult>(getResult);

            var count = await db.ActivityLogs.CountAsync();
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task NexoraRequests_CreateAndGetAll_PersistsRequests()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var emailServiceMock = new Mock<IEmailService>();
            var controller = new NexoraRequestsController(db, emailServiceMock.Object);

            var req = new NexoraRequest
            {
                Id = Guid.NewGuid(),
                ClientName = "Blue Horizon Ltd",
                ServiceType = "Company Registration",
                Details = "Request for new company registration",
                Status = "PENDING"
            };

            // Act - Create
            var createResult = await controller.Create(req);
            Assert.IsType<OkObjectResult>(createResult);

            // Act - Get
            var getResult = await controller.GetAll();
            Assert.IsType<OkObjectResult>(getResult);

            var count = await db.NexoraRequests.CountAsync();
            Assert.Equal(1, count);
        }
    }
}
