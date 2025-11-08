# Script PowerShell para eliminar un usuario de la base de datos usando Windows Authentication
# Uso: .\delete-user-by-email-windows-auth.ps1 -Email "zastuto5@gmail.com"

param(
    [Parameter(Mandatory=$true)]
    [string]$Email
)

# Usar Windows Authentication
$connectionString = "Server=localhost\SQLEXPRESS;Database=MinimarketDB;Integrated Security=true;TrustServerCertificate=true;"

Write-Host "Conectando a la base de datos..." -ForegroundColor Yellow
Write-Host "Buscando usuario con email: $Email" -ForegroundColor Yellow

$sql = @"
DECLARE @UserEmail NVARCHAR(256) = '$Email';
DECLARE @UserId UNIQUEIDENTIFIER;

SELECT @UserId = Id FROM [Users] WHERE Email = @UserEmail;

IF @UserId IS NULL
BEGIN
    SELECT 'Usuario no encontrado' AS Result;
    RETURN;
END

SELECT @UserId AS UserId, Email, UserName FROM [Users] WHERE Id = @UserId;
"@

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = $sql
    $reader = $command.ExecuteReader()
    
    if ($reader.Read()) {
        $userId = $reader["UserId"].ToString()
        $userEmail = $reader["Email"].ToString()
        $userName = $reader["UserName"].ToString()
        
        Write-Host "Usuario encontrado:" -ForegroundColor Green
        Write-Host "  ID: $userId" -ForegroundColor Cyan
        Write-Host "  Email: $userEmail" -ForegroundColor Cyan
        Write-Host "  Username: $userName" -ForegroundColor Cyan
        
        $reader.Close()
        
        $confirm = Read-Host "¿Estás seguro de eliminar este usuario y todos sus datos? (S/N)"
        if ($confirm -ne "S" -and $confirm -ne "s") {
            Write-Host "Operación cancelada." -ForegroundColor Yellow
            $connection.Close()
            return
        }
        
        Write-Host "Eliminando usuario..." -ForegroundColor Yellow
        
        $deleteSql = @"
BEGIN TRANSACTION;

BEGIN TRY
    DECLARE @UserId UNIQUEIDENTIFIER = '$userId';
    DECLARE @UserEmail NVARCHAR(256) = '$Email';
    
    -- Eliminar direcciones del usuario
    DELETE FROM [UserAddresses] WHERE UserId = @UserId;
    
    -- Eliminar métodos de pago del usuario
    DELETE FROM [UserPaymentMethods] WHERE UserId = @UserId;
    
    -- Eliminar items de pedidos web (usando CustomerEmail porque WebOrder no tiene UserId)
    DELETE FROM [WebOrderItems] WHERE WebOrderId IN (SELECT Id FROM [WebOrders] WHERE CustomerEmail = @UserEmail);
    
    -- Eliminar pedidos web del usuario (usando CustomerEmail)
    DELETE FROM [WebOrders] WHERE CustomerEmail = @UserEmail;
    
    -- Eliminar perfil del usuario
    DELETE FROM [UserProfiles] WHERE UserId = @UserId;
    
    -- Eliminar claims del usuario
    DELETE FROM [UserClaims] WHERE UserId = @UserId;
    
    -- Eliminar roles del usuario
    DELETE FROM [UserRoles] WHERE UserId = @UserId;
    
    -- Eliminar logins del usuario
    DELETE FROM [UserLogins] WHERE UserId = @UserId;
    
    -- Eliminar tokens del usuario
    DELETE FROM [UserTokens] WHERE UserId = @UserId;
    
    -- Finalmente, eliminar el usuario de Identity
    DELETE FROM [Users] WHERE Id = @UserId;
    
    COMMIT TRANSACTION;
    SELECT 'Usuario eliminado exitosamente' AS Result;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    SELECT 'Error: ' + ERROR_MESSAGE() AS Result;
    THROW;
END CATCH;
"@
        
        $deleteCommand = $connection.CreateCommand()
        $deleteCommand.CommandText = $deleteSql
        $result = $deleteCommand.ExecuteScalar()
        
        Write-Host $result -ForegroundColor Green
    } else {
        Write-Host "Usuario no encontrado con el email: $Email" -ForegroundColor Red
    }
    
    $connection.Close()
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
    if ($connection.State -eq 'Open') {
        $connection.Close()
    }
}

