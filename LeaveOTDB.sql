IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'LeaveOTDB')
    CREATE DATABASE LeaveOTDB;
GO

USE LeaveOTDB;
GO

CREATE TABLE Departments (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    ParentDepartmentId INT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Department_Parent
        FOREIGN KEY (ParentDepartmentId) REFERENCES Departments(Id)
);
CREATE INDEX IX_Department_Parent ON Departments(ParentDepartmentId);

CREATE TABLE Roles (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE,
    IsActive BIT DEFAULT 1
);


CREATE TABLE Users (
    Id INT IDENTITY PRIMARY KEY,
    EmployeeCode NVARCHAR(50) UNIQUE NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) UNIQUE NOT NULL,
    DepartmentId INT NOT NULL,
    RoleId INT NOT NULL,
    ManagerId INT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),

    FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    FOREIGN KEY (ManagerId) REFERENCES Users(Id)
);

CREATE INDEX IX_User_Department ON Users(DepartmentId);
CREATE INDEX IX_User_Manager ON Users(ManagerId);


CREATE TABLE LeaveTypes (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    IsPaid BIT NOT NULL,
    MaxDaysPerYear INT NULL,
    IsActive BIT DEFAULT 1
);


CREATE TABLE LeaveBalances (
    Id INT IDENTITY PRIMARY KEY,
    UserId INT NOT NULL,
    LeaveTypeId INT NOT NULL,
    Year INT NOT NULL,
    TotalDays DECIMAL(5,2) NOT NULL,
    UsedDays DECIMAL(5,2) DEFAULT 0,

    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (LeaveTypeId) REFERENCES LeaveTypes(Id),

    CONSTRAINT UQ_LeaveBalance UNIQUE(UserId, LeaveTypeId, Year)
);

CREATE INDEX IX_LeaveBalance_User ON LeaveBalances(UserId);


CREATE TABLE LeaveRequests (
    Id BIGINT IDENTITY PRIMARY KEY,
    UserId INT NOT NULL,
    LeaveTypeId INT NOT NULL,
    FromDate DATE NOT NULL,
    ToDate DATE NOT NULL,
    TotalDays DECIMAL(5,2),
    Reason NVARCHAR(1000),
    Status VARCHAR(50) DEFAULT 'Pending',
    CurrentApprovalLevel INT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),

    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (LeaveTypeId) REFERENCES LeaveTypes(Id),

    CONSTRAINT CK_Leave_Status CHECK (Status IN ('Pending','Approved','Rejected','Cancelled'))
);

CREATE INDEX IX_Leave_User_Status ON LeaveRequests(UserId, Status);
CREATE INDEX IX_Leave_Date ON LeaveRequests(FromDate, ToDate);


CREATE TABLE OTRequests (
    Id BIGINT IDENTITY PRIMARY KEY,
    UserId INT NOT NULL,
    Reason NVARCHAR(1000),
    Status VARCHAR(50) DEFAULT 'Pending',
    CurrentApprovalLevel INT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),

    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX IX_OT_User_Status ON OTRequests(UserId, Status);


CREATE TABLE OTDetails (
    Id BIGINT IDENTITY PRIMARY KEY,
    OTRequestId BIGINT NOT NULL,
    WorkDate DATE NOT NULL,
    FromTime TIME NOT NULL,
    ToTime TIME NOT NULL,
    Hours DECIMAL(5,2) NOT NULL,

    FOREIGN KEY (OTRequestId) REFERENCES OTRequests(Id)
);

CREATE INDEX IX_OTDetail_Request ON OTDetails(OTRequestId);


CREATE TABLE ApprovalWorkflows (
    Id INT IDENTITY PRIMARY KEY,
    RequestType VARCHAR(20) NOT NULL,
    Level INT NOT NULL,
    RoleId INT NOT NULL,
    IsActive BIT DEFAULT 1,

    FOREIGN KEY (RoleId) REFERENCES Roles(Id),

    CONSTRAINT UQ_Workflow UNIQUE(RequestType, Level)
);


CREATE TABLE Approvals (
    Id BIGINT IDENTITY PRIMARY KEY,
    RequestId BIGINT NOT NULL,
    RequestType VARCHAR(20) NOT NULL,
    ApprovalLevel INT NOT NULL,
    ApproverId INT NOT NULL,
    Status VARCHAR(50) DEFAULT 'Pending',
    Comment NVARCHAR(1000),
    ActionDate DATETIME2 NULL,

    FOREIGN KEY (ApproverId) REFERENCES Users(Id)
);

CREATE INDEX IX_Approval_Inbox 
ON Approvals(ApproverId, Status);


CREATE TABLE Attachments (
    Id BIGINT IDENTITY PRIMARY KEY,
    EntityId BIGINT NOT NULL,
    EntityType VARCHAR(20) NOT NULL,
    FileName NVARCHAR(255),
    FilePath NVARCHAR(500),
    UploadedAt DATETIME2 DEFAULT SYSDATETIME()
);


CREATE TABLE Holidays (
    Id INT IDENTITY PRIMARY KEY,
    HolidayDate DATE NOT NULL UNIQUE,
    Name NVARCHAR(200)
);


CREATE TABLE AuditLogs (
    Id BIGINT IDENTITY PRIMARY KEY,
    EntityId BIGINT,
    EntityType VARCHAR(50),
    Action VARCHAR(50),
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    ActionBy INT,
    ActionDate DATETIME2 DEFAULT SYSDATETIME(),

    FOREIGN KEY (ActionBy) REFERENCES Users(Id)
);

CREATE INDEX IX_Audit_Entity ON AuditLogs(EntityType, EntityId);

USE LeaveOTDB;
GO


INSERT INTO Roles (Name) VALUES
('Admin'),
('Manager'),
('HR'),
('Employee');


INSERT INTO Departments (Name) VALUES
('Head Office');

INSERT INTO Departments (Name, ParentDepartmentId)
VALUES
('IT Department', 1),
('HR Department', 1);


INSERT INTO Users (EmployeeCode, FullName, Email, DepartmentId, RoleId)
VALUES ('EMP001','IT Manager','manager@company.com',2,2);

INSERT INTO Users (EmployeeCode, FullName, Email, DepartmentId, RoleId)
VALUES ('EMP002','HR Manager','hr@company.com',3,3);

INSERT INTO Users (EmployeeCode, FullName, Email, DepartmentId, RoleId, ManagerId)
VALUES 
('EMP003','Nguyen Van A','a@company.com',2,4,1),
('EMP004','Tran Thi B','b@company.com',2,4,1),
('EMP005','Le Van C','c@company.com',2,4,1);


INSERT INTO LeaveTypes (Name, IsPaid, MaxDaysPerYear)
VALUES
('Annual Leave',1,12),
('Sick Leave',1,10),
('Unpaid Leave',0,NULL);

DECLARE @Year INT = YEAR(GETDATE());

INSERT INTO LeaveBalances (UserId, LeaveTypeId, Year, TotalDays, UsedDays)
VALUES
(3,1,@Year,12,2),
(4,1,@Year,12,0),
(5,1,@Year,12,1);


INSERT INTO ApprovalWorkflows (RequestType, Level, RoleId)
VALUES
('Leave',1,2),
('Leave',2,3);

INSERT INTO ApprovalWorkflows (RequestType, Level, RoleId)
VALUES
('OT',1,2);

INSERT INTO Holidays (HolidayDate, Name)
VALUES
('2026-01-01','New Year'),
('2026-04-30','Reunification Day'),
('2026-09-02','National Day');


INSERT INTO LeaveRequests
(UserId, LeaveTypeId, FromDate, ToDate, TotalDays, Reason, Status)
VALUES
(3,1,'2026-03-10','2026-03-12',3,'Family trip','Pending');


INSERT INTO OTRequests
(UserId, Reason, Status)
VALUES
(4,'Project deadline support','Pending');

INSERT INTO OTDetails
(OTRequestId, WorkDate, FromTime, ToTime, Hours)
VALUES
(1,'2026-03-05','18:00','21:00',3);

INSERT INTO Approvals
(RequestId, RequestType, ApprovalLevel, ApproverId, Status)
VALUES
(1,'Leave',1,1,'Pending');

INSERT INTO Approvals
(RequestId, RequestType, ApprovalLevel, ApproverId, Status)
VALUES
(1,'OT',1,1,'Pending');


INSERT INTO Attachments
(EntityId, EntityType, FileName, FilePath)
VALUES
(1,'Leave','leave_form.pdf','/uploads/leave_form.pdf');

INSERT INTO AuditLogs
(EntityId, EntityType, Action, OldValue, NewValue, ActionBy)
VALUES
(1,'Leave','Create',NULL,'Leave created','3');

CREATE TABLE Accounts (
    Id INT IDENTITY PRIMARY KEY,
    UserId INT NOT NULL UNIQUE,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    PasswordSalt NVARCHAR(255) NULL,
    IsLocked BIT DEFAULT 0,
    FailedLoginCount INT DEFAULT 0,
    LastLoginAt DATETIME2 NULL,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),

    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX IX_Accounts_UserId ON Accounts(UserId);

INSERT INTO Accounts (UserId, Username, PasswordHash)
VALUES
(1,'manager','HASHED_PASSWORD'),
(2,'hr','HASHED_PASSWORD'),
(3,'a','HASHED_PASSWORD'),
(4,'b','HASHED_PASSWORD'),
(5,'c','HASHED_PASSWORD');