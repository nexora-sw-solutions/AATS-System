using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AuditApp.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "branches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexora_services",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexora_services", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordType = table.Column<string>(type: "text", nullable: false),
                    RecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentOption = table.Column<int>(type: "integer", nullable: true),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sync_tracking",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableName = table.Column<string>(type: "text", nullable: false),
                    RecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    Operation = table.Column<int>(type: "integer", nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeviceId = table.Column<string>(type: "text", nullable: true),
                    ConflictResolved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_tracking", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientCode = table.Column<string>(type: "text", nullable: false),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    TotalRevenue = table.Column<decimal>(type: "numeric", nullable: false),
                    OutstandingBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    LogoStorageKey = table.Column<string>(type: "text", nullable: true),
                    LastActiveAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clients_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    UserLogo = table.Column<string>(type: "text", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cheque_details",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankName = table.Column<string>(type: "text", nullable: true),
                    ChequeNumber = table.Column<string>(type: "text", nullable: true),
                    ChequeDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cheque_details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cheque_details_payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "boi_registrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BoiCode = table.Column<string>(type: "text", nullable: true),
                    Assignment = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    CountryAddress = table.Column<string>(type: "text", nullable: true),
                    InvestmentValueUsd = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordCode = table.Column<string>(type: "text", nullable: false),
                    RecordDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: true),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPayment = table.Column<decimal>(type: "numeric", nullable: false),
                    PartialAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentOption = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_boi_registrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_boi_registrations_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_boi_registrations_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "business_plan_valuations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Assignment = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordCode = table.Column<string>(type: "text", nullable: false),
                    RecordDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: true),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPayment = table.Column<decimal>(type: "numeric", nullable: false),
                    PartialAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentOption = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_business_plan_valuations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_business_plan_valuations_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_business_plan_valuations_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "company_registrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationCode = table.Column<string>(type: "text", nullable: false),
                    RegistrationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: false),
                    CompanyType = table.Column<string>(type: "text", nullable: true),
                    Objective = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    Process = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPayment = table.Column<decimal>(type: "numeric", nullable: false),
                    PartialAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentOption = table.Column<string>(type: "text", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company_registrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_company_registrations_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_company_registrations_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "epf_etf_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordCode = table.Column<string>(type: "text", nullable: false),
                    RecordDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: false),
                    NumberOfStaff = table.Column<int>(type: "integer", nullable: false),
                    Process = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPayment = table.Column<decimal>(type: "numeric", nullable: false),
                    PartialAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentOption = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_epf_etf_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_epf_etf_records_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_epf_etf_records_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "hr_management_consulting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Assignment = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordCode = table.Column<string>(type: "text", nullable: false),
                    RecordDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: true),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPayment = table.Column<decimal>(type: "numeric", nullable: false),
                    PartialAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentOption = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_management_consulting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hr_management_consulting_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_hr_management_consulting_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "import_export_clearances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClearanceCode = table.Column<string>(type: "text", nullable: true),
                    Assignment = table.Column<string>(type: "text", nullable: true),
                    TinNumber = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordCode = table.Column<string>(type: "text", nullable: false),
                    RecordDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: true),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPayment = table.Column<decimal>(type: "numeric", nullable: false),
                    PartialAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentOption = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_import_export_clearances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_import_export_clearances_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_import_export_clearances_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "nexora_service_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: true),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexora_service_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_nexora_service_requests_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_nexora_service_requests_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_nexora_service_requests_nexora_services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "nexora_services",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "other_secretarial_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Assignment = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordCode = table.Column<string>(type: "text", nullable: false),
                    RecordDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: true),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPayment = table.Column<decimal>(type: "numeric", nullable: false),
                    PartialAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentOption = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_other_secretarial_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_other_secretarial_records_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_other_secretarial_records_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tax_filings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FilingCode = table.Column<string>(type: "text", nullable: false),
                    TaxType = table.Column<string>(type: "text", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    FilingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TaxNumber = table.Column<string>(type: "text", nullable: false),
                    PeriodNumber = table.Column<string>(type: "text", nullable: true),
                    PeriodType = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tax_filings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tax_filings_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tax_filings_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "trade_licenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LicenseCode = table.Column<string>(type: "text", nullable: true),
                    Assignment = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordCode = table.Column<string>(type: "text", nullable: false),
                    RecordDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: true),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPayment = table.Column<decimal>(type: "numeric", nullable: false),
                    PartialAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentOption = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trade_licenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trade_licenses_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_trade_licenses_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "trade_marks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrademarkCode = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordCode = table.Column<string>(type: "text", nullable: false),
                    RecordDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: true),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPayment = table.Column<decimal>(type: "numeric", nullable: false),
                    PartialAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentOption = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trade_marks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trade_marks_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_trade_marks_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "activity_logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Module = table.Column<string>(type: "text", nullable: false),
                    RecordType = table.Column<string>(type: "text", nullable: true),
                    RecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_activity_logs_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_activity_logs_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "audit_assurance_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Assignment = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordCode = table.Column<string>(type: "text", nullable: false),
                    RecordDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientLogo = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    Process = table.Column<string>(type: "text", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPayment = table.Column<decimal>(type: "numeric", nullable: false),
                    PartialAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentOption = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_assurance_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_assurance_records_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_audit_assurance_records_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_audit_assurance_records_users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordType = table.Column<string>(type: "text", nullable: false),
                    RecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentCategory = table.Column<string>(type: "text", nullable: true),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<string>(type: "text", nullable: true),
                    MimeType = table.Column<string>(type: "text", nullable: true),
                    StorageKey = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    UploadedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    UploaderId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_documents_users_UploaderId",
                        column: x => x.UploaderId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "forensic_audit_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Assignment = table.Column<string>(type: "text", nullable: true),
                    PeriodNumber = table.Column<string>(type: "text", nullable: true),
                    PeriodType = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordCode = table.Column<string>(type: "text", nullable: false),
                    RecordDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientLogo = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    Process = table.Column<string>(type: "text", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPayment = table.Column<decimal>(type: "numeric", nullable: false),
                    PartialAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentOption = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forensic_audit_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_forensic_audit_records_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_forensic_audit_records_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_forensic_audit_records_users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "internal_audit_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Assignment = table.Column<string>(type: "text", nullable: true),
                    PeriodNumber = table.Column<string>(type: "text", nullable: true),
                    PeriodType = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordCode = table.Column<string>(type: "text", nullable: false),
                    RecordDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientLogo = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    Process = table.Column<string>(type: "text", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPayment = table.Column<decimal>(type: "numeric", nullable: false),
                    PartialAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentOption = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_internal_audit_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_internal_audit_records_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_internal_audit_records_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_internal_audit_records_users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "internal_control_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Assignment = table.Column<string>(type: "text", nullable: true),
                    PeriodNumber = table.Column<string>(type: "text", nullable: true),
                    PeriodType = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordCode = table.Column<string>(type: "text", nullable: false),
                    RecordDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientLogo = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    Process = table.Column<string>(type: "text", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPayment = table.Column<decimal>(type: "numeric", nullable: false),
                    PartialAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentOption = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_internal_control_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_internal_control_records_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_internal_control_records_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_internal_control_records_users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "management_account_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Assignment = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordCode = table.Column<string>(type: "text", nullable: false),
                    RecordDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientLogo = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    Process = table.Column<string>(type: "text", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPayment = table.Column<decimal>(type: "numeric", nullable: false),
                    PartialAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentOption = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_management_account_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_management_account_records_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_management_account_records_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_management_account_records_users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "other_audit_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Assignment = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Company = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordCode = table.Column<string>(type: "text", nullable: false),
                    RecordDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientLogo = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    Process = table.Column<string>(type: "text", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPayment = table.Column<decimal>(type: "numeric", nullable: false),
                    PartialAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentOption = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_other_audit_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_other_audit_records_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_other_audit_records_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_other_audit_records_users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tax_account_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedTo = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordCode = table.Column<string>(type: "text", nullable: false),
                    RecordDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientLogo = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    Process = table.Column<string>(type: "text", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPayment = table.Column<decimal>(type: "numeric", nullable: false),
                    PartialAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentOption = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tax_account_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tax_account_records_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tax_account_records_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tax_account_records_users_AssignedUserId",
                        column: x => x.AssignedUserId,
                        principalTable: "users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tax_account_records_users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "company_officers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<string>(type: "text", nullable: true),
                    OfficerType = table.Column<string>(type: "text", nullable: false),
                    SharePercentage = table.Column<decimal>(type: "numeric", nullable: true),
                    NicNumber = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company_officers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_company_officers_company_registrations_CompanyRegistrationId",
                        column: x => x.CompanyRegistrationId,
                        principalTable: "company_registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "epf_etf_staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpfEtfRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffCode = table.Column<string>(type: "text", nullable: false),
                    StaffName = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Process = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_epf_etf_staff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_epf_etf_staff_epf_etf_records_EpfEtfRecordId",
                        column: x => x.EpfEtfRecordId,
                        principalTable: "epf_etf_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_BranchId",
                table: "activity_logs",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_UserId",
                table: "activity_logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_assurance_records_BranchId",
                table: "audit_assurance_records",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_assurance_records_ClientId",
                table: "audit_assurance_records",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_assurance_records_CreatorId",
                table: "audit_assurance_records",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_boi_registrations_BranchId",
                table: "boi_registrations",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_boi_registrations_ClientId",
                table: "boi_registrations",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_business_plan_valuations_BranchId",
                table: "business_plan_valuations",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_business_plan_valuations_ClientId",
                table: "business_plan_valuations",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_cheque_details_PaymentId",
                table: "cheque_details",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_clients_BranchId",
                table: "clients",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_company_officers_CompanyRegistrationId",
                table: "company_officers",
                column: "CompanyRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_company_registrations_BranchId",
                table: "company_registrations",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_company_registrations_ClientId",
                table: "company_registrations",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_documents_UploaderId",
                table: "documents",
                column: "UploaderId");

            migrationBuilder.CreateIndex(
                name: "IX_epf_etf_records_BranchId",
                table: "epf_etf_records",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_epf_etf_records_ClientId",
                table: "epf_etf_records",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_epf_etf_staff_EpfEtfRecordId",
                table: "epf_etf_staff",
                column: "EpfEtfRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_forensic_audit_records_BranchId",
                table: "forensic_audit_records",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_forensic_audit_records_ClientId",
                table: "forensic_audit_records",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_forensic_audit_records_CreatorId",
                table: "forensic_audit_records",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_hr_management_consulting_BranchId",
                table: "hr_management_consulting",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_hr_management_consulting_ClientId",
                table: "hr_management_consulting",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_import_export_clearances_BranchId",
                table: "import_export_clearances",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_import_export_clearances_ClientId",
                table: "import_export_clearances",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_internal_audit_records_BranchId",
                table: "internal_audit_records",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_internal_audit_records_ClientId",
                table: "internal_audit_records",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_internal_audit_records_CreatorId",
                table: "internal_audit_records",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_internal_control_records_BranchId",
                table: "internal_control_records",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_internal_control_records_ClientId",
                table: "internal_control_records",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_internal_control_records_CreatorId",
                table: "internal_control_records",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_management_account_records_BranchId",
                table: "management_account_records",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_management_account_records_ClientId",
                table: "management_account_records",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_management_account_records_CreatorId",
                table: "management_account_records",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_nexora_service_requests_BranchId",
                table: "nexora_service_requests",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_nexora_service_requests_ClientId",
                table: "nexora_service_requests",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_nexora_service_requests_ServiceId",
                table: "nexora_service_requests",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_other_audit_records_BranchId",
                table: "other_audit_records",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_other_audit_records_ClientId",
                table: "other_audit_records",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_other_audit_records_CreatorId",
                table: "other_audit_records",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_other_secretarial_records_BranchId",
                table: "other_secretarial_records",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_other_secretarial_records_ClientId",
                table: "other_secretarial_records",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_tax_account_records_AssignedUserId",
                table: "tax_account_records",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_tax_account_records_BranchId",
                table: "tax_account_records",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_tax_account_records_ClientId",
                table: "tax_account_records",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_tax_account_records_CreatorId",
                table: "tax_account_records",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_tax_filings_BranchId",
                table: "tax_filings",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_tax_filings_ClientId",
                table: "tax_filings",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_trade_licenses_BranchId",
                table: "trade_licenses",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_trade_licenses_ClientId",
                table: "trade_licenses",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_trade_marks_BranchId",
                table: "trade_marks",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_trade_marks_ClientId",
                table: "trade_marks",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_users_BranchId",
                table: "users",
                column: "BranchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_logs");

            migrationBuilder.DropTable(
                name: "audit_assurance_records");

            migrationBuilder.DropTable(
                name: "boi_registrations");

            migrationBuilder.DropTable(
                name: "business_plan_valuations");

            migrationBuilder.DropTable(
                name: "cheque_details");

            migrationBuilder.DropTable(
                name: "company_officers");

            migrationBuilder.DropTable(
                name: "documents");

            migrationBuilder.DropTable(
                name: "epf_etf_staff");

            migrationBuilder.DropTable(
                name: "forensic_audit_records");

            migrationBuilder.DropTable(
                name: "hr_management_consulting");

            migrationBuilder.DropTable(
                name: "import_export_clearances");

            migrationBuilder.DropTable(
                name: "internal_audit_records");

            migrationBuilder.DropTable(
                name: "internal_control_records");

            migrationBuilder.DropTable(
                name: "management_account_records");

            migrationBuilder.DropTable(
                name: "nexora_service_requests");

            migrationBuilder.DropTable(
                name: "other_audit_records");

            migrationBuilder.DropTable(
                name: "other_secretarial_records");

            migrationBuilder.DropTable(
                name: "sync_tracking");

            migrationBuilder.DropTable(
                name: "tax_account_records");

            migrationBuilder.DropTable(
                name: "tax_filings");

            migrationBuilder.DropTable(
                name: "trade_licenses");

            migrationBuilder.DropTable(
                name: "trade_marks");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "company_registrations");

            migrationBuilder.DropTable(
                name: "epf_etf_records");

            migrationBuilder.DropTable(
                name: "nexora_services");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "branches");
        }
    }
}
