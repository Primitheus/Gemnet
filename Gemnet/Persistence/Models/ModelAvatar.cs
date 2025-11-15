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

    public static readonly string QueryCreateTable = @" CREATE TABLE IF NOT EXISTS `avatar` (
                                    `AvatarID` int NOT NULL AUTO_INCREMENT,
                                    `OwnerID` int NOT NULL,
                                    `Job` int NOT NULL DEFAULT 0,
                                    `Hair` int NOT NULL DEFAULT 0,
                                    `Forehead` int NOT NULL DEFAULT 0,
                                    `Top` int NOT NULL DEFAULT 0,
                                    `Bottom` int NOT NULL DEFAULT 0,
                                    `Gloves` int NOT NULL DEFAULT 0,
                                    `Shoes` int NOT NULL DEFAULT 0,
                                    `Eyes` int NOT NULL DEFAULT 0,
                                    `Nose` int NOT NULL DEFAULT 0,
                                    `Mouth` int NOT NULL DEFAULT 0,
                                    `Scroll` int NOT NULL DEFAULT 0,
                                    `ExoA` int NOT NULL DEFAULT 0,
                                    `ExoB` int NOT NULL DEFAULT 0,
                                    `Null` int NOT NULL DEFAULT 0,
                                    `Back` int NOT NULL DEFAULT 0,
                                    `Neck` int NOT NULL DEFAULT 0,
                                    `Ears` int NOT NULL DEFAULT 0,
                                    `Glasses` int NOT NULL DEFAULT 0,
                                    `Mask` int NOT NULL DEFAULT 0,
                                    `Waist` int NOT NULL DEFAULT 0,
                                    `Scroll_BU` int NOT NULL DEFAULT 0,
                                    `Unknown_1` int NOT NULL DEFAULT 0,
                                    `Unknown_2` int NOT NULL DEFAULT 0,
                                    `Inventory_1` int NOT NULL DEFAULT 0,
                                    `Inventory_2` int NOT NULL DEFAULT 0,
                                    `Inventory_3` int NOT NULL DEFAULT 0,
                                    `Unknown_3` int NOT NULL DEFAULT 0,
                                    `Unknown_4` int NOT NULL DEFAULT 0,
                                    `Unknown_5` int NOT NULL DEFAULT 0,
                                    `Unknown_6` int NOT NULL DEFAULT 0,
                                    `Unknown_7` int NOT NULL DEFAULT 0,
                                    `Title` int NOT NULL DEFAULT 0,
                                    `Merit` int NOT NULL DEFAULT 0,
                                    `Avalon` int NOT NULL DEFAULT 0,
                                    `Hair_BP` int NOT NULL DEFAULT 0,
                                    `Top_BP` int NOT NULL DEFAULT 0,
                                    `Bottom_BP` int NOT NULL DEFAULT 0,
                                    `Gloves_BP` int NOT NULL DEFAULT 0,
                                    `Shoes_BP` int NOT NULL DEFAULT 0,
                                    `Back_BP` int NOT NULL DEFAULT 0,
                                    `Neck_BP` int NOT NULL DEFAULT 0,
                                    `Ears_BP` int NOT NULL DEFAULT 0,
                                    `Glasses_BP` int NOT NULL DEFAULT 0,
                                    `Mask_BP` int NOT NULL DEFAULT 0,
                                    `Waist_BP` int NOT NULL DEFAULT 0,
                                    PRIMARY KEY (`AvatarID`),
                                    KEY `itemOwner_idx` (`OwnerID`),
                                    CONSTRAINT `itemOwner_avatar` FOREIGN KEY (`OwnerID`) REFERENCES `accounts` (`UUID`) ON DELETE CASCADE
                                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

                                                                                                      
    public static readonly string QueryGetAvatarIDs = "SELECT AvatarID FROM rumblefighter.avatar WHERE OwnerID = @ID";
    public static readonly string QueryGetDefaultAvatar = "SELECT * FROM rumblefighter.avatar WHERE OwnerID = @ID LIMIT 1";
    public static readonly string QueryGetAvatarData = "SELECT `Job`, `Hair`, `Forehead`, `Top`, `Bottom`, `Gloves`, `Shoes`, `Eyes`, `Nose`, `Mouth`, `Scroll`, `ExoA`, `ExoB`, `Null`, `Back`, `Neck`, `Ears`, `Glasses`, `Mask`, `Waist`, `Scroll_BU`, `Unknown_1`, `Unknown_2`, `Inventory_1`, `Inventory_2`, `Inventory_3`, `Unknown_3`, `Unknown_4`, `Unknown_5`, `Unknown_6`, `Unknown_7`, `Title`, `Merit`, `Avalon`, `Hair_BP`, `Top_BP`, `Bottom_BP`, `Gloves_BP`, `Shoes_BP`, `Back_BP`, `Neck_BP`, `Ears_BP`, `Glasses_BP`, `Mask_BP`, `Waist_BP` FROM `avatar` WHERE `AvatarID` = @AID";
    public static string GetQueryUpdateAvatar(string slotName)
    {
        // Validate and sanitize slotName here to prevent SQL injection
        return $"UPDATE rumblefighter.avatar SET `{slotName}` = @ServerID WHERE `AvatarID` = @AID";
    }

    public static readonly string QueryInsertNewAvatar = @"INSERT INTO rumblefighter.avatar (OwnerID, Job, ExoA) VALUES (@OID, @JobServerID, @ExoID);";


    public static readonly int TableCreationOrder = 997;

    public object AvatarProperties { get; internal set; }
}