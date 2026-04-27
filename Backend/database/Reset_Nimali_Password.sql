-- Reset Nimali's password to 'nimali123'
UPDATE "users" 
SET "PasswordHash" = '$2a$11$sSqVtIX6oKRl7SDij9Tdqu8AocSmNzLLHPGzT5Ycq0wDs5Ln/XYgm',
    "IsActive" = TRUE,
    "IsDeleted" = FALSE
WHERE "Email" = 'nimali@aats.com';
