namespace Gemnet.Persistence.Models;


public class ModelAvatar : IModel
{

    public int AvatarID;
    public int OwnerID;
    public int Job { get; set; }
    public int Hair { get; set; }
    public int Forehead { get; set; }
    public int Top { get; set; }
    public int Bottom { get; set; }
    public int Gloves { get; set; }
    public int Shoes { get; set; }
    public int Eyes { get; set; }
    public int Nose { get; set; }
    public int Mouth { get; set; }
    public int Scroll { get; set; }
    public int ExoA { get; set; }
    public int ExoB { get; set; }
    public int Null { get; set; }
    public int Back { get; set; }
    public int Neck { get; set; }
    public int Ears { get; set; }
    public int Glasses { get; set; }
    public int Mask { get; set; }
    public int Waist { get; set; }
    public int Scroll_BU { get; set; }
    public int Unknown_1 { get; set; }
    public int Unknown_2 { get; set; }
    public int Inventory_1 { get; set; }
    public int Inventory_2 { get; set; }
    public int Inventory_3 { get; set; }
    public int Unknown_3 { get; set; }
    public int Unknown_4 { get; set; }
    public int Unknown_5 { get; set; }
    public int Unknown_6 { get; set; }
    public int Unknown_7 { get; set; }
    public int Title { get; set; }
    public int Merit { get; set; }
    public int Avalon { get; set; }
    public int Hair_BP { get; set; }
    public int Top_BP { get; set; }
    public int Bottom_BP { get; set; }
    public int Gloves_BP { get; set; }
    public int Shoes_BP { get; set; }
    public int Back_BP { get; set; }
    public int Neck_BP { get; set; }
    public int Ears_BP { get; set; }
    public int Glasses_BP { get; set; }
    public int Mask_BP { get; set; }
    public int Waist_BP { get; set; }
public static readonly string QueryCreateTable = @"CREATE TABLE IF NOT EXISTS `avatar` (
                                                      `AvatarID` int NOT NULL,
                                                      `OwnerID` int NOT NULL,
                                                      `Job` int,
                                                      `Hair` int,
                                                      `Forehead` int,
                                                      `Top` int,
                                                      `Bottom` int,
                                                      `Gloves` int,
                                                      `Shoes` int,
                                                      `Eyes` int,
                                                      `Nose` int,
                                                      `Mouth` int,
                                                      `Scroll` int,
                                                      `ExoA` int,
                                                      `ExoB` int,
                                                      `Null` int,
                                                      `Back` int,
                                                      `Neck` int,
                                                      `Ears` int,
                                                      `Glasses` int,
                                                      `Mask` int,
                                                      `Waist` int,
                                                      `Scroll_BU` int,
                                                      `Unknown_1` int,
                                                      `Unknown_2` int,
                                                      `Inventory_1` int,
                                                      `Inventory_2` int,
                                                      `Inventory_3` int,
                                                      `Unknown_3` int,
                                                      `Unknown_4` int,
                                                      `Unknown_5` int,
                                                      `Unknown_6` int,
                                                      `Unknown_7` int,
                                                      `Title` int,
                                                      `Merit` int,
                                                      `Avalon` int,
                                                      `Hair_BP` int,
                                                      `Top_BP` int,
                                                      `Bottom_BP` int,
                                                      `Gloves_BP` int,
                                                      `Shoes_BP` int,
                                                      `Back_BP` int,
                                                      `Neck_BP` int,
                                                      `Ears_BP` int,
                                                      `Glasses_BP` int,
                                                      `Mask_BP` int,
                                                      `Waist_BP` int,
                                                      PRIMARY KEY (`AvatarID`),
                                                      KEY `itemOwner_idx` (`OwnerID`),
                                                      CONSTRAINT `itemOwner_avatar` FOREIGN KEY (`OwnerID`) REFERENCES `accounts` (`UUID`) ON DELETE CASCADE
                                                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";
                                                                                                      
    public static readonly string QueryGetAvatarIDs = "SELECT AvatarID FROM rumblefighter.avatar WHERE OwnerID = @ID";

    public static readonly string QueryGetAvatarData = "SELECT `Job`, `Hair`, `Forehead`, `Top`, `Bottom`, `Gloves`, `Shoes`, `Eyes`, `Nose`, `Mouth`, `Scroll`, `ExoA`, `ExoB`, `Null`, `Back`, `Neck`, `Ears`, `Glasses`, `Mask`, `Waist`, `Scroll_BU`, `Unknown_1`, `Unknown_2`, `Inventory_1`, `Inventory_2`, `Inventory_3`, `Unknown_3`, `Unknown_4`, `Unknown_5`, `Unknown_6`, `Unknown_7`, `Title`, `Merit`, `Avalon`, `Hair_BP`, `Top_BP`, `Bottom_BP`, `Gloves_BP`, `Shoes_BP`, `Back_BP`, `Neck_BP`, `Ears_BP`, `Glasses_BP`, `Mask_BP`, `Waist_BP` FROM `avatar` WHERE `AvatarID` = @AID";
    public static string GetQueryUpdateAvatar(string slotName)
    {
        // Validate and sanitize slotName here to prevent SQL injection
        return $"UPDATE rumblefighter.avatar SET `{slotName}` = @ServerID WHERE `AvatarID` = @AID";
    }
    public static readonly int TableCreationOrder = 997;

    public object AvatarProperties { get; internal set; }
}