-- ============================================================================
-- AATS Management System — PostgreSQL Database Schema
-- Version: 1.0.0
-- Date: 2026-03-21
-- Description: Complete DDL for the AATS (Audit, Accounting, Tax, Secretarial)
--              management system. Designed for PostgreSQL 15+ with Dotmim.Sync
--              bi-directional synchronization support.
-- ============================================================================

-- Enable UUID generation
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ============================================================================
-- UTILITY: Auto-update trigger function for updated_at timestamps
-- ============================================================================
CREATE OR REPLACE FUNCTION trigger_set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


-- ############################################################################
-- DOMAIN 1: CORE & AUTH
-- ############################################################################

-- ============================================================================
-- TABLE: branches
-- Lookup table for the 4 firm branches.
-- ============================================================================
CREATE TABLE branches (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(100)    NOT NULL UNIQUE,
    code            VARCHAR(20)     NOT NULL UNIQUE,
    address         TEXT,
    phone           VARCHAR(30),
    is_active       BOOLEAN         NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

CREATE TRIGGER set_branches_updated_at
    BEFORE UPDATE ON branches
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

-- Seed data: 4 branches
INSERT INTO branches (name, code) VALUES
    ('South',       'SOUTH'),
    ('West',        'WEST'),
    ('Central',     'CENTRAL'),
    ('Northeast',   'NORTHEAST');


-- ============================================================================
-- TABLE: users
-- System users (auditors, admins, staff, managers).
-- ============================================================================
CREATE TABLE users (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username        VARCHAR(100)    NOT NULL UNIQUE,
    email           VARCHAR(255)    NOT NULL UNIQUE,
    phone           VARCHAR(30),
    user_logo       VARCHAR(1000),
    password_hash   VARCHAR(255)    NOT NULL,
    role            VARCHAR(20)     NOT NULL CHECK (role IN ('Admin', 'Staff')),
    branch_id       UUID            NOT NULL REFERENCES branches(id),
    is_active       BOOLEAN         NOT NULL DEFAULT TRUE,
    last_login_at   TIMESTAMPTZ,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_users_branch      ON users(branch_id);
CREATE INDEX idx_users_role        ON users(role);
CREATE INDEX idx_users_email       ON users(email);
CREATE INDEX idx_users_is_deleted  ON users(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_users_updated_at
    BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ============================================================================
-- TABLE: clients
-- All firm clients (Corporate, Individual, SME).
-- ============================================================================
CREATE TABLE clients (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_code         VARCHAR(20)     NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    client_name         VARCHAR(255)    NOT NULL,
    email               VARCHAR(255),
    phone               VARCHAR(30),
    --category            VARCHAR(20)     CHECK (category IN ('Corporate', 'Individual', 'SME')),
    status              VARCHAR(20)     NOT NULL DEFAULT 'Active' CHECK (status IN ('Active', 'Inactive', 'Pending')),
    branch_id           UUID            REFERENCES branches(id),
    total_revenue       NUMERIC(15,2)   NOT NULL DEFAULT 0,
    outstanding_balance NUMERIC(15,2)   NOT NULL DEFAULT 0,
    logo_storage_key    VARCHAR(1000),
    --address             TEXT,
    --tin_number          VARCHAR(50),
    last_active_at      TIMESTAMPTZ,
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted          BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_clients_branch     ON clients(branch_id);
--CREATE INDEX idx_clients_category   ON clients(category);
CREATE INDEX idx_clients_status     ON clients(status);
CREATE INDEX idx_clients_code       ON clients(client_code);
CREATE INDEX idx_clients_is_deleted ON clients(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_clients_updated_at
    BEFORE UPDATE ON clients
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ############################################################################
-- DOMAIN 2: ACCOUNTS & AUDIT
-- ############################################################################

-- ============================================================================
-- TABLE: audit_assurance_records
-- Audit & Assurance service records.
-- ============================================================================
CREATE TABLE audit_assurance_records (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_code     VARCHAR(20)     NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    record_date     DATE            NOT NULL,
    client_id       UUID            REFERENCES clients(id),
    client_name     VARCHAR(255)    NOT NULL,
    branch_id       UUID            REFERENCES branches(id),
    client_logo     VARCHAR(1000),
    payment_status  VARCHAR(20)     CHECK (payment_status IN ('Paid', 'Unpaid', 'Partial')),
    process         VARCHAR(50)     NOT NULL DEFAULT 'Bookkeep' CHECK (process IN ('Bookkeep', 'Draft Account', 'Finalize', 'Handover', 'Return', 'Submit')),
    assignment      TEXT,
    sub_total       NUMERIC(15,2)   NOT NULL DEFAULT 0,
    discount        NUMERIC(15,2)   NOT NULL DEFAULT 0,
    total_payment   NUMERIC(15,2)   NOT NULL DEFAULT 0,
    partial_amount  NUMERIC(15,2)   NOT NULL DEFAULT 0,
    payment_option  VARCHAR(20)     CHECK (payment_option IN ('Cash', 'Online', 'Cheque')),
    notes           TEXT,
    created_by      UUID            REFERENCES users(id),
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_audit_assurance_client     ON audit_assurance_records(client_id);
CREATE INDEX idx_audit_assurance_branch     ON audit_assurance_records(branch_id);
CREATE INDEX idx_audit_assurance_date       ON audit_assurance_records(record_date);
CREATE INDEX idx_audit_assurance_status     ON audit_assurance_records(payment_status);
CREATE INDEX idx_audit_assurance_deleted    ON audit_assurance_records(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_audit_assurance_updated_at
    BEFORE UPDATE ON audit_assurance_records
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ============================================================================
-- TABLE: forensic_audit_records
-- Forensic Audit service records with period tracking.
-- ============================================================================
CREATE TABLE forensic_audit_records (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_code     VARCHAR(20)     NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    record_date     DATE            NOT NULL,
    client_id       UUID            REFERENCES clients(id),
    client_name     VARCHAR(255)    NOT NULL,
    branch_id       UUID            REFERENCES branches(id),
    client_logo     VARCHAR(1000),
    payment_status  VARCHAR(20)     CHECK (payment_status IN ('Paid', 'Unpaid', 'Partial')),
    process         VARCHAR(50)     NOT NULL DEFAULT 'Reporting' check (process in ('Reporting', 'Meeting Complete')),
    assignment      TEXT,
    period_number   VARCHAR(20),
    period_type     VARCHAR(10)     CHECK (period_type IN ('Date', 'Month', 'Year')),
    sub_total       NUMERIC(15,2)   NOT NULL DEFAULT 0,
    discount        NUMERIC(15,2)   NOT NULL DEFAULT 0,
    total_payment   NUMERIC(15,2)   NOT NULL DEFAULT 0,
    partial_amount  NUMERIC(15,2)   NOT NULL DEFAULT 0,
    payment_option  VARCHAR(20)     CHECK (payment_option IN ('Cash', 'Online', 'Cheque')),
    notes           TEXT,
    created_by      UUID            REFERENCES users(id),
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_forensic_audit_client  ON forensic_audit_records(client_id);
CREATE INDEX idx_forensic_audit_branch  ON forensic_audit_records(branch_id);
CREATE INDEX idx_forensic_audit_date    ON forensic_audit_records(record_date);
CREATE INDEX idx_forensic_audit_deleted ON forensic_audit_records(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_forensic_audit_updated_at
    BEFORE UPDATE ON forensic_audit_records
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ============================================================================
-- TABLE: internal_audit_records
-- Internal Audit service records with period tracking.
-- ============================================================================
CREATE TABLE internal_audit_records (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_code     VARCHAR(20)     NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    record_date     DATE            NOT NULL,
    client_id       UUID            REFERENCES clients(id),
    client_name     VARCHAR(255)    NOT NULL,
    branch_id       UUID            REFERENCES branches(id),
    client_logo     VARCHAR(1000),
    payment_status  VARCHAR(20)     CHECK (payment_status IN ('Paid', 'Unpaid', 'Partial')),
    process         VARCHAR(50)     NOT NULL DEFAULT 'Reporting' check (process in ('Reporting', 'Meeting Complete')),
    assignment      TEXT,
    period_number   VARCHAR(20),
    period_type     VARCHAR(10)     CHECK (period_type IN ('Date', 'Month', 'Year')),
    sub_total       NUMERIC(15,2)   NOT NULL DEFAULT 0,
    discount        NUMERIC(15,2)   NOT NULL DEFAULT 0,
    total_payment   NUMERIC(15,2)   NOT NULL DEFAULT 0,
    partial_amount  NUMERIC(15,2)   NOT NULL DEFAULT 0,
    payment_option  VARCHAR(20)     CHECK (payment_option IN ('Cash', 'Online', 'Cheque')),
    notes           TEXT,
    created_by      UUID            REFERENCES users(id),
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_internal_audit_client  ON internal_audit_records(client_id);
CREATE INDEX idx_internal_audit_branch  ON internal_audit_records(branch_id);
CREATE INDEX idx_internal_audit_date    ON internal_audit_records(record_date);
CREATE INDEX idx_internal_audit_deleted ON internal_audit_records(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_internal_audit_updated_at
    BEFORE UPDATE ON internal_audit_records
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ============================================================================
-- TABLE: management_account_records
-- Management Account service records with source document support.
-- ============================================================================
CREATE TABLE management_account_records (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_code     VARCHAR(20)     NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    record_date     DATE            NOT NULL,
    client_id       UUID            REFERENCES clients(id),
    client_name     VARCHAR(255)    NOT NULL,
    branch_id       UUID            REFERENCES branches(id),
    client_logo     VARCHAR(1000),
    payment_status  VARCHAR(20)     CHECK (payment_status IN ('Paid', 'Unpaid', 'Partial')),
    process         VARCHAR(50)     NOT NULL DEFAULT 'Bookkeep' check (process in ('Bookkeep', 'Draft Account', 'Finalize', 'Handover')),
    assignment      TEXT,
    sub_total       NUMERIC(15,2)   NOT NULL DEFAULT 0,
    discount        NUMERIC(15,2)   NOT NULL DEFAULT 0,
    total_payment   NUMERIC(15,2)   NOT NULL DEFAULT 0,
    partial_amount  NUMERIC(15,2)   NOT NULL DEFAULT 0,
    payment_option  VARCHAR(20)     CHECK (payment_option IN ('Cash', 'Online', 'Cheque')),
    notes           TEXT,
    created_by      UUID            REFERENCES users(id),
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_mgmt_account_client    ON management_account_records(client_id);
CREATE INDEX idx_mgmt_account_branch    ON management_account_records(branch_id);
CREATE INDEX idx_mgmt_account_date      ON management_account_records(record_date);
CREATE INDEX idx_mgmt_account_deleted   ON management_account_records(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_mgmt_account_updated_at
    BEFORE UPDATE ON management_account_records
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ============================================================================
-- TABLE: internal_control_records
-- Internal Control Systems & Outsourcing records with billing.
-- ============================================================================
CREATE TABLE internal_control_records (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_code     VARCHAR(20)     NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    record_date     DATE            NOT NULL,
    client_id       UUID            REFERENCES clients(id),
    client_name     VARCHAR(255)    NOT NULL,
    branch_id       UUID            REFERENCES branches(id),
    client_logo     VARCHAR(1000),
    assignment      TEXT,
    period_number   VARCHAR(20),
    period_type     VARCHAR(10)     CHECK (period_type IN ('Date', 'Month', 'Year')),
    payment_status  VARCHAR(20)     CHECK (payment_status IN ('Paid', 'Unpaid', 'Partial')),
    process         VARCHAR(50)     NOT NULL DEFAULT 'Reporting' check (process in ('Reporting', 'Meeting Complete')),
    sub_total       NUMERIC(15,2)   NOT NULL DEFAULT 0,
    discount        NUMERIC(15,2)   NOT NULL DEFAULT 0,
    total_payment   NUMERIC(15,2)   NOT NULL DEFAULT 0,
    partial_amount  NUMERIC(15,2)   NOT NULL DEFAULT 0,
    payment_option  VARCHAR(20)     CHECK (payment_option IN ('Cash', 'Online', 'Cheque')),
    notes           TEXT,
    created_by      UUID            REFERENCES users(id),
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_internal_ctrl_client   ON internal_control_records(client_id);
CREATE INDEX idx_internal_ctrl_branch   ON internal_control_records(branch_id);
CREATE INDEX idx_internal_ctrl_date     ON internal_control_records(record_date);
CREATE INDEX idx_internal_ctrl_deleted  ON internal_control_records(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_internal_ctrl_updated_at
    BEFORE UPDATE ON internal_control_records
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ============================================================================
-- TABLE: tax_account_records
-- Tax Account records with detailed billing and assignment info.
-- ============================================================================
CREATE TABLE tax_account_records (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_code     VARCHAR(20)     NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    record_date     DATE            NOT NULL,
    client_id       UUID            REFERENCES clients(id),
    client_name     VARCHAR(255)    NOT NULL,
    branch_id       UUID            REFERENCES branches(id),
    client_logo     VARCHAR(1000),
    assigned_to     UUID            REFERENCES users(id),
    payment_status  VARCHAR(20)     CHECK (payment_status IN ('Paid', 'Unpaid', 'Partial')),
    process         VARCHAR(50)     NOT NULL DEFAULT 'Bookkeep' check (process IN ('Bookkeep', 'Tax Amount', 'Finalize', 'Tax Paid', 'Submit')),
    sub_total       NUMERIC(15,2)   NOT NULL DEFAULT 0,
    discount        NUMERIC(15,2)   NOT NULL DEFAULT 0,
    total_payment   NUMERIC(15,2)   NOT NULL DEFAULT 0,
    partial_amount  NUMERIC(15,2)   NOT NULL DEFAULT 0,
    payment_option  VARCHAR(20)     CHECK (payment_option IN ('Cash', 'Online', 'Cheque')),
    notes           TEXT,
    created_by      UUID            REFERENCES users(id),
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_tax_account_client     ON tax_account_records(client_id);
CREATE INDEX idx_tax_account_branch     ON tax_account_records(branch_id);
CREATE INDEX idx_tax_account_assigned   ON tax_account_records(assigned_to);
CREATE INDEX idx_tax_account_date       ON tax_account_records(record_date);
CREATE INDEX idx_tax_account_deleted    ON tax_account_records(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_tax_account_updated_at
    BEFORE UPDATE ON tax_account_records
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ============================================================================
-- TABLE: other_audit_records
-- Others
-- ============================================================================

CREATE TABLE other_audit_records (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_code     VARCHAR(20)     NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    record_date     DATE            NOT NULL,
    client_id       UUID            REFERENCES clients(id),
    client_name     VARCHAR(255)    NOT NULL,
    company         VARCHAR(255)    NOT NULL,
    branch_id       UUID            REFERENCES branches(id),
    assignment      TEXT,
    payment_status  VARCHAR(20)     CHECK (payment_status IN ('Paid', 'Unpaid', 'Partial')),
    sub_total       NUMERIC(15,2)   NOT NULL DEFAULT 0,
    discount        NUMERIC(15,2)   NOT NULL DEFAULT 0,
    total_payment   NUMERIC(15,2)   NOT NULL DEFAULT 0,
    partial_amount  NUMERIC(15,2)   NOT NULL DEFAULT 0,
    payment_option  VARCHAR(20)     CHECK (payment_option IN ('Cash', 'Online', 'Cheque')),
    description     TEXT,
    created_by      UUID            REFERENCES users(id),
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_other_audit_client   ON other_audit_records(client_id);
CREATE INDEX idx_other_audit_branch   ON other_audit_records(branch_id);
CREATE INDEX idx_other_audit_date     ON other_audit_records(record_date);
CREATE INDEX idx_other_audit_deleted  ON other_audit_records(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_other_audit_updated_at
    BEFORE UPDATE ON other_audit_records
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();



-- ############################################################################
-- DOMAIN 3: TAX
-- ############################################################################

-- ============================================================================
-- TABLE: tax_filings
-- Tax filing records for CIT, IIT, VAT, SSCL, WHT.
-- ============================================================================
CREATE TABLE tax_filings (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    filing_code     VARCHAR(20)     NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    tax_type        VARCHAR(10)     NOT NULL CHECK (tax_type IN ('CIT', 'IIT', 'VAT', 'SSCL', 'WHT', 'Others')),
    client_id       UUID            REFERENCES clients(id),
    client_name     VARCHAR(255)    NOT NULL,
    branch_id       UUID            REFERENCES branches(id),
    filing_date     DATE            NOT NULL DEFAULT NOW(),
    tax_number      VARCHAR(20)     NOT NULL,
    period_number   VARCHAR(20),
    period_type     VARCHAR(10)     CHECK (period_type IN ('Date', 'Month', 'Year')),
    payment_status  VARCHAR(20)     CHECK (payment_status IN ('Paid', 'Pending', 'IRD Paid')),
    notes           TEXT,
    created_by      UUID            REFERENCES users(id),
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_tax_filings_client     ON tax_filings(client_id);
CREATE INDEX idx_tax_filings_type       ON tax_filings(tax_type);
CREATE INDEX idx_tax_filings_branch     ON tax_filings(branch_id);
CREATE INDEX idx_tax_filings_date       ON tax_filings(filing_date);
CREATE INDEX idx_tax_filings_number     ON tax_filings(tax_number);
CREATE INDEX idx_tax_filings_deleted    ON tax_filings(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_tax_filings_updated_at
    BEFORE UPDATE ON tax_filings
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ############################################################################
-- DOMAIN 4: SECRETARIAL & ADVISORY
-- ############################################################################

-- ============================================================================
-- TABLE: company_registrations
-- Company Registration service records with TIN data support.
-- ============================================================================
CREATE TABLE company_registrations (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    registration_code   VARCHAR(20)     NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    registration_date   DATE            NOT NULL DEFAULT NOW(),
    client_id           UUID            REFERENCES clients(id),
    client_name         VARCHAR(255)    NOT NULL,
    company_name        VARCHAR(255)    NOT NULL,
    company_type        VARCHAR(100), -- Private Limited, etc.
    objective           TEXT,
    address             TEXT,
    email               VARCHAR(255),
    phone               VARCHAR(30),
    payment_status      VARCHAR(20)     CHECK (payment_status IN ('Paid', 'Unpaid', 'Partial')),
    process             VARCHAR(50)     CHECK (process IN ('Name Approve', 'Forms Preparation', 'Signature', 'Payment', 'Incorporation', 'Seal', 'Certified copy', 'Document hand over')),
    description         TEXT,
    sub_total           NUMERIC(15,2)   NOT NULL DEFAULT 0,
    discount            NUMERIC(15,2)   NOT NULL DEFAULT 0,
    total_payment       NUMERIC(15,2)   NOT NULL DEFAULT 0,
    partial_amount      NUMERIC(15,2)   NOT NULL DEFAULT 0,
    payment_option      VARCHAR(20)     CHECK (payment_option IN ('Cash', 'Online', 'Cheque')),
    branch_id           UUID            REFERENCES branches(id),
    created_by          UUID            REFERENCES users(id),
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted          BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_comp_reg_client    ON company_registrations(client_id);
CREATE INDEX idx_comp_reg_branch    ON company_registrations(branch_id);
CREATE INDEX idx_comp_reg_date      ON company_registrations(registration_date);
CREATE INDEX idx_comp_reg_deleted   ON company_registrations(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_comp_reg_updated_at
    BEFORE UPDATE ON company_registrations
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ============================================================================
-- TABLE: company_officers
-- Directors, secretaries, and other officers for company registrations.
-- ============================================================================
CREATE TABLE company_officers (
    id                          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_registration_id     UUID            NOT NULL REFERENCES company_registrations(id) ON DELETE CASCADE,
    name                        VARCHAR(255)    NOT NULL,
    position                    VARCHAR(100),
    officer_type                VARCHAR(30)     NOT NULL CHECK (officer_type IN ('director', 'secretary', 'alternate_director', 'shareholder', 'other')),
    share_percentage            NUMERIC(5,2),
    nic_number                  VARCHAR(30),
    created_at                  TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at                  TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_comp_officers_reg ON company_officers(company_registration_id);

CREATE TRIGGER set_comp_officers_updated_at
    BEFORE UPDATE ON company_officers
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ============================================================================
-- TABLE: epf_etf_records
-- Employee Provident Fund & Employee Trust Fund records.
-- ============================================================================
CREATE TABLE epf_etf_records (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_code     VARCHAR(20)     NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    record_date     DATE            NOT NULL,
    client_id       UUID            REFERENCES clients(id),
    client_name     VARCHAR(255)    NOT NULL,
    company_name    VARCHAR(255)    NOT NULL,
    number_of_staff INTEGER         NOT NULL DEFAULT 0,
    process         VARCHAR(50),
    phone           VARCHAR(30),
    sub_total       NUMERIC(15,2)   NOT NULL DEFAULT 0,
    discount        NUMERIC(15,2)   NOT NULL DEFAULT 0,
    total_payment   NUMERIC(15,2)   NOT NULL DEFAULT 0,
    partial_amount  NUMERIC(15,2)   NOT NULL DEFAULT 0,
    payment_option  VARCHAR(20)     CHECK (payment_option IN ('Cash', 'Online', 'Cheque')),
    payment_status  VARCHAR(20)     CHECK (payment_status IN ('Paid', 'Unpaid', 'Partial')),
    branch_id       UUID            REFERENCES branches(id),
    created_by      UUID            REFERENCES users(id),
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_epf_etf_client     ON epf_etf_records(client_id);
CREATE INDEX idx_epf_etf_branch     ON epf_etf_records(branch_id);
CREATE INDEX idx_epf_etf_date       ON epf_etf_records(record_date);
CREATE INDEX idx_epf_etf_deleted    ON epf_etf_records(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_epf_etf_updated_at
    BEFORE UPDATE ON epf_etf_records
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ============================================================================
-- TABLE: epf_etf_staff
-- Individual staff members linked to an EPF/ETF record.
-- ============================================================================
CREATE TABLE epf_etf_staff (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    epf_etf_record_id   UUID            NOT NULL REFERENCES epf_etf_records(id) ON DELETE CASCADE,
    staff_code          VARCHAR(30)     NOT NULL,
    staff_name          VARCHAR(255)    NOT NULL,
    phone               VARCHAR(30),
    process             VARCHAR(30)     NOT NULL DEFAULT 'Submit' CHECK (process IN ('Submit', 'Complete')),
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_epf_staff_record ON epf_etf_staff(epf_etf_record_id);

CREATE TRIGGER set_epf_staff_updated_at
    BEFORE UPDATE ON epf_etf_staff
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ============================================================================
-- TABLE: trade_marks
-- Trade Mark registration records.
-- ============================================================================
CREATE TABLE trade_marks (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_code     VARCHAR(20)     NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    record_date     DATE            NOT NULL,
    client_id       UUID            REFERENCES clients(id),
    client_name     VARCHAR(255)    NOT NULL,
    company_name    VARCHAR(255),
    trademark_code  VARCHAR(50),
    status          VARCHAR(30),
    sub_total       NUMERIC(15,2)   NOT NULL DEFAULT 0,
    discount        NUMERIC(15,2)   NOT NULL DEFAULT 0,
    total_payment   NUMERIC(15,2)   NOT NULL DEFAULT 0,
    partial_amount  NUMERIC(15,2)   NOT NULL DEFAULT 0,
    payment_option  VARCHAR(20)     CHECK (payment_option IN ('Cash', 'Online', 'Cheque')),
    payment_status  VARCHAR(20)     CHECK (payment_status IN ('Paid', 'Unpaid', 'Partial')),
    branch_id       UUID            REFERENCES branches(id),
    created_by      UUID            REFERENCES users(id),
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_trade_marks_client     ON trade_marks(client_id);
CREATE INDEX idx_trade_marks_branch     ON trade_marks(branch_id);
CREATE INDEX idx_trade_marks_date       ON trade_marks(record_date);
CREATE INDEX idx_trade_marks_deleted    ON trade_marks(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_trade_marks_updated_at
    BEFORE UPDATE ON trade_marks
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ============================================================================
-- TABLE: trade_licenses
-- Trade License registration records.
-- ============================================================================
CREATE TABLE trade_licenses (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_code     VARCHAR(20)     NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    record_date     DATE            NOT NULL,
    client_id       UUID            REFERENCES clients(id),
    client_name     VARCHAR(255)    NOT NULL,
    company_name    VARCHAR(255),
    license_code    VARCHAR(50),
    assignment      TEXT,
    status          VARCHAR(30),
    sub_total       NUMERIC(15,2)   NOT NULL DEFAULT 0,
    discount        NUMERIC(15,2)   NOT NULL DEFAULT 0,
    total_payment   NUMERIC(15,2)   NOT NULL DEFAULT 0,
    partial_amount  NUMERIC(15,2)   NOT NULL DEFAULT 0,
    payment_option  VARCHAR(20)     CHECK (payment_option IN ('Cash', 'Online', 'Cheque')),
    payment_status  VARCHAR(20)     CHECK (payment_status IN ('Paid', 'Unpaid', 'Partial')),
    branch_id       UUID            REFERENCES branches(id),
    created_by      UUID            REFERENCES users(id),
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_trade_licenses_client  ON trade_licenses(client_id);
CREATE INDEX idx_trade_licenses_branch  ON trade_licenses(branch_id);
CREATE INDEX idx_trade_licenses_date    ON trade_licenses(record_date);
CREATE INDEX idx_trade_licenses_deleted ON trade_licenses(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_trade_licenses_updated_at
    BEFORE UPDATE ON trade_licenses
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ============================================================================
-- TABLE: import_export_clearances
-- Import/Export Clearance service records.
-- ============================================================================
CREATE TABLE import_export_clearances (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_code     VARCHAR(20)     NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    record_date     DATE            NOT NULL,
    client_id       UUID            REFERENCES clients(id),
    client_name     VARCHAR(255)    NOT NULL,
    company_name    VARCHAR(255),
    clearance_code  VARCHAR(50),
    assignment      TEXT,
    tin_number      VARCHAR(50),
    status          VARCHAR(30),
    sub_total       NUMERIC(15,2)   NOT NULL DEFAULT 0,
    discount        NUMERIC(15,2)   NOT NULL DEFAULT 0,
    total_payment   NUMERIC(15,2)   NOT NULL DEFAULT 0,
    partial_amount  NUMERIC(15,2)   NOT NULL DEFAULT 0,
    payment_option  VARCHAR(20)     CHECK (payment_option IN ('Cash', 'Online', 'Cheque')),
    payment_status  VARCHAR(20)     CHECK (payment_status IN ('Paid', 'Unpaid', 'Partial')),
    branch_id       UUID            REFERENCES branches(id),
    created_by          UUID            REFERENCES users(id),
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted          BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_imp_exp_client     ON import_export_clearances(client_id);
CREATE INDEX idx_imp_exp_branch     ON import_export_clearances(branch_id);
CREATE INDEX idx_imp_exp_date       ON import_export_clearances(record_date);
CREATE INDEX idx_imp_exp_deleted    ON import_export_clearances(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_imp_exp_updated_at
    BEFORE UPDATE ON import_export_clearances
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ============================================================================
-- TABLE: hr_management_consulting
-- HR Management Consulting service records.
-- ============================================================================
CREATE TABLE hr_management_consulting (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_code     VARCHAR(20)     NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    record_date     DATE            NOT NULL,
    client_id       UUID            REFERENCES clients(id),
    client_name     VARCHAR(255)    NOT NULL,
    company_name    VARCHAR(255),
    assignment      TEXT,
    status          VARCHAR(30),
    sub_total       NUMERIC(15,2)   NOT NULL DEFAULT 0,
    discount        NUMERIC(15,2)   NOT NULL DEFAULT 0,
    total_payment   NUMERIC(15,2)   NOT NULL DEFAULT 0,
    partial_amount  NUMERIC(15,2)   NOT NULL DEFAULT 0,
    payment_option  VARCHAR(20)     CHECK (payment_option IN ('Cash', 'Online', 'Cheque')),
    payment_status  VARCHAR(20)     CHECK (payment_status IN ('Paid', 'Unpaid', 'Partial')),
    branch_id       UUID            REFERENCES branches(id),
    created_by      UUID            REFERENCES users(id),
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_hr_consult_client  ON hr_management_consulting(client_id);
CREATE INDEX idx_hr_consult_branch  ON hr_management_consulting(branch_id);
CREATE INDEX idx_hr_consult_date    ON hr_management_consulting(record_date);
CREATE INDEX idx_hr_consult_deleted ON hr_management_consulting(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_hr_consult_updated_at
    BEFORE UPDATE ON hr_management_consulting
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ============================================================================
-- TABLE: business_plan_valuations
-- Business Plan & Valuation service records.
-- ============================================================================
CREATE TABLE business_plan_valuations (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_code     VARCHAR(20)     NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    record_date     DATE            NOT NULL,
    client_id       UUID            REFERENCES clients(id),
    client_name     VARCHAR(255)    NOT NULL,
    company_name    VARCHAR(255),
    assignment      TEXT,
    status          VARCHAR(30),
    sub_total       NUMERIC(15,2)   NOT NULL DEFAULT 0,
    discount        NUMERIC(15,2)   NOT NULL DEFAULT 0,
    total_payment   NUMERIC(15,2)   NOT NULL DEFAULT 0,
    partial_amount  NUMERIC(15,2)   NOT NULL DEFAULT 0,
    payment_option  VARCHAR(20)     CHECK (payment_option IN ('Cash', 'Online', 'Cheque')),
    payment_status  VARCHAR(20)     CHECK (payment_status IN ('Paid', 'Unpaid', 'Partial')),
    branch_id       UUID            REFERENCES branches(id),
    created_by      UUID            REFERENCES users(id),
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_biz_plan_client    ON business_plan_valuations(client_id);
CREATE INDEX idx_biz_plan_branch    ON business_plan_valuations(branch_id);
CREATE INDEX idx_biz_plan_date      ON business_plan_valuations(record_date);
CREATE INDEX idx_biz_plan_deleted   ON business_plan_valuations(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_biz_plan_updated_at
    BEFORE UPDATE ON business_plan_valuations
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ============================================================================
-- TABLE: boi_registrations
-- Board of Investment registration records with foreign investment data.
-- ============================================================================
CREATE TABLE boi_registrations (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_code         VARCHAR(20)     NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    record_date         DATE            NOT NULL,
    client_id           UUID            REFERENCES clients(id),
    client_name         VARCHAR(255)    NOT NULL,
    company_name        VARCHAR(255),
    boi_code            VARCHAR(50),
    assignment          TEXT,
    country             VARCHAR(100),
    country_address     TEXT,
    investment_value_usd NUMERIC(18,2),
    status              VARCHAR(30),
    sub_total           NUMERIC(15,2)   NOT NULL DEFAULT 0,
    discount            NUMERIC(15,2)   NOT NULL DEFAULT 0,
    total_payment       NUMERIC(15,2)   NOT NULL DEFAULT 0,
    partial_amount      NUMERIC(15,2)   NOT NULL DEFAULT 0,
    payment_option      VARCHAR(20)     CHECK (payment_option IN ('Cash', 'Online', 'Cheque')),
    payment_status      VARCHAR(20)     CHECK (payment_status IN ('Paid', 'Unpaid', 'Partial')),
    branch_id           UUID            REFERENCES branches(id),
    created_by          UUID            REFERENCES users(id),
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted          BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_boi_client     ON boi_registrations(client_id);
CREATE INDEX idx_boi_branch     ON boi_registrations(branch_id);
CREATE INDEX idx_boi_date       ON boi_registrations(record_date);
CREATE INDEX idx_boi_country    ON boi_registrations(country);
CREATE INDEX idx_boi_deleted    ON boi_registrations(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_boi_updated_at
    BEFORE UPDATE ON boi_registrations
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ============================================================================
-- TABLE: other_secretarial_records
-- Catch-all table for miscellaneous secretarial & advisory service records.
-- ============================================================================
CREATE TABLE other_secretarial_records (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_code     VARCHAR(20)     NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    record_date     DATE            NOT NULL,
    client_id       UUID            REFERENCES clients(id),
    client_name     VARCHAR(255)    NOT NULL,
    company         VARCHAR(255)    NOT NULL,
    assignment      TEXT,
    description     TEXT,
    sub_total       NUMERIC(15,2)   NOT NULL DEFAULT 0,
    discount        NUMERIC(15,2)   NOT NULL DEFAULT 0,
    total_payment   NUMERIC(15,2)   NOT NULL DEFAULT 0,
    partial_amount  NUMERIC(15,2)   NOT NULL DEFAULT 0,
    payment_option  VARCHAR(20)     CHECK (payment_option IN ('Cash', 'Online', 'Cheque')),
    payment_status  VARCHAR(20)     CHECK (payment_status IN ('Paid', 'Unpaid', 'Partial')),
    branch_id       UUID            REFERENCES branches(id),
    created_by      UUID            REFERENCES users(id),
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_other_sec_client   ON other_secretarial_records(client_id);
CREATE INDEX idx_other_sec_branch   ON other_secretarial_records(branch_id);
CREATE INDEX idx_other_sec_date     ON other_secretarial_records(record_date);
CREATE INDEX idx_other_sec_deleted  ON other_secretarial_records(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_other_sec_updated_at
    BEFORE UPDATE ON other_secretarial_records
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ############################################################################
-- DOMAIN 5: NEXORA DIGITAL SERVICES
-- ############################################################################

-- ============================================================================
-- TABLE: nexora_services
-- Lookup table for available Nexora digital service types.
-- ============================================================================
CREATE TABLE nexora_services (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(100)    NOT NULL UNIQUE,
    description     TEXT,
    is_active       BOOLEAN         NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

CREATE TRIGGER set_nexora_services_updated_at
    BEFORE UPDATE ON nexora_services
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

-- Seed data: Nexora service types
INSERT INTO nexora_services (name) VALUES
    ('Accounting Software'),
    ('Payroll Management'),
    ('KOT System'),
    ('POS System'),
    ('Website'),
    ('Marketing & Digital Marketing'),
    ('Other');


-- ============================================================================
-- TABLE: nexora_service_requests
-- Client service requests tracked through the Nexora dashboard.
-- ============================================================================
CREATE TABLE nexora_service_requests (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    request_date    DATE            NOT NULL,
    client_id       UUID            REFERENCES clients(id),
    client_name     VARCHAR(255)    NOT NULL,
    company_name    VARCHAR(255),
    service_id      UUID            REFERENCES nexora_services(id),
    phone           VARCHAR(30),
    notes           TEXT,
    branch_id       UUID            REFERENCES branches(id),
    created_by      UUID            REFERENCES users(id),
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_nexora_req_client  ON nexora_service_requests(client_id);
CREATE INDEX idx_nexora_req_service ON nexora_service_requests(service_id);
CREATE INDEX idx_nexora_req_branch  ON nexora_service_requests(branch_id);
CREATE INDEX idx_nexora_req_date    ON nexora_service_requests(request_date);
CREATE INDEX idx_nexora_req_status  ON nexora_service_requests(status);
CREATE INDEX idx_nexora_req_deleted ON nexora_service_requests(is_deleted) WHERE is_deleted = FALSE;

CREATE TRIGGER set_nexora_req_updated_at
    BEFORE UPDATE ON nexora_service_requests
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ############################################################################
-- DOMAIN 6: SHARED SUB-ENTITY TABLES
-- ############################################################################

-- ============================================================================
-- TABLE: payments
-- Reusable payment records linked to any service record via polymorphic ref.
-- record_type examples: 'tax_account', 'internal_control', 'management_account'
-- ============================================================================
CREATE TABLE payments (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_type     VARCHAR(50)     NOT NULL,
    record_id       UUID            NOT NULL,
    payment_date    DATE,
    sub_total       NUMERIC(15,2)   NOT NULL DEFAULT 0,
    discount        NUMERIC(15,2)   NOT NULL DEFAULT 0,
    total_amount    NUMERIC(15,2)   NOT NULL DEFAULT 0,
    paid_amount     NUMERIC(15,2)   NOT NULL DEFAULT 0,
    remaining_amount NUMERIC(15,2)  NOT NULL DEFAULT 0,
    payment_option  VARCHAR(20)     CHECK (payment_option IN ('Cash', 'Online', 'Cheque')),
    payment_status  VARCHAR(20)     CHECK (payment_status IN ('Paid', 'Unpaid', 'Partial')),
    notes           TEXT,
    created_by      UUID            REFERENCES users(id),
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_payments_record    ON payments(record_type, record_id);
CREATE INDEX idx_payments_date      ON payments(payment_date);
CREATE INDEX idx_payments_status    ON payments(payment_status);

CREATE TRIGGER set_payments_updated_at
    BEFORE UPDATE ON payments
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ============================================================================
-- TABLE: cheque_details
-- Cheque payment details linked to a payment record.
-- ============================================================================
CREATE TABLE cheque_details (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    payment_id      UUID            NOT NULL REFERENCES payments(id) ON DELETE CASCADE,
    bank_name       VARCHAR(100),
    cheque_number   VARCHAR(50),
    cheque_date     DATE,
    status          VARCHAR(20)     CHECK (status IN ('Pending', 'Cleared', 'Return')),
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_cheque_payment ON cheque_details(payment_id);

CREATE TRIGGER set_cheque_updated_at
    BEFORE UPDATE ON cheque_details
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();


-- ############################################################################
-- DOMAIN 7: FILE STORAGE
-- ############################################################################

-- ============================================================================
-- TABLE: documents
-- File metadata only — actual binaries stored in Cloudflare R2.
-- Uses polymorphic reference (record_type + record_id) to link to any parent.
-- ============================================================================
CREATE TABLE documents (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    record_type         VARCHAR(50)     NOT NULL,
    record_id           UUID            NOT NULL,
    document_category   VARCHAR(50),
    file_name           VARCHAR(500)    NOT NULL,
    file_size           VARCHAR(20),
    mime_type           VARCHAR(100),
    storage_key         VARCHAR(1000)   NOT NULL,
    description         TEXT,
    uploaded_by         UUID            REFERENCES users(id),
    uploaded_at         TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    is_deleted          BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_documents_record   ON documents(record_type, record_id);
CREATE INDEX idx_documents_category ON documents(document_category);
CREATE INDEX idx_documents_deleted  ON documents(is_deleted) WHERE is_deleted = FALSE;


-- ############################################################################
-- DOMAIN 8: ACTIVITY & SYNC
-- ############################################################################

-- ============================================================================
-- TABLE: activity_logs
-- Complete audit trail of all user actions in the system.
-- ============================================================================
CREATE TABLE activity_logs (
    id              BIGSERIAL PRIMARY KEY,
    timestamp       TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    user_id         UUID            REFERENCES users(id),
    branch_id       UUID            REFERENCES branches(id),
    action          VARCHAR(20)     NOT NULL,
    module          VARCHAR(50)     NOT NULL,
    record_type     VARCHAR(50),
    record_id       UUID,
    description     TEXT,
    ip_address      INET,
    user_agent      TEXT
);

CREATE INDEX idx_activity_user      ON activity_logs(user_id);
CREATE INDEX idx_activity_branch    ON activity_logs(branch_id);
CREATE INDEX idx_activity_timestamp ON activity_logs(timestamp);
CREATE INDEX idx_activity_action    ON activity_logs(action);
CREATE INDEX idx_activity_module    ON activity_logs(module);
CREATE INDEX idx_activity_record    ON activity_logs(record_type, record_id);


-- ============================================================================
-- TABLE: sync_tracking
-- Dotmim.Sync conflict resolution and tombstone tracking.
-- ============================================================================
CREATE TABLE sync_tracking (
    id                  BIGSERIAL PRIMARY KEY,
    table_name          VARCHAR(100)    NOT NULL,
    record_id           UUID            NOT NULL,
    operation           VARCHAR(10)     NOT NULL CHECK (operation IN ('INSERT', 'UPDATE', 'DELETE')),
    synced_at           TIMESTAMPTZ,
    device_id           VARCHAR(100),
    conflict_resolved   BOOLEAN         NOT NULL DEFAULT FALSE,
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_sync_table     ON sync_tracking(table_name);
CREATE INDEX idx_sync_record    ON sync_tracking(record_id);
CREATE INDEX idx_sync_device    ON sync_tracking(device_id);
CREATE INDEX idx_sync_synced    ON sync_tracking(synced_at);


-- ============================================================================
-- SCHEMA COMPLETE
-- ============================================================================
-- Total Tables: 27
--   Core & Auth:           branches, users, clients
--   Accounts & Audit:      audit_assurance_records, forensic_audit_records,
--                          internal_audit_records, management_account_records,
--                          internal_control_records
--   Tax:                   tax_account_records, tax_filings
--   Secretarial & Advisory: company_registrations, company_officers,
--                          epf_etf_records, epf_etf_staff, trade_marks,
--                          trade_licenses, import_export_clearances,
--                          hr_management_consulting, business_plan_valuations,
--                          boi_registrations, other_secretarial_records
--   Nexora Services:       nexora_services, nexora_service_requests
--   Shared Sub-entities:   payments, cheque_details
--   File Storage:          documents
--   Activity & Sync:       activity_logs, sync_tracking
-- ============================================================================
