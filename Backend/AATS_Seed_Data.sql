-- ============================================================================
-- AATS Mock Data Generation Script (EF Core PostgreSQL Compatible)
-- Inserts 10+ realistic records for Core, Audit, Tax, and Secretarial domains
-- Ensures all columns are quoted ("Id", "ClientName") to bypass PostgreSQL case folding
-- ============================================================================

DO $$ 
DECLARE
    south_id UUID;
    west_id UUID;
    central_id UUID;
    ne_id UUID;
    admin_id UUID;
    
    c1_id UUID := gen_random_uuid();
    c2_id UUID := gen_random_uuid();
    c3_id UUID := gen_random_uuid();
    c4_id UUID := gen_random_uuid();
    c5_id UUID := gen_random_uuid();
    c6_id UUID := gen_random_uuid();
    c7_id UUID := gen_random_uuid();
    c8_id UUID := gen_random_uuid();
    c9_id UUID := gen_random_uuid();
    c10_id UUID := gen_random_uuid();

BEGIN
    -- 1. Ensure Branches Exist & Get IDs
    SELECT "Id" INTO south_id FROM branches WHERE "Code" = 'SOUTH' LIMIT 1;
    IF south_id IS NULL THEN
        south_id := gen_random_uuid();
        INSERT INTO branches ("Id", "Name", "Code", "IsActive", "CreatedAt", "UpdatedAt") VALUES (south_id, 'South', 'SOUTH', TRUE, NOW(), NOW());
    END IF;

    SELECT "Id" INTO west_id FROM branches WHERE "Code" = 'WEST' LIMIT 1;
    IF west_id IS NULL THEN
        west_id := gen_random_uuid();
        INSERT INTO branches ("Id", "Name", "Code", "IsActive", "CreatedAt", "UpdatedAt") VALUES (west_id, 'West', 'WEST', TRUE, NOW(), NOW());
    END IF;

    SELECT "Id" INTO central_id FROM branches WHERE "Code" = 'CENTRAL' LIMIT 1;
    IF central_id IS NULL THEN
        central_id := gen_random_uuid();
        INSERT INTO branches ("Id", "Name", "Code", "IsActive", "CreatedAt", "UpdatedAt") VALUES (central_id, 'Central', 'CENTRAL', TRUE, NOW(), NOW());
    END IF;

    SELECT "Id" INTO ne_id FROM branches WHERE "Code" = 'NORTHEAST' LIMIT 1;
    IF ne_id IS NULL THEN
        ne_id := gen_random_uuid();
        INSERT INTO branches ("Id", "Name", "Code", "IsActive", "CreatedAt", "UpdatedAt") VALUES (ne_id, 'Northeast', 'NORTHEAST', TRUE, NOW(), NOW());
    END IF;

    -- 2. Ensure at least one Admin User exists
    SELECT "Id" INTO admin_id FROM users WHERE "Username" = 'admin' LIMIT 1;
    IF admin_id IS NULL THEN
        admin_id := gen_random_uuid();
        INSERT INTO users ("Id", "Username", "Email", "PasswordHash", "Role", "BranchId", "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt")
        VALUES (admin_id, 'admin', 'admin@nexora.com', 'hashed_pwd', 1, south_id, TRUE, FALSE, NOW(), NOW());
    END IF;

    -- 3. Insert 10 Clients
    INSERT INTO clients ("Id", "ClientCode", "ClientName", "Email", "Phone", "Status", "TotalRevenue", "OutstandingBalance", "BranchId", "IsDeleted", "CreatedAt", "UpdatedAt") VALUES
    (c1_id, 'C-001', 'TechCorp Solutions', 'contact@techcorp.lk', '0112345678', 'Active', 0, 0, south_id, FALSE, NOW(), NOW()),
    (c2_id, 'C-002', 'Global Logistics Pvt Ltd', 'info@globallogistics.com', '0119876543', 'Active', 0, 0, west_id, FALSE, NOW(), NOW()),
    (c3_id, 'C-003', 'Sunrise Holdings', 'admin@sunrise.lk', '0812345678', 'Active', 0, 0, central_id, FALSE, NOW(), NOW()),
    (c4_id, 'C-004', 'Oceanic Exports', 'export@oceanic.com', '0111122334', 'Pending', 0, 0, west_id, FALSE, NOW(), NOW()),
    (c5_id, 'C-005', 'Apex Manufacturing', 'hello@apex.lk', '0212345678', 'Active', 0, 0, ne_id, FALSE, NOW(), NOW()),
    (c6_id, 'C-006', 'NextGen Retailers', 'sales@nextgen.lk', '0114455667', 'Active', 0, 0, south_id, FALSE, NOW(), NOW()),
    (c7_id, 'C-007', 'Pioneer Construction', 'build@pioneer.com', '0312345678', 'Inactive', 0, 0, west_id, FALSE, NOW(), NOW()),
    (c8_id, 'C-008', 'Smart Agri Ventures', 'farm@smartagri.lk', '0412345678', 'Active', 0, 0, central_id, FALSE, NOW(), NOW()),
    (c9_id, 'C-009', 'BlueWave IT Services', 'info@bluewave.lk', '0119988776', 'Active', 0, 0, south_id, FALSE, NOW(), NOW()),
    (c10_id, 'C-010', 'Lanka Motors Traders', 'trade@lankamotors.com', '0116677889', 'Active', 0, 0, ne_id, FALSE, NOW(), NOW());

    -- 4. Insert 10 Audit & Assurance Records (Processes: Bookkeep, Draft Account, Finalize, Handover, Return, Submit)
    INSERT INTO audit_assurance_records ("Id", "RecordCode", "RecordDate", "ClientId", "ClientName", "BranchId", "Process", "PaymentStatus", "TotalPayment", "CreatedBy", "IsDeleted", "CreatedAt", "UpdatedAt", "SubTotal", "Discount", "PartialAmount") VALUES
    (gen_random_uuid(), 'AUD-001', '2026-01-10', c1_id, 'TechCorp Solutions', south_id, 'Bookkeep', 'Paid', 50000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'AUD-002', '2026-01-15', c2_id, 'Global Logistics Pvt Ltd', west_id, 'Draft Account', 'Partial', 75000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'AUD-003', '2026-02-01', c3_id, 'Sunrise Holdings', central_id, 'Finalize', 'Unpaid', 120000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'AUD-004', '2026-02-10', c4_id, 'Oceanic Exports', west_id, 'Handover', 'Paid', 95000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'AUD-005', '2026-02-20', c5_id, 'Apex Manufacturing', ne_id, 'Submit', 'Paid', 60000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'AUD-006', '2026-03-01', c6_id, 'NextGen Retailers', south_id, 'Bookkeep', 'Partial', 45000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'AUD-007', '2026-03-05', c7_id, 'Pioneer Construction', west_id, 'Return', 'Unpaid', 110000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'AUD-008', '2026-03-10', c8_id, 'Smart Agri Ventures', central_id, 'Draft Account', 'Paid', 85000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'AUD-009', '2026-03-15', c9_id, 'BlueWave IT Services', south_id, 'Finalize', 'Partial', 150000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'AUD-010', '2026-03-20', c10_id, 'Lanka Motors Traders', ne_id, 'Submit', 'Unpaid', 90000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0);

    -- 5. Insert 10 Tax Account Records (Processes: Bookkeep, Tax Amount, Finalize, Tax Paid, Submit)
    INSERT INTO tax_account_records ("Id", "RecordCode", "RecordDate", "ClientId", "ClientName", "BranchId", "Process", "PaymentStatus", "TotalPayment", "AssignedTo", "CreatedBy", "IsDeleted", "CreatedAt", "UpdatedAt", "SubTotal", "Discount", "PartialAmount") VALUES
    (gen_random_uuid(), 'TAX-A-001', '2026-01-05', c1_id, 'TechCorp Solutions', south_id, 'Tax Amount', 'Paid', 25000, admin_id, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'TAX-A-002', '2026-01-12', c2_id, 'Global Logistics Pvt Ltd', west_id, 'Finalize', 'Unpaid', 35000, admin_id, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'TAX-A-003', '2026-01-20', c3_id, 'Sunrise Holdings', central_id, 'Tax Paid', 'Paid', 40000, admin_id, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'TAX-A-004', '2026-02-05', c4_id, 'Oceanic Exports', west_id, 'Submit', 'Partial', 30000, admin_id, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'TAX-A-005', '2026-02-15', c5_id, 'Apex Manufacturing', ne_id, 'Bookkeep', 'Paid', 20000, admin_id, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'TAX-A-006', '2026-02-28', c6_id, 'NextGen Retailers', south_id, 'Tax Amount', 'Unpaid', 28000, admin_id, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'TAX-A-007', '2026-03-02', c7_id, 'Pioneer Construction', west_id, 'Finalize', 'Paid', 45000, admin_id, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'TAX-A-008', '2026-03-10', c8_id, 'Smart Agri Ventures', central_id, 'Tax Paid', 'Partial', 32000, admin_id, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'TAX-A-009', '2026-03-18', c9_id, 'BlueWave IT Services', south_id, 'Submit', 'Unpaid', 50000, admin_id, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'TAX-A-010', '2026-03-22', c10_id, 'Lanka Motors Traders', ne_id, 'Bookkeep', 'Paid', 22000, admin_id, admin_id, FALSE, NOW(), NOW(), 0, 0, 0);

    -- 6. Insert 10 Tax Filings (Types: CIT, IIT, VAT, SSCL, WHT, Others)
    INSERT INTO tax_filings ("Id", "FilingCode", "TaxType", "ClientId", "ClientName", "BranchId", "FilingDate", "TaxNumber", "PaymentStatus", "CreatedBy", "IsDeleted", "CreatedAt", "UpdatedAt") VALUES
    (gen_random_uuid(), 'TFL-001', 'CIT', c1_id, 'TechCorp Solutions', south_id, '2026-01-15', 'TN-1001', 'Paid', admin_id, FALSE, NOW(), NOW()),
    (gen_random_uuid(), 'TFL-002', 'VAT', c2_id, 'Global Logistics Pvt Ltd', west_id, '2026-02-10', 'TN-1002', 'Pending', admin_id, FALSE, NOW(), NOW()),
    (gen_random_uuid(), 'TFL-003', 'SSCL', c3_id, 'Sunrise Holdings', central_id, '2026-02-15', 'TN-1003', 'IRD Paid', admin_id, FALSE, NOW(), NOW()),
    (gen_random_uuid(), 'TFL-004', 'WHT', c4_id, 'Oceanic Exports', west_id, '2026-02-20', 'TN-1004', 'Paid', admin_id, FALSE, NOW(), NOW()),
    (gen_random_uuid(), 'TFL-005', 'IIT', c5_id, 'Apex Manufacturing', ne_id, '2026-03-01', 'TN-1005', 'Pending', admin_id, FALSE, NOW(), NOW()),
    (gen_random_uuid(), 'TFL-006', 'VAT', c6_id, 'NextGen Retailers', south_id, '2026-03-05', 'TN-1006', 'Paid', admin_id, FALSE, NOW(), NOW()),
    (gen_random_uuid(), 'TFL-007', 'CIT', c7_id, 'Pioneer Construction', west_id, '2026-03-10', 'TN-1007', 'IRD Paid', admin_id, FALSE, NOW(), NOW()),
    (gen_random_uuid(), 'TFL-008', 'SSCL', c8_id, 'Smart Agri Ventures', central_id, '2026-03-12', 'TN-1008', 'Pending', admin_id, FALSE, NOW(), NOW()),
    (gen_random_uuid(), 'TFL-009', 'WHT', c9_id, 'BlueWave IT Services', south_id, '2026-03-15', 'TN-1009', 'Paid', admin_id, FALSE, NOW(), NOW()),
    (gen_random_uuid(), 'TFL-010', 'IIT', c10_id, 'Lanka Motors Traders', ne_id, '2026-03-25', 'TN-1010', 'Pending', admin_id, FALSE, NOW(), NOW());

    -- 7. Insert 10 Company Registrations (Process: Name Approve, Forms Preparation, Signature, Payment, Incorporation, Seal, Certified copy, Document hand over)
    INSERT INTO company_registrations ("Id", "RegistrationCode", "RegistrationDate", "ClientId", "ClientName", "CompanyName", "BranchId", "Process", "PaymentStatus", "TotalPayment", "CreatedBy", "IsDeleted", "CreatedAt", "UpdatedAt", "SubTotal", "Discount", "PartialAmount") VALUES
    (gen_random_uuid(), 'REG-001', '2026-01-02', c1_id, 'TechCorp Solutions', 'TechCorp Sub1 Pvt Ltd', south_id, 'Name Approve', 'Paid', 25000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'REG-002', '2026-01-15', c2_id, 'Global Logistics Pvt Ltd', 'Global Freight Pvt Ltd', west_id, 'Forms Preparation', 'Unpaid', 25000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'REG-003', '2026-01-28', c3_id, 'Sunrise Holdings', 'Sunrise Estates Pvt Ltd', central_id, 'Signature', 'Partial', 25000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'REG-004', '2026-02-05', c4_id, 'Oceanic Exports', 'Oceanic Trading Pvt Ltd', west_id, 'Payment', 'Paid', 25000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'REG-005', '2026-02-14', c5_id, 'Apex Manufacturing', 'Apex Metals Pvt Ltd', ne_id, 'Incorporation', 'Paid', 25000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'REG-006', '2026-02-22', c6_id, 'NextGen Retailers', 'NextGen Marts Pvt Ltd', south_id, 'Seal', 'Unpaid', 25000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'REG-007', '2026-03-01', c7_id, 'Pioneer Construction', 'Pioneer Builders Pvt Ltd', west_id, 'Certified copy', 'Partial', 25000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'REG-008', '2026-03-08', c8_id, 'Smart Agri Ventures', 'Smart Farms Pvt Ltd', central_id, 'Document hand over', 'Paid', 25000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'REG-009', '2026-03-15', c9_id, 'BlueWave IT Services', 'BlueWave AI Pvt Ltd', south_id, 'Name Approve', 'Unpaid', 25000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'REG-010', '2026-03-20', c10_id, 'Lanka Motors Traders', 'Lanka Spares Pvt Ltd', ne_id, 'Incorporation', 'Paid', 25000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0);

    -- 8. Insert 10 EPF/ETF Records
    INSERT INTO epf_etf_records ("Id", "RecordCode", "RecordDate", "ClientId", "ClientName", "CompanyName", "NumberOfStaff", "BranchId", "PaymentStatus", "TotalPayment", "CreatedBy", "IsDeleted", "CreatedAt", "UpdatedAt", "SubTotal", "Discount", "PartialAmount") VALUES
    (gen_random_uuid(), 'EPF-001', '2026-01-10', c1_id, 'TechCorp Solutions', 'TechCorp Solutions', 15, south_id, 'Paid', 10000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'EPF-002', '2026-01-20', c2_id, 'Global Logistics Pvt Ltd', 'Global Logistics Pvt Ltd', 45, west_id, 'Partial', 20000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'EPF-003', '2026-02-05', c3_id, 'Sunrise Holdings', 'Sunrise Holdings', 30, central_id, 'Unpaid', 15000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'EPF-004', '2026-02-12', c4_id, 'Oceanic Exports', 'Oceanic Exports', 22, west_id, 'Paid', 12000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'EPF-005', '2026-02-25', c5_id, 'Apex Manufacturing', 'Apex Manufacturing', 80, ne_id, 'Paid', 30000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'EPF-006', '2026-03-02', c6_id, 'NextGen Retailers', 'NextGen Retailers', 12, south_id, 'Partial', 8000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'EPF-007', '2026-03-09', c7_id, 'Pioneer Construction', 'Pioneer Construction', 150, west_id, 'Unpaid', 50000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'EPF-008', '2026-03-14', c8_id, 'Smart Agri Ventures', 'Smart Agri Ventures', 25, central_id, 'Paid', 14000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'EPF-009', '2026-03-20', c9_id, 'BlueWave IT Services', 'BlueWave IT Services', 55, south_id, 'Unpaid', 25000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0),
    (gen_random_uuid(), 'EPF-010', '2026-03-24', c10_id, 'Lanka Motors Traders', 'Lanka Motors Traders', 35, ne_id, 'Paid', 18000, admin_id, FALSE, NOW(), NOW(), 0, 0, 0);

END $$;
