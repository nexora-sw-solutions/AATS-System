# AATS - Accounting, Audit, Tax & Secretarial Desktop Application

AATS is an enterprise desktop management solution built with **Avalonia UI (.NET 10)** for the frontend desktop client, **C# .NET 10 Web API** for the backend services, and **Supabase PostgreSQL** for cloud data persistence.

---

## 🚀 Quick Start Guide for Testers

### Prerequisites
Before running the application, ensure your environment has the following installed:
1. **.NET 10 SDK** (Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0))
2. **Git** (for cloning and branch management)
3. Access to **Supabase PostgreSQL** (or your target PostgreSQL database instance).

---

## 🛠️ Step 1: Database Setup (Supabase)

If setting up or updating a database on Supabase, execute the following SQL migration script in your **Supabase SQL Editor**:

```sql
-- Add Soft Delete & Status Columns
ALTER TABLE branches ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT FALSE, ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMPTZ NULL;
ALTER TABLE users ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT FALSE, ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMPTZ NULL, ADD COLUMN IF NOT EXISTS status VARCHAR(50) DEFAULT 'Active';
ALTER TABLE clients ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT FALSE, ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMPTZ NULL;
ALTER TABLE audit_records ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT FALSE, ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMPTZ NULL;
ALTER TABLE tax_records ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT FALSE, ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMPTZ NULL;
ALTER TABLE company_officers ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT FALSE, ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMPTZ NULL;
ALTER TABLE staff_members ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT FALSE, ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMPTZ NULL;
ALTER TABLE source_documents ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT FALSE, ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMPTZ NULL;
ALTER TABLE nexora_requests ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT FALSE, ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMPTZ NULL;
ALTER TABLE app_notifications ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT FALSE, ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMPTZ NULL;

-- Soft Delete Indexes
CREATE INDEX IF NOT EXISTS idx_branches_is_deleted ON branches(is_deleted);
CREATE INDEX IF NOT EXISTS idx_users_is_deleted ON users(is_deleted);
CREATE INDEX IF NOT EXISTS idx_clients_is_deleted ON clients(is_deleted);
CREATE INDEX IF NOT EXISTS idx_audit_records_is_deleted ON audit_records(is_deleted);
CREATE INDEX IF NOT EXISTS idx_tax_records_is_deleted ON tax_records(is_deleted);
```

---

## 🖥️ Step 2: Running the Application

### 1. Launch Backend API
Open a terminal in the root project directory:
```bash
cd "AATS Backend\AATS.API"
dotnet run
```
The API server will launch locally at `http://localhost:5000` (or `https://localhost:5001`).

### 2. Launch Desktop Application
Open a second terminal window:
```bash
cd "AATS Frontend\AATS.Desktop"
dotnet run
```
The Avalonia desktop application window will open automatically.

---

## 🔐 Default Test Login Credentials

| Role | Username | Access Scope |
| :--- | :--- | :--- |
| **Admin** | `admin` | Full access to all pages & administrative actions |
| **Audit** | `audit_user` | Client, Dashboard, Audit & Assurance section |
| **Tax** | `tax_user` | Client, Dashboard, Tax Filing section |
| **Secretarial** | `sec_user` | Client, Dashboard, Secretarial & Advisory section |

---
- [ ] Verify that branch names display actual branch names (e.g. `Central`, `South`) instead of `Unknown branch`.
