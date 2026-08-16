using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using AATS.API.Controllers;
using AATS.API.Models;
using AATS.Application.Common.Interfaces;
using AATS.Domain.Entities;
using AATS.Infrastructure.Persistence;
using Xunit;

namespace AATS.Tests
{
    public class ClientsAndDocumentsTests
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
        public async Task CreateClient_WithAttachments_SavesSourceDocumentsToSupabase()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var branch = new Branch { Id = Guid.NewGuid(), Name = "Central" };
            db.Branches.Add(branch);
            await db.SaveChangesAsync();

            var repoMock = new Mock<IRepository<Client>>();

            var client = new Client
            {
                Id = Guid.NewGuid(),
                Name = "Apex Global Ltd",
                ClientCode = "CL-1001",
                Email = "contact@apex.com",
                Phone = "0112345678",
                BranchId = branch.Id,
                Category = "Corporate",
                Status = "Active",
                BrAttachments = new List<SourceDocument>
                {
                    new SourceDocument
                    {
                        FileName = "apex_br.pdf",
                        Url = "https://pub-697e67016f144d8eab3f2f688eeb13cc.r2.dev/Secretarial/BR/apex_br.pdf",
                        Description = "BR Document"
                    }
                },
                TinAttachments = new List<SourceDocument>
                {
                    new SourceDocument
                    {
                        FileName = "apex_tin.pdf",
                        Url = "https://pub-697e67016f144d8eab3f2f688eeb13cc.r2.dev/Secretarial/TIN/apex_tin.pdf",
                        Description = "TIN Document"
                    }
                }
            };

            repoMock.Setup(r => r.AddAsync(client)).Returns(Task.CompletedTask);

            var controller = new ClientsController(repoMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = new ServiceCollection()
                        .AddSingleton(db)
                        .BuildServiceProvider()
                }
            };

            // Act
            var actionResult = await controller.Create(client);

            // Assert
            var objectResult = Assert.IsAssignableFrom<ObjectResult>(actionResult.Result);
            Assert.NotNull(objectResult.Value);

            var savedDocs = await db.SourceDocuments.Where(d => d.RecordId == client.Id).ToListAsync();
            Assert.Equal(2, savedDocs.Count);
            Assert.Contains(savedDocs, d => d.AttachmentCategory == "BR" && d.FileName == "apex_br.pdf");
            Assert.Contains(savedDocs, d => d.AttachmentCategory == "TIN" && d.FileName == "apex_tin.pdf");
        }

        [Fact]
        public async Task EnrichClientDocumentsAsync_PopulatesClientAttachmentCollections()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var clientId = Guid.NewGuid();
            var client = new Client
            {
                Id = clientId,
                Name = "Lanka Holdings",
                ClientCode = "CL-2002"
            };
            db.Clients.Add(client);

            db.SourceDocuments.AddRange(
                new SourceDocument
                {
                    RecordId = clientId,
                    RecordType = "Client",
                    AttachmentCategory = "BR",
                    FileName = "lanka_br.pdf",
                    Url = "https://pub-697e67016f144d8eab3f2f688eeb13cc.r2.dev/lanka_br.pdf"
                },
                new SourceDocument
                {
                    RecordId = clientId,
                    RecordType = "Client",
                    AttachmentCategory = "Form01",
                    FileName = "lanka_form01.pdf",
                    Url = "https://pub-697e67016f144d8eab3f2f688eeb13cc.r2.dev/lanka_form01.pdf"
                }
            );
            await db.SaveChangesAsync();

            var repoMock = new Mock<IRepository<Client>>();
            repoMock.Setup(r => r.GetWithInclude(c => c.Branch))
                    .ReturnsAsync(new List<Client> { client });

            var controller = new ClientsController(repoMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = new ServiceCollection()
                        .AddSingleton(db)
                        .BuildServiceProvider()
                }
            };

            // Act
            var actionResult = await controller.GetAll(enrich: true);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var apiResp = okResult.Value as ApiResponse<PaginatedResult<Client>>;
            Assert.NotNull(apiResp);
            Assert.NotNull(apiResp.Data);

            var clients = apiResp.Data.Items;
            var targetClient = clients.First(c => c.Id == clientId);
            Assert.NotNull(targetClient.BrAttachments);
            Assert.Single(targetClient.BrAttachments);
            Assert.Equal("lanka_br.pdf", targetClient.BrAttachments[0].FileName);

            Assert.NotNull(targetClient.Form01Attachments);
            Assert.Single(targetClient.Form01Attachments);
            Assert.Equal("lanka_form01.pdf", targetClient.Form01Attachments[0].FileName);
        }
    }
}
