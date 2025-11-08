-- Script para eliminar un usuario y todos sus datos relacionados
-- IMPORTANTE: Reemplaza 'zastuto5@gmail.com' con el email del usuario que deseas eliminar

DECLARE @UserEmail NVARCHAR(256) = 'zastuto5@gmail.com';
DECLARE @UserId UNIQUEIDENTIFIER;

-- Obtener el ID del usuario
SELECT @UserId = Id 
FROM [Users] 
WHERE Email = @UserEmail;

IF @UserId IS NULL
BEGIN
    PRINT 'Usuario no encontrado con el email: ' + @UserEmail;
    RETURN;
END

PRINT 'Eliminando usuario con ID: ' + CAST(@UserId AS NVARCHAR(36));
PRINT 'Email: ' + @UserEmail;

BEGIN TRANSACTION;

BEGIN TRY
    -- 1. Eliminar direcciones del usuario
    DELETE FROM [UserAddresses] WHERE UserId = @UserId;
    PRINT 'Direcciones eliminadas';

    -- 2. Eliminar métodos de pago del usuario
    DELETE FROM [UserPaymentMethods] WHERE UserId = @UserId;
    PRINT 'Métodos de pago eliminados';

    -- 3. Eliminar pedidos web del usuario (si no tienen restricciones)
    -- NOTA: Si hay restricciones de integridad referencial, puede que necesites eliminar primero los items
    DELETE FROM [WebOrderItems] WHERE OrderId IN (SELECT Id FROM [WebOrders] WHERE UserId = @UserId);
    DELETE FROM [WebOrders] WHERE UserId = @UserId;
    PRINT 'Pedidos web eliminados';

    -- 4. Eliminar perfil del usuario
    DELETE FROM [UserProfiles] WHERE UserId = @UserId;
    PRINT 'Perfil eliminado';

    -- 5. Eliminar claims del usuario
    DELETE FROM [UserClaims] WHERE UserId = @UserId;
    PRINT 'Claims eliminados';

    -- 6. Eliminar roles del usuario
    DELETE FROM [UserRoles] WHERE UserId = @UserId;
    PRINT 'Roles eliminados';

    -- 7. Eliminar logins del usuario
    DELETE FROM [UserLogins] WHERE UserId = @UserId;
    PRINT 'Logins eliminados';

    -- 8. Eliminar tokens del usuario
    DELETE FROM [UserTokens] WHERE UserId = @UserId;
    PRINT 'Tokens eliminados';

    -- 9. Finalmente, eliminar el usuario de Identity
    DELETE FROM [Users] WHERE Id = @UserId;
    PRINT 'Usuario eliminado de Identity';

    COMMIT TRANSACTION;
    PRINT 'Usuario eliminado exitosamente!';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error al eliminar usuario: ' + ERROR_MESSAGE();
    THROW;
END CATCH;

