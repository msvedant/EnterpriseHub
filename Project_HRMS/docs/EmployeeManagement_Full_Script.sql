/* =====================================================================
   EMPLOYEE MANAGEMENT SYSTEM - SQL SERVER SCRIPT (schema + seed + mock data)
   ---------------------------------------------------------------------
   Purpose : Practice / testing database that matches the EF Core model
             used by the API (same tables, columns, types and indexes).
   Target  : SQL Server 2017+ / Azure SQL / SQL Server in Docker.
   Run     : Execute the whole script in SSMS, Azure Data Studio or:
             sqlcmd -S localhost,1433 -U sa -P "<password>" -C -i EmployeeManagement_Full_Script.sql
   Safe to re-run: every object and every data row is created only if missing.
   Full reset    : set @ResetExistingTables = 1 in SECTION 2, then re-run.

   IMPORTANT
   * This script is for LOCAL PRACTICE. It creates the schema itself, so do NOT
     also run 'dotnet ef database update' against the same database
     (EF would try to create the same tables again). Use one approach or the other.
   * Mock users: admin@ems.local / Admin@12345 (matches the API dev seed),
     hr@ems.local, manager@ems.local, employee@ems.local / Test@12345678.
   * Never use these accounts or passwords outside a practice database.

   CONTENTS
     1. Create database
     2. Create tables (+ optional reset)
     3. Indexes
     4. Master data
     5. Mock users, employees and detail rows
     6. Helper view
     7. Verification queries
   ===================================================================== */

/* ---------------------------------------------------------------------
   SECTION 1 - DATABASE
   --------------------------------------------------------------------- */
SET NOCOUNT ON;
IF DB_ID(N'EmployeeManagementDb') IS NULL
BEGIN
    CREATE DATABASE EmployeeManagementDb;
    PRINT 'Database EmployeeManagementDb created.';
END
ELSE
    PRINT 'Database EmployeeManagementDb already exists.';
GO

USE EmployeeManagementDb;
GO

/* ---------------------------------------------------------------------
   SECTION 2 - TABLES
   Conventions: plural PascalCase tables, <Parent>Id foreign keys, explicit
   nvarchar lengths, UTC datetime2 audit columns, soft delete (IsDeleted),
   rowversion for optimistic concurrency, named constraints, CHECK constraints
   for enum-like and range-limited columns.
   --------------------------------------------------------------------- */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

DECLARE @ResetExistingTables bit = 0;   -- <<< set to 1 to DROP all tables below and recreate them (DESTROYS DATA)

IF @ResetExistingTables = 1
BEGIN
    PRINT 'Dropping existing tables...';
    DROP TABLE IF EXISTS dbo.EmployeeExperiences;
    DROP TABLE IF EXISTS dbo.EmployeeEducations;
    DROP TABLE IF EXISTS dbo.EmployeeAddresses;
    DROP TABLE IF EXISTS dbo.EmployeeFamilies;
    DROP TABLE IF EXISTS dbo.Employees;
    DROP TABLE IF EXISTS dbo.Users;
    DROP TABLE IF EXISTS dbo.Cities;
    DROP TABLE IF EXISTS dbo.States;
    DROP TABLE IF EXISTS dbo.Countries;
    DROP TABLE IF EXISTS dbo.Relationships;
    DROP TABLE IF EXISTS dbo.EmployeeTypes;
    DROP TABLE IF EXISTS dbo.Designations;
    DROP TABLE IF EXISTS dbo.Departments;
END

/* Common columns on EVERY table:
   Id, IsActive, IsDeleted, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy, RowVersion */

IF OBJECT_ID(N'dbo.Departments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Departments
    (
    Id          int IDENTITY(1,1) NOT NULL,
    IsActive    bit           NOT NULL CONSTRAINT DF_Departments_IsActive  DEFAULT (1),
    IsDeleted   bit           NOT NULL CONSTRAINT DF_Departments_IsDeleted DEFAULT (0),
    CreatedAt   datetime2     NOT NULL CONSTRAINT DF_Departments_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   nvarchar(256) NULL,
    UpdatedAt   datetime2     NULL,
    UpdatedBy   nvarchar(256) NULL,
    DeletedAt   datetime2     NULL,
    DeletedBy   nvarchar(256) NULL,
    RowVersion  rowversion    NOT NULL,
        Name        nvarchar(150) NOT NULL,
        Code        nvarchar(20)  NULL,
        Description nvarchar(500) NULL,
        CONSTRAINT PK_Departments PRIMARY KEY CLUSTERED (Id)
    );
    PRINT 'Created dbo.Departments';
END

IF OBJECT_ID(N'dbo.Designations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Designations
    (
    Id          int IDENTITY(1,1) NOT NULL,
    IsActive    bit           NOT NULL CONSTRAINT DF_Designations_IsActive  DEFAULT (1),
    IsDeleted   bit           NOT NULL CONSTRAINT DF_Designations_IsDeleted DEFAULT (0),
    CreatedAt   datetime2     NOT NULL CONSTRAINT DF_Designations_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   nvarchar(256) NULL,
    UpdatedAt   datetime2     NULL,
    UpdatedBy   nvarchar(256) NULL,
    DeletedAt   datetime2     NULL,
    DeletedBy   nvarchar(256) NULL,
    RowVersion  rowversion    NOT NULL,
        Name        nvarchar(150) NOT NULL,
        Code        nvarchar(20)  NULL,
        Description nvarchar(500) NULL,
        CONSTRAINT PK_Designations PRIMARY KEY CLUSTERED (Id)
    );
    PRINT 'Created dbo.Designations';
END

IF OBJECT_ID(N'dbo.EmployeeTypes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeTypes
    (
    Id          int IDENTITY(1,1) NOT NULL,
    IsActive    bit           NOT NULL CONSTRAINT DF_EmployeeTypes_IsActive  DEFAULT (1),
    IsDeleted   bit           NOT NULL CONSTRAINT DF_EmployeeTypes_IsDeleted DEFAULT (0),
    CreatedAt   datetime2     NOT NULL CONSTRAINT DF_EmployeeTypes_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   nvarchar(256) NULL,
    UpdatedAt   datetime2     NULL,
    UpdatedBy   nvarchar(256) NULL,
    DeletedAt   datetime2     NULL,
    DeletedBy   nvarchar(256) NULL,
    RowVersion  rowversion    NOT NULL,
        Name        nvarchar(150) NOT NULL,
        Code        nvarchar(20)  NULL,
        Description nvarchar(500) NULL,
        CONSTRAINT PK_EmployeeTypes PRIMARY KEY CLUSTERED (Id)
    );
    PRINT 'Created dbo.EmployeeTypes';
END

IF OBJECT_ID(N'dbo.Relationships', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Relationships
    (
    Id          int IDENTITY(1,1) NOT NULL,
    IsActive    bit           NOT NULL CONSTRAINT DF_Relationships_IsActive  DEFAULT (1),
    IsDeleted   bit           NOT NULL CONSTRAINT DF_Relationships_IsDeleted DEFAULT (0),
    CreatedAt   datetime2     NOT NULL CONSTRAINT DF_Relationships_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   nvarchar(256) NULL,
    UpdatedAt   datetime2     NULL,
    UpdatedBy   nvarchar(256) NULL,
    DeletedAt   datetime2     NULL,
    DeletedBy   nvarchar(256) NULL,
    RowVersion  rowversion    NOT NULL,
        Name        nvarchar(150) NOT NULL,
        Code        nvarchar(20)  NULL,
        Description nvarchar(500) NULL,
        CONSTRAINT PK_Relationships PRIMARY KEY CLUSTERED (Id)
    );
    PRINT 'Created dbo.Relationships';
END

IF OBJECT_ID(N'dbo.Countries', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Countries
    (
    Id          int IDENTITY(1,1) NOT NULL,
    IsActive    bit           NOT NULL CONSTRAINT DF_Countries_IsActive  DEFAULT (1),
    IsDeleted   bit           NOT NULL CONSTRAINT DF_Countries_IsDeleted DEFAULT (0),
    CreatedAt   datetime2     NOT NULL CONSTRAINT DF_Countries_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   nvarchar(256) NULL,
    UpdatedAt   datetime2     NULL,
    UpdatedBy   nvarchar(256) NULL,
    DeletedAt   datetime2     NULL,
    DeletedBy   nvarchar(256) NULL,
    RowVersion  rowversion    NOT NULL,
        Name        nvarchar(150) NOT NULL,
        Code        nvarchar(20)  NULL,
        Description nvarchar(500) NULL,
        CONSTRAINT PK_Countries PRIMARY KEY CLUSTERED (Id)
    );
    PRINT 'Created dbo.Countries';
END

IF OBJECT_ID(N'dbo.States', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.States
    (
    Id          int IDENTITY(1,1) NOT NULL,
    IsActive    bit           NOT NULL CONSTRAINT DF_States_IsActive  DEFAULT (1),
    IsDeleted   bit           NOT NULL CONSTRAINT DF_States_IsDeleted DEFAULT (0),
    CreatedAt   datetime2     NOT NULL CONSTRAINT DF_States_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   nvarchar(256) NULL,
    UpdatedAt   datetime2     NULL,
    UpdatedBy   nvarchar(256) NULL,
    DeletedAt   datetime2     NULL,
    DeletedBy   nvarchar(256) NULL,
    RowVersion  rowversion    NOT NULL,
        CountryId   int           NOT NULL,
        Name        nvarchar(150) NOT NULL,
        Code        nvarchar(20)  NULL,
        Description nvarchar(500) NULL,
        CONSTRAINT PK_States PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_States_Countries_CountryId FOREIGN KEY (CountryId) REFERENCES dbo.Countries (Id)
    );
    PRINT 'Created dbo.States';
END

IF OBJECT_ID(N'dbo.Cities', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cities
    (
    Id          int IDENTITY(1,1) NOT NULL,
    IsActive    bit           NOT NULL CONSTRAINT DF_Cities_IsActive  DEFAULT (1),
    IsDeleted   bit           NOT NULL CONSTRAINT DF_Cities_IsDeleted DEFAULT (0),
    CreatedAt   datetime2     NOT NULL CONSTRAINT DF_Cities_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   nvarchar(256) NULL,
    UpdatedAt   datetime2     NULL,
    UpdatedBy   nvarchar(256) NULL,
    DeletedAt   datetime2     NULL,
    DeletedBy   nvarchar(256) NULL,
    RowVersion  rowversion    NOT NULL,
        StateId     int           NOT NULL,
        Name        nvarchar(150) NOT NULL,
        Code        nvarchar(20)  NULL,
        Description nvarchar(500) NULL,
        CONSTRAINT PK_Cities PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Cities_States_StateId FOREIGN KEY (StateId) REFERENCES dbo.States (Id)
    );
    PRINT 'Created dbo.Cities';
END

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
    Id          int IDENTITY(1,1) NOT NULL,
    IsActive    bit           NOT NULL CONSTRAINT DF_Users_IsActive  DEFAULT (1),
    IsDeleted   bit           NOT NULL CONSTRAINT DF_Users_IsDeleted DEFAULT (0),
    CreatedAt   datetime2     NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   nvarchar(256) NULL,
    UpdatedAt   datetime2     NULL,
    UpdatedBy   nvarchar(256) NULL,
    DeletedAt   datetime2     NULL,
    DeletedBy   nvarchar(256) NULL,
    RowVersion  rowversion    NOT NULL,
        Email               nvarchar(256) NOT NULL,
        PasswordHash        nvarchar(512) NOT NULL,   -- salted PBKDF2 hash, never the plain password
        FullName            nvarchar(150) NOT NULL,
        Role                nvarchar(20)  NOT NULL,
        FailedLoginAttempts int           NOT NULL CONSTRAINT DF_Users_FailedLoginAttempts DEFAULT (0),
        LockoutEnd          datetime2     NULL,
        LastLoginAt         datetime2     NULL,
        CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT CK_Users_Role CHECK (Role IN (N'Admin', N'HR', N'Manager', N'Employee'))
    );
    PRINT 'Created dbo.Users';
END

IF OBJECT_ID(N'dbo.Employees', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Employees   -- HEADER table
    (
    Id          int IDENTITY(1,1) NOT NULL,
    IsActive    bit           NOT NULL CONSTRAINT DF_Employees_IsActive  DEFAULT (1),
    IsDeleted   bit           NOT NULL CONSTRAINT DF_Employees_IsDeleted DEFAULT (0),
    CreatedAt   datetime2     NOT NULL CONSTRAINT DF_Employees_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   nvarchar(256) NULL,
    UpdatedAt   datetime2     NULL,
    UpdatedBy   nvarchar(256) NULL,
    DeletedAt   datetime2     NULL,
    DeletedBy   nvarchar(256) NULL,
    RowVersion  rowversion    NOT NULL,
        EmployeeCode       nvarchar(20)  NOT NULL,
        FirstName          nvarchar(100) NOT NULL,
        LastName           nvarchar(100) NOT NULL,
        DateOfBirth        date          NOT NULL,   -- personal data
        Gender             nvarchar(20)  NOT NULL,
        Email              nvarchar(256) NOT NULL,   -- personal data
        Mobile             nvarchar(20)  NOT NULL,   -- personal data
        JoiningDate        date          NOT NULL,
        DepartmentId       int           NOT NULL,
        DesignationId      int           NOT NULL,
        EmployeeTypeId     int           NOT NULL,
        ReportingManagerId int           NULL,       -- self reference (manager)
        CONSTRAINT PK_Employees PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Employees_Departments_DepartmentId     FOREIGN KEY (DepartmentId)       REFERENCES dbo.Departments (Id),
        CONSTRAINT FK_Employees_Designations_DesignationId   FOREIGN KEY (DesignationId)      REFERENCES dbo.Designations (Id),
        CONSTRAINT FK_Employees_EmployeeTypes_EmployeeTypeId FOREIGN KEY (EmployeeTypeId)     REFERENCES dbo.EmployeeTypes (Id),
        CONSTRAINT FK_Employees_Employees_ReportingManagerId FOREIGN KEY (ReportingManagerId) REFERENCES dbo.Employees (Id),
        CONSTRAINT CK_Employees_Gender CHECK (Gender IN (N'Male', N'Female', N'Other', N'PreferNotToSay')),
        CONSTRAINT CK_Employees_JoiningAfterBirth CHECK (JoiningDate > DateOfBirth),
        CONSTRAINT CK_Employees_NotOwnManager CHECK (ReportingManagerId IS NULL OR ReportingManagerId <> Id)
    );
    PRINT 'Created dbo.Employees';
END

IF OBJECT_ID(N'dbo.EmployeeFamilies', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeFamilies   -- DETAIL table
    (
    Id          int IDENTITY(1,1) NOT NULL,
    IsActive    bit           NOT NULL CONSTRAINT DF_EmployeeFamilies_IsActive  DEFAULT (1),
    IsDeleted   bit           NOT NULL CONSTRAINT DF_EmployeeFamilies_IsDeleted DEFAULT (0),
    CreatedAt   datetime2     NOT NULL CONSTRAINT DF_EmployeeFamilies_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   nvarchar(256) NULL,
    UpdatedAt   datetime2     NULL,
    UpdatedBy   nvarchar(256) NULL,
    DeletedAt   datetime2     NULL,
    DeletedBy   nvarchar(256) NULL,
    RowVersion  rowversion    NOT NULL,
        EmployeeId         int           NOT NULL,
        RelationshipId     int           NOT NULL,
        FullName           nvarchar(150) NOT NULL,
        DateOfBirth        date          NULL,
        Occupation         nvarchar(100) NULL,
        Phone              nvarchar(20)  NULL,
        IsDependent        bit           NOT NULL,
        IsEmergencyContact bit           NOT NULL,
        CONSTRAINT PK_EmployeeFamilies PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_EmployeeFamilies_Employees_EmployeeId      FOREIGN KEY (EmployeeId)     REFERENCES dbo.Employees (Id) ON DELETE CASCADE,
        CONSTRAINT FK_EmployeeFamilies_Relationships_RelationshipId FOREIGN KEY (RelationshipId) REFERENCES dbo.Relationships (Id)
    );
    PRINT 'Created dbo.EmployeeFamilies';
END

IF OBJECT_ID(N'dbo.EmployeeAddresses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeAddresses   -- DETAIL table
    (
    Id          int IDENTITY(1,1) NOT NULL,
    IsActive    bit           NOT NULL CONSTRAINT DF_EmployeeAddresses_IsActive  DEFAULT (1),
    IsDeleted   bit           NOT NULL CONSTRAINT DF_EmployeeAddresses_IsDeleted DEFAULT (0),
    CreatedAt   datetime2     NOT NULL CONSTRAINT DF_EmployeeAddresses_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   nvarchar(256) NULL,
    UpdatedAt   datetime2     NULL,
    UpdatedBy   nvarchar(256) NULL,
    DeletedAt   datetime2     NULL,
    DeletedBy   nvarchar(256) NULL,
    RowVersion  rowversion    NOT NULL,
        EmployeeId  int           NOT NULL,
        AddressType nvarchar(20)  NOT NULL,
        Line1       nvarchar(200) NOT NULL,
        Line2       nvarchar(200) NULL,
        CountryId   int           NOT NULL,
        StateId     int           NOT NULL,
        CityId      int           NOT NULL,
        PostalCode  nvarchar(12)  NOT NULL,
        IsPrimary   bit           NOT NULL,
        CONSTRAINT PK_EmployeeAddresses PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_EmployeeAddresses_Employees_EmployeeId FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees (Id) ON DELETE CASCADE,
        CONSTRAINT FK_EmployeeAddresses_Countries_CountryId  FOREIGN KEY (CountryId)  REFERENCES dbo.Countries (Id),
        CONSTRAINT FK_EmployeeAddresses_States_StateId       FOREIGN KEY (StateId)    REFERENCES dbo.States (Id),
        CONSTRAINT FK_EmployeeAddresses_Cities_CityId        FOREIGN KEY (CityId)     REFERENCES dbo.Cities (Id),
        CONSTRAINT CK_EmployeeAddresses_AddressType CHECK (AddressType IN (N'Permanent', N'Current', N'Correspondence'))
    );
    PRINT 'Created dbo.EmployeeAddresses';
END

IF OBJECT_ID(N'dbo.EmployeeEducations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeEducations   -- DETAIL table
    (
    Id          int IDENTITY(1,1) NOT NULL,
    IsActive    bit           NOT NULL CONSTRAINT DF_EmployeeEducations_IsActive  DEFAULT (1),
    IsDeleted   bit           NOT NULL CONSTRAINT DF_EmployeeEducations_IsDeleted DEFAULT (0),
    CreatedAt   datetime2     NOT NULL CONSTRAINT DF_EmployeeEducations_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   nvarchar(256) NULL,
    UpdatedAt   datetime2     NULL,
    UpdatedBy   nvarchar(256) NULL,
    DeletedAt   datetime2     NULL,
    DeletedBy   nvarchar(256) NULL,
    RowVersion  rowversion    NOT NULL,
        EmployeeId    int           NOT NULL,
        Degree        nvarchar(150) NOT NULL,
        FieldOfStudy  nvarchar(150) NULL,
        Institution   nvarchar(200) NOT NULL,
        YearOfPassing int           NOT NULL,
        Percentage    decimal(5,2)  NULL,
        CONSTRAINT PK_EmployeeEducations PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_EmployeeEducations_Employees_EmployeeId FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees (Id) ON DELETE CASCADE,
        CONSTRAINT CK_EmployeeEducations_Percentage CHECK (Percentage IS NULL OR (Percentage >= 0 AND Percentage <= 100))
    );
    PRINT 'Created dbo.EmployeeEducations';
END

IF OBJECT_ID(N'dbo.EmployeeExperiences', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeExperiences   -- DETAIL table
    (
    Id          int IDENTITY(1,1) NOT NULL,
    IsActive    bit           NOT NULL CONSTRAINT DF_EmployeeExperiences_IsActive  DEFAULT (1),
    IsDeleted   bit           NOT NULL CONSTRAINT DF_EmployeeExperiences_IsDeleted DEFAULT (0),
    CreatedAt   datetime2     NOT NULL CONSTRAINT DF_EmployeeExperiences_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   nvarchar(256) NULL,
    UpdatedAt   datetime2     NULL,
    UpdatedBy   nvarchar(256) NULL,
    DeletedAt   datetime2     NULL,
    DeletedBy   nvarchar(256) NULL,
    RowVersion  rowversion    NOT NULL,
        EmployeeId       int            NOT NULL,
        CompanyName      nvarchar(200)  NOT NULL,
        JobTitle         nvarchar(150)  NOT NULL,
        StartDate        date           NOT NULL,
        EndDate          date           NULL,          -- NULL = current job
        Responsibilities nvarchar(1000) NULL,
        ReasonForLeaving nvarchar(500)  NULL,
        CONSTRAINT PK_EmployeeExperiences PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_EmployeeExperiences_Employees_EmployeeId FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees (Id) ON DELETE CASCADE,
        CONSTRAINT CK_EmployeeExperiences_Dates CHECK (EndDate IS NULL OR EndDate >= StartDate)
    );
    PRINT 'Created dbo.EmployeeExperiences';
END
GO

/* ---------------------------------------------------------------------
   SECTION 3 - INDEXES
   * Unique indexes are FILTERED on IsDeleted = 0, so a soft-deleted code/name can be reused.
   * Every foreign key column is indexed (fast joins, fast cascade checks).
   * Index names follow EF Core's IX_<Table>_<Column> convention.
   --------------------------------------------------------------------- */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

DECLARE @ix TABLE (TableName sysname, IndexName sysname, Ddl nvarchar(max));
INSERT @ix (TableName, IndexName, Ddl) VALUES
 (N'Departments',   N'IX_Departments_Name',   N'CREATE UNIQUE INDEX IX_Departments_Name ON dbo.Departments (Name) WHERE IsDeleted = 0'),
 (N'Designations',  N'IX_Designations_Name',  N'CREATE UNIQUE INDEX IX_Designations_Name ON dbo.Designations (Name) WHERE IsDeleted = 0'),
 (N'EmployeeTypes', N'IX_EmployeeTypes_Name', N'CREATE UNIQUE INDEX IX_EmployeeTypes_Name ON dbo.EmployeeTypes (Name) WHERE IsDeleted = 0'),
 (N'Relationships', N'IX_Relationships_Name', N'CREATE UNIQUE INDEX IX_Relationships_Name ON dbo.Relationships (Name) WHERE IsDeleted = 0'),
 (N'Countries',     N'IX_Countries_Name',     N'CREATE UNIQUE INDEX IX_Countries_Name ON dbo.Countries (Name) WHERE IsDeleted = 0'),
 (N'States',        N'IX_States_CountryId_Name', N'CREATE UNIQUE INDEX IX_States_CountryId_Name ON dbo.States (CountryId, Name) WHERE IsDeleted = 0'),
 (N'Cities',        N'IX_Cities_StateId_Name',   N'CREATE UNIQUE INDEX IX_Cities_StateId_Name ON dbo.Cities (StateId, Name) WHERE IsDeleted = 0'),
 (N'Users',         N'IX_Users_Email',        N'CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users (Email) WHERE IsDeleted = 0'),
 (N'Employees',     N'IX_Employees_EmployeeCode', N'CREATE UNIQUE INDEX IX_Employees_EmployeeCode ON dbo.Employees (EmployeeCode) WHERE IsDeleted = 0'),
 (N'Employees',     N'IX_Employees_Email',    N'CREATE UNIQUE INDEX IX_Employees_Email ON dbo.Employees (Email) WHERE IsDeleted = 0'),
 (N'Employees',     N'IX_Employees_LastName_FirstName', N'CREATE INDEX IX_Employees_LastName_FirstName ON dbo.Employees (LastName, FirstName)'),
 (N'Employees',     N'IX_Employees_DepartmentId',       N'CREATE INDEX IX_Employees_DepartmentId ON dbo.Employees (DepartmentId)'),
 (N'Employees',     N'IX_Employees_DesignationId',      N'CREATE INDEX IX_Employees_DesignationId ON dbo.Employees (DesignationId)'),
 (N'Employees',     N'IX_Employees_EmployeeTypeId',     N'CREATE INDEX IX_Employees_EmployeeTypeId ON dbo.Employees (EmployeeTypeId)'),
 (N'Employees',     N'IX_Employees_ReportingManagerId', N'CREATE INDEX IX_Employees_ReportingManagerId ON dbo.Employees (ReportingManagerId)'),
 (N'Employees',     N'IX_Employees_IsActive',           N'CREATE INDEX IX_Employees_IsActive ON dbo.Employees (IsActive)'),
 (N'EmployeeFamilies',    N'IX_EmployeeFamilies_EmployeeId',     N'CREATE INDEX IX_EmployeeFamilies_EmployeeId ON dbo.EmployeeFamilies (EmployeeId)'),
 (N'EmployeeFamilies',    N'IX_EmployeeFamilies_RelationshipId', N'CREATE INDEX IX_EmployeeFamilies_RelationshipId ON dbo.EmployeeFamilies (RelationshipId)'),
 (N'EmployeeAddresses',   N'IX_EmployeeAddresses_EmployeeId',    N'CREATE INDEX IX_EmployeeAddresses_EmployeeId ON dbo.EmployeeAddresses (EmployeeId)'),
 (N'EmployeeAddresses',   N'IX_EmployeeAddresses_CountryId',     N'CREATE INDEX IX_EmployeeAddresses_CountryId ON dbo.EmployeeAddresses (CountryId)'),
 (N'EmployeeAddresses',   N'IX_EmployeeAddresses_StateId',       N'CREATE INDEX IX_EmployeeAddresses_StateId ON dbo.EmployeeAddresses (StateId)'),
 (N'EmployeeAddresses',   N'IX_EmployeeAddresses_CityId',        N'CREATE INDEX IX_EmployeeAddresses_CityId ON dbo.EmployeeAddresses (CityId)'),
 (N'EmployeeEducations',  N'IX_EmployeeEducations_EmployeeId',   N'CREATE INDEX IX_EmployeeEducations_EmployeeId ON dbo.EmployeeEducations (EmployeeId)'),
 (N'EmployeeExperiences', N'IX_EmployeeExperiences_EmployeeId',  N'CREATE INDEX IX_EmployeeExperiences_EmployeeId ON dbo.EmployeeExperiences (EmployeeId)');

DECLARE @tbl sysname, @idx sysname, @sql nvarchar(max);
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT TableName, IndexName, Ddl FROM @ix;
OPEN cur;
FETCH NEXT FROM cur INTO @tbl, @idx, @sql;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = @idx AND object_id = OBJECT_ID(N'dbo.' + @tbl))
    BEGIN
        EXEC sys.sp_executesql @sql;
        PRINT 'Created index ' + @idx;
    END
    FETCH NEXT FROM cur INTO @tbl, @idx, @sql;
END
CLOSE cur;
DEALLOCATE cur;
GO

/* ---------------------------------------------------------------------
   SECTION 4 - MASTER DATA (reference lists used by drop-downs)
   Idempotent: a row is inserted only if the name does not already exist.
   --------------------------------------------------------------------- */
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY
BEGIN TRANSACTION;

INSERT dbo.Departments (Name, Code, CreatedBy)
SELECT v.Name, v.Code, N'seed-script'
FROM (VALUES
    (N'Human Resources', N'HR'),
    (N'Engineering', N'ENG'),
    (N'Finance', N'FIN'),
    (N'Sales', N'SAL'),
    (N'Marketing', N'MKT'),
    (N'Operations', N'OPS'),
    (N'Information Technology', N'IT'),
    (N'Legal', N'LEG')
) AS v (Name, Code)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Departments x WHERE x.Name = v.Name AND x.IsDeleted = 0);
PRINT 'Departments: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' row(s) inserted';

INSERT dbo.Designations (Name, Code, CreatedBy)
SELECT v.Name, v.Code, N'seed-script'
FROM (VALUES
    (N'Chief Executive Officer', N'CEO'),
    (N'Director', N'DIR'),
    (N'Senior Manager', N'SMGR'),
    (N'Manager', N'MGR'),
    (N'Team Lead', N'TL'),
    (N'Senior Software Engineer', N'SSE'),
    (N'Software Engineer', N'SE'),
    (N'HR Executive', N'HRE'),
    (N'Accountant', N'ACC'),
    (N'Sales Executive', N'SEX')
) AS v (Name, Code)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Designations x WHERE x.Name = v.Name AND x.IsDeleted = 0);
PRINT 'Designations: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' row(s) inserted';

INSERT dbo.EmployeeTypes (Name, Code, CreatedBy)
SELECT v.Name, v.Code, N'seed-script'
FROM (VALUES
    (N'Full-Time', N'FT'),
    (N'Part-Time', N'PT'),
    (N'Contract', N'CON'),
    (N'Intern', N'INT'),
    (N'Consultant', N'CNS')
) AS v (Name, Code)
WHERE NOT EXISTS (SELECT 1 FROM dbo.EmployeeTypes x WHERE x.Name = v.Name AND x.IsDeleted = 0);
PRINT 'EmployeeTypes: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' row(s) inserted';

INSERT dbo.Relationships (Name, Code, CreatedBy)
SELECT v.Name, v.Code, N'seed-script'
FROM (VALUES
    (N'Father', NULL),
    (N'Mother', NULL),
    (N'Spouse', NULL),
    (N'Son', NULL),
    (N'Daughter', NULL),
    (N'Brother', NULL),
    (N'Sister', NULL),
    (N'Guardian', NULL),
    (N'Other', NULL)
) AS v (Name, Code)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Relationships x WHERE x.Name = v.Name AND x.IsDeleted = 0);
PRINT 'Relationships: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' row(s) inserted';

INSERT dbo.Countries (Name, Code, CreatedBy)
SELECT v.Name, v.Code, N'seed-script'
FROM (VALUES
    (N'India', N'IN'),
    (N'United States', N'US'),
    (N'United Kingdom', N'GB'),
    (N'Canada', N'CA'),
    (N'Australia', N'AU'),
    (N'United Arab Emirates', N'AE'),
    (N'Singapore', N'SG'),
    (N'Germany', N'DE')
) AS v (Name, Code)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Countries x WHERE x.Name = v.Name AND x.IsDeleted = 0);
PRINT 'Countries: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' row(s) inserted';

INSERT dbo.States (CountryId, Name, Code, CreatedBy)
SELECT c.Id, v.StateName, v.Code, N'seed-script'
FROM (VALUES
    (N'India', N'Maharashtra', N'MH'),
    (N'India', N'Karnataka', N'KA'),
    (N'India', N'Delhi', N'DL'),
    (N'India', N'Tamil Nadu', N'TN'),
    (N'India', N'Gujarat', N'GJ'),
    (N'United States', N'California', N'CA'),
    (N'United States', N'Texas', N'TX'),
    (N'United States', N'New York', N'NY'),
    (N'United Kingdom', N'England', N'ENG')
) AS v (CountryName, StateName, Code)
JOIN dbo.Countries c ON c.Name = v.CountryName AND c.IsDeleted = 0
WHERE NOT EXISTS (SELECT 1 FROM dbo.States s WHERE s.CountryId = c.Id AND s.Name = v.StateName AND s.IsDeleted = 0);
PRINT 'States: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' row(s) inserted';

INSERT dbo.Cities (StateId, Name, CreatedBy)
SELECT s.Id, v.CityName, N'seed-script'
FROM (VALUES
    (N'India', N'Maharashtra', N'Mumbai'),
    (N'India', N'Maharashtra', N'Pune'),
    (N'India', N'Maharashtra', N'Nagpur'),
    (N'India', N'Maharashtra', N'Navi Mumbai'),
    (N'India', N'Karnataka', N'Bengaluru'),
    (N'India', N'Karnataka', N'Mysuru'),
    (N'India', N'Delhi', N'New Delhi'),
    (N'India', N'Tamil Nadu', N'Chennai'),
    (N'India', N'Gujarat', N'Ahmedabad'),
    (N'United States', N'California', N'Los Angeles'),
    (N'United States', N'California', N'San Francisco'),
    (N'United States', N'Texas', N'Houston'),
    (N'United States', N'Texas', N'Austin'),
    (N'United States', N'New York', N'New York City'),
    (N'United Kingdom', N'England', N'London')
) AS v (CountryName, StateName, CityName)
JOIN dbo.Countries c ON c.Name = v.CountryName AND c.IsDeleted = 0
JOIN dbo.States s    ON s.CountryId = c.Id AND s.Name = v.StateName AND s.IsDeleted = 0
WHERE NOT EXISTS (SELECT 1 FROM dbo.Cities x WHERE x.StateId = s.Id AND x.Name = v.CityName AND x.IsDeleted = 0);
PRINT 'Cities: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' row(s) inserted';

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

/* ---------------------------------------------------------------------
   SECTION 5 - MOCK DATA: users, employees (header) and detail rows
   Designed so the data looks realistic and exercises every feature:
     - 12 employees in a 4-level reporting hierarchy (EMP001 is the CEO)
     - EMP011 is INACTIVE (IsActive = 0)
     - EMP012 is SOFT-DELETED (IsDeleted = 1) - hidden from the API, kept for audit
     - families, addresses, education and experience rows for most employees
   Idempotent: rows are matched on natural keys (email / employee code / names).
   --------------------------------------------------------------------- */
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY
BEGIN TRANSACTION;

-- Users (password hashes are ASP.NET Core Identity PBKDF2 hashes; plain passwords are listed in the script header)
INSERT dbo.Users (Email, PasswordHash, FullName, Role, CreatedBy)
SELECT v.Email, v.PasswordHash, v.FullName, v.Role, N'seed-script'
FROM (VALUES
    (N'admin@ems.local', N'AQAAAAEAACcQAAAAEDHvMxKCWX8plalHzo4UZC2IVAEfca+959I95QgivwVQHcf29PMT2HIZg5K0aPoEaw==', N'System Administrator', N'Admin'),
    (N'hr@ems.local', N'AQAAAAEAACcQAAAAEB4eerqhR6TN1GUBEFFVPP4z1EdTHL0fTAR3HKro1efZ8evheEZ7o7O/wdU9adGDVQ==', N'Hannah HR', N'HR'),
    (N'manager@ems.local', N'AQAAAAEAACcQAAAAEK01LXH4O3kIrmjy6UdYBaVfMjLDXSyuh80OavViDmEajL3ott3YaTFG8xnh38KamQ==', N'Manish Manager', N'Manager'),
    (N'employee@ems.local', N'AQAAAAEAACcQAAAAEIWebZ8RpKZ86tkSbWUiwmH7vCxlqjvzkzGiBNgeBazy8R399t1pqeWACkIhAfUUXg==', N'Esha Employee', N'Employee')
) AS v (Email, PasswordHash, FullName, Role)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Users u WHERE u.Email = v.Email AND u.IsDeleted = 0);
PRINT 'Users: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' row(s) inserted';

-- Employees (header). Manager links are filled in the next statement so insert order never matters.
INSERT dbo.Employees (EmployeeCode, FirstName, LastName, DateOfBirth, Gender, Email, Mobile, JoiningDate,
                      DepartmentId, DesignationId, EmployeeTypeId, IsActive, CreatedBy)
SELECT v.Code, v.FirstName, v.LastName, CONVERT(date, v.Dob, 23), v.Gender, v.Email, v.Mobile, CONVERT(date, v.Joining, 23),
       d.Id, g.Id, t.Id, CAST(v.IsActive AS bit), N'seed-script'
FROM (VALUES
    (N'EMP001', N'Rajesh', N'Mehta', N'1975-04-12', N'Male', N'rajesh.mehta@example.com', N'+919820000001', N'2015-01-05', N'Operations', N'Chief Executive Officer', N'Full-Time', 1),
    (N'EMP002', N'Priya', N'Nair', N'1982-09-23', N'Female', N'priya.nair@example.com', N'+919820000002', N'2016-03-14', N'Engineering', N'Director', N'Full-Time', 1),
    (N'EMP003', N'Amit', N'Deshmukh', N'1985-11-02', N'Male', N'amit.deshmukh@example.com', N'+919820000003', N'2017-06-01', N'Engineering', N'Senior Manager', N'Full-Time', 1),
    (N'EMP004', N'Sneha', N'Kulkarni', N'1990-02-17', N'Female', N'sneha.kulkarni@example.com', N'+919820000004', N'2019-08-19', N'Engineering', N'Team Lead', N'Full-Time', 1),
    (N'EMP005', N'Rohan', N'Joshi', N'1994-07-30', N'Male', N'rohan.joshi@example.com', N'+919820000005', N'2021-01-11', N'Engineering', N'Software Engineer', N'Full-Time', 1),
    (N'EMP006', N'Neha', N'Sharma', N'1996-12-05', N'Female', N'neha.sharma@example.com', N'+919820000006', N'2022-05-23', N'Engineering', N'Software Engineer', N'Full-Time', 1),
    (N'EMP007', N'Karan', N'Singh', N'1984-06-18', N'Male', N'karan.singh@example.com', N'+919820000007', N'2018-02-12', N'Human Resources', N'Manager', N'Full-Time', 1),
    (N'EMP008', N'Anjali', N'Gupta', N'1995-01-09', N'Female', N'anjali.gupta@example.com', N'+919820000008', N'2022-09-05', N'Human Resources', N'HR Executive', N'Full-Time', 1),
    (N'EMP009', N'Vikram', N'Rao', N'1983-10-21', N'Male', N'vikram.rao@example.com', N'+919820000009', N'2018-07-02', N'Finance', N'Manager', N'Full-Time', 1),
    (N'EMP010', N'Meera', N'Iyer', N'1993-03-27', N'Female', N'meera.iyer@example.com', N'+919820000010', N'2020-11-16', N'Finance', N'Accountant', N'Part-Time', 1),
    (N'EMP011', N'John', N'Carter', N'1989-08-14', N'Male', N'john.carter@example.com', N'+14155550111', N'2020-03-02', N'Sales', N'Sales Executive', N'Contract', 0),
    (N'EMP012', N'Sarah', N'Lee', N'1999-05-08', N'Female', N'sarah.lee@example.com', N'+14155550112', N'2023-06-12', N'Information Technology', N'Software Engineer', N'Intern', 1)
) AS v (Code, FirstName, LastName, Dob, Gender, Email, Mobile, Joining, Dept, Desig, EmpType, IsActive)
JOIN dbo.Departments  d ON d.Name = v.Dept   AND d.IsDeleted = 0
JOIN dbo.Designations g ON g.Name = v.Desig  AND g.IsDeleted = 0
JOIN dbo.EmployeeTypes t ON t.Name = v.EmpType AND t.IsDeleted = 0
WHERE NOT EXISTS (SELECT 1 FROM dbo.Employees e WHERE e.EmployeeCode = v.Code);   -- any row, so a soft-deleted mock employee is never re-inserted
PRINT 'Employees: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' row(s) inserted';

-- Reporting hierarchy
UPDATE e
SET    e.ReportingManagerId = m.Id, e.UpdatedAt = SYSUTCDATETIME(), e.UpdatedBy = N'seed-script'
FROM   dbo.Employees e
JOIN   (VALUES
    (N'EMP002', N'EMP001'),
    (N'EMP003', N'EMP002'),
    (N'EMP004', N'EMP003'),
    (N'EMP005', N'EMP004'),
    (N'EMP006', N'EMP004'),
    (N'EMP007', N'EMP001'),
    (N'EMP008', N'EMP007'),
    (N'EMP009', N'EMP001'),
    (N'EMP010', N'EMP009'),
    (N'EMP011', N'EMP001'),
    (N'EMP012', N'EMP003')
) AS v (Code, ManagerCode) ON v.Code = e.EmployeeCode
JOIN   dbo.Employees m ON m.EmployeeCode = v.ManagerCode
WHERE  e.ReportingManagerId IS NULL;

-- Family members
INSERT dbo.EmployeeFamilies (EmployeeId, RelationshipId, FullName, DateOfBirth, Occupation, Phone, IsDependent, IsEmergencyContact, CreatedBy)
SELECT e.Id, r.Id, v.FullName, CONVERT(date, v.Dob, 23), v.Occupation, v.Phone, CAST(v.IsDependent AS bit), CAST(v.IsEmergency AS bit), N'seed-script'
FROM (VALUES
    (N'EMP001', N'Spouse', N'Kavita Mehta', N'1978-02-10', N'Doctor', N'+919820100001', 1, 1),
    (N'EMP001', N'Son', N'Aarav Mehta', N'2008-07-15', N'Student', NULL, 1, 0),
    (N'EMP002', N'Spouse', N'Rahul Nair', N'1980-05-21', N'Architect', N'+919820100002', 0, 1),
    (N'EMP003', N'Father', N'Sunil Deshmukh', N'1957-03-03', N'Retired', N'+919820100003', 1, 1),
    (N'EMP003', N'Spouse', N'Pooja Deshmukh', N'1988-12-19', N'Teacher', N'+919820100004', 0, 0),
    (N'EMP004', N'Mother', N'Lata Kulkarni', N'1963-09-09', N'Homemaker', N'+919820100005', 1, 1),
    (N'EMP005', N'Father', N'Dinesh Joshi', N'1964-01-25', N'Businessman', N'+919820100006', 0, 1),
    (N'EMP006', N'Brother', N'Nikhil Sharma', N'1999-10-02', N'Student', NULL, 0, 1),
    (N'EMP007', N'Spouse', N'Ritu Singh', N'1986-04-14', N'Banker', N'+919820100007', 0, 1),
    (N'EMP007', N'Daughter', N'Anaya Singh', N'2015-11-30', N'Student', NULL, 1, 0),
    (N'EMP009', N'Spouse', N'Deepa Rao', N'1985-08-08', N'Designer', N'+919820100008', 0, 1),
    (N'EMP010', N'Mother', N'Sunita Iyer', N'1966-06-06', N'Teacher', N'+919820100009', 1, 1),
    (N'EMP011', N'Spouse', N'Emily Carter', N'1991-03-30', N'Nurse', N'+14155550199', 0, 1)
) AS v (Code, Relationship, FullName, Dob, Occupation, Phone, IsDependent, IsEmergency)
JOIN dbo.Employees e ON e.EmployeeCode = v.Code
JOIN dbo.Relationships r ON r.Name = v.Relationship AND r.IsDeleted = 0
WHERE NOT EXISTS (SELECT 1 FROM dbo.EmployeeFamilies f WHERE f.EmployeeId = e.Id AND f.FullName = v.FullName);
PRINT 'EmployeeFamilies: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' row(s) inserted';

-- Addresses (country -> state -> city are resolved by name so the hierarchy is always consistent)
INSERT dbo.EmployeeAddresses (EmployeeId, AddressType, Line1, Line2, CountryId, StateId, CityId, PostalCode, IsPrimary, CreatedBy)
SELECT e.Id, v.AddressType, v.Line1, v.Line2, c.Id, s.Id, ci.Id, v.PostalCode, CAST(v.IsPrimary AS bit), N'seed-script'
FROM (VALUES
    (N'EMP001', N'Permanent', N'14 Marine Drive', NULL, N'India', N'Maharashtra', N'Mumbai', N'400020', 1),
    (N'EMP002', N'Permanent', N'7 Koregaon Park Road', N'Near Osho Garden', N'India', N'Maharashtra', N'Pune', N'411001', 1),
    (N'EMP002', N'Current', N'A-402 Lake View Residency', NULL, N'India', N'Maharashtra', N'Pune', N'411045', 0),
    (N'EMP003', N'Permanent', N'21 Shivaji Nagar', NULL, N'India', N'Maharashtra', N'Nagpur', N'440010', 1),
    (N'EMP004', N'Permanent', N'55 Baner Road', NULL, N'India', N'Maharashtra', N'Pune', N'411045', 1),
    (N'EMP005', N'Permanent', N'9 Residency Road', NULL, N'India', N'Karnataka', N'Bengaluru', N'560025', 1),
    (N'EMP005', N'Current', N'Flat 12, Orchid Apartments', N'Whitefield', N'India', N'Karnataka', N'Bengaluru', N'560066', 0),
    (N'EMP006', N'Permanent', N'3 Anna Salai', NULL, N'India', N'Tamil Nadu', N'Chennai', N'600002', 1),
    (N'EMP007', N'Permanent', N'88 Connaught Place', NULL, N'India', N'Delhi', N'New Delhi', N'110001', 1),
    (N'EMP008', N'Permanent', N'42 CG Road', NULL, N'India', N'Gujarat', N'Ahmedabad', N'380009', 1),
    (N'EMP009', N'Permanent', N'17 Vashi Sector 9', NULL, N'India', N'Maharashtra', N'Navi Mumbai', N'400703', 1),
    (N'EMP010', N'Permanent', N'6 Gandhi Nagar', NULL, N'India', N'Karnataka', N'Mysuru', N'570001', 1),
    (N'EMP011', N'Permanent', N'500 Market Street', N'Suite 300', N'United States', N'California', N'San Francisco', N'94105', 1),
    (N'EMP012', N'Permanent', N'1200 Main Street', NULL, N'United States', N'Texas', N'Houston', N'77002', 1)
) AS v (Code, AddressType, Line1, Line2, CountryName, StateName, CityName, PostalCode, IsPrimary)
JOIN dbo.Employees e  ON e.EmployeeCode = v.Code
JOIN dbo.Countries c  ON c.Name = v.CountryName AND c.IsDeleted = 0
JOIN dbo.States s     ON s.CountryId = c.Id AND s.Name = v.StateName AND s.IsDeleted = 0
JOIN dbo.Cities ci    ON ci.StateId = s.Id AND ci.Name = v.CityName AND ci.IsDeleted = 0
WHERE NOT EXISTS (SELECT 1 FROM dbo.EmployeeAddresses a WHERE a.EmployeeId = e.Id AND a.AddressType = v.AddressType AND a.Line1 = v.Line1);
PRINT 'EmployeeAddresses: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' row(s) inserted';

-- Education
INSERT dbo.EmployeeEducations (EmployeeId, Degree, FieldOfStudy, Institution, YearOfPassing, Percentage, CreatedBy)
SELECT e.Id, v.Degree, v.FieldOfStudy, v.Institution, v.YearOfPassing, v.Percentage, N'seed-script'
FROM (VALUES
    (N'EMP001', N'MBA', N'Operations Management', N'IIM Ahmedabad', 2001, 84.00),
    (N'EMP002', N'M.Tech', N'Computer Science', N'IIT Bombay', 2006, 82.50),
    (N'EMP002', N'B.E.', N'Computer Engineering', N'Pune University', 2004, 78.00),
    (N'EMP003', N'B.E.', N'Information Technology', N'VJTI Mumbai', 2007, 74.20),
    (N'EMP004', N'B.Tech', N'Computer Science', N'COEP Pune', 2012, 80.10),
    (N'EMP005', N'B.Tech', N'Electronics', N'NIT Surathkal', 2016, 76.80),
    (N'EMP006', N'MCA', N'Computer Applications', N'Anna University', 2018, 85.00),
    (N'EMP007', N'MBA', N'Human Resources', N'Symbiosis Pune', 2008, 72.50),
    (N'EMP008', N'BBA', N'Business Administration', N'Gujarat University', 2016, 69.90),
    (N'EMP009', N'CA', N'Accounting', N'ICAI', 2007, NULL),
    (N'EMP010', N'B.Com', N'Commerce', N'University of Mysore', 2014, 71.30),
    (N'EMP011', N'BBA', N'Business', N'UC Berkeley', 2011, NULL)
) AS v (Code, Degree, FieldOfStudy, Institution, YearOfPassing, Percentage)
JOIN dbo.Employees e ON e.EmployeeCode = v.Code
WHERE NOT EXISTS (SELECT 1 FROM dbo.EmployeeEducations x WHERE x.EmployeeId = e.Id AND x.Degree = v.Degree AND x.Institution = v.Institution);
PRINT 'EmployeeEducations: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' row(s) inserted';

-- Experience
INSERT dbo.EmployeeExperiences (EmployeeId, CompanyName, JobTitle, StartDate, EndDate, Responsibilities, ReasonForLeaving, CreatedBy)
SELECT e.Id, v.CompanyName, v.JobTitle, CONVERT(date, v.StartDate, 23), CONVERT(date, v.EndDate, 23), v.Responsibilities, v.ReasonForLeaving, N'seed-script'
FROM (VALUES
    (N'EMP001', N'Global Ventures Ltd', N'Operations Head', N'2005-06-01', N'2014-12-31', N'Led plant operations across three regions.', N'Joined as CEO'),
    (N'EMP002', N'TechNova Systems', N'Engineering Manager', N'2008-07-01', N'2016-02-28', N'Managed a 25 member product engineering team.', N'Career growth'),
    (N'EMP003', N'Infosoft', N'Technical Lead', N'2009-01-05', N'2017-05-15', N'Delivered enterprise integration projects.', N'Better role'),
    (N'EMP004', N'CodeCraft', N'Senior Developer', N'2012-08-01', N'2019-07-31', N'Built microservices on .NET.', N'Relocation'),
    (N'EMP005', N'StartupHub', N'Developer', N'2016-07-01', N'2020-12-31', N'Full-stack development.', N'Higher studies'),
    (N'EMP007', N'PeopleFirst', N'HR Business Partner', N'2008-09-01', N'2018-01-31', N'Recruitment and employee engagement.', N'Career growth'),
    (N'EMP009', N'Audit & Co', N'Senior Auditor', N'2007-08-01', N'2018-06-15', N'Statutory audits.', N'Moved to industry'),
    (N'EMP011', N'SalesForce Partners', N'Account Executive', N'2012-01-09', N'2020-02-14', N'Managed enterprise accounts.', N'Contract offer')
) AS v (Code, CompanyName, JobTitle, StartDate, EndDate, Responsibilities, ReasonForLeaving)
JOIN dbo.Employees e ON e.EmployeeCode = v.Code
WHERE NOT EXISTS (SELECT 1 FROM dbo.EmployeeExperiences x WHERE x.EmployeeId = e.Id AND x.CompanyName = v.CompanyName AND x.JobTitle = v.JobTitle);
PRINT 'EmployeeExperiences: ' + CAST(@@ROWCOUNT AS varchar(10)) + ' row(s) inserted';

-- Soft-delete EMP012 (demonstrates the IsDeleted pattern: row stays, API hides it)
UPDATE dbo.Employees
SET    IsDeleted = 1, IsActive = 0, DeletedAt = SYSUTCDATETIME(), DeletedBy = N'seed-script',
       UpdatedAt = SYSUTCDATETIME(), UpdatedBy = N'seed-script'
WHERE  EmployeeCode = N'EMP012' AND IsDeleted = 0;

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

/* ---------------------------------------------------------------------
   SECTION 6 - HELPER VIEW (practice only; not used by the API)
   Flattens header + lookup names. Remember: raw SQL must filter IsDeleted itself.
   --------------------------------------------------------------------- */
CREATE OR ALTER VIEW dbo.vw_EmployeeSummary
AS
SELECT  e.Id,
        e.EmployeeCode,
        e.FirstName + N' ' + e.LastName AS FullName,
        e.Email,
        d.Name  AS Department,
        g.Name  AS Designation,
        t.Name  AS EmployeeType,
        e.JoiningDate,
        m.FirstName + N' ' + m.LastName AS ReportingManager,
        e.IsActive
FROM    dbo.Employees e
JOIN    dbo.Departments   d ON d.Id = e.DepartmentId
JOIN    dbo.Designations  g ON g.Id = e.DesignationId
JOIN    dbo.EmployeeTypes t ON t.Id = e.EmployeeTypeId
LEFT JOIN dbo.Employees   m ON m.Id = e.ReportingManagerId AND m.IsDeleted = 0
WHERE   e.IsDeleted = 0;
GO

/* ---------------------------------------------------------------------
   SECTION 7 - VERIFICATION (run these to confirm everything loaded)
   --------------------------------------------------------------------- */
SET NOCOUNT ON;

-- 7.1 Row counts (active, i.e. not soft-deleted) per table
SELECT 'Departments' AS TableName, COUNT(*) AS ActiveRows FROM dbo.Departments WHERE IsDeleted = 0
UNION ALL SELECT 'Designations',        COUNT(*) FROM dbo.Designations        WHERE IsDeleted = 0
UNION ALL SELECT 'EmployeeTypes',       COUNT(*) FROM dbo.EmployeeTypes       WHERE IsDeleted = 0
UNION ALL SELECT 'Relationships',       COUNT(*) FROM dbo.Relationships       WHERE IsDeleted = 0
UNION ALL SELECT 'Countries',           COUNT(*) FROM dbo.Countries           WHERE IsDeleted = 0
UNION ALL SELECT 'States',              COUNT(*) FROM dbo.States              WHERE IsDeleted = 0
UNION ALL SELECT 'Cities',              COUNT(*) FROM dbo.Cities              WHERE IsDeleted = 0
UNION ALL SELECT 'Users',               COUNT(*) FROM dbo.Users               WHERE IsDeleted = 0
UNION ALL SELECT 'Employees',           COUNT(*) FROM dbo.Employees           WHERE IsDeleted = 0
UNION ALL SELECT 'EmployeeFamilies',    COUNT(*) FROM dbo.EmployeeFamilies    WHERE IsDeleted = 0
UNION ALL SELECT 'EmployeeAddresses',   COUNT(*) FROM dbo.EmployeeAddresses   WHERE IsDeleted = 0
UNION ALL SELECT 'EmployeeEducations',  COUNT(*) FROM dbo.EmployeeEducations  WHERE IsDeleted = 0
UNION ALL SELECT 'EmployeeExperiences', COUNT(*) FROM dbo.EmployeeExperiences WHERE IsDeleted = 0;
-- Expected: Departments 8, Designations 10, EmployeeTypes 5, Relationships 9, Countries 8, States 9, Cities 15,
--           Users 4, Employees 11 (EMP012 is soft-deleted), Families 13, Addresses 14, Educations 12, Experiences 8

-- 7.2 Employee overview (uses the helper view)
SELECT * FROM dbo.vw_EmployeeSummary ORDER BY EmployeeCode;

-- 7.3 Reporting hierarchy (recursive CTE)
WITH Tree AS
(
    SELECT Id, EmployeeCode, FirstName + N' ' + LastName AS Name, ReportingManagerId, 0 AS Level,
           CAST(EmployeeCode AS nvarchar(400)) AS Path
    FROM   dbo.Employees WHERE ReportingManagerId IS NULL AND IsDeleted = 0
    UNION ALL
    SELECT e.Id, e.EmployeeCode, e.FirstName + N' ' + e.LastName, e.ReportingManagerId, t.Level + 1,
           CAST(t.Path + N' > ' + e.EmployeeCode AS nvarchar(400))
    FROM   dbo.Employees e JOIN Tree t ON e.ReportingManagerId = t.Id
    WHERE  e.IsDeleted = 0
)
SELECT REPLICATE(N'    ', Level) + EmployeeCode + N'  ' + Name AS Hierarchy, Path
FROM   Tree ORDER BY Path;

-- 7.4 Head-count per department
SELECT d.Name AS Department, COUNT(*) AS Employees
FROM   dbo.Employees e JOIN dbo.Departments d ON d.Id = e.DepartmentId
WHERE  e.IsDeleted = 0 AND e.IsActive = 1
GROUP  BY d.Name ORDER BY Employees DESC;

-- 7.5 Soft-deleted rows are still in the table
SELECT EmployeeCode, FirstName, LastName, IsDeleted, DeletedAt, DeletedBy FROM dbo.Employees WHERE IsDeleted = 1;

-- 7.6 Integrity check: every address city must belong to its state, and every state to its country
SELECT a.Id AS BadAddressId
FROM   dbo.EmployeeAddresses a
JOIN   dbo.Cities ci ON ci.Id = a.CityId
JOIN   dbo.States s  ON s.Id  = a.StateId
WHERE  ci.StateId <> a.StateId OR s.CountryId <> a.CountryId;   -- expect 0 rows
PRINT 'Script finished.';
