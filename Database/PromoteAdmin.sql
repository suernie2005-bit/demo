-- Manual example only. This file is never executed by the application or by M6.
-- By default it is locked to the disposable Week 5 validation database.
-- If you choose a different TEST database, update the database guard first.
IF DB_NAME() <> N'QLSanphamDB_Week5_Test'
BEGIN
    THROW 51000, 'Switch to the disposable QLSanphamDB_Week5_Test database before promoting a user.', 1;
END;
GO

DECLARE @Email nvarchar(256) = N'user@example.com';
DECLARE @UserId nvarchar(450);
DECLARE @AdminRoleId nvarchar(450);

SELECT @UserId = Id
FROM dbo.AspNetUsers
WHERE NormalizedEmail = UPPER(@Email);

SELECT @AdminRoleId = Id
FROM dbo.AspNetRoles
WHERE NormalizedName = N'ADMIN';

IF @UserId IS NULL
    THROW 51001, 'No account exists for this email. Register first.', 1;

IF @AdminRoleId IS NULL
    THROW 51002, 'The Admin role does not exist. Register an account first to initialize roles.', 1;

BEGIN TRANSACTION;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.AspNetUserRoles
    WHERE UserId = @UserId AND RoleId = @AdminRoleId
)
BEGIN
    INSERT INTO dbo.AspNetUserRoles (UserId, RoleId)
    VALUES (@UserId, @AdminRoleId);
END;

COMMIT TRANSACTION;
GO
