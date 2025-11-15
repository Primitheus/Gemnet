using System;
using Gemnet.Network.Header;
using Gemnet.Network.Packets;
using Gemnet.Packets.Login;
using Gemnet.Persistence.Models;
using System.Net.Sockets;
using Org.BouncyCastle.Asn1.Ocsp;
using static Gemnet.Program;
using static Gemnet.Server;
using System.Text;
using BCrypt.Net;
using Org.BouncyCastle.Asn1.Misc;



namespace Gemnet.PacketProcessors
{
    public enum AccountState
    {
        NoAvatar = 0,
        Active = 1,
        Locked = 2,
        TimedOut = 3,
        Banned = 4

    }

    internal class Login
    {

        private static PlayerManager _playerManager = ServerHolder._playerManager;
        private static GameManager _gameManager = ServerHolder._gameManager;

        public static void VersionCheck(ushort type, ushort action, NetworkStream stream)
        {

            Console.WriteLine("[Server] Version Check Successful");
            action++;
            _ = ServerHolder.ServerInstance.SendPacket(type, 0x6, action, stream);


        }

        public static void CredentialCheck(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;
            Console.WriteLine("[Credential Check]");

            string hexOutput = string.Join(", ", body.Select(b => $"0x{b:X2}"));
            Console.WriteLine($"TEST: {hexOutput}");

            LoginReq request = LoginReq.Deserialize(body);

            Console.WriteLine($"Request: {request.Email}");
            //string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);


            static void SendToAccountCreation(NetworkStream stream, int UserId)
            {
                NoAvatarRes response = new NoAvatarRes();
                response.Type = 528;
                response.Action = 389;

                response.Unknown1 = 4097;
                response.Message = "no game account";

                PlayerManager.Player player = new PlayerManager.Player
                {

                    UserID = UserId

                };

                _playerManager.TryAddPlayer(stream, player);
                _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);

            }

            static void SendLoginFailResponse(string errorMessage, NetworkStream stream)
            {
                LoginFailRes response = new LoginFailRes
                {
                    Type = 528,
                    Action = 645,
                    Error = errorMessage,
                    Code = 29374
                };

                _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);
            }


            var LoginQuery = ServerHolder.DatabaseInstance.SelectFirst<ModelAccount>(ModelAccount.QueryLoginAccountByEmail, new
            {
                Email = request.Email
            });

            if (LoginQuery == null)
            {

                Console.WriteLine("Email or Password is incorrect.");
                SendLoginFailResponse("Email or Password is incorrect.", stream);
                return;
            }
            else
            {

                bool passwordMatches = BCrypt.Net.BCrypt.Verify(request.Password, LoginQuery.Password);

                if (!passwordMatches)
                {
                    Console.WriteLine("Password does not match.");
                    SendLoginFailResponse("Email or Password is incorrect.", stream);

                    return;
                }


                if (LoginQuery.State == (int)AccountState.Banned)
                {

                    Console.WriteLine("Account is Banned.");
                    SendLoginFailResponse("Account is Banned.", stream);
                    return;
                }

                else if (LoginQuery.State == (int)AccountState.Locked)
                {
                    Console.WriteLine("Account is Locked.");
                    SendLoginFailResponse("Account is Locked.", stream);
                    return;
                }
                else if (LoginQuery.State == (int)AccountState.TimedOut)
                {
                    Console.WriteLine("Account is Timed Out.");
                    SendLoginFailResponse("Account is Timed Out.", stream);
                    return;
                }
                else if (LoginQuery.State == (int)AccountState.NoAvatar)
                {


                    SendToAccountCreation(stream, LoginQuery.UUID);
                    return;
                }

                else
                {

                    LoginRes response = new LoginRes();
                    response.Type = type;
                    response.Action = action;
                    response.UserID = LoginQuery.UUID;
                    response.IGN = LoginQuery.IGN;
                    response.Exp = LoginQuery.EXP;
                    response.Carats = LoginQuery.Carats;
                    response.Medals = LoginQuery.Medals;
                    response.GUID = "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX";
                    response.Token = GenerateRandomString(30);
                    response.Region = "NA";
                    response.Country = "US";
                    response.ForumName = LoginQuery.ForumName;

                    Console.WriteLine($"Token: {response.Token}");

                    if (LoginQuery.CurrentAvatar == 0)
                    {
                        
                        Console.WriteLine($"No avatar equipped, equipping first avatar. searching for avatars for user {LoginQuery.UUID}");

                        var firstAvatarQuery = ServerHolder.DatabaseInstance.SelectFirst<ModelAvatar>(ModelAvatar.QueryGetDefaultAvatar, new
                        {
                            ID = LoginQuery.UUID
                        });

                        if (firstAvatarQuery == null)
                        {
                            //SendToAccountCreation(stream, LoginQuery.UUID);
                            Console.WriteLine("User has no avatars, cannot log in.");

                            return;
                        }
                        else
                        {
                            // var updateState = ServerHolder.DatabaseInstance.Execute(ModelAccount.QueryUpdateState, new
                            // {
                            //     state = (int)AccountState.Active,
                            //     ID = LoginQuery.UUID
                            // });

                            var updateAvatar = ServerHolder.DatabaseInstance.Execute(ModelAccount.QueryUpdateAvatar, new
                            {
                                avatar = firstAvatarQuery.AvatarID,
                                ID = LoginQuery.UUID
                            });

                            LoginQuery.CurrentAvatar = firstAvatarQuery.AvatarID;
                        }

                    }

                    PlayerManager.Player player = new PlayerManager.Player
                    {

                        UserID = response.UserID,
                        UserIGN = response.IGN,
                        Carats = response.Carats,
                        EXP = response.Exp,
                        Token = response.Token,
                        ForumName = response.ForumName,
                        Region = response.Region,
                        Country = response.Country,
                        GUID = response.GUID,
                        CurrentAvatar = LoginQuery.CurrentAvatar,
                        Stream = stream

                    };

                    _playerManager.TryAddPlayer(stream, player);

                    _ = ServerHolder.ServerInstance.SendPacketAsync(response.Serialize(), stream);
                }
            }


        }

        public static void GemLogin(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;
            Console.WriteLine("[Gem Fighter Login]");

            GemLoginReq request = GemLoginReq.Deserialize(body);

            if (request.Token != null && request.ID != null)
            {
                Console.WriteLine($"Logging in with {request.Token} {request.ID}");
                var LoginQuery = ServerHolder.DatabaseInstance.SelectFirst<ModelAccount>(ModelAccount.QueryLoginAccountByEmail, new
                {
                    Email = "rumblefighter187@outlook.com"
                });

                LoginRes response = new LoginRes();
                response.Type = type;
                response.Action = action;
                response.UserID = LoginQuery.UUID;
                response.IGN = LoginQuery.IGN;
                response.Exp = LoginQuery.EXP;
                response.Carats = LoginQuery.Carats;
                response.Medals = LoginQuery.Medals;
                response.GUID = "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX";
                response.Token = GenerateRandomString(30);
                response.Region = "NA";
                response.Country = "US";
                response.ForumName = LoginQuery.ForumName;

                Console.WriteLine($"Token: {response.Token}");

                PlayerManager.Player player = new PlayerManager.Player
                {

                    UserID = response.UserID,
                    UserIGN = response.IGN,
                    Carats = response.Carats,
                    EXP = response.Exp,
                    Token = response.Token,
                    ForumName = response.ForumName,
                    // Region = response.Region,
                    // Country = response.Country,
                    GUID = response.GUID,
                    CurrentAvatar = LoginQuery.CurrentAvatar,
                    Stream = stream

                };

                _playerManager.TryAddPlayer(stream, player);

                _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);

            }




        }

        static string GenerateRandomString(int length)
        {
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            Random random = new Random();
            StringBuilder stringBuilder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(characters.Length);
                stringBuilder.Append(characters[index]);
            }

            return stringBuilder.ToString();
        }

        public static void CreateAccount(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;
            CreateAccountReq request = CreateAccountReq.Deserialize(body);

            var player = _playerManager.GetPlayerByStream(stream);


            Console.WriteLine($"Avatar Creation: {request.UserIGN} with class: {request.ClassId}");

            Dictionary<int, int> classIdToDefaultExo = new Dictionary<int, int>
            {
                { 1000175, 2040034 }, // Striker (M) -> Default Striker Exo
                { 1000176, 2040035 }, // Soul Fighter (M) -> Default Soul Exo
                { 1000177, 2040037 }, // Elementalist (M) -> Default Elementalist Exo
                { 1000178, 2040038 }, // Alchemist (M) -> Default Alchemist Exo
                { 1000247, 2040034 }, // Striker (F) -> Default Striker Exo (assuming same as male)
                { 1000248, 2040035 }, // Soul Fighter (F) -> Default Soul Exo
                { 1000249, 2040037 }, // Elementalist (F) -> Default Elementalist Exo
                { 1000250, 2040038 }, // Alchemist (F) -> Default Alchemist Exo
                { 1001792, 2040086 }, // Shaman (M) -> Default Shama Exo
                { 1001793, 2040086 }, // Shaman (F) -> Default Shama Exo
                { 1002689, 0 }        // Android (F) -> No default exo? (Or add if available)
            };

            int[] validClassIds = new int[]
            {
                1000175, // Striker (M)
                1000176, // Soul Fighter (M)
                1000177, // Elementalist (M)
                1000178, // Alchemist (M)
                1000247, // Striker (F)
                1000248, // Soul Fighter (F)
                1000249, // Elementalist (F)
                1000250, // Alchemist (F)
                1001792, // Shaman (M)
                1001793, // Shaman (F)
                1002689  // Android (F)
            };

            // 2040034 - Default Striker Exo
            // 2040035 - Default Soul Exo
            // 2040037 - Default Elementalist Exo
            // 2040038 - Default Alchemist Exo
            // 2040086 - Default Shama Exo

            if (!validClassIds.Contains(request.ClassId))
                return;

            int defaultExoId = classIdToDefaultExo.ContainsKey(request.ClassId) ? classIdToDefaultExo[request.ClassId] : 0;

            // Update State
            ServerHolder.DatabaseInstance.Execute(ModelAccount.QueryUpdateState, new
            {
                ID = player.UserID,
                state = AccountState.Active

            });


            // Update PlayerIGN
            ServerHolder.DatabaseInstance.Execute(ModelAccount.QueryUpdateUserIGN, new
            {
                ID = player.UserID,
                IGN = request.UserIGN

            });

            // Populate Inventory
            ServerHolder.DatabaseInstance.Execute(ModelInventory.InsertItem, new
            {
                OID = player.UserID,
                ID = request.ClassId,
                END = 1000
            });

            // 5135224 -> Fishing Rod
            ServerHolder.DatabaseInstance.Execute(ModelInventory.InsertItem, new
            {
                OID = player.UserID,
                ID = 5135224,
                END = 1000
            });

            ServerHolder.DatabaseInstance.Execute(ModelInventory.InsertItem, new
            {
                OID = player.UserID,
                ID = defaultExoId,
                END = 1000
            });


            ModelInventory serverIdQueryClass = null;
            ModelInventory serverIdQueryExo = null;

            // Populate Avatars with Class and Exo
            try
            {
                serverIdQueryClass = ServerHolder.DatabaseInstance.SelectFirst<ModelInventory>(ModelInventory.GetServerID, new
                {
                    OID = player.UserID,
                    ID = request.ClassId
                });

                serverIdQueryExo = ServerHolder.DatabaseInstance.SelectFirst<ModelInventory>(ModelInventory.GetServerID, new
                {
                    OID = player.UserID,
                    ID = defaultExoId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching server IDs: {ex.Message}");
            }

            Console.WriteLine($"Insert Avatar Items: {serverIdQueryClass.ServerID}, {serverIdQueryExo.ServerID}");

            // Create 10 new avatars.
            int NumberOfAvatars = 10;

            for (int i = 0; i < NumberOfAvatars; i++)
            {
                ServerHolder.DatabaseInstance.Execute(ModelAvatar.QueryInsertNewAvatar, new
                {
                    OID = player.UserID,
                    JobServerID = serverIdQueryClass.ServerID,
                    ExoID = serverIdQueryExo.ServerID

                });
            }

            // Get First Avatar ID
            var firstAvatarQuery = ServerHolder.DatabaseInstance.SelectFirst<ModelAvatar>(ModelAvatar.QueryGetAvatarIDs, new
            {
                ID = player.UserID
            });

            var Query = ServerHolder.DatabaseInstance.Select<ModelAccount>(ModelAccount.QueryUpdateAvatar, new
            {
                avatar = firstAvatarQuery.AvatarID,
                ID = player.UserID
            });



            Console.WriteLine($"Avatars inserted successfully.");


            LoginRes response = new LoginRes();

            var LoginQuery = ServerHolder.DatabaseInstance.SelectFirst<ModelAccount>(ModelAccount.QueryGetPlayerInfo, new
            {
                username = request.UserIGN
            });

            response.Type = type;
            response.Action = action;
            response.UserID = LoginQuery.UUID;
            response.IGN = LoginQuery.IGN;
            response.Exp = LoginQuery.EXP;
            response.Carats = LoginQuery.Carats;
            response.Medals = LoginQuery.Medals;
            response.GUID = "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX";
            response.Token = GenerateRandomString(30);
            response.Region = "NA";
            response.Country = "US";
            response.ForumName = LoginQuery.ForumName;

            Console.WriteLine($"Token: {response.Token}");

            player.UserID = response.UserID;
            player.UserIGN = response.IGN;
            player.Carats = response.Carats;
            player.EXP = response.Exp;
            player.Token = response.Token;
            player.ForumName = response.ForumName;
            player.Region = response.Region;
            player.Country = response.Country;
            player.GUID = response.GUID;
            player.CurrentAvatar = firstAvatarQuery.AvatarID;

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);


        }

        public static void ServerTime(ushort type, ushort action, NetworkStream stream)
        {
            action++;

            // Get the current UTC time
            DateTime utcNow = DateTime.UtcNow;

            // Convert it to PST
            TimeZoneInfo pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            DateTime pstNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, pstZone);


            // Get the current time in milliseconds since Unix epoch
            long unixTime = ((DateTimeOffset)pstNow).ToUnixTimeMilliseconds();
            long windows64Timestamp = (unixTime + 11644473600000) * 10000;
            byte[] timeArray = BitConverter.GetBytes(windows64Timestamp);

            ServerTime serverTime = new ServerTime();
            serverTime.Type = type;
            serverTime.Action = action;
            serverTime.Time = timeArray;

            Console.WriteLine($"[Server Time : {utcNow}] Data={BitConverter.ToString(timeArray)}");

            _ = ServerHolder.ServerInstance.SendPacket(serverTime.Serialize(), stream);
        }

        public static void CashUnknown(ushort type, ushort action, NetworkStream stream)
        {
            action++;


            byte[] data = { 0x01, 0x00 };
            _ = ServerHolder.ServerInstance.SendPacket(type, 0x8, action, data, stream);


        }
        public static void GetBuddyList(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;
            Console.WriteLine("Get Buddy List");

            GetBuddyListReq request = GetBuddyListReq.Deserialize(body);

            GetBuddyListRes response = new GetBuddyListRes
            {
                Type = type,
                Action = action
            };

            var player = _playerManager.GetPlayerByStream(stream);

            if (player == null)
            {
                Console.WriteLine("Player not found for stream.");

            }

            // Get all accepted friends where the player is either the requester or receiver
            var buddyListQuery = ServerHolder.DatabaseInstance.Select<ModelFriends>(
                "SELECT * FROM friends WHERE (RequesterUUID = @PlayerID OR ReceiverUUID = @PlayerID);",
                new { PlayerID = player.UserID }
            );

            var Buddies = new List<Buddy>(); // Moved outside so it's accessible throughout the method

            if (buddyListQuery == null || buddyListQuery.Count() == 0 || !buddyListQuery.Any())
            {
                Console.WriteLine("Failed to get buddy list.");
            }
            else
            {
                //Buddies.Add(new Buddy { UserID = 1337, UserIGN = "Welcome To Gemnet", StatusA = 0x4E, StatusB = 0x4F });

                foreach (var friendship in buddyListQuery)
                {
                    int friendId = (friendship.RequesterUUID == player.UserID) ? friendship.ReceiverUUID : friendship.RequesterUUID;

                    var friendPlayer = ServerHolder.DatabaseInstance.SelectFirst<ModelAccount>(ModelAccount.QuerySelectAccount, new
                    {
                        ID = friendId
                    });

                    Console.WriteLine($"Buddies: {friendPlayer.IGN}");

                    bool isOnline = false;

                    if (_playerManager.IsPlayerOnline(friendPlayer.IGN))
                        isOnline = true;

                    // if online is true then StatusB = 0x4F, else StatusB = 0x46
                    byte value = (byte)(isOnline ? 0x4F : 0x46);

                    Buddies.Add(new Buddy
                    {
                        UserID = friendId,
                        UserIGN = friendPlayer?.IGN ?? "Unknown",
                        StatusA = (byte)(friendship.Status == "Pending" ? 0x4E : 0x52),
                        StatusB = value
                    });

                }
                
            }



            // Temporary hardcoded buddy list


            // StatusA: 0x4E = Accepted, 0x52 = Pending. 
            // StatusB: 0x46 = Offline, 0x4F = Online.

            // var Buddies = new List<Buddy>
            // {
            //     new Buddy { UserID = 999, UserIGN = "Nimonix", StatusA = 0x4E, StatusB = 0x4F },
            //     new Buddy { UserID = 998, UserIGN = "Gemnet" , StatusA = 0x4E, StatusB = 0x46},

            //     new Buddy { UserID = 997, UserIGN = "Pending" , StatusA = 0x52, StatusB = 0x4F},
            //     new Buddy { UserID = 996, UserIGN = "Online" , StatusA = 0x4E, StatusB = 0x4F},
            //     new Buddy { UserID = 994, UserIGN = "Offline" , StatusA = 0x52, StatusB = 0x46},


            //     new Buddy { UserID = 1337, UserIGN = "BUDDY6" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY7" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY8" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY9" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY10" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY11" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY12" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY13" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY14" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY15" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY16" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY17" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY18" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY19" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY20" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY21" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY22" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY23" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY24" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY25" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY26" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY27" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY28" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY29" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY30" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY31" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY32" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY33" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY34" , StatusA = 0x52, StatusB = 0x46},
            //     new Buddy { UserID = 1337, UserIGN = "BUDDY35" , StatusA = 0x52, StatusB = 0x46},

            // };


            response.Buddies = Buddies;

            string hexOutput = string.Join(", ", response.Serialize().Select(b => $"0x{b:X2}"));
            Console.WriteLine($"Get Test Data Buddy List: {hexOutput}");

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);

        }

        public static void ADD_BUDDY(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;
            AddBuddyReq request = AddBuddyReq.Deserialize(body);
            Console.WriteLine($"Add Buddy: {request.UserIGN}");

            var player = _playerManager.GetPlayerByStream(stream);
            var addingPlayer = _playerManager.GetPlayerByIGN(request.UserIGN);

            var result = false;

            if (player == null)
            {
                Console.WriteLine("Player not found in active players.");
                result = false;
            }

            if (addingPlayer == null)
            {
                Console.WriteLine("Adding player not found.");
                result = false;

            }

            try
            {
                var addBuddyQuery = ServerHolder.DatabaseInstance.Select<ModelFriends>(ModelFriends.QuerySendFriendRequest, new
                {
                    RequesterUUID = player.UserID,
                    ReceiverUUID = addingPlayer.UserID
                });

                if (addBuddyQuery == null)
                {
                    Console.WriteLine("Failed to add buddy. Query returned null.");
                    result = false;
                }
                else
                {
                    Console.WriteLine($"Buddy {request.UserIGN} added successfully.");
                    result = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while trying to add buddy:");
                Console.WriteLine($"Exception Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                result = false;
            }


            if (result)
            {
                // ON Success, send a response packet and send 
                _ = ServerHolder.ServerInstance.SendPacket(528, 0x6, action, stream);

                // also send to the friend a notification that they have been added
                AddBuddyRes notRequest = new AddBuddyRes();

                notRequest.Type = 528;
                notRequest.Action = (ushort)Packets.Enums.Packets.ActionLogin.S2C_FRIEND_REQ_RECIEVE;
                notRequest.UserID = player.UserID;
                notRequest.UserIGN = player.UserIGN;

                var onlinePlayer = _playerManager.GetPlayerById(addingPlayer.UserID);
                
                if (onlinePlayer.Stream != null)
                {
                    _ = ServerHolder.ServerInstance.SendNotificationPacket(notRequest.Serialize(),  onlinePlayer.Stream);

                }
                
            }
            else
            {
                // FAIL
                byte[] data = { 0x02, 0x10, 0x02, 0x08, 0xAB, 0x01, 0x00, 0x00, 0x41, 0x49, 0x4C, 0x28, 0x75, 0x6E, 0x6B, 0x6E, 0x6F, 0x77, 0x6E, 0x29, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x60, 0x01, 0x5C, 0x13, 0x60, 0x01, 0x02, 0x00, 0x00, 0x00, 0x30, 0x13, 0x60, 0x01, 0xA0, 0xFD, 0xF9, 0x01, 0xCD, 0x98, 0x35, 0x76, 0x00, 0x00,
                0x8D, 0x00, 0x00, 0x00, 0x00, 0x00, 0xDA, 0x98, 0x35, 0x76, 0x45, 0xC9, 0x67, 0xB7, 0x5C, 0x13, 0x60, 0x01, 0x38, 0x13, 0x60, 0x01, 0x02, 0x00, 0x00, 0x00, 0x14, 0x8B, 0xC5,
                0x72, 0x28, 0xBF, 0xB9, 0x01, 0xD8, 0xC6, 0x8D, 0x00, 0xD8, 0xC6, 0x8D, 0x00, 0x68, 0xFE, 0x8D, 0x00, 0x04, 0xC7, 0x8D, 0x00, 0x9C, 0xFD, 0xF9, 0x01, 0xC7, 0x70, 0xC5, 0x72,
                0x0C, 0xC4, 0xB9, 0x01, 0xB8, 0xFD, 0xF9, 0x01, 0x04, 0xC7, 0x8D, 0x00, 0xB8, 0xFD, 0xF9, 0x01, 0xFD, 0x9E, 0xC5, 0x72, 0x98, 0xC9, 0x8D, 0x00, 0x38, 0x13, 0x60, 0x01, 0x00,
                0x00, 0x00, 0x00, 0xD8, 0xFD, 0xF9, 0x01, 0x99, 0x97, 0xC5, 0x72, 0xD8, 0xC6, 0x8D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x38, 0x13, 0x60, 0x01, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFC, 0xFD, 0xF9, 0x01, 0xFC, 0xFD, 0xF9, 0x01, 0x82, 0xA2, 0xCA, 0x72, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC8, 0x8F, 0x87,
                0x01, 0xC8, 0x8F, 0x87, 0x01, 0x60, 0x52, 0xEB, 0x05, 0xFF, 0xFF, 0xFF, 0xFF, 0x4A, 0x70, 0x7D, 0x01, 0xEA, 0x57, 0x88, 0x00, 0x38, 0x13, 0x60, 0x01, 0x01, 0x00, 0x00, 0x00,
                0xC8, 0x8F, 0x87, 0x01, 0xB4, 0x58, 0x87, 0x00, 0xCB, 0x58, 0x87, 0x00, 0x01, 0x00, 0x00, 0x00, 0xFD, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xE3, 0x7C, 0x03, 0x00, 0x04,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14, 0x82, 0x87, 0x01, 0x98, 0x81, 0x87, 0x01, 0x14, 0x82, 0x87, 0x01, 0x60, 0x52, 0xEB, 0x05, 0x98, 0x81, 0x87, 0x01, 0x4A, 0x70 };

                _ = ServerHolder.ServerInstance.SendPacket(data, stream);

            }
            
        }

        public static void AGREE_BUDDY(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;
            var player = _playerManager.GetPlayerByStream(stream);
            
            AgreeBuddyReq request = AgreeBuddyReq.Deserialize(body);
            
            var query = ServerHolder.DatabaseInstance.Select<ModelFriends>(ModelFriends.QueryAcceptFriendRequest, new
            {
                RequesterUUID = request.UserID,
                ReceiverUUID = player.UserID
            });
            
            if (query == null)
            {
                // Fail
                Console.WriteLine("Failed to add buddy. Query returned null.");
            }
            else
            {
                
                // Success
                byte[] data = { 0x02, 0x10, 0x00, 0x06, 0xAD, 0x00 };
                _ = ServerHolder.ServerInstance.SendPacket(data, stream);
                
                AddBuddyRes notify = new AddBuddyRes();

                notify.Type = 528;
                notify.Action = (ushort)Packets.Enums.Packets.ActionLogin.S2C_FRIEND_ACCEPTED;
                notify.UserID = player.UserID;
                notify.UserIGN = player.UserIGN;

                var onlinePlayer = _playerManager.GetPlayerById(request.UserID);
                
                if (onlinePlayer.Stream != null)
                {
                    _ = ServerHolder.ServerInstance.SendNotificationPacket(notify.Serialize(),  onlinePlayer.Stream);

                }

            }
            

        }

        public static void DELETE_BUDDY(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;
            DeleteBuddyReq request = DeleteBuddyReq.Deserialize(body);
            var player = _playerManager.GetPlayerByStream(stream);
            
            var query = ServerHolder.DatabaseInstance.Select<ModelFriends>(ModelFriends.QueryDeleteFriend, new
            {
                RequesterUUID = player.UserID,
                ReceiverUUID = request.UserID
            });

            if (query == null)
            {
                Console.WriteLine("Failed to delete buddy. Query returned null.");
            }
            else
            {
                // Successfully Deleted
                byte[] data = { 0x00, 0x10, 0x00, 0x06, 0xAF, 0x00 };
                _ = ServerHolder.ServerInstance.SendPacket(data, stream);
                
                
            }
            



        }

        public static void TO_LOBBY(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;
            ToLobbyReq request = ToLobbyReq.Deserialize(body);
            Console.WriteLine($"To Lobby");

            ToLobbyRes response = new ToLobbyRes();
            response.Type = 528;
            response.Action = action;

            response.Result = 1;

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);
        }

        public static void SetOptionInventory(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;

            Console.WriteLine($"Set Option Inventory");

            byte[] data = { 0x00, 0x10, 0x00, 0x08, 0xD3, 0x00, 0x01, 0x00 };

            _ = ServerHolder.ServerInstance.SendPacket(data, stream);

        }

        public static void UseMegaphone(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;

            UseMegaphoneReq request = UseMegaphoneReq.Deserialize(body);
            UseMegaphoneRes response = new UseMegaphoneRes();

            response.Type = 528;
            response.Action = 0x1A;

            var player = _playerManager.GetPlayerByStream(stream);

            response.Action = action;
            response.Type = type;

            response.Message = request.Message;

            byte[] dataSuccess = { 0x02, 0x10, 0x00, 0x06, 0xB9, 0x00 };
            _ = ServerHolder.ServerInstance.SendPacket(dataSuccess, stream);

            _ = ServerHolder.ServerInstance.SendPacket(response.Serialize(), stream);
        }

        public static void ChangeNickname(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;

            ChangeNicknameReq request = ChangeNicknameReq.Deserialize(body);
            ChangeNicknameRes response = new ChangeNicknameRes();

            var player = _playerManager.GetPlayerByStream(stream);

            Console.WriteLine($"Changing Nickname {player.UserIGN} to {request.NewIGN}");

            response.Type = type;
            response.Action = action;

            player.UserIGN = request.NewIGN;


            // Test - Complete Guess
            byte[] data = { 0x00, 0x10, 0x00, 0x06, 0x97, 0x00 };

            _ = ServerHolder.ServerInstance.SendPacket(data, stream);



        }

        public static void AddBuddyID(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;

            ChangeNicknameReq request = ChangeNicknameReq.Deserialize(body);
            ChangeNicknameRes response = new ChangeNicknameRes();

            var player = _playerManager.GetPlayerByStream(stream);

            Console.WriteLine($"Changing Nickname {player.UserIGN} to {request.NewIGN}");

            response.Type = type;
            response.Action = action;

            player.UserIGN = request.NewIGN;


            // Test - Complete Guess
            byte[] data = { 0x00, 0x10, 0x00, 0x06, 0x99, 0x00 };

            _ = ServerHolder.ServerInstance.SendPacket(data, stream);



        }

        public static void GemUnknown1(ushort type, ushort action, byte[] body, NetworkStream stream)
        {
            action++;

            byte[] data = { 0x00, 0x10, 0x00, 0x10, 0xC6, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x58 };

            _ = ServerHolder.ServerInstance.SendPacket(data, stream);


        }

    }


}
