-- ============================================================
-- Migration: Add Additional Column to Policies table
-- Purpose: Transition from generic columns to JSON storage 
--          consistent with the Services table structure.
-- Author: Gemini
-- Date: 2026-03-07
-- ============================================================

-- STEP 1: Add the [Additional] column if it doesn't exist
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Policies]') 
    AND name = 'Additional'
)
BEGIN
    ALTER TABLE [dbo].[Policies]
    ADD [Additional] NVARCHAR(MAX) NULL;
    
    PRINT 'SUCCESS: Added [Additional] column to [Policies] table.';
END
ELSE
BEGIN
    PRINT 'INFO: [Additional] column already exists.';
END
GO

-- STEP 2: Migrate existing data from generic columns to JSON format
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
PRINT 'SUCCESS: Migrated TypeId 1002 (Check-In/Out).';
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
PRINT 'SUCCESS: Migrated TypeId 1003 (Cancellation).';
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
PRINT 'SUCCESS: Migrated TypeId 1004 (Children & Extra Beds).';
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
PRINT 'SUCCESS: Migrated TypeId 2002 (Pets).';
GO

-- STEP 3: Data Verification (Review before dropping columns)
SELECT 
    Id, 
    Name, 
    TypeId,
    Additional,
    -- Legacy columns for verification
    TimeFrom, TimeTo, IntValue1, IntValue2, Amount, [Percent], BoolValue
FROM [dbo].[Policies]
ORDER BY TypeId, Id;
GO

-- STEP 4: Post-Migration JSON Cleanup 
-- Convert string "true"/"false" to native JSON booleans for better compatibility
UPDATE Policies 
SET Additional = REPLACE(REPLACE(Additional, '"true"', 'true'), '"false"', 'false')
WHERE TypeId IN (1003, 2002);
PRINT 'SUCCESS: Sanitized JSON boolean values.';
GO

-- STEP 5: (OPTIONAL) Cleanup Legacy Columns
-- WARNING: Execute only after thorough verification of the JSON data!
/*
ALTER TABLE [dbo].[Policies] DROP COLUMN [TimeFrom];
ALTER TABLE [dbo].[Policies] DROP COLUMN [TimeTo];
ALTER TABLE [dbo].[Policies] DROP COLUMN [IntValue1];
ALTER TABLE [dbo].[Policies] DROP COLUMN [IntValue2];
ALTER TABLE [dbo].[Policies] DROP COLUMN [Amount];
ALTER TABLE [dbo].[Policies] DROP COLUMN [Percent];
ALTER TABLE [dbo].[Policies] DROP COLUMN [BoolValue];
PRINT 'CLEANUP: Dropped legacy generic columns.';
*/