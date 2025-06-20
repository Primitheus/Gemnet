namespace Gemnet.Persistence.Models;

public class ModelInventory : IModel
{

    public int OwnerID { get; set; }
    public int ItemID { get; set; }
    public int ServerID { get; set; }
    public int ItemEnd { get; set; }

    /* public int State;
  public DateTime LastLogin;
  public string HWID;
  public string LastIP;*/

  public static readonly string QuerySelectAllItemsForAccount = "SELECT * FROM `rumblefighter`.`inventory` WHERE `rumblefighter`.`inventory`.`OwnerID` = @OID;";


    public static readonly string QueryCreateTable = @"CREATE TABLE IF NOT EXISTS `inventory` (
                                                              `OwnerID` INT NOT NULL,
                                                              `ServerID` int NOT NULL AUTO_INCREMENT,
                                                              `ItemID` int NOT NULL,
                                                              `ItemEnd` int NOT NULL DEFAULT 0,
                                                              PRIMARY KEY (`ServerID`),
                                                              KEY `itemOwner_idx` (`OwnerID`),
                                                              CONSTRAINT `itemOwner` FOREIGN KEY (`OwnerID`) REFERENCES `accounts` (`UUID`) ON DELETE CASCADE
                                                            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";


    public static readonly string InsertItem = "INSERT INTO rumblefighter.inventory (OwnerID, ItemID, ItemEnd) VALUES (@OID, @ID, @END)";
    public static readonly string GetServerID = "SELECT ServerID FROM rumblefighter.inventory WHERE OwnerID = @OID ORDER BY ServerID DESC LIMIT 1";
    public static readonly string GetInventory = "SELECT * FROM rumblefighter.inventory WHERE OwnerID = @OID";
    public static readonly string DeleteItem = "DELETE FROM rumblefighter.inventory WHERE ServerID = @SID";
    public static readonly string GetItemFromServerID = "SELECT * FROM rumblefighter.inventory WHERE ServerID = @SID";
    public static readonly int TableCreationOrder = 998;

}