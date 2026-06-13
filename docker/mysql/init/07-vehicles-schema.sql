CREATE TABLE IF NOT EXISTS Vehicles (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    TenantId CHAR(36) NOT NULL,
    Plate VARCHAR(10) NOT NULL,
    Brand VARCHAR(100) NULL,
    Model VARCHAR(100) NULL,
    Color VARCHAR(50) NULL,
    ApartmentId CHAR(36) NULL,
    OwnerName VARCHAR(200) NULL,
    VehicleType INT NOT NULL DEFAULT 0,
    Active TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAtUtc DATETIME NOT NULL DEFAULT UTC_TIMESTAMP(),
    UpdatedAtUtc DATETIME NULL,
    INDEX IX_Vehicles_TenantId (TenantId),
    INDEX IX_Vehicles_Plate (Plate),
    INDEX IX_Vehicles_ApartmentId (ApartmentId),
    INDEX IX_Vehicles_Active (Active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;