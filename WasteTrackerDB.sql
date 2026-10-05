USE WasteTrackerDb;
GO

-- Insert a new test user
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'testuser')
BEGIN
    INSERT INTO Users (Username, Password) 
    VALUES ('testuser', 'password123');
END
GO

-- Verify user was inserted
SELECT * FROM Users;
GO