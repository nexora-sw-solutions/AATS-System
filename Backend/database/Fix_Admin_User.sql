-- Fix Admin User (Email: admin@aats.com, Password: admin123, Role: Admin)
-- Use this to fix the 401 Unauthorized and 403 Forbidden issues.

DO $$ 
DECLARE
    v_admin_id UUID;
    v_branch_id UUID;
BEGIN
    -- 1. Get a valid branch ID (Any branch will do for the admin)
    SELECT "Id" INTO v_branch_id FROM "branches" ORDER BY "Name" LIMIT 1;
    
    -- 2. Check if admin user exists by Username or Email
    SELECT "Id" INTO v_admin_id FROM "users" WHERE "Username" = 'admin' OR "Email" = 'admin@aats.com' LIMIT 1;

    IF v_admin_id IS NOT NULL THEN
        -- Update existing admin
        UPDATE "users" 
        SET "Email" = 'admin@aats.com',
            "PasswordHash" = '$2a$11$MYqfwG4g2D3MORFo89dYOuhPznaN5yZUGAhc0hiVlIasnNAA3qmzC',
            "Role" = 'Admin',
            "IsActive" = TRUE,
            "IsDeleted" = FALSE,
            "BranchId" = COALESCE(v_branch_id, "BranchId")
        WHERE "Id" = v_admin_id;
        RAISE NOTICE 'Updated existing admin user.';
    ELSE
        -- Insert new admin
        INSERT INTO "users" ("Id", "Username", "Email", "PasswordHash", "Role", "BranchId", "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), 'admin', 'admin@aats.com', '$2a$11$MYqfwG4g2D3MORFo89dYOuhPznaN5yZUGAhc0hiVlIasnNAA3qmzC', 'Admin', v_branch_id, TRUE, FALSE, NOW(), NOW());
        RAISE NOTICE 'Inserted new admin user.';
    END IF;
END $$;
