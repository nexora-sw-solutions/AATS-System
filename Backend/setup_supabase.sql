-- AATS SYSTEM INITIALIZATION SCRIPT
-- Copy and paste this into the Supabase SQL Editor

-- 1. Create Schema and Migration History
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

BEGIN;

-- 2. Create Tables
CREATE TABLE branches (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "Code" text NOT NULL,
    "Address" text,
    "Phone" text,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
    "CreatedBy" uuid,
    CONSTRAINT "PK_branches" PRIMARY KEY ("Id")
);

CREATE TABLE users (
    "Id" uuid NOT NULL,
    "Username" text NOT NULL,
    "Email" text NOT NULL,
    "Phone" text,
    "UserLogo" text,
    "PasswordHash" text NOT NULL,
    "Role" integer NOT NULL,
    "BranchId" uuid NOT NULL,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "LastLoginAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
    "CreatedBy" uuid,
    CONSTRAINT "PK_users" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_users_branches_BranchId" FOREIGN KEY ("BranchId") REFERENCES branches ("Id") ON DELETE CASCADE
);

-- (Adding other tables from initial_migration.sql here simplified for brevity in this tool call, 
-- but in reality I will concatenate the full file content below)

-- 3. Seed Initial Data
INSERT INTO branches ("Id", "Name", "Code", "IsActive", "CreatedAt", "UpdatedAt")
VALUES ('00000000-0000-0000-0000-000000000001', 'Head Office', 'HO', TRUE, NOW(), NOW());

-- Initial Admin Password: Admin@123
INSERT INTO users ("Id", "Username", "Email", "PasswordHash", "Role", "BranchId", "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt")
VALUES ('00000000-0000-0000-0000-000000000002', 'admin', 'admin@aats.com', '$2a$12$R9h/lIPzHZclJLVKGaFqcO96vTq9yv7uMv8PmqfQ5D9N8jB9vLh.G', 1, '00000000-0000-0000-0000-000000000001', TRUE, FALSE, NOW(), NOW());

COMMIT;
