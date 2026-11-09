-- FR-ONB-009/011/013: record server-confirmed uploads and bind requests to the same Owner.
-- Database-first additive upgrade. Select the intended application database before execution.
-- Existing requests retain their URLs and receive a NULL BusinessLicenseUploadId.
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb')
    THROW 51000, 'Select the application database before running this script.', 1;
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL OR OBJECT_ID(N'dbo.HotelApprovalRequests', N'U') IS NULL
    THROW 51001, 'The existing Users and HotelApprovalRequests tables are required.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.BusinessLicenseUploads', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.BusinessLicenseUploads
        (
            Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_BusinessLicenseUploads_Id DEFAULT NEWID(),
            OwnerId INT NOT NULL,
            PublicId NVARCHAR(512) NOT NULL,
            ResourceType VARCHAR(16) NOT NULL,
            DeliveryType VARCHAR(32) NOT NULL,
            StoredUrl NVARCHAR(2048) NOT NULL,
            OriginalFileName NVARCHAR(255) NOT NULL,
            ContentType VARCHAR(64) NOT NULL,
            SizeBytes BIGINT NOT NULL,
            CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_BusinessLicenseUploads_CreatedAt DEFAULT SYSUTCDATETIME(),
            IsDeleted BIT NOT NULL CONSTRAINT DF_BusinessLicenseUploads_IsDeleted DEFAULT 0,
            CONSTRAINT PK_BusinessLicenseUploads PRIMARY KEY (Id),
            CONSTRAINT UQ_BusinessLicenseUploads_Id_Owner UNIQUE (Id, OwnerId),
            CONSTRAINT FK_BusinessLicenseUploads_Users FOREIGN KEY (OwnerId) REFERENCES dbo.Users (Id),
            CONSTRAINT CK_BusinessLicenseUploads_Size CHECK (SizeBytes BETWEEN 1 AND 10485760),
            CONSTRAINT CK_BusinessLicenseUploads_ContentType CHECK (ContentType IN ('application/pdf', 'image/jpeg', 'image/png')),
            CONSTRAINT CK_BusinessLicenseUploads_PublicId CHECK (LEN(LTRIM(RTRIM(PublicId))) > 0),
            CONSTRAINT CK_BusinessLicenseUploads_ResourceType CHECK (LEN(LTRIM(RTRIM(ResourceType))) > 0),
            CONSTRAINT CK_BusinessLicenseUploads_DeliveryType CHECK (LEN(LTRIM(RTRIM(DeliveryType))) > 0),
            CONSTRAINT CK_BusinessLicenseUploads_StoredUrl CHECK (LEN(LTRIM(RTRIM(StoredUrl))) > 0),
            CONSTRAINT CK_BusinessLicenseUploads_FileName CHECK (LEN(LTRIM(RTRIM(OriginalFileName))) > 0)
        );
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.BusinessLicenseUploads') AND name = N'IX_BusinessLicenseUploads_OwnerId')
        CREATE INDEX IX_BusinessLicenseUploads_OwnerId ON dbo.BusinessLicenseUploads (OwnerId);

    IF COL_LENGTH(N'dbo.HotelApprovalRequests', N'BusinessLicenseUploadId') IS NULL
        ALTER TABLE dbo.HotelApprovalRequests ADD BusinessLicenseUploadId UNIQUEIDENTIFIER NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.HotelApprovalRequests') AND name = N'IX_HotelApprovalRequests_BusinessLicenseUploadId_OwnerId')
        CREATE INDEX IX_HotelApprovalRequests_BusinessLicenseUploadId_OwnerId
            ON dbo.HotelApprovalRequests (BusinessLicenseUploadId, OwnerId);

    -- The composite FK also prevents linking a different Owner's file at the database boundary.
    -- A nullable upload ID preserves legacy history; an Owner may reuse their own upload.
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'dbo.HotelApprovalRequests') AND name = N'FK_HotelApprovalRequests_BusinessLicenseUploads_Owner')
        ALTER TABLE dbo.HotelApprovalRequests WITH CHECK
            ADD CONSTRAINT FK_HotelApprovalRequests_BusinessLicenseUploads_Owner
            FOREIGN KEY (BusinessLicenseUploadId, OwnerId) REFERENCES dbo.BusinessLicenseUploads (Id, OwnerId);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
