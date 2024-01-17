namespace Gemnet.Persistence.Models;

public class ModelAvatar : IModel
{

    public int AvatarID;
    public int UserID;


    public static readonly string QueryCreateTable = @"CREATE TABLE IF NOT EXISTS `avatar` (
                                                              `AvatarID` int NOT NULL AUTO_INCREMENT, UNIQUE,
                                                              `UserID` int NOT NULL,
                                                               
                                                            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

    public static readonly string QueryGetAvatarIDs = "SELECT AvatarID FROM rumblefighter.avatar WHERE UserID = @UserID";

    



    public static readonly int TableCreationOrder = 999;
}