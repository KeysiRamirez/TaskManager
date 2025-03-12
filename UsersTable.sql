-- User Table

USE TaskManager
GO

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    UserPassword NVARCHAR(255) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

INSERT INTO Users (Username, UserPassword, Role)  
VALUES ('admin', 'password', 'Admin');
