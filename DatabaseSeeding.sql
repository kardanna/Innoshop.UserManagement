USE UserManagement;

GO

-- Users
INSERT INTO [Identity].[User] (
    Id,
    FirstName,
    LastName,
    DateOfBirth,
    Email,
    PasswordHash,
    IsEmailVerified
)
VALUES 
    (
        '30fc2d9e-3bb0-4bdc-d15b-08de2383d454',
        'Admin',
        'Admin',
        '2000-01-01',
        'admin@innoshop.by',
        'AQAAAAIAAYagAAAAEBZ2EtG4oB80p/B/1tWjr27MgHcqtVLPyaf7a/wnQsC7/rzf0J2fVO1jMhrGPy5vQw==',
        1
    ),

    (
        '160be924-907f-4d70-d15c-08de2383d454',
        'Ivan',
        'Ivanov',
        '2000-01-01',
        'ivan.ivanov@gmail.com',
        'AQAAAAIAAYagAAAAEDUID6axCz6cvyUWqrPGPCrA+Mm5w8K+1vSgeMrXoqk+NjrjeiCIS9IevKEbet2QdQ==',
        1
    );

GO

-- UserRole
INSERT INTO [Identity].[UserRole] (UserId, RoleId)
VALUES
    ('30fc2d9e-3bb0-4bdc-d15b-08de2383d454', 1),
    ('160be924-907f-4d70-d15c-08de2383d454', 2);
