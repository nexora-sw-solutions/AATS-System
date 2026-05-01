-- ==========================================
-- AATS SYSTEM - CORRECTED SAMPLE DATA SEEDING
-- ==========================================
-- INSTRUCTIONS: Run this in the Supabase SQL Editor.

BEGIN;

-- 1. SEED CLIENTS
INSERT INTO clients (
    "Id", "ClientCode", "ClientName", "Email", "Phone", "Status", 
    "BranchId", "TotalRevenue", "OutstandingBalance", "IsDeleted", 
    "CreatedAt", "UpdatedAt"
)
VALUES 
(
    '019d2519-67ec-7e04-a9e0-78e165716531', 
    'CL-001', 
    'Nexus Tech Solutions (Pvt) Ltd', 
    'info@nexustech.com', 
    '0112345678', 
    1, -- Active
    '00000000-0000-0000-0000-000000000001', 
    50000.00, 
    5000.00, 
    FALSE, 
    NOW(), 
    NOW()
),
(
    '019d2519-67ec-7e04-a9e0-78e165716532', 
    'CL-002', 
    'Global Trading Hub PLC', 
    'contact@globaltrading.com', 
    '0118765432', 
    1, -- Active
    '00000000-0000-0000-0000-000000000001', 
    120000.00, 
    0.00, 
    FALSE, 
    NOW(), 
    NOW()
)
ON CONFLICT ("Id") DO NOTHING;

-- 2. SEED TAX FILINGS
INSERT INTO tax_filings (
    "Id", "FilingCode", "TaxType", "ClientId", "ClientName", 
    "BranchId", "FilingDate", "TaxNumber", "PeriodNumber", 
    "PeriodType", "PaymentStatus", "IsDeleted", "CreatedAt", "UpdatedAt"
)
VALUES 
(
    '019d2519-67ec-7e04-a9e0-78e365716551', 
    'TAX-2024-001', 
    'IncomeTax', 
    '019d2519-67ec-7e04-a9e0-78e165716531', 
    'Nexus Tech Solutions (Pvt) Ltd', 
    '00000000-0000-0000-0000-000000000001', 
    '2024-03-25', 
    'T123456789', 
    'Q1', 
    'Year', 
    'Paid', 
    FALSE, 
    NOW(), 
    NOW()
)
ON CONFLICT ("Id") DO NOTHING;

-- 3. SEED COMPANY REGISTRATIONS
INSERT INTO company_registrations (
    "Id", "RegistrationCode", "RegistrationDate", "ClientId", "ClientName", 
    "CompanyName", "CompanyType", "SubTotal", "Discount", "TotalPayment", 
    "PartialAmount", "BranchId", "IsDeleted", "CreatedAt", "UpdatedAt"
)
VALUES 
(
    '019d2519-67ec-7e04-a9e0-78e465716561', 
    'REG-2024-101', 
    '2024-01-15', 
    '019d2519-67ec-7e04-a9e0-78e165716531', 
    'Nexus Tech Solutions (Pvt) Ltd', 
    'Nexus Subsidiary Tech', 
    'Private Limited', 
    25000.00, 
    0.00, 
    25000.00, 
    25000.00, 
    '00000000-0000-0000-0000-000000000001', 
    FALSE, 
    NOW(), 
    NOW()
)
ON CONFLICT ("Id") DO NOTHING;

-- 4. SEED INTERNAL AUDIT RECORDS
INSERT INTO internal_audit_records (
    "Id", "RecordCode", "RecordDate", "ClientId", "ClientName", 
    "BranchId", "Assignment", "Process", "SubTotal", "Discount", 
    "TotalPayment", "PartialAmount", "IsDeleted", "CreatedAt", "UpdatedAt"
)
VALUES 
(
    '019d2519-67ec-7e04-a9e0-78e265716541', 
    'AUD-INT-2024-01', 
    '2024-02-10', 
    '019d2519-67ec-7e04-a9e0-78e165716531', 
    'Nexus Tech Solutions (Pvt) Ltd', 
    '00000000-0000-0000-0000-000000000001', 
    'Annual Internal Audit', 
    'Ongoing', 
    45000.00, 
    5000.00, 
    40000.00, 
    20000.00, 
    FALSE, 
    NOW(), 
    NOW()
)
ON CONFLICT ("Id") DO NOTHING;

COMMIT;
