namespace Gemnet.Persistence.Models;

public class ModelAccount : IModel
{

    public int UUID;
    public string Email;
    public string IGN;
    public int EXP;
    public int Carats;
    public int Astros;
    public int Medals;
    public string Password; // TODO: hashing
    public string ForumName;
    public int State;
    public int CurrentAvatar;

    /* public int State;
    public DateTime LastLogin;
    public string HWID;
    public string LastIP;*/

    public static readonly string QueryCreateTable = @"CREATE TABLE IF NOT EXISTS `accounts` (
                                                              `UUID` int NOT NULL AUTO_INCREMENT,
                                                              `Email` varchar(50) NOT NULL,
                                                              `Password` varchar(50) NOT NULL,
                                                              `IGN` varchar(50),
                                                              `EXP` int NOT NULL DEFAULT '0',
                                                              `Carats` int NOT NULL DEFAULT '10000',
                                                              `Astros` int NOT NULL DEFAULT '0',
                                                              `Medals` int NOT NULL DEFAULT '0',
                                                              `ForumName` varchar(50) NOT NULL,
                                                              `State` int NOT NULL DEFAULT '0',
                                                              `CurrentAvatar` int NOT NULL DEFAULT '0',
                                                              PRIMARY KEY (`UUID`)
                                                            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

    public static readonly string QuerySelectAccount = "SELECT * FROM rumblefighter.accounts WHERE UUID = @ID";

    public static readonly string QueryLoginAccount = "SELECT * FROM rumblefighter.accounts WHERE Email = @Email AND Password = @Password;";
    public static readonly string QueryLoginAccountByEmail = "SELECT * FROM rumblefighter.accounts WHERE Email = @Email;";

    public static readonly string QueryCashCarats = "SELECT Carats FROM rumblefighter.accounts WHERE UUID = @ID";
    public static readonly string QueryCashAstros = "SELECT Astros FROM rumblefighter.accounts WHERE UUID = @ID";
    public static readonly string QueryAddCarats = "UPDATE accounts SET Carats = Carats + @amount WHERE UUID = @ID";
    public static readonly string QueryAddAstros = "UPDATE accounts SET Astros = Astros + @amount WHERE UUID = @ID";
    public static readonly string QueryGetIdFromUsername = "SELECT UUID FROM accounts WHERE IGN = @username";
    public static readonly string QueryUpdateAvatar = "UPDATE accounts SET CurrentAvatar = @avatar WHERE UUID = @ID";
    public static readonly string QueryGetEquippedAvatar = "SELECT CurrentAvatar FROM accounts WHERE IGN = @username";

    // Update State
    public static readonly string QueryUpdateState = "UPDATE accounts SET State = @state WHERE UUID = @ID";

    // Update UserIGN
    public static readonly string QueryUpdateUserIGN = "UPDATE accounts SET IGN = @IGN WHERE UUID = @ID";
    

    // Get Player Data.
    public static readonly string QueryGetPlayerInfo = "SELECT UUID, IGN, EXP, Carats, Astros, Medals, ForumName, State, CurrentAvatar FROM rumblefighter.accounts WHERE IGN = @username;";

    public static readonly int TableCreationOrder = 999;
}