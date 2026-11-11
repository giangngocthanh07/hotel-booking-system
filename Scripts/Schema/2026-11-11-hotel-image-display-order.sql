-- FR-MED-005: preserve explicit gallery order without changing image URLs or deletion flags.
-- Database-first upgrade. Select the intended application database and execute this entire file.
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb')
    THROW 51100, 'Select the application database before running this script.', 1;
IF OBJECT_ID(N'dbo.HotelImages', N'U') IS NULL
    THROW 51101, 'The existing dbo.HotelImages table is required.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH(N'dbo.HotelImages', N'DisplayOrder') IS NULL
    BEGIN
        -- Dynamic SQL allows the new column and its backfill to run in one batch without GO.
        EXEC sys.sp_executesql N'ALTER TABLE dbo.HotelImages
            ADD DisplayOrder INT NOT NULL CONSTRAINT DF_HotelImages_DisplayOrder DEFAULT (0) WITH VALUES;';
        EXEC sys.sp_executesql N';WITH OrderedImages AS (
            SELECT Id, CONVERT(INT, ROW_NUMBER() OVER (PARTITION BY HotelId ORDER BY Id) - 1) AS Position
            FROM dbo.HotelImages
        )
        UPDATE image SET DisplayOrder = ordered.Position
        FROM dbo.HotelImages AS image JOIN OrderedImages AS ordered ON ordered.Id = image.Id;';
    END
    ELSE IF EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.HotelImages') AND name = N'DisplayOrder'
          AND (system_type_id <> 56 OR is_nullable = 1)
    )
        THROW 51102, 'Existing DisplayOrder must be a non-nullable INT. Review the schema before continuing.', 1;

    IF NOT EXISTS (
        SELECT 1 FROM sys.default_constraints AS defaults
        JOIN sys.columns AS columns ON columns.object_id = defaults.parent_object_id AND columns.column_id = defaults.parent_column_id
        WHERE defaults.parent_object_id = OBJECT_ID(N'dbo.HotelImages') AND columns.name = N'DisplayOrder'
    )
        EXEC sys.sp_executesql N'ALTER TABLE dbo.HotelImages ADD CONSTRAINT DF_HotelImages_DisplayOrder DEFAULT (0) FOR DisplayOrder;';

    IF NOT EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE parent_object_id = OBJECT_ID(N'dbo.HotelImages') AND name = N'CK_HotelImages_DisplayOrder'
    )
        EXEC sys.sp_executesql N'ALTER TABLE dbo.HotelImages WITH CHECK
            ADD CONSTRAINT CK_HotelImages_DisplayOrder CHECK (DisplayOrder >= 0);';

    EXEC sys.sp_executesql N'ALTER TABLE dbo.HotelImages WITH CHECK CHECK CONSTRAINT CK_HotelImages_DisplayOrder;';

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.HotelImages') AND name = N'IX_HotelImages_DisplayOrder'
    )
        EXEC sys.sp_executesql N'CREATE INDEX IX_HotelImages_DisplayOrder
            ON dbo.HotelImages (HotelId, DisplayOrder, Id) INCLUDE (IsDeleted, ImageUrl);';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
