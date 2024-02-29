using System.Net.Sockets;
using Gemnet.Packets;
using Gemnet.Packets.Enums;
using static Gemnet.Packets.Enums.Packets;
using Gemnet.PacketProcessors;
using Gemnet.Network.Packets;
using Gemnet.Network;

namespace SendPacket
{

    public class Parser
    {

        private const int MaxBufferSize = 1024;
        private const int PacketHeaderSize = 6; // 2 bytes for type + 2 bytes for length + 2 bytes for action
        private const int TypeOffset = 0;
        private const int LengthOffset = 2;
        private const int ActionOffset = 4;

        public async Task ProcessPacketAsync(ushort type, ushort action, byte[] packetBody, NetworkStream stream)
        {
            // Add packet processing logic here

                switch ((HeaderType)type)
                {
                    case (HeaderType.LOGIN):
                        ProcessLoginPacket(action, packetBody, stream);
                        break;
                    case (HeaderType.GENERAL):
                        ProcessGeneralPacket(action, packetBody, stream);
                        break;
                    case (HeaderType.GAMEGUARD):
                        ProcessGameGuardPacket(action, packetBody, stream);
                        break;
                    case (HeaderType.INVENTORY):
                        ProcessInventoryPacket(action, packetBody, stream);
                        break;
                    case (HeaderType.QUERY):
                        ProcessQueryPacket(action, packetBody, stream);
                        break;
                    default:
                        Console.WriteLine($"Unknown packet type: {type}");
                        break;

                }
        }

        public void ProcessLoginPacket(ushort action, byte[] packetBody, NetworkStream stream)
        {
            String TypeName = Enum.GetName(HeaderType.LOGIN);
            String headerAction = Enum.GetName((ActionLogin)action);

            Console.WriteLine($"Received packet: Type={TypeName}, Action={headerAction}"); // Body={BitConverter.ToString(packetBody)}
            ushort type = (ushort)HeaderType.LOGIN;

            switch ((ActionLogin)action)
            {
                case ActionLogin.VERSION_CHECK: // ACTION.VERSION_CHECK
                    Login.VersionCheck((ushort)HeaderType.LOGIN, action, stream);
                    break;
                case ActionLogin.CRED_CHECK:
                    // Process CRED_CHECK
                    Login.CredentialCheck((ushort)HeaderType.LOGIN, action, packetBody, stream);
                    break;
                case ActionLogin.SERVER_TIME:
                    Login.ServerTime((ushort)HeaderType.LOGIN, action, stream);
                    break;
                case ActionLogin.CREATE_ACCOUNT:
                    // Process ACCOUNT_CREATION
                    break;
                case ActionLogin.CASH_UNKNOWN:
                    Login.CashUnknown((ushort)HeaderType.LOGIN, action, stream);
                    break;
                case ActionLogin.BUDDY_LIST:
                    // Process BUDDY_LIST
                    Login.GetBuddyList(type, action, stream);
                    break;
                case ActionLogin.TO_LOBBY:
                    // Process TO_LOBBY
                    Login.TO_LOBBY(type, action, packetBody, stream);
                    break;
                default:
                    Console.WriteLine($"Unknown action for Login packet: {action}");
                    break;
            }
        }

        public void ProcessGeneralPacket(ushort action, byte[] packetBody, NetworkStream stream)
        {
            String TypeName = Enum.GetName(HeaderType.GENERAL);
            String headerAction = Enum.GetName((ActionGeneral)action);

            Console.WriteLine($"Received packet: Type={TypeName}, Action={headerAction}"); // Body={BitConverter.ToString(packetBody, NetworkStream stream)}
            ushort type = (ushort)HeaderType.GENERAL;
            switch ((ActionGeneral)action)
            {
                case ActionGeneral.GET_PROPERTY:
                    General.GetProperty(type, action, packetBody, stream);
                    break;
                case ActionGeneral.GIFT_FROM:
                    // Process GIFT_FROM action
                    General.GetGiftFrom(type, action, stream);
                    break;
                case ActionGeneral.GIFT_TO: // ACTION.GIFT_TO
                                            // Process GIFT_TO action
                    General.GetGiftTo(type, action, stream);
                    break;
                case ActionGeneral.UNKNOWN_1: // ACTION.UNKNOWN_1
                                              // Process UNKNOWN_1 action
                    General.GetAvatars(type, action, packetBody, stream);
                    break;
                case ActionGeneral.UNKNOWN_2: // ACTION.UNKNOWN_2
                                              // Process UNKNOWN_2 action
                    General.Unknown2(type, action, packetBody, stream);
                    break;
                case ActionGeneral.UNKNOWN_3: // ACTION.UNKNOWN_3
                                              // Process UNKNOWN_3 action
                    General.Unknown3(type, action, stream); 
                    break;
                case ActionGeneral.UNKNOWN_4: // ACTION.UNKNOWN_4
                                              // Process UNKNOWN_4 action
                    General.Unknown4(type, action, stream);
                    break;
                case ActionGeneral.AVATAR_LIST: // ACTION.AVATAR_LIST
                                                // Process AVATAR_LIST action
                    General.GetAvatarList(type, action, packetBody, stream);
                    break;
                case ActionGeneral.GET_GRADE: // ACTION.GET_GRADE
                                              // Process GET_GRADE action
                    General.GetGrade(type, action, stream);
                    break;
                case ActionGeneral.TO_INFO: // ACTION.TO_INFO
                                            // Process TO_INFO action
                    General.MyInfo(type, action, stream);
                    break;
                case ActionGeneral.QUESTS: // ACTION.UNKNOWN_5
                                              // Process UNKNOWN_5 action
                    General.Quests(type, action, packetBody, stream);
                    break;
                case ActionGeneral.BUYING: // ACTION.BUYING
                                           // Process BUYING action
                    General.Unknown(type, action, stream);
                    break;
                case ActionGeneral.BEGIN_MATCH: // ACTION.TO_TRAINING
                                                // Process TO_TRAINING action
                    General.BeginMatch(type, action, packetBody, stream);
                    break;
                case ActionGeneral.EQUIP_ITEM: // ACTION.EQUIP_ITEM
                                               // Process EQUIP_ITEM action
                    General.Equip(type, action, packetBody, stream);
                    break;
                case ActionGeneral.GET_ZM_STATS:
                    General.GetZMStats(type, action, packetBody, stream);
                    break;
                case ActionGeneral.GET_ZM_STATS_2:
                    General.GetZMStats2(type, action, packetBody, stream);
                    break;
                case ActionGeneral.CHAT:
                    General.GlobalChat(type, action, packetBody, stream);
                    break;
                default:
                    Console.WriteLine($"Unknown action for General packet: {action}");
                    break;
            }
        }

        public void ProcessInventoryPacket(ushort action, byte[] packetBody, NetworkStream stream)
        {
            String TypeName = Enum.GetName(HeaderType.INVENTORY);
            String headerAction = Enum.GetName((ActionInventory)action);

            Console.WriteLine($"Received packet: Type={TypeName}, Action={headerAction}"); // Body={BitConverter.ToString(packetBody, NetworkStream stream)}

            ushort type = (ushort)HeaderType.INVENTORY;

            switch ((ActionInventory)action)
            {
                case ActionInventory.OLD_RF: // ACTION.OLD_RF
                                             // Process OLD_RF action
                    General.Unknown(type, action, stream);
                    break;
                case ActionInventory.CASH: // ACTION.CASH
                    Inventory.GetCash((ushort)(HeaderType.INVENTORY), action, packetBody, stream);
                    break;
                case ActionInventory.ADD_ITEM: // ACTION.UNKNOWN_6
                    Inventory.Unknown6((ushort)(HeaderType.INVENTORY), action, packetBody, stream);
                    break;
                case ActionInventory.BUY_ITEM: // ACTION.BUY_ITEM
                    Inventory.BuyItem(type, action, packetBody, stream);
                    break;

                case ActionInventory.OPEN_BOX: // ACTION.BUY_ITEM
                    Inventory.OpenBox(type, action, packetBody, stream);
                    break;
                case ActionInventory.UNKNOWN_10: // ACTION.UNKNOWN_10
                    //General.Unknown(type, action, stream);
                    break;
                default:
                    Console.WriteLine($"Unknown action for Inventory packet: {action}");
                    break;
            }
        }

        public void ProcessGameGuardPacket(ushort action, byte[] packetBody, NetworkStream stream)
        {
            String TypeName = Enum.GetName(HeaderType.GAMEGUARD);
            String headerAction = Enum.GetName((ActionGG)action);

            Console.WriteLine($"Received packet: Type={TypeName}, Action={headerAction}"); // Body={BitConverter.ToString(packetBody, NetworkStream stream)}
            ushort type = (ushort)(HeaderType.GAMEGUARD);

            switch ((ActionGG)action)
            {
                case ActionGG.GAMEGUARD_START: // ACTION.GAMEGUARD_START
                    GameGuard.SendGameGuard(type, action, stream);
                    break;
                default:
                    Console.WriteLine($"Unknown action for GameGuard packet: {action}");
                    break;
            }
        }

        public void ProcessQueryPacket(ushort action, byte[] packetBody, NetworkStream stream)
        {
            String TypeName = Enum.GetName(HeaderType.QUERY);
            String headerAction = Enum.GetName((ActionQuery)action);

            Console.WriteLine($"Received packet: Type={TypeName}, Action={headerAction}"); // Body={BitConverter.ToString(packetBody)}

            ushort type = (ushort)(HeaderType.QUERY);

            switch ((ActionQuery)action)
            {
                case ActionQuery.EQUIPPED_AVATAR: // ACTION.EQUIPPED_AVATAR
                                                  // Process EQUIPPED_AVATAR action
                    Query.GetEquippedAvatar(type, action, packetBody, stream);
                    break;
                case ActionQuery.CLEAR_AVATAR_SLOT: // ACTION.UNKNOWN_8
                                            // Process UNKNOWN_8 action
                    Query.ClearAvatarSlot(type, action, packetBody, stream);
                    break;
                case ActionQuery.UPDATE_AVATAR: // ACTION.UNKNOWN_9
                                            // Process UNKNOWN_9 action
                    Query.UpdateAvatar(type, action, packetBody, stream);
                   break;
                case ActionQuery.GET_ROOM: // ACTION.GET_ROOM
                           // Process GET_ROOM action
                    Query.GetRoomList(type, action, packetBody, stream);
                    break;
                case ActionQuery.CREATE_ROOM: // ACTION.CREATE_ROOM
                           // Process CREATE_ROOM action
                    Query.CreateRoom(type, action, packetBody, stream);
                    break;
                case ActionQuery.JOIN_ROOM: // ACTION.JOIN_ROOM_1
                           // Process JOIN_ROOM_1 action
                    Query.JoinRoom(type, action, packetBody, stream);
                    break;
                case ActionQuery.JOIN_ROOM_GET_PLAYERS: // ACTION.JOIN_ROOM_2
                           // Process JOIN_ROOM_2 action
                    Query.JoinGetPlayers(type, action, packetBody, stream);
                    break;
                case ActionQuery.USER_JOINED_ROOM:
                    break;
                case ActionQuery.USER_READY: // ACTION.USER_READY
                           // Process USER_READY action
                    Query.UserReady(type, action, packetBody, stream);
                    break;
                case ActionQuery.LEAVE_ROOM: // ACTION.LEAVE_ROOM
                           // Process LEAVE_ROOM action
                    Query.LeaveRoom(type, action, packetBody, stream);
                    break;
                case ActionQuery.CHANGE_MAP: // ACTION.CHANGE_MAP
                           // Process CHANGE_MAP action
                    Query.ChangeMap(type, action, packetBody, stream);
                    break;
                case ActionQuery.START_GAME: // ACTION.START_GAME
                    Query.StartMatch(type, action, packetBody, stream);
                    break;
                case ActionQuery.LOADING_GAME_1:
                    Query.LoadGame1(type, action, packetBody, stream);
                    break;
                case ActionQuery.LOADING_GAME_2:
                    Query.LoadGame2(type, action, packetBody, stream);
                    break;
                case ActionQuery.END_MATCH:
                    Query.GetReward(type, action, packetBody, stream);
                    break;
                case ActionQuery.MATCH_REWARD:
                    Query.GetReward(type, action, packetBody, stream);
                    break;
                case ActionQuery.CHAT:
                    Query.Chat(type, action, packetBody, stream);
                    break;
                case ActionQuery.CHANGE_AVATAR:
                    Query.ChangeAvatar(type, action, packetBody, stream);
                    break;
                //case ActionQuery.EQUIPPING: // ACTION.EQUIPPING
                                            // Process EQUIPPING action
                    //Query.Unknown8(type, action);
                    //break;
                default:
                    Console.WriteLine($"Unknown action for Query packet: {action}");
                    break;
            }
        }


    }
}

