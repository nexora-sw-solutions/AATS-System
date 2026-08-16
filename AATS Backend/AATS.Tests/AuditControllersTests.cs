using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using AATS.API.Controllers;
using AATS.Application.Common.Interfaces;
using AATS.Domain.Entities;
using AATS.Infrastructure.Persistence;
using Xunit;

namespace AATS.Tests
{
    public class AuditControllersTests
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

        private static int GetTotalCount(object? okResultValue)
        {
            if (okResultValue == null) return 0;
            var dataProp = okResultValue.GetType().GetProperty("data")?.GetValue(okResultValue);
            if (dataProp == null) return 0;
            var countProp = dataProp.GetType().GetProperty("totalCount")?.GetValue(dataProp);
            return countProp is int count ? count : 0;
        }

        [Fact]
        public async Task AuditAssurance_CreateAndGetAll_ReturnsSuccess()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var recordServiceMock = new Mock<IRecordService>();
            recordServiceMock.Setup(s => s.GenerateRecordCodeAsync("AUD-ASR")).ReturnsAsync("AUD-ASR-0001");

            var controller = new AuditAssuranceController(db, recordServiceMock.Object);

            var record = new AuditRecord
            {
                Id = Guid.NewGuid(),
                ClientName = "Financial Assurance Ltd",
                ClientCode = "CL-3001",
                Status = "Active",
                Process = "Audit Completed",
                CurrentStep = 4,
                TotalPayment = 250000,
                CreatedAt = DateTime.UtcNow
            };

            // Act - Create
            var createResult = await controller.Create(record);
            var okCreate = Assert.IsType<OkObjectResult>(createResult);
            Assert.NotNull(okCreate.Value);

            // Act - GetAll
            var getResult = await controller.GetAll(enrich: false);
            var okGet = Assert.IsType<OkObjectResult>(getResult);
            Assert.Equal(1, GetTotalCount(okGet.Value));
        }

        [Fact]
        public async Task InternalAudit_CreateAndGetAll_ReturnsSuccess()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var recordServiceMock = new Mock<IRecordService>();
            recordServiceMock.Setup(s => s.GenerateRecordCodeAsync("AUD-INT")).ReturnsAsync("AUD-INT-0001");

            var controller = new InternalAuditController(db, recordServiceMock.Object);

            var record = new AuditRecord
            {
                Id = Guid.NewGuid(),
                ClientName = "Internal Controls Bank",
                ClientCode = "CL-3002",
                Status = "Active",
                Process = "In Review"
            };

            // Act
            var createResult = await controller.Create(record);
            Assert.IsType<OkObjectResult>(createResult);

            var getResult = await controller.GetAll(enrich: false);
            var okGet = Assert.IsType<OkObjectResult>(getResult);
            Assert.Equal(1, GetTotalCount(okGet.Value));
        }
    }
}
