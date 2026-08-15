-- =============================================================================
-- AATS (Accounting, Audit, Tax & Secretarial) Database Schema for Supabase PostgreSQL
-- WARNING: THIS SCRIPT WIPES ALL EXISTING TABLES AND DATA BEFORE RECREATING
-- =============================================================================

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 0. DROP ALL EXISTING TABLES & CONSTRAINTS
DROP TABLE IF EXISTS app_notifications CASCADE;
DROP TABLE IF EXISTS activity_logs CASCADE;
DROP TABLE IF EXISTS nexora_requests CASCADE;
DROP TABLE IF EXISTS source_documents CASCADE;
DROP TABLE IF EXISTS staff_members CASCADE;
DROP TABLE IF EXISTS company_officers CASCADE;
DROP TABLE IF EXISTS tax_records CASCADE;
DROP TABLE IF EXISTS audit_records CASCADE;
DROP TABLE IF EXISTS clients CASCADE;
DROP TABLE IF EXISTS users CASCADE;
DROP TABLE IF EXISTS branches CASCADE;

-- 1. BRANCHES TABLE
CREATE TABLE branches (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL UNIQUE,
    code VARCHAR(20),
    address TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 2. USERS TABLE
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(150) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    phone VARCHAR(30),
    role INT NOT NULL DEFAULT 2, -- 1: Admin, 2: User/Staff, 3: Manager
    branch_id UUID REFERENCES branches(id) ON DELETE SET NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3. CLIENTS TABLE
CREATE TABLE clients (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(200) NOT NULL,
    email VARCHAR(150),
    phone VARCHAR(30),
    branch_id UUID REFERENCES branches(id) ON DELETE SET NULL,
    category VARCHAR(50) DEFAULT 'Active', -- Active, Black Listed, Suspended, Loyal, Corporate
    status VARCHAR(50) DEFAULT 'Active', -- Active, Inactive
    total_revenue NUMERIC(18,2) DEFAULT 0.00,
    outstanding_balance NUMERIC(18,2) DEFAULT 0.00,
    logo_storage_key TEXT,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 4. AUDIT & SECRETARIAL RECORDS TABLE
CREATE TABLE audit_records (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_code VARCHAR(50),
    category VARCHAR(100) NOT NULL, -- e.g. "Assurance", "Internal Audit", "Company Registration", "EPF / ETF", etc.
    client_id UUID REFERENCES clients(id) ON DELETE SET NULL,
    client_name VARCHAR(200),
    client_code VARCHAR(50),
    created_by UUID REFERENCES users(id) ON DELETE SET NULL,
    created_by_name VARCHAR(100),
    branch_id UUID REFERENCES branches(id) ON DELETE SET NULL,
    branch_name VARCHAR(100),
    company_name VARCHAR(200),
    company_type VARCHAR(100),
    status VARCHAR(50) DEFAULT 'ACTIVE', -- ACTIVE, COMPLETED, etc.
    payment_status VARCHAR(50) DEFAULT 'Unpaid', -- Paid, Unpaid, Partial
    process VARCHAR(50) DEFAULT 'DRAFT', -- BOOKKEEP, DRAFT, FINALIZE, IN PROGRESS, COMPLETED, REVIEW, SUBMITTED, ISSUE RAISED
    current_step INT DEFAULT 1,
    sub_total NUMERIC(18,2) DEFAULT 0.00,
    discount NUMERIC(18,2) DEFAULT 0.00,
    total_payment NUMERIC(18,2) DEFAULT 0.00,
    partial_amount NUMERIC(18,2) DEFAULT 0.00,
    payment_option VARCHAR(50), -- Online, Cash, Cheque, Transfer
    assignment VARCHAR(200),
    no_of_staffs INT DEFAULT 0,
    country VARCHAR(100),
    country_address TEXT,
    notes TEXT,
    period VARCHAR(50),
    tin VARCHAR(50),
    director_id VARCHAR(50),
    investment_value VARCHAR(100),
    investment_value_usd NUMERIC(18,2),
    period_number VARCHAR(50),
    period_type VARCHAR(50),
    cheque_bank VARCHAR(100),
    cheque_number VARCHAR(50),
    cheque_date TIMESTAMPTZ,
    cheque_amount NUMERIC(18,2),
    cheque_status VARCHAR(50),
    login_id VARCHAR(100),
    password VARCHAR(100),
    address TEXT,
    email VARCHAR(150),
    phone VARCHAR(30),
    objective TEXT,
    description TEXT,
    bo_responsible_person_name VARCHAR(200),
    bo_responsible_person_nic_file_name TEXT,
    record_date TIMESTAMPTZ DEFAULT NOW(),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 5. TAX RECORDS TABLE
CREATE TABLE tax_records (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_code VARCHAR(50),
    tax_type VARCHAR(100) NOT NULL, -- e.g. "VAT", "CIT", "IIT", "SSCL", "WHT", "filings", "records"
    client_id UUID REFERENCES clients(id) ON DELETE SET NULL,
    client_name VARCHAR(200),
    client_code VARCHAR(50),
    client_name_sub VARCHAR(200),
    director_id VARCHAR(50),
    tin VARCHAR(50),
    period VARCHAR(50),
    period_number VARCHAR(50),
    period_type VARCHAR(50),
    status VARCHAR(50) DEFAULT 'Pending', -- Pending, Paid, IRD pending, IRD Paid
    process VARCHAR(50) DEFAULT 'DRAFT',
    total_payment NUMERIC(18,2) DEFAULT 0.00,
    branch_id UUID REFERENCES branches(id) ON DELETE SET NULL,
    branch_name VARCHAR(100),
    created_by UUID REFERENCES users(id) ON DELETE SET NULL,
    created_by_name VARCHAR(100),
    notes TEXT,
    record_date TIMESTAMPTZ DEFAULT NOW(),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 6. COMPANY OFFICERS TABLE (Directors, Secretaries, Shareholders, Others)
CREATE TABLE company_officers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_id UUID REFERENCES audit_records(id) ON DELETE CASCADE,
    name VARCHAR(200) NOT NULL,
    position VARCHAR(100) NOT NULL, -- Director, Secretary, Shareholder, Other
    nic_number VARCHAR(50),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 7. STAFF MEMBERS TABLE (EPF/ETF Staff List)
CREATE TABLE staff_members (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_id UUID REFERENCES audit_records(id) ON DELETE CASCADE,
    staff_code VARCHAR(50),
    name VARCHAR(200) NOT NULL,
    phone VARCHAR(30),
    process_status VARCHAR(50) DEFAULT 'PROCESSING',
    history_json JSONB DEFAULT '[]'::jsonb, -- Array of { date, description, amount }
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 8. SOURCE DOCUMENTS TABLE (File Attachments)
CREATE TABLE source_documents (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_id UUID, -- References client_id, audit_records.id, or tax_records.id
    record_type VARCHAR(50) DEFAULT 'Audit', -- Audit, Client, Secretarial, Tax
    file_name VARCHAR(255) NOT NULL,
    url TEXT NOT NULL,
    description TEXT,
    file_size BIGINT DEFAULT 0,
    file_type VARCHAR(100),
    attachment_category VARCHAR(100), -- BR, TIN, Form01, ArticleOfAssociation, NIC, Form05, etc.
    uploader_id UUID REFERENCES users(id) ON DELETE SET NULL,
    uploader_name VARCHAR(100),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 9. NEXORA REQUESTS TABLE
CREATE TABLE nexora_requests (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id UUID REFERENCES clients(id) ON DELETE SET NULL,
    client_name VARCHAR(200),
    service_type VARCHAR(100),
    details TEXT,
    status VARCHAR(50) DEFAULT 'PENDING',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 10. ACTIVITY LOGS TABLE
CREATE TABLE activity_logs (
    id BIGSERIAL PRIMARY KEY,
    user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    user_name VARCHAR(100),
    branch_id UUID REFERENCES branches(id) ON DELETE SET NULL,
    branch_name VARCHAR(100),
    action VARCHAR(100) NOT NULL,
    module VARCHAR(100) NOT NULL,
    description TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 11. APP NOTIFICATIONS TABLE
CREATE TABLE app_notifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    title VARCHAR(200) NOT NULL,
    message TEXT NOT NULL,
    is_read BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- INDEXES
CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_clients_code ON clients(client_code);
CREATE INDEX idx_clients_branch ON clients(branch_id);
CREATE INDEX idx_audit_category ON audit_records(category);
CREATE INDEX idx_audit_client ON audit_records(client_id);
CREATE INDEX idx_tax_type ON tax_records(tax_type);
CREATE INDEX idx_tax_client ON tax_records(client_id);
CREATE INDEX idx_source_docs_record ON source_documents(record_id);
CREATE INDEX idx_activity_user ON activity_logs(user_id);

-- =============================================================================
-- INITIAL SEED DATA
-- =============================================================================

-- Default Branches
INSERT INTO branches (id, name, code) VALUES
    ('00000000-0000-0000-0000-000000000001', 'Central', 'BR-CENTRAL'),
    ('00000000-0000-0000-0000-000000000002', 'South', 'BR-SOUTH'),
    ('00000000-0000-0000-0000-000000000003', 'West', 'BR-WEST'),
    ('00000000-0000-0000-0000-000000000004', 'Northeast', 'BR-NE')
ON CONFLICT (name) DO NOTHING;

-- Default Admin User (Username: admin, Password: Admin@123)
INSERT INTO users (id, username, email, password_hash, role, branch_id) VALUES
    ('11111111-1111-1111-1111-111111111111', 'admin', 'admin@aats.com', '$2a$11$q09l9rO8f9ZkXz7K.nB26eRzVq.9V0hQZ1z8eZ7kXz7K.nB26eRzV', 1, '00000000-0000-0000-0000-000000000001')
ON CONFLICT (username) DO NOTHING;
