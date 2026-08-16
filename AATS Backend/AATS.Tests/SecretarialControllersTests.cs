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
    public class SecretarialControllersTests
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
        public async Task CompanyRegistrations_CreateWithOfficers_SavesRecordAndChildEntities()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var recordServiceMock = new Mock<IRecordService>();
            recordServiceMock.Setup(s => s.GenerateRecordCodeAsync("SEC-REG")).ReturnsAsync("SEC-REG-0001");

            var controller = new CompanyRegistrationsController(db, recordServiceMock.Object);

            var record = new AuditRecord
            {
                Id = Guid.NewGuid(),
                Category = "Company Registration",
                CompanyName = "Lanka Tech Solutions Ltd",
                CompanyType = "Private Limited",
                ClientName = "Samantha Perera",
                ClientCode = "CL-4004",
                Status = "Active",
                Process = "Name Approval",
                CurrentStep = 1,
                Officers = new List<CompanyOfficer>
                {
                    new CompanyOfficer
                    {
                        Id = Guid.NewGuid(),
                        Name = "Samantha Perera",
                        Position = "Director",
                        NicNumber = "198512345V"
                    }
                }
            };

            // Act
            var createResult = await controller.Create(record);

            // Assert
            var okCreate = Assert.IsType<OkObjectResult>(createResult);
            Assert.NotNull(okCreate.Value);

            var count = await db.AuditRecords.CountAsync(r => r.Category == "Company Registration");
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task EpfEtfController_CreateAndGetAll_ReturnsSuccess()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var recordServiceMock = new Mock<IRecordService>();
            recordServiceMock.Setup(s => s.GenerateRecordCodeAsync("SEC-EPF")).ReturnsAsync("SEC-EPF-0001");

            var controller = new EpfEtfController(db, recordServiceMock.Object);

            var record = new AuditRecord
            {
                Id = Guid.NewGuid(),
                Category = "EPF / ETF",
                ClientName = "Global Apparel Ltd",
                ClientCode = "CL-4005",
                Status = "Active",
                Process = "Monthly Filing"
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
