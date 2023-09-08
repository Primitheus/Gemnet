using System.Net.Sockets;
using Gemnet.Packets;
using Gemnet.Packets.Enums;
using static Gemnet.Packets.Enums.Packets;
using Gemnet.PacketProcessors;
using Gemnet.Network.Packets;

namespace SendPacket
{

    public class Parser
    {

        private const int MaxBufferSize = 1024;
        private const int PacketHeaderSize = 6; // 2 bytes for type + 2 bytes for length + 2 bytes for action
        private const int TypeOffset = 0;
        private const int LengthOffset = 2;
        private const int ActionOffset = 4;

        public async Task ProcessPacketAsync(ushort type, ushort action, byte[] packetBody)
        {
            // Add packet processing logic here

                switch ((HeaderType)type)
                {
                    case (HeaderType.LOGIN):
                        ProcessLoginPacket(action, packetBody);
                        break;
                    case (HeaderType.GENERAL):
                        ProcessGeneralPacket(action, packetBody);
                        break;
                    case (HeaderType.GAMEGUARD):
                        ProcessGameGuardPacket(action, packetBody);
                        break;
                    case (HeaderType.INVENTORY):
                        ProcessInventoryPacket(action, packetBody);
                        break;
                    case (HeaderType.QUERY):
                        ProcessQueryPacket(action, packetBody);
                        break;
                    default:
                        Console.WriteLine($"Unknown packet type: {type}");
                        break;

                }
        }

        public void ProcessLoginPacket(ushort action, byte[] packetBody)
        {
            String TypeName = Enum.GetName(HeaderType.LOGIN);
            String headerAction = headerAction = Enum.GetName((ActionLogin)action);

            Console.WriteLine($"Received packet: Type={TypeName}, Action={headerAction}"); // Body={BitConverter.ToString(packetBody)}
            ushort type = (ushort)HeaderType.LOGIN;

            switch ((ActionLogin)action)
            {
                case ActionLogin.VERSION_CHECK: // ACTION.VERSION_CHECK
                    Login.VersionCheck((ushort)HeaderType.LOGIN, action);
                    break;
                case ActionLogin.CRED_CHECK:
                    // Process CRED_CHECK
                    Login.CredentialCheck((ushort)HeaderType.LOGIN, action, packetBody);
                    break;
                case ActionLogin.SERVER_TIME:
                    Login.ServerTime((ushort)HeaderType.LOGIN, action);
                    break;
                case ActionLogin.ACCOUNT_CREATION:
                    // Process ACCOUNT_CREATION
                    break;
                case ActionLogin.CASH_UNKNOWN:
                    Login.CashUnknown((ushort)HeaderType.LOGIN, action);
                    break;
                case ActionLogin.BUDDY_LIST:
                    // Process BUDDY_LIST
                    Login.GetBuddyList(type, action);
                    break;
                case ActionLogin.TO_LOBBY:
                    // Process TO_LOBBY
                    Login.TO_LOBBY(type, action, packetBody);
                    break;
                default:
                    Console.WriteLine($"Unknown action for Login packet: {action}");
                    break;
            }
        }

        public void ProcessGeneralPacket(ushort action, byte[] packetBody)
        {
            String TypeName = Enum.GetName(HeaderType.GENERAL);
            String headerAction = headerAction = Enum.GetName((ActionGeneral)action);

            Console.WriteLine($"Received packet: Type={TypeName}, Action={headerAction}"); // Body={BitConverter.ToString(packetBody)}
            ushort type = (ushort)HeaderType.GENERAL;
            switch ((ActionGeneral)action)
            {
                case ActionGeneral.GET_PROPERTY:
                    General.GetProperty(type, action, packetBody);
                    break;
                case ActionGeneral.GIFT_FROM:
                    // Process GIFT_FROM action
                    General.GetGiftFrom(type, action);
                    break;
                case ActionGeneral.GIFT_TO: // ACTION.GIFT_TO
                                            // Process GIFT_TO action
                    General.GetGiftTo(type, action);
                    break;
                case ActionGeneral.UNKNOWN_1: // ACTION.UNKNOWN_1
                                              // Process UNKNOWN_1 action
                    General.Unknown1(type, action);
                    break;
                case ActionGeneral.UNKNOWN_2: // ACTION.UNKNOWN_2
                                              // Process UNKNOWN_2 action
                    General.Unknown2(type, action, packetBody);
                    break;
                case ActionGeneral.UNKNOWN_3: // ACTION.UNKNOWN_3
                                              // Process UNKNOWN_3 action
                    General.Unknown3(type, action); 
                    break;
                case ActionGeneral.UNKNOWN_4: // ACTION.UNKNOWN_4
                                              // Process UNKNOWN_4 action
                    General.Unknown4(type, action);
                    break;
                case ActionGeneral.AVATAR_LIST: // ACTION.AVATAR_LIST
                                                // Process AVATAR_LIST action
                    General.GetAvatarList(type, action);
                    break;
                case ActionGeneral.GET_GRADE: // ACTION.GET_GRADE
                                              // Process GET_GRADE action
                    General.GetGrade(type, action);
                    break;
                case ActionGeneral.TO_INFO: // ACTION.TO_INFO
                                            // Process TO_INFO action
                    General.MyInfo(type, action);
                    break;
                case ActionGeneral.QUESTS: // ACTION.UNKNOWN_5
                                              // Process UNKNOWN_5 action
                    General.Quests(type, action, packetBody);

                    break;
                case ActionGeneral.BUYING: // ACTION.BUYING
                                           // Process BUYING action
                    General.Unknown(type, action);
                    break;
                case ActionGeneral.TO_TRAINING: // ACTION.TO_TRAINING
                                                // Process TO_TRAINING action
                    General.ToTraining(type, action, packetBody);
                    break;
                case ActionGeneral.EQUIP_ITEM: // ACTION.EQUIP_ITEM
                                               // Process EQUIP_ITEM action
                    General.Equip(type, action, packetBody);
                    break;
                default:
                    Console.WriteLine($"Unknown action for General packet: {action}");
                    break;
            }
        }

        public void ProcessInventoryPacket(ushort action, byte[] packetBody)
        {
            String TypeName = Enum.GetName(HeaderType.INVENTORY);
            String headerAction = headerAction = Enum.GetName((ActionInventory)action);

            Console.WriteLine($"Received packet: Type={TypeName}, Action={headerAction}"); // Body={BitConverter.ToString(packetBody)}

            ushort type = (ushort)HeaderType.INVENTORY;

            switch ((ActionInventory)action)
            {
                case ActionInventory.OLD_RF: // ACTION.OLD_RF
                                             // Process OLD_RF action
                    General.Unknown(type, action);
                    break;
                case ActionInventory.CASH: // ACTION.CASH
                    Inventory.GetCash((ushort)(HeaderType.INVENTORY), action, packetBody);
                    break;
                case ActionInventory.UNKNOWN_6: // ACTION.UNKNOWN_6
                           // Process UNKNOWN_6 action
                    General.Unknown(type, action);
                    break;
                case ActionInventory.BUY_ITEM: // ACTION.BUY_ITEM
                                               // Process BUY_ITEM action
                    Inventory.BuyItem(type, action, packetBody);
                    
                    break;

                case ActionInventory.OPEN_BOX: // ACTION.BUY_ITEM
                                               // Process BUY_ITEM action
                    Inventory.OpenBox(type, action, packetBody);

                    break;
                default:
                    Console.WriteLine($"Unknown action for Inventory packet: {action}");
                    break;
            }
        }

        public void ProcessGameGuardPacket(ushort action, byte[] packetBody)
        {
            String TypeName = Enum.GetName(HeaderType.GAMEGUARD);
            String headerAction = headerAction = Enum.GetName((ActionGG)action);

            Console.WriteLine($"Received packet: Type={TypeName}, Action={headerAction}"); // Body={BitConverter.ToString(packetBody)}
            ushort type = (ushort)(HeaderType.GAMEGUARD);

            switch ((ActionGG)action)
            {
                case ActionGG.GAMEGUARD_START: // ACTION.GAMEGUARD_START
                    GameGuard.SendGameGuard(type, action);
                    break;
                default:
                    Console.WriteLine($"Unknown action for GameGuard packet: {action}");
                    break;
            }
        }

        public void ProcessQueryPacket(ushort action, byte[] packetBody)
        {
            String TypeName = Enum.GetName(HeaderType.QUERY);
            String headerAction = headerAction = Enum.GetName((ActionQuery)action);

            Console.WriteLine($"Received packet: Type={TypeName}, Action={headerAction}"); // Body={BitConverter.ToString(packetBody)}

            ushort type = (ushort)(HeaderType.QUERY);

            switch ((ActionQuery)action)
            {
                case ActionQuery.EQUIPPED_AVATAR: // ACTION.EQUIPPED_AVATAR
                                                  // Process EQUIPPED_AVATAR action
                    Query.GetEquippedAvatar(type, action);
                    break;
                case ActionQuery.UNKNOWN_8: // ACTION.UNKNOWN_8
                                            // Process UNKNOWN_8 action
                    Query.Unknown8(type, action);
                    break;
                case ActionQuery.UNKNOWN_9: // ACTION.UNKNOWN_9
                                            // Process UNKNOWN_9 action
                    Query.Unknown9(type, action);
                   break;
                case ActionQuery.GET_ROOM: // ACTION.GET_ROOM
                           // Process GET_ROOM action
                    Query.GetRoomList(type, action, packetBody);
                    break;
                case ActionQuery.CREATE_ROOM: // ACTION.CREATE_ROOM
                           // Process CREATE_ROOM action
                    break;
                case ActionQuery.JOIN_ROOM_1: // ACTION.JOIN_ROOM_1
                           // Process JOIN_ROOM_1 action
                    break;
                case ActionQuery.JOIN_ROOM_2: // ACTION.JOIN_ROOM_2
                           // Process JOIN_ROOM_2 action
                    break;
                case ActionQuery.USER_READY: // ACTION.USER_READY
                           // Process USER_READY action
                    break;
                case ActionQuery.LEAVE_ROOM: // ACTION.LEAVE_ROOM
                           // Process LEAVE_ROOM action
                    break;
                case ActionQuery.CHANGE_MAP: // ACTION.CHANGE_MAP
                           // Process CHANGE_MAP action
                    break;
                case ActionQuery.START_GAME: // ACTION.START_GAME
                           // Process START_GAME action
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

