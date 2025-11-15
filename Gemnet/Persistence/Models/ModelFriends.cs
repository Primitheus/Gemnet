namespace Gemnet.Persistence.Models;


public class ModelFriends : IModel
{

    public int ID; 
    public int RequesterUUID;
    public int ReceiverUUID;
    public string Status;

public static readonly string QueryCreateTable = @"CREATE TABLE IF NOT EXISTS `friends` (
                                                    `ID` INT NOT NULL AUTO_INCREMENT,
                                                    `RequesterUUID` INT NOT NULL,
                                                    `ReceiverUUID` INT NOT NULL,
                                                    `Status` ENUM('Pending', 'Accepted', 'Blocked') NOT NULL DEFAULT 'Pending',
                                                    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                                    PRIMARY KEY (`ID`),
                                                    FOREIGN KEY (`RequesterUUID`) REFERENCES `accounts`(`UUID`) ON DELETE CASCADE,
                                                    FOREIGN KEY (`ReceiverUUID`) REFERENCES `accounts`(`UUID`) ON DELETE CASCADE,
                                                    UNIQUE KEY `UniqueFriendship` (`RequesterUUID`, `ReceiverUUID`)
                                                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";


    // Send Friend Request
    public static readonly string QuerySendFriendRequest = "INSERT INTO friends (RequesterUUID, ReceiverUUID, Status) VALUES (@RequesterUUID, @ReceiverUUID, 'Accepted');";

    // Accept Friend Request
    public static readonly string QueryAcceptFriendRequest = "UPDATE friends SET Status = 'Accepted' WHERE ((RequesterUUID = @RequesterUUID AND ReceiverUUID = @ReceiverUUID) OR (RequesterUUID = @ReceiverUUID AND ReceiverUUID = @RequesterUUID)) AND Status = 'Pending';";

    // Delete Friend 
    public static readonly string QueryDeleteFriend = "DELETE FROM friends WHERE (RequesterUUID = @RequesterUUID AND ReceiverUUID = @ReceiverUUID) OR (RequesterUUID = @ReceiverUUID AND ReceiverUUID = @RequesterUUID);";

    // Get List of Friends
    public static readonly string QueryGetBuddyList = "SELECT * FROM friends WHERE RequesterUUID = @PlayerID OR ReceiverUUID = @PlayerID;";


    public static readonly int TableCreationOrder = 996;

}