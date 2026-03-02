-- ============================================================
-- Migration: Add Additional Column to Policies table
-- Purpose: Chuyển từ generic columns sang JSON như Services
-- Author: Copilot
-- Date: 2026-03-02
-- ============================================================

-- STEP 1: Add Additional column (giống Services table)
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Policies]') 
    AND name = 'Additional'
)
BEGIN
    ALTER TABLE [dbo].[Policies]
    ADD [Additional] NVARCHAR(MAX) NULL;
    
    PRINT 'Added Additional column to Policies table';
END
ELSE
BEGIN
    PRINT 'Additional column already exists';
END
GO

-- STEP 2: Migrate existing data from generic columns to JSON
-- TypeId 1002: Check-In/Check-Out Policy
UPDATE [dbo].[Policies]
SET [Additional] = (
    SELECT 
        CONVERT(VARCHAR(5), [TimeFrom], 108) AS CheckInTime,
        CONVERT(VARCHAR(5), [TimeTo], 108) AS CheckOutTime,
        [Amount] AS EarlyCheckInFee
    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
)
WHERE TypeId = 1002;
PRINT 'Migrated TypeId 1002 (Check-In/Out)';
GO

-- TypeId 1003: Cancellation Policy
UPDATE [dbo].[Policies]
SET [Additional] = (
    SELECT 
        [IntValue1] AS DaysBeforeCheckIn,
        [Percent] AS RefundPercent,
        CASE WHEN [BoolValue] = 1 THEN 'true' ELSE 'false' END AS IsRefundable
    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
)
WHERE TypeId = 1003;
PRINT 'Migrated TypeId 1003 (Cancellation)';
GO

-- TypeId 1004: Children & Extra Bed Policy
UPDATE [dbo].[Policies]
SET [Additional] = (
    SELECT 
        [IntValue1] AS MinAge,
        [IntValue2] AS MaxAge,
        [Amount] AS ExtraBedFee
    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
)
WHERE TypeId = 1004;
PRINT 'Migrated TypeId 1004 (Children)';
GO

-- TypeId 2002: Pet Policy
UPDATE [dbo].[Policies]
SET [Additional] = (
    SELECT 
        [Amount] AS PetFee,
        CASE WHEN [BoolValue] = 1 THEN 'true' ELSE 'false' END AS IsPetAllowed
    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
)
WHERE TypeId = 2002;
PRINT 'Migrated TypeId 2002 (Pets)';
GO

-- STEP 3: Verify migration (optional - comment out in production)
SELECT 
    Id, 
    Name, 
    TypeId,
    Additional,
    -- Old columns for comparison
    TimeFrom, TimeTo, IntValue1, IntValue2, Amount, [Percent], BoolValue
FROM [dbo].[Policies]
ORDER BY TypeId, Id;
GO

-- STEP 4: (OPTIONAL) Drop old generic columns after verification
-- WARNING: Only run this after confirming migration is successful!
-- Uncomment the lines below when ready:
/*
ALTER TABLE [dbo].[Policies] DROP COLUMN [TimeFrom];
ALTER TABLE [dbo].[Policies] DROP COLUMN [TimeTo];
ALTER TABLE [dbo].[Policies] DROP COLUMN [IntValue1];
ALTER TABLE [dbo].[Policies] DROP COLUMN [IntValue2];
ALTER TABLE [dbo].[Policies] DROP COLUMN [Amount];
ALTER TABLE [dbo].[Policies] DROP COLUMN [Percent];
ALTER TABLE [dbo].[Policies] DROP COLUMN [BoolValue];
PRINT 'Dropped old generic columns';
*/

-- STEP 5: Update JSON format if needed (e.g., convert "true"/"false" strings to boolean)
-- Sửa cho Cancellation Policy
UPDATE Policies 
SET Additional = REPLACE(REPLACE(Additional, '"true"', 'true'), '"false"', 'false')
WHERE TypeId = 1003;

-- Sửa cho Pet Policy
UPDATE Policies 
SET Additional = REPLACE(REPLACE(Additional, '"true"', 'true'), '"false"', 'false')
WHERE TypeId = 2002;
