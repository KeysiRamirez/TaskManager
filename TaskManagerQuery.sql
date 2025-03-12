CREATE DATABASE TaskManager

USE TaskManager
GO

CREATE TABLE Task
(
TaskId INT PRIMARY KEY IDENTITY(100, 1),
TaskDescription VARCHAR (255) NOT NULL,
Duedate DATE NOT NULL,
TaskStatus BIT NOT NULL, -- 0 is pending, 1 is completed
AdditionalData VARCHAR(255)
);

INSERT INTO Task (TaskDescription, DueDate, TaskStatus, AdditionalData)
VALUES 
('Complete project documentation', '2025-02-01', 0, 'Priority: High'),
('Prepare presentation slides', '2025-01-20', 0, 'Related to Q1 review'),
('Submit expense reports', '2025-01-25', 1, 'Submitted via email'),
('Schedule team meeting', '2025-01-18', 0, 'Topic: Sprint Planning'),
('Review code changes', '2025-01-19', 0, 'Assigned by lead');