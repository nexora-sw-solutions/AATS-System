<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 10" />
  <img src="https://img.shields.io/badge/Avalonia_UI-11.3-8B44F7?style=for-the-badge&logo=dotnet&logoColor=white" alt="Avalonia UI" />
  <img src="https://img.shields.io/badge/React-19-61DAFB?style=for-the-badge&logo=react&logoColor=black" alt="React 19" />
  <img src="https://img.shields.io/badge/PostgreSQL-Supabase-4169E1?style=for-the-badge&logo=postgresql&logoColor=white" alt="PostgreSQL" />
  <img src="https://img.shields.io/badge/Aspire-13.2-6C3483?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET Aspire" />
  <img src="https://img.shields.io/badge/License-Proprietary-EF4444?style=for-the-badge" alt="License" />
</p>

<h1 align="center">AATS — Audit, Accounting, Tax & Secretarial</h1>
<h3 align="center">Enterprise Practice Management Platform</h3>

<p align="center">
  <b>A comprehensive, multi-platform practice management system designed for audit, accounting, tax, and corporate secretarial firms.</b><br/>
  Built with Clean Architecture · Powered by .NET Aspire · Desktop & Web
</p>

---

## ✨ Overview

**AATS** is an end-to-end practice management platform built by **Nexora** that streamlines the operations of professional services firms handling audit engagements, tax filings, secretarial work, and advisory services. It provides a native **Windows desktop application** alongside a modern **web frontend**, both powered by a robust **.NET API backend** with PostgreSQL persistence.

The platform centralizes client management, record tracking, document storage, team coordination, financial reporting, and real-time notifications — replacing fragmented spreadsheets and manual workflows with a unified, professional-grade solution.

---

## 🏗️ Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│                        .NET Aspire AppHost                          │
│                     (Service Orchestration)                         │
├──────────────────┬───────────────────┬───────────────────────────────┤
│                  │                   │                               │
│  ┌───────────┐   │  ┌────────────┐   │   ┌──────────────────────┐   │
│  │  Desktop   │   │  │    Web     │   │   │     AATS.API         │   │
│  │  Client    │   │  │  Frontend  │   │   │  (ASP.NET Core)      │   │
│  │ (Avalonia) │   │  │  (React)   │   │   │                      │   │
│  │            │◄──┼──┤            │◄──┼───┤  • REST Controllers  │   │
│  │  • MVVM    │   │  │  • Vite    │   │   │  • JWT Auth          │   │
│  │  • XAML    │   │  │  • SPA     │   │   │  • Swagger/OpenAPI   │   │
│  └───────────┘   │  └────────────┘   │   └──────────┬───────────┘   │
│                  │                   │              │               │
├──────────────────┴───────────────────┴──────────────┼───────────────┤
│                                                     │               │
│  ┌──────────────────────────────────────────────────┼────────────┐  │
│  │                   Backend Layers                  │            │  │
│  │                                                   ▼            │  │
│  │  ┌─────────────┐   ┌──────────────┐   ┌────────────────────┐  │  │
│  │  │   Domain    │   │ Application  │   │  Infrastructure    │  │  │
│  │  │             │   │              │   │                    │  │  │
│  │  │  • Entities │◄──┤  • Interfaces│◄──┤  • EF Core / PG   │  │  │
│  │  │  • Enums    │   │  • DTOs      │   │  • Repositories    │  │  │
│  │  │  • Base     │   │  • Common    │   │  • Auth Service    │  │  │
│  │  │    Classes  │   │              │   │  • Email (SMTP)    │  │  │
│  │  └─────────────┘   └──────────────┘   │  • R2 Storage      │  │  │
│  │                                       └────────────────────┘  │  │
│  └───────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │               External Services                              │    │
│  │  PostgreSQL (Supabase)  •  Cloudflare R2  •  SMTP (Gmail)   │    │
│  └─────────────────────────────────────────────────────────────┘    │
└──────────────────────────────────────────────────────────────────────┘
```

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **Orchestration** | `AATS.AppHost` | .NET Aspire host — orchestrates API, server, and web frontend |
| **Presentation** | `AATS.Desktop` | Native Windows desktop client built with Avalonia UI + MVVM |
| **Presentation** | `frontend` | Web SPA built with React 19 + Vite |
| **API** | `AATS.API` | ASP.NET Core Web API with REST controllers and JWT authentication |
| **Application** | `AATS.Application` | Interfaces, DTOs, and shared contracts |
| **Domain** | `AATS.Domain` | Core entity models and business enums |
| **Infrastructure** | `AATS.Infrastructure` | EF Core persistence, repository pattern, external services |

---

## 📋 Modules & Features

### 🔍 Audit & Assurance
| Module | Description |
|--------|-------------|
| **Audit & Assurance** | Statutory audit engagement tracking with multi-step workflows |
| **Forensic Audit** | Forensic investigation records with period-based tracking |
| **Internal Audit** | Internal audit engagement management |
| **Internal Control** | Internal control review and assessment tracking |
| **Management Accounts** | Preparation and delivery of management accounts |
| **Other Audit Services** | Custom audit-adjacent engagements |

### 💰 Tax Filing
| Module | Description |
|--------|-------------|
| **Tax Accounts** | Tax computation and advisory record management |
| **VAT Filing** | Value Added Tax returns with turnover and input/output tracking |
| **CIT Filing** | Corporate Income Tax filings with profit, expenses, and liability |
| **IIT Filing** | Individual Income Tax filings with multi-source income breakdown |
| **SSCL Filing** | Social Security Contribution Levy calculations |
| **WHT Filing** | Withholding Tax returns with rate and liability management |

### 📝 Secretarial & Advisory
| Module | Description |
|--------|-------------|
| **Company Registration** | Full incorporation workflow including directors, secretaries, shareholders |
| **EPF / ETF** | Employee Provident Fund & Trust Fund registration with staff management |
| **BOI Registration** | Board of Investment registration and investment tracking |
| **Trade Mark** | Trademark registration and renewal management |
| **Trade License** | Business trade license applications and compliance |
| **Import/Export Clearance** | Import/export compliance with TIN-based tracking |
| **Business Plan & Valuation** | Business plan preparation and company valuation services |
| **HR & Management Consulting** | Human resources and management advisory services |
| **Form 15 Filing** | Statutory Form 15 filing support |

### 🏢 Core Platform
| Feature | Description |
|---------|-------------|
| **Dashboard** | Real-time analytics with interactive charts (LiveCharts) |
| **Client Management** | Full CRM with status tracking, categorization, and revenue analytics |
| **Team Management** | Staff directory, role-based access (Admin / Manager / Staff) |
| **Multi-Branch Support** | Branch-level data isolation and cross-branch reporting |
| **Document Management** | Cloudflare R2-powered document storage with per-record attachments |
| **Payment Tracking** | Payment records with cheque detail management and partial payments |
| **Activity Logging** | Comprehensive audit trail of all user actions across modules |
| **Real-Time Notifications** | SignalR-powered push notifications |
| **PDF Report Generation** | Professional PDF reports via QuestPDF |
| **Email Notifications** | Automated SMTP-based email alerts |
| **Nexora Integration** | Web service request intake from the Nexora platform |
| **Outstanding Balances** | Firm-wide receivables tracking and balance reporting |

---

## 🛠️ Tech Stack

### Backend
| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10.0 | Runtime & SDK |
| ASP.NET Core | 10.0 | Web API framework |
| .NET Aspire | 13.2 | Cloud-native orchestration |
| Entity Framework Core | 9.0 | ORM & data access |
| Npgsql | 9.0 | PostgreSQL provider |
| JWT Bearer | 9.0 | Authentication |
| Swagger / Swashbuckle | 7.2 | API documentation |
| BCrypt.NET | 4.0 | Password hashing |
| AWS SDK (S3) | 4.0 | Cloudflare R2 storage |

### Desktop Frontend
| Technology | Version | Purpose |
|------------|---------|---------|
| Avalonia UI | 11.3 | Cross-platform XAML UI framework |
| CommunityToolkit.Mvvm | 8.2 | MVVM architecture toolkit |
| LiveCharts2 | 2.0 | Interactive data visualization |
| QuestPDF | 2026.2 | PDF report generation |
| SignalR Client | 10.0 | Real-time communication |
| Font Awesome Icons | 9.6 | Icon library |
| PdfiumViewer | 2.13 | In-app PDF preview |

### Web Frontend
| Technology | Version | Purpose |
|------------|---------|---------|
| React | 19.x | UI component library |
| Vite | 8.x | Build tool & dev server |
| OxLint | 1.71 | Fast code linting |

### Infrastructure
| Service | Purpose |
|---------|---------|
| PostgreSQL (Supabase) | Primary database |
| Cloudflare R2 | Object storage for documents |
| Gmail SMTP | Transactional email delivery |

---

## 🚀 Getting Started

### Prerequisites

| Requirement | Minimum Version |
|-------------|-----------------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0+ |
| [Node.js](https://nodejs.org/) | 20.x+ |
| [PostgreSQL](https://www.postgresql.org/) | 15+ (or Supabase) |
| IDE | Visual Studio 2022+ / Rider / VS Code |

### 1. Clone the Repository

```bash
git clone https://github.com/nexora-sw-solutions/AATS.git
cd AATS
```

### 2. Configure Environment

```bash
# Navigate to the backend directory
cd "AATS Backend"

# Copy the example environment file
cp .env.example .env
```

Edit `.env` with your actual credentials:

```env
# Database
ConnectionStrings__DefaultConnection=Server=<host>;Database=postgres;Port=5432;...

# JWT
Jwt__Key=<your-secret-key>
Jwt__Issuer=AATS.API
Jwt__Audience=AATS.App

# SMTP
Smtp__Host=smtp.gmail.com
Smtp__Username=<your-email>
Smtp__Password=<your-app-password>

# Cloudflare R2
CloudflareR2__AccountId=<account-id>
CloudflareR2__AccessKey=<access-key>
CloudflareR2__SecretKey=<secret-key>
CloudflareR2__BucketName=aats
```

### 3. Install Dependencies

```bash
# Backend — restore NuGet packages
cd "AATS Backend"
dotnet restore

# Frontend — install Node packages
cd "../AATS Frontend/frontend"
npm install
```

### 4. Run the Application

#### Option A: Using .NET Aspire (Recommended)

```bash
cd "AATS Backend/AATS.AppHost"
dotnet run
```

> This starts the API, server, and web frontend together with service discovery and health monitoring.

#### Option B: Running Individually

```bash
# Terminal 1 — API
cd "AATS Backend/AATS.API"
dotnet run

# Terminal 2 — Web Frontend
cd "AATS Frontend/frontend"
npm run dev

# Terminal 3 — Desktop App
cd "AATS Frontend/AATS.Desktop"
dotnet run
```

### 5. Access the Application

| Interface | URL |
|-----------|-----|
| Web Frontend | `http://localhost:5173` |
| API (Swagger) | `http://localhost:5000/swagger` |
| Desktop App | Launches natively on Windows |

---

## 📁 Project Structure

```
AATS-4.0/
├── .github/
│   └── workflows/              # CI/CD pipeline definitions
│
├── AATS Backend/
│   ├── AATS.AppHost/           # .NET Aspire orchestration host
│   ├── AATS.API/               # ASP.NET Core REST API
│   │   ├── Controllers/        # API endpoint controllers
│   │   │   ├── AuthController          # Authentication & user management
│   │   │   ├── AuditControllers        # Audit module endpoints
│   │   │   ├── TaxControllers          # Tax filing endpoints
│   │   │   ├── SecretarialControllers  # Secretarial module endpoints
│   │   │   ├── CoreControllers         # Client, branch, & system endpoints
│   │   │   ├── DashboardController     # Analytics & dashboard data
│   │   │   ├── SystemControllers       # System configuration endpoints
│   │   │   └── UploadController        # Document upload handling
│   │   └── Models/             # API request/response models
│   ├── AATS.Application/       # Application layer (interfaces & DTOs)
│   ├── AATS.Domain/            # Domain entities & enums
│   │   └── Entities/
│   │       ├── CoreEntities        # Branch, User, Client
│   │       ├── AuditAndTaxEntities # Audit & tax record types
│   │       ├── SecretarialEntities # Secretarial record types
│   │       └── SystemEntities      # Payments, Documents, Activity Logs
│   ├── AATS.Infrastructure/    # Data access & external services
│   │   ├── Persistence/        # EF Core DbContext & seeder
│   │   ├── Repositories/       # Generic repository pattern
│   │   └── Services/           # Auth, Email, R2 Storage, Records
│   └── AATS.Server/            # Server-side rendering & hosting
│
├── AATS Frontend/
│   ├── AATS.Desktop/           # Avalonia UI desktop application
│   │   ├── Views/              # XAML views organized by module
│   │   │   ├── AuditAndAccounts/     # 20+ audit module views
│   │   │   ├── SecretarialAdvisory/  # 30+ secretarial module views
│   │   │   ├── TaxFiling/           # 17+ tax filing views
│   │   │   ├── Clients/             # Client management views
│   │   │   ├── Team/                # Team management views
│   │   │   ├── Reports/             # Report generation views
│   │   │   ├── Notifications/       # Real-time notification views
│   │   │   └── Nexora/              # Nexora integration views
│   │   ├── ViewModels/         # MVVM view models
│   │   ├── Models/             # Client-side data models
│   │   ├── Services/           # API, Data, OTP, Report services
│   │   ├── Converters/         # XAML value converters
│   │   ├── Helpers/            # Utility classes
│   │   ├── Styles/             # Shared XAML styles & themes
│   │   └── Assets/             # Icons, images, fonts
│   │
│   └── frontend/               # React web application
│       ├── src/
│       │   ├── App.jsx         # Root application component
│       │   └── assets/         # Static web assets
│       ├── package.json
│       └── vite.config.js
│
└── README.md
```

---

## 🔐 Security

| Feature | Implementation |
|---------|---------------|
| **Authentication** | JWT Bearer tokens with configurable expiry |
| **Password Storage** | BCrypt hashing with salt |
| **Role-Based Access** | Admin, Manager, and Staff permission levels |
| **API Security** | CORS policies, request validation, HTTPS support |
| **Environment Secrets** | `.env` file-based configuration (excluded from VCS) |
| **OTP Verification** | One-time password service for sensitive operations |

---

## 🤝 Contributing

This is a proprietary project developed by **Nexora Software Solutions**. For internal contributors:

1. Create a feature branch from `main`
2. Follow the existing code structure and naming conventions
3. Ensure all existing tests pass before submitting
4. Submit a Pull Request with a clear description of changes

---

## 📄 License

This software is proprietary and confidential. Unauthorized copying, distribution, or modification is strictly prohibited.

**© 2026 Nexora Software Solutions. All rights reserved.**

---

<p align="center">
  <sub>Built with ❤️ by the <b>Nexora</b> team</sub>
</p>
