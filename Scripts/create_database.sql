-- =====================================================================
-- HotelBooking Database — Full Creation Script (SQL Server)
-- Generated : 11-Apr-2026
-- Project   : Hotel_Blazor (CyberSoft Final Project)
-- EF Core   : Database-First (schema is source of truth)
-- =====================================================================
-- EXECUTION ORDER (respects FK dependencies):
--   1.  Countries, Roles
--   2.  Cities
--   3.  AmenityTypes, PolicyTypes, ServiceTypes
--   4.  BedTypes, RoomViews, RoomQualityGroups, UnitTypes
--   5.  Amenities, Policies, Services, RoomQualities
--   6.  Users
--   7.  UserRoles, UpgradeRequests
--   8.  Hotels
--   9.  HotelImages, HotelAmenities, HotelPolicies
--  10.  RoomTypes
--  11.  RoomTypeBedConfigs, RoomAmenities, RoomImages, Rooms
--  12.  Bookings
--  13.  BookingRooms, BookingServices
--  14.  Payments, Reviews, Messages, Notifications
-- =====================================================================

USE HotelBooking;
GO

-- =====================================================================
-- 1. LOOKUP: Countries
-- =====================================================================
CREATE TABLE Countries (
    Id      INT             NOT NULL IDENTITY(1,1),
    Name    NVARCHAR(100)   NOT NULL,
    Code    NVARCHAR(10)    NULL,

    CONSTRAINT PK__Countries PRIMARY KEY (Id),
    CONSTRAINT UQ_Countries_Name UNIQUE (Name)
);
GO

-- =====================================================================
-- 2. LOOKUP: Cities
-- =====================================================================
CREATE TABLE Cities (
    Id          INT             NOT NULL IDENTITY(1,1),
    Name        NVARCHAR(100)   NOT NULL,
    CountryId   INT             NOT NULL,

    CONSTRAINT PK__Cities PRIMARY KEY (Id),
    CONSTRAINT FK__Cities__CountryId FOREIGN KEY (CountryId)
        REFERENCES Countries(Id)
);
GO

CREATE INDEX IX_Cities_Name
    ON Cities(Name);

CREATE UNIQUE INDEX UQ_Cities_CountryId_Name
    ON Cities(CountryId, Name);
GO

-- =====================================================================
-- 3. USERS & AUTH: Roles
-- =====================================================================
CREATE TABLE Roles (
    Id          INT             NOT NULL IDENTITY(1,1),
    Name        NVARCHAR(50)    NOT NULL,
    Description NVARCHAR(255)   NULL,
    IsDeleted   BIT             NOT NULL CONSTRAINT DF_Roles_IsDeleted DEFAULT (0),

    CONSTRAINT PK__Roles PRIMARY KEY (Id),
    CONSTRAINT UQ__Roles__Name UNIQUE (Name)
);
GO

-- =====================================================================
-- 4. USERS
-- =====================================================================
CREATE TABLE Users (
    Id              INT             NOT NULL IDENTITY(1,1),
    UserName        NVARCHAR(100)   NOT NULL,
    Email           NVARCHAR(255)   NOT NULL,
    PasswordHash    NVARCHAR(255)   NOT NULL,
    FullName        NVARCHAR(150)   NULL,
    PhoneNumber     NVARCHAR(20)    NOT NULL,
    AvatarUrl       NVARCHAR(500)   NULL,
    DateOfBirth     DATE            NULL,
    Address         NVARCHAR(255)   NULL,
    TaxCode         NVARCHAR(50)    NULL,
    Additional      NVARCHAR(MAX)   NULL,
    IsActive        BIT             NOT NULL CONSTRAINT DF_Users_IsActive    DEFAULT (1),
    IsDeleted       BIT             NOT NULL CONSTRAINT DF_Users_IsDeleted   DEFAULT (0),
    CreatedAt       DATETIME        NOT NULL CONSTRAINT DF_Users_CreatedAt   DEFAULT (GETDATE()),

    CONSTRAINT PK__Users PRIMARY KEY (Id),
    CONSTRAINT UQ__Users__Email    UNIQUE (Email),
    CONSTRAINT UQ__Users__UserName UNIQUE (UserName)
);
GO

-- =====================================================================
-- 5. USER ROLES (junction)
-- =====================================================================
CREATE TABLE UserRoles (
    UserId      INT         NOT NULL,
    RoleId      INT         NOT NULL,
    CreatedAt   DATETIME    NOT NULL CONSTRAINT DF_UserRoles_CreatedAt DEFAULT (GETDATE()),

    CONSTRAINT PK__UserRoles        PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Users   FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_UserRoles_Roles   FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);
GO

-- =====================================================================
-- 6. UPGRADE REQUESTS (Customer → Owner)
-- =====================================================================
CREATE TABLE UpgradeRequests (
    Id          INT             NOT NULL IDENTITY(1,1),
    UserId      INT             NOT NULL,
    Status      NVARCHAR(20)    NOT NULL,   -- Pending | Approved | Rejected
    RequestedAt DATETIME        NOT NULL,
    ApprovedAt  DATETIME        NULL,
    ApprovedBy  INT             NULL,
    Address     NVARCHAR(255)   NULL,
    TaxCode     NVARCHAR(50)    NULL,
    Additional  NVARCHAR(MAX)   NULL,

    CONSTRAINT PK__UpgradeRequests PRIMARY KEY (Id),
    CONSTRAINT FK_UpgradeRequests_Users FOREIGN KEY (UserId)
        REFERENCES Users(Id),
    CONSTRAINT FK_UpgradeRequests_Admin FOREIGN KEY (ApprovedBy)
        REFERENCES Users(Id),
    CONSTRAINT CK_UpgradeRequests_Status
        CHECK (Status IN ('Pending', 'Approved', 'Rejected'))
);
GO

CREATE INDEX IX_UpgradeRequests_Status
    ON UpgradeRequests(Status);
GO

-- =====================================================================
-- 7. AMENITY CATALOG
-- =====================================================================
CREATE TABLE AmenityTypes (
    Id          INT             NOT NULL IDENTITY(1,1),
    Name        NVARCHAR(100)   NOT NULL,
    Description NVARCHAR(255)   NULL,
    IsDeleted   BIT             NOT NULL CONSTRAINT DF_AmenityTypes_IsDeleted DEFAULT (0),

    CONSTRAINT PK__AmenityTypes PRIMARY KEY (Id)
);
GO

CREATE TABLE Amenities (
    Id          INT             NOT NULL IDENTITY(1,1),
    Name        NVARCHAR(100)   NOT NULL,
    TypeId      INT             NOT NULL,
    IsDeleted   BIT             NOT NULL CONSTRAINT DF_Amenities_IsDeleted DEFAULT (0),

    CONSTRAINT PK__Amenities PRIMARY KEY (Id),
    CONSTRAINT UQ__Amenities__Name  UNIQUE (Name),
    CONSTRAINT FK_Amenities_Type    FOREIGN KEY (TypeId) REFERENCES AmenityTypes(Id)
);
GO

-- =====================================================================
-- 8. POLICY CATALOG
-- =====================================================================
CREATE TABLE PolicyTypes (
    Id          INT             NOT NULL IDENTITY(1,1),
    Name        NVARCHAR(50)    NOT NULL,
    IsDeleted   BIT             NOT NULL CONSTRAINT DF_PolicyTypes_IsDeleted DEFAULT (0),

    CONSTRAINT PK__PolicyTypes PRIMARY KEY (Id)
);
GO

CREATE TABLE Policies (
    Id          INT             NOT NULL IDENTITY(1,1),
    Name        NVARCHAR(150)   NOT NULL,
    TypeId      INT             NOT NULL,
    Value       NVARCHAR(MAX)   NULL,
    IsDeleted   BIT             NOT NULL CONSTRAINT DF_Policies_IsDeleted DEFAULT (0),

    CONSTRAINT PK__Policies         PRIMARY KEY (Id),
    CONSTRAINT UQ_Policies_Name     UNIQUE (Name),
    CONSTRAINT FK_Policies_PolicyTypes FOREIGN KEY (TypeId) REFERENCES PolicyTypes(Id)
);
GO

-- =====================================================================
-- 9. SERVICE CATALOG
-- =====================================================================
CREATE TABLE ServiceTypes (
    Id          INT             NOT NULL IDENTITY(1,1),
    Name        NVARCHAR(50)    NOT NULL,
    Description NVARCHAR(100)   NULL,
    IsDeleted   BIT             NOT NULL CONSTRAINT DF_ServiceTypes_IsDeleted DEFAULT (0),

    CONSTRAINT PK__ServiceTypes PRIMARY KEY (Id)
);
GO

CREATE TABLE Services (
    Id          INT             NOT NULL IDENTITY(1,1),
    Name        NVARCHAR(100)   NOT NULL,
    Description NVARCHAR(255)   NULL,
    Price       DECIMAL(18,2)   NOT NULL,
    TypeId      INT             NOT NULL,
    IsDeleted   BIT             NOT NULL CONSTRAINT DF_Services_IsDeleted DEFAULT (0),

    CONSTRAINT PK__Services         PRIMARY KEY (Id),
    CONSTRAINT UQ_Services_Name     UNIQUE (Name),
    CONSTRAINT FK_Services_ServiceTypes FOREIGN KEY (TypeId) REFERENCES ServiceTypes(Id)
);
GO

-- =====================================================================
-- 10. ROOM ATTRIBUTE CATALOG
-- =====================================================================
CREATE TABLE BedTypes (
    Id              INT             NOT NULL IDENTITY(1,1),
    Name            NVARCHAR(100)   NOT NULL,
    Description     NVARCHAR(100)   NULL,
    DefaultCapacity INT             NOT NULL CONSTRAINT DF_BedTypes_DefaultCapacity DEFAULT (1),
    IsDeleted       BIT             NOT NULL CONSTRAINT DF_BedTypes_IsDeleted DEFAULT (0),

    CONSTRAINT PK__BedTypes PRIMARY KEY (Id)
);
GO

CREATE TABLE RoomViews (
    Id          INT             NOT NULL IDENTITY(1,1),
    Name        NVARCHAR(100)   NOT NULL,
    Description NVARCHAR(100)   NULL,
    IsDeleted   BIT             NOT NULL CONSTRAINT DF_RoomViews_IsDeleted DEFAULT (0),

    CONSTRAINT PK__RoomViews PRIMARY KEY (Id)
);
GO

CREATE TABLE RoomQualityGroups (
    Id          INT             NOT NULL IDENTITY(1,1),
    Name        NVARCHAR(100)   NOT NULL,
    Description NVARCHAR(100)   NULL,
    SortOrder   INT             NOT NULL CONSTRAINT DF_RoomQualityGroups_SortOrder DEFAULT (0),
    IsDeleted   BIT             NOT NULL CONSTRAINT DF_RoomQualityGroups_IsDeleted DEFAULT (0),

    CONSTRAINT PK__RoomQualityGroups PRIMARY KEY (Id)
);
GO

CREATE TABLE RoomQualities (
    Id          INT             NOT NULL IDENTITY(1,1),
    Name        NVARCHAR(100)   NOT NULL,
    Description NVARCHAR(100)   NULL,
    SortOrder   INT             NOT NULL CONSTRAINT DF_RoomQualities_SortOrder DEFAULT (0),
    TypeId      INT             NOT NULL,
    IsDeleted   BIT             NOT NULL CONSTRAINT DF_RoomQualities_IsDeleted DEFAULT (0),

    CONSTRAINT PK__RoomQualities PRIMARY KEY (Id),
    CONSTRAINT FK__RoomQualities__Group FOREIGN KEY (TypeId) REFERENCES RoomQualityGroups(Id)
);
GO

CREATE TABLE UnitTypes (
    Id              INT             NOT NULL IDENTITY(1,1),
    Name            NVARCHAR(100)   NOT NULL,
    Description     NVARCHAR(100)   NULL,
    IsEntirePlace   BIT             NOT NULL CONSTRAINT DF_UnitTypes_IsEntirePlace DEFAULT (0),
    IsDeleted       BIT             NOT NULL CONSTRAINT DF_UnitTypes_IsDeleted DEFAULT (0),

    CONSTRAINT PK__UnitTypes PRIMARY KEY (Id)
);
GO

-- =====================================================================
-- 11. HOTELS
-- =====================================================================
CREATE TABLE Hotels (
    Id              INT             NOT NULL IDENTITY(1,1),
    Name            NVARCHAR(200)   NOT NULL,
    Address         NVARCHAR(500)   NOT NULL,
    Description     NVARCHAR(MAX)   NULL,
    CoverImageUrl   NVARCHAR(500)   NULL,
    OwnerId         INT             NOT NULL,
    CityId          INT             NULL,
    CountryId       INT             NULL,
    IsVerified      BIT             NOT NULL CONSTRAINT DF_Hotels_IsVerified  DEFAULT (0),
    Status          NVARCHAR(50)    NOT NULL CONSTRAINT DF_Hotels_Status      DEFAULT ('PendingVerification'),
    Additional      NVARCHAR(MAX)   NULL,
    IsDeleted       BIT             NOT NULL CONSTRAINT DF_Hotels_IsDeleted   DEFAULT (0),
    CreatedAt       DATETIME        NOT NULL CONSTRAINT DF_Hotels_CreatedAt   DEFAULT (GETDATE()),

    CONSTRAINT PK__Hotels       PRIMARY KEY (Id),
    CONSTRAINT FK_Hotels_Owner  FOREIGN KEY (OwnerId)    REFERENCES Users(Id),
    CONSTRAINT FK__Hotels__CityId    FOREIGN KEY (CityId)    REFERENCES Cities(Id),
    CONSTRAINT FK__Hotels__CountryId FOREIGN KEY (CountryId) REFERENCES Countries(Id),
    CONSTRAINT CK_Hotels_Status CHECK (
        Status IN ('PendingVerification','Active','Suspended','Closed')
    )
);
GO

CREATE INDEX IX_Hotels_OwnerId
    ON Hotels(OwnerId);

CREATE INDEX IX_Hotels_CityId
    ON Hotels(CityId);

CREATE INDEX IX_Hotels_CountryId
    ON Hotels(CountryId);

CREATE INDEX IX_Hotels_City_Verified_Status
    ON Hotels(CityId, IsVerified, Status);
GO

-- =====================================================================
-- 12. HOTEL IMAGES
-- =====================================================================
CREATE TABLE HotelImages (
    Id          INT             NOT NULL IDENTITY(1,1),
    HotelId     INT             NOT NULL,
    ImageUrl    NVARCHAR(500)   NOT NULL,
    IsDeleted   BIT             NOT NULL CONSTRAINT DF_HotelImages_IsDeleted DEFAULT (0),

    CONSTRAINT PK__HotelImages     PRIMARY KEY (Id),
    CONSTRAINT FK_HotelImages_Hotels FOREIGN KEY (HotelId) REFERENCES Hotels(Id)
);
GO

CREATE INDEX IX_HotelImages_HotelId
    ON HotelImages(HotelId);
GO

-- =====================================================================
-- 13. HOTEL AMENITIES (junction)
-- =====================================================================
CREATE TABLE HotelAmenities (
    HotelId     INT         NOT NULL,
    AmenityId   INT         NOT NULL,
    CreatedAt   DATETIME    NOT NULL CONSTRAINT DF_HotelAmenities_CreatedAt DEFAULT (GETDATE()),

    CONSTRAINT PK__HotelAmenities          PRIMARY KEY (HotelId, AmenityId),
    CONSTRAINT FK_HotelAmenities_Hotels    FOREIGN KEY (HotelId)   REFERENCES Hotels(Id),
    CONSTRAINT FK_HotelAmenities_Amenities FOREIGN KEY (AmenityId) REFERENCES Amenities(Id)
);
GO

CREATE INDEX IX_HotelAmenities_HotelId
    ON HotelAmenities(HotelId);
GO

-- =====================================================================
-- 14. HOTEL POLICIES (junction)
-- =====================================================================
CREATE TABLE HotelPolicies (
    HotelId     INT         NOT NULL,
    PolicyId    INT         NOT NULL,
    CreatedAt   DATETIME    NOT NULL CONSTRAINT DF_HotelPolicies_CreatedAt DEFAULT (GETDATE()),

    CONSTRAINT PK__HotelPolicies           PRIMARY KEY (HotelId, PolicyId),
    CONSTRAINT FK_HotelPolicies_Hotels     FOREIGN KEY (HotelId)  REFERENCES Hotels(Id),
    CONSTRAINT FK_HotelPolicies_Policies   FOREIGN KEY (PolicyId) REFERENCES Policies(Id)
);
GO

CREATE INDEX IX_HotelPolicies_HotelId
    ON HotelPolicies(HotelId);
GO

-- =====================================================================
-- 15. ROOM TYPES
-- =====================================================================
CREATE TABLE RoomTypes (
    Id                  INT             NOT NULL IDENTITY(1,1),
    HotelId             INT             NOT NULL,
    Name                NVARCHAR(100)   NOT NULL,
    Description         NVARCHAR(MAX)   NULL,
    PricePerNight       DECIMAL(18,2)   NOT NULL,
    Capacity            INT             NOT NULL,
    AdultCapacity       INT             NOT NULL,
    ChildCapacity       INT             NOT NULL,
    UnitTypeId          INT             NOT NULL,
    QualityId           INT             NULL,
    RoomViewId          INT             NULL,
    IsPrivateBathroom   BIT             NOT NULL CONSTRAINT DF_RoomTypes_IsPrivateBathroom DEFAULT (1),
    HasBalcony          BIT             NOT NULL,
    HasTerrace          BIT             NOT NULL,
    CanAddExtraBed      BIT             NOT NULL,
    MaxExtraBeds        INT             NULL CONSTRAINT DF_RoomTypes_MaxExtraBeds DEFAULT (0),
    AreaSqm             FLOAT           NULL,
    Additional          NVARCHAR(MAX)   NULL,
    IsDeleted           BIT             NOT NULL CONSTRAINT DF_RoomTypes_IsDeleted DEFAULT (0),

    CONSTRAINT PK__RoomTypes            PRIMARY KEY (Id),
    CONSTRAINT FK_RoomTypes_Hotels      FOREIGN KEY (HotelId)    REFERENCES Hotels(Id),
    CONSTRAINT FK_RoomTypes_UnitType    FOREIGN KEY (UnitTypeId) REFERENCES UnitTypes(Id),
    CONSTRAINT FK_RoomTypes_Quality     FOREIGN KEY (QualityId)  REFERENCES RoomQualities(Id),
    CONSTRAINT FK_RoomTypes_View        FOREIGN KEY (RoomViewId) REFERENCES RoomViews(Id),
    CONSTRAINT CK_RoomTypes_Price       CHECK (PricePerNight >= 0),
    CONSTRAINT CK_RoomTypes_Capacity    CHECK (Capacity > 0 AND AdultCapacity >= 0 AND ChildCapacity >= 0)
);
GO

CREATE INDEX IX_RoomTypes_HotelId
    ON RoomTypes(HotelId);

CREATE INDEX IX_RoomTypes_Hotel_Capacity
    ON RoomTypes(HotelId, AdultCapacity, ChildCapacity);

CREATE UNIQUE INDEX UQ_RoomTypes_Name_Hotel
    ON RoomTypes(HotelId, Name)
    WHERE IsDeleted = 0;
GO

-- =====================================================================
-- 16. ROOM TYPE — BED CONFIGURATIONS
-- =====================================================================
CREATE TABLE RoomTypeBedConfigs (
    Id          INT     NOT NULL IDENTITY(1,1),
    RoomTypeId  INT     NOT NULL,
    BedTypeId   INT     NOT NULL,
    Quantity    INT     NOT NULL CONSTRAINT DF_RoomTypeBedConfigs_Quantity DEFAULT (1),

    CONSTRAINT PK__RoomTypeBedConfigs      PRIMARY KEY (Id),
    CONSTRAINT FK_BedConfig_RoomType        FOREIGN KEY (RoomTypeId) REFERENCES RoomTypes(Id),
    CONSTRAINT FK_BedConfig_BedType         FOREIGN KEY (BedTypeId)  REFERENCES BedTypes(Id),
    CONSTRAINT CK_RoomTypeBedConfigs_Qty    CHECK (Quantity > 0)
);
GO

-- =====================================================================
-- 17. ROOM TYPE — AMENITIES (junction)
-- =====================================================================
CREATE TABLE RoomAmenities (
    RoomTypeId  INT         NOT NULL,
    AmenityId   INT         NOT NULL,
    CreatedAt   DATETIME    NOT NULL CONSTRAINT DF_RoomAmenities_CreatedAt DEFAULT (GETDATE()),

    CONSTRAINT PK__RoomAmenities           PRIMARY KEY (RoomTypeId, AmenityId),
    CONSTRAINT FK_RoomAmenities_RoomTypes  FOREIGN KEY (RoomTypeId) REFERENCES RoomTypes(Id),
    CONSTRAINT FK_RoomAmenities_Amenities  FOREIGN KEY (AmenityId)  REFERENCES Amenities(Id)
);
GO

CREATE INDEX IX_RoomAmenities_RoomTypeId
    ON RoomAmenities(RoomTypeId);
GO

-- =====================================================================
-- 18. ROOM TYPE — IMAGES
-- =====================================================================
CREATE TABLE RoomImages (
    Id          INT             NOT NULL IDENTITY(1,1),
    RoomTypeId  INT             NOT NULL,
    ImageUrl    NVARCHAR(500)   NOT NULL,
    IsDeleted   BIT             NOT NULL CONSTRAINT DF_RoomImages_IsDeleted DEFAULT (0),

    CONSTRAINT PK__RoomImages          PRIMARY KEY (Id),
    CONSTRAINT FK_RoomImages_RoomTypes FOREIGN KEY (RoomTypeId) REFERENCES RoomTypes(Id)
);
GO

CREATE INDEX IX_RoomImages_RoomTypeId
    ON RoomImages(RoomTypeId);
GO

-- =====================================================================
-- 19. ROOMS (physical room units)
-- =====================================================================
CREATE TABLE Rooms (
    Id          INT             NOT NULL IDENTITY(1,1),
    RoomTypeId  INT             NOT NULL,
    RoomNumber  NVARCHAR(50)    NOT NULL,
    Status      NVARCHAR(50)    NOT NULL CONSTRAINT DF_Rooms_Status DEFAULT ('Available'),
    Additional  NVARCHAR(MAX)   NULL,
    IsDeleted   BIT             NOT NULL CONSTRAINT DF_Rooms_IsDeleted DEFAULT (0),

    CONSTRAINT PK__Rooms            PRIMARY KEY (Id),
    CONSTRAINT FK_Rooms_RoomTypes   FOREIGN KEY (RoomTypeId) REFERENCES RoomTypes(Id),
    CONSTRAINT CK_Rooms_Status      CHECK (
        Status IN ('Available','Occupied','Maintenance','Cleaning')
    )
);
GO

CREATE INDEX IX_Rooms_RoomTypeId
    ON Rooms(RoomTypeId);

CREATE INDEX IX_Rooms_RoomType_IsDeleted
    ON Rooms(RoomTypeId, IsDeleted);
GO

-- =====================================================================
-- 20. BOOKINGS
-- =====================================================================
CREATE TABLE Bookings (
    Id              INT             NOT NULL IDENTITY(1,1),
    CustomerId      INT             NOT NULL,
    HotelId         INT             NOT NULL,
    RoomTypeId      INT             NOT NULL,
    CheckInDate     DATE            NOT NULL,
    CheckOutDate    DATE            NOT NULL,
    TotalPrice      DECIMAL(18,2)   NOT NULL,
    Status          NVARCHAR(50)    NOT NULL CONSTRAINT DF_Bookings_Status DEFAULT ('PendingPayment'),
    Additional      NVARCHAR(MAX)   NULL,
    IsDeleted       BIT             NOT NULL CONSTRAINT DF_Bookings_IsDeleted DEFAULT (0),
    CreatedAt       DATETIME        NOT NULL CONSTRAINT DF_Bookings_CreatedAt DEFAULT (GETDATE()),

    CONSTRAINT PK__Bookings             PRIMARY KEY (Id),
    CONSTRAINT FK_Bookings_Customers    FOREIGN KEY (CustomerId)  REFERENCES Users(Id),
    CONSTRAINT FK_Bookings_Hotels       FOREIGN KEY (HotelId)     REFERENCES Hotels(Id),
    CONSTRAINT FK_Bookings_RoomTypes    FOREIGN KEY (RoomTypeId)  REFERENCES RoomTypes(Id),
    CONSTRAINT CK_Bookings_Dates        CHECK (CheckOutDate > CheckInDate),
    CONSTRAINT CK_Bookings_TotalPrice   CHECK (TotalPrice >= 0),
    CONSTRAINT CK_Bookings_Status       CHECK (
        Status IN ('PendingPayment','Confirmed','CheckedIn','CheckedOut','Cancelled','NoShow')
    )
);
GO

CREATE INDEX IX_Bookings_CustomerId
    ON Bookings(CustomerId);

CREATE INDEX IX_Bookings_HotelId
    ON Bookings(HotelId);

CREATE INDEX IX_Bookings_Status_Date
    ON Bookings(Status, CheckInDate, CheckOutDate);
GO

-- =====================================================================
-- 21. BOOKING ROOMS (junction — which physical rooms are booked)
-- =====================================================================
CREATE TABLE BookingRooms (
    BookingId   INT         NOT NULL,
    RoomId      INT         NOT NULL,
    CreatedAt   DATETIME    NOT NULL CONSTRAINT DF_BookingRooms_CreatedAt DEFAULT (GETDATE()),

    CONSTRAINT PK__BookingRooms             PRIMARY KEY (BookingId, RoomId),
    CONSTRAINT FK_BookingRooms_Bookings     FOREIGN KEY (BookingId) REFERENCES Bookings(Id),
    CONSTRAINT FK_BookingRooms_Rooms        FOREIGN KEY (RoomId)    REFERENCES Rooms(Id)
);
GO

CREATE INDEX IX_BookingRooms_BookingId
    ON BookingRooms(BookingId);

CREATE INDEX IX_BookingRooms_RoomId
    ON BookingRooms(RoomId);
GO

-- =====================================================================
-- 22. BOOKING SERVICES (add-ons attached to a booking)
-- =====================================================================
CREATE TABLE BookingServices (
    Id          INT             NOT NULL IDENTITY(1,1),
    BookingId   INT             NOT NULL,
    ServiceId   INT             NOT NULL,
    Quantity    INT             NOT NULL CONSTRAINT DF_BookingServices_Quantity DEFAULT (0),
    Price       DECIMAL(18,2)   NOT NULL,
    IsPaid      BIT             NOT NULL CONSTRAINT DF_BookingServices_IsPaid DEFAULT (0),
    CreatedAt   DATETIME        NOT NULL CONSTRAINT DF_BookingServices_CreatedAt DEFAULT (GETDATE()),

    CONSTRAINT PK__BookingServices          PRIMARY KEY (Id),
    CONSTRAINT FK_BookingServices_Bookings  FOREIGN KEY (BookingId)  REFERENCES Bookings(Id),
    CONSTRAINT FK_BookingServices_Services  FOREIGN KEY (ServiceId)  REFERENCES Services(Id),
    CONSTRAINT CK_BookingServices_Qty       CHECK (Quantity >= 0)
);
GO

-- =====================================================================
-- 23. PAYMENTS
-- =====================================================================
CREATE TABLE Payments (
    Id              INT             NOT NULL IDENTITY(1,1),
    BookingId       INT             NOT NULL,
    Amount          DECIMAL(18,2)   NOT NULL,
    PaymentMethod   NVARCHAR(50)    NOT NULL,
    TransactionId   NVARCHAR(100)   NOT NULL,
    Status          NVARCHAR(50)    NOT NULL CONSTRAINT DF_Payments_Status DEFAULT ('Pending'),
    PaidAt          DATETIME        NULL,
    Additional      NVARCHAR(MAX)   NULL,

    CONSTRAINT PK__Payments             PRIMARY KEY (Id),
    CONSTRAINT FK_Payments_Bookings     FOREIGN KEY (BookingId) REFERENCES Bookings(Id),
    CONSTRAINT CK_Payments_Amount       CHECK (Amount >= 0),
    CONSTRAINT CK_Payments_Status       CHECK (
        Status IN ('Pending','Completed','Failed','Refunded')
    )
);
GO

CREATE INDEX IX_Payments_BookingId
    ON Payments(BookingId);
GO

-- =====================================================================
-- 24. REVIEWS
-- =====================================================================
CREATE TABLE Reviews (
    Id          INT             NOT NULL IDENTITY(1,1),
    HotelId     INT             NOT NULL,
    CustomerId  INT             NOT NULL,
    Rating      INT             NULL,
    Comment     NVARCHAR(MAX)   NULL,
    Additional  NVARCHAR(MAX)   NULL,
    IsDeleted   BIT             NOT NULL CONSTRAINT DF_Reviews_IsDeleted DEFAULT (0),
    CreatedAt   DATETIME        NOT NULL CONSTRAINT DF_Reviews_CreatedAt  DEFAULT (GETDATE()),

    CONSTRAINT PK__Reviews              PRIMARY KEY (Id),
    CONSTRAINT FK_Reviews_Hotels        FOREIGN KEY (HotelId)    REFERENCES Hotels(Id),
    CONSTRAINT FK_Reviews_Customers     FOREIGN KEY (CustomerId) REFERENCES Users(Id),
    CONSTRAINT CK_Reviews_Rating        CHECK (Rating BETWEEN 1 AND 5)
);
GO

CREATE INDEX IX_Reviews_HotelId
    ON Reviews(HotelId);

CREATE INDEX IX_Reviews_CustomerId
    ON Reviews(CustomerId);
GO

-- =====================================================================
-- 25. MESSAGES
-- =====================================================================
CREATE TABLE Messages (
    Id          INT             NOT NULL IDENTITY(1,1),
    SenderId    INT             NOT NULL,
    ReceiverId  INT             NOT NULL,
    HotelId     INT             NULL,
    BookingId   INT             NULL,
    Content     NVARCHAR(MAX)   NOT NULL,
    SentAt      DATETIME        NOT NULL CONSTRAINT DF_Messages_SentAt   DEFAULT (GETDATE()),
    IsRead      BIT             NOT NULL CONSTRAINT DF_Messages_IsRead   DEFAULT (0),
    IsDeleted   BIT             NOT NULL CONSTRAINT DF_Messages_IsDeleted DEFAULT (0),
    Additional  NVARCHAR(MAX)   NULL,

    CONSTRAINT PK__Messages             PRIMARY KEY (Id),
    CONSTRAINT FK_Messages_Sender       FOREIGN KEY (SenderId)   REFERENCES Users(Id),
    CONSTRAINT FK_Messages_Receiver     FOREIGN KEY (ReceiverId) REFERENCES Users(Id),
    CONSTRAINT FK_Messages_HotelId      FOREIGN KEY (HotelId)    REFERENCES Hotels(Id),
    CONSTRAINT FK_Messages_Bookings     FOREIGN KEY (BookingId)  REFERENCES Bookings(Id)
);
GO

CREATE INDEX IX_Messages_SenderId
    ON Messages(SenderId);

CREATE INDEX IX_Messages_ReceiverId
    ON Messages(ReceiverId);

CREATE INDEX IX_Messages_HotelId
    ON Messages(HotelId)
    WHERE HotelId IS NOT NULL;

CREATE INDEX IX_Messages_BookingId
    ON Messages(BookingId)
    WHERE BookingId IS NOT NULL;
GO

-- =====================================================================
-- 26. NOTIFICATIONS
-- =====================================================================
CREATE TABLE Notifications (
    Id          INT             NOT NULL IDENTITY(1,1),
    UserId      INT             NOT NULL,
    Message     NVARCHAR(255)   NOT NULL,
    IsRead      BIT             NOT NULL CONSTRAINT DF_Notifications_IsRead    DEFAULT (0),
    IsDeleted   BIT             NOT NULL CONSTRAINT DF_Notifications_IsDeleted DEFAULT (0),
    CreatedAt   DATETIME        NOT NULL CONSTRAINT DF_Notifications_CreatedAt DEFAULT (GETDATE()),

    CONSTRAINT PK__Notifications           PRIMARY KEY (Id),
    CONSTRAINT FK_Notifications_Users      FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

CREATE INDEX IX_Notifications_UserId
    ON Notifications(UserId);
GO

-- =====================================================================
-- DONE — All 26 tables created with constraints and indexes
-- =====================================================================
PRINT 'HotelBooking database schema created successfully.';
GO
