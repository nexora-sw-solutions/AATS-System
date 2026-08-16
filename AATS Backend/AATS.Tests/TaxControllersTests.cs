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
    public class TaxControllersTests
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
            var itemsProp = dataProp.GetType().GetProperty("items")?.GetValue(dataProp);
            if (itemsProp is System.Collections.ICollection collection) return collection.Count;
            return 0;
        }

        [Fact]
        public async Task VatFilingController_CreateAndGetAll_ReturnsSuccess()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var recordServiceMock = new Mock<IRecordService>();
            recordServiceMock.Setup(s => s.GenerateRecordCodeAsync("TAX-VAT")).ReturnsAsync("TAX-VAT-0001");

            var controller = new VatFilingController(db, recordServiceMock.Object);

            var record = new TaxRecord
            {
                Id = Guid.NewGuid(),
                TaxType = "VAT",
                ClientName = "Island Traders Pvt Ltd",
                ClientCode = "CL-6006",
                Status = "Active",
                Process = "Return Filed",
                Period = "Q3 2024",
                TotalPayment = 75000,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var createResult = await controller.Create(record);
            var okCreate = Assert.IsType<OkObjectResult>(createResult);
            Assert.NotNull(okCreate.Value);

            var getResult = await controller.GetAll();
            var okGet = Assert.IsType<OkObjectResult>(getResult);
            Assert.Equal(1, GetTotalCount(okGet.Value));
        }

        [Fact]
        public async Task CitFilingController_CreateAndGetAll_ReturnsSuccess()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var recordServiceMock = new Mock<IRecordService>();
            recordServiceMock.Setup(s => s.GenerateRecordCodeAsync("TAX-CIT")).ReturnsAsync("TAX-CIT-0001");

            var controller = new CitFilingController(db, recordServiceMock.Object);

            var record = new TaxRecord
            {
                Id = Guid.NewGuid(),
                TaxType = "CIT",
                ClientName = "Corporate Holdings Ltd",
                ClientCode = "CL-6007",
                Status = "Active",
                Process = "Assessment Pending"
            };

            // Act
            var createResult = await controller.Create(record);
            Assert.IsType<OkObjectResult>(createResult);

            var getResult = await controller.GetAll();
            var okGet = Assert.IsType<OkObjectResult>(getResult);
            Assert.Equal(1, GetTotalCount(okGet.Value));
        }
    }
}
