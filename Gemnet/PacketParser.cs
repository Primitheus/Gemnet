using System.Net.Sockets;
using Gemnet.Packets;
using Gemnet.Packets.Enums;
using static Gemnet.Packets.Enums.Packets;
using Gemnet.PacketProcessors;
using Gemnet.Network.Packets;
using Gemnet.Network;

namespace SendPacket
{
    /// <summary>
    /// Legacy packet parser - now delegates to the new PacketProcessor
    /// This maintains backward compatibility while using the new architecture
    /// </summary>
    public class Parser
    {
        private readonly PacketProcessor _packetProcessor;

        public Parser()
        {
            // Get the packet processor from the server instance
            _packetProcessor = Gemnet.Program.ServerHolder.ServerInstance?.GetPacketProcessor();
        }

        public async Task ProcessPacketAsync(ushort type, ushort action, byte[] packetBody, NetworkStream stream)
        {
            if (_packetProcessor != null)
            {
                // Create a buffer with the packet body for the new processor
                var buffer = new byte[packetBody.Length + 6]; // 6 bytes for header
                
                // Reconstruct the packet header
                buffer[0] = (byte)(type >> 8);
                buffer[1] = (byte)type;
                buffer[2] = (byte)(packetBody.Length >> 8);
                buffer[3] = (byte)packetBody.Length;
                buffer[4] = (byte)action;
                buffer[5] = (byte)(action >> 8);
                
                // Copy the packet body
                packetBody.CopyTo(buffer, 6);
                
                await _packetProcessor.ProcessPacketAsync(buffer, buffer.Length, stream);
            }
            else
            {
                // Fallback to legacy processing if packet processor is not available
                await ProcessPacketLegacy(type, action, packetBody, stream);
            }
        }

        private async Task ProcessPacketLegacy(ushort type, ushort action, byte[] packetBody, NetworkStream stream)
        {
            // Legacy packet processing logic
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
                case (HeaderType.GUILD):
                    ProcessGuildPacket(action, packetBody, stream);
                    break;
                default:
                    Console.WriteLine($"Unknown packet type: {type}");
                    break;
            }
        }

        // Legacy packet processing methods - kept for backward compatibility
        public void ProcessLoginPacket(ushort action, byte[] packetBody, NetworkStream stream)
        {
            String TypeName = Enum.GetName(HeaderType.LOGIN);
            String headerAction = Enum.GetName((ActionLogin)action);

            Console.WriteLine($"Received packet: Type={TypeName}, Action={headerAction}");
            ushort type = (ushort)HeaderType.LOGIN;

            switch ((ActionLogin)action)
            {
                case ActionLogin.VERSION_CHECK:
                    Login.VersionCheck((ushort)HeaderType.LOGIN, action, stream);
                    break;
                case ActionLogin.CRED_CHECK:
                    Login.CredentialCheck((ushort)HeaderType.LOGIN, action, packetBody, stream);
                    break;
                case ActionLogin.SERVER_TIME:
                    Login.ServerTime((ushort)HeaderType.LOGIN, action, stream);
                    break;
                case ActionLogin.CREATE_ACCOUNT:
                    Login.CreateAccount((ushort)HeaderType.LOGIN, action, packetBody, stream);
                    break;
                case ActionLogin.CASH_UNKNOWN:
                    Login.CashUnknown((ushort)HeaderType.LOGIN, action, stream);
                    break;
                case ActionLogin.BUDDY_LIST:
                    Login.GetBuddyList(type, action, packetBody, stream);
                    break;
                case ActionLogin.TO_LOBBY:
                    Login.TO_LOBBY(type, action, packetBody, stream);
                    break;
                case ActionLogin.SET_OPTION_INV:
                    Login.SetOptionInventory(type, action, packetBody, stream);
                    break;
                case ActionLogin.ADD_BUDDY:
                    Login.ADD_BUDDY(type, action, packetBody, stream);
                    break;
                case ActionLogin.USE_MEGAPHONE:
                    Login.UseMegaphone(type, action, packetBody, stream);
                    break;
                case ActionLogin.CHANGE_NICKNAME:
                    Login.ChangeNickname(type, action, packetBody, stream);
                    break;
                case ActionLogin.ADD_BUDDY_ID:
                    Login.AddBuddyID(type, action, packetBody, stream);
                    break;
                case ActionLogin.GEM_LOGIN:
                    Login.GemLogin(type, action, packetBody, stream);
                    break;
                case ActionLogin.GEM_UNKNOWN_1:
                    Login.GemUnknown1(type, action, packetBody, stream);
                    break;
                default:
                    Console.WriteLine($"Unknown action for Login packet: {action}");
                    break;
            }
        }

        public void ProcessGuildPacket(ushort action, byte[] packetBody, NetworkStream stream)
        {
            String TypeName = Enum.GetName(HeaderType.GUILD);
            String headerAction = Enum.GetName((ActionGuild)action);

            Console.WriteLine($"Received packet: Type={TypeName}, Action={headerAction}");
            ushort type = (ushort)(HeaderType.GUILD);

            switch ((ActionGuild)action)
            {
                case ActionGuild.GET_GUILD_RANK:
                    Guild.GetGuildRank(type, action, packetBody, stream);
                    break;
                case ActionGuild.GET_GUILD_RANK_LIST:
                    Guild.GetGuildRankList(type, action, packetBody, stream);
                    break;
                default:
                    Console.WriteLine($"Unknown action for Guild packet: {action}");
                    break;
            }
        }
        
        public void ProcessGeneralPacket(ushort action, byte[] packetBody, NetworkStream stream)
        {
            String TypeName = Enum.GetName(HeaderType.GENERAL);
            String headerAction = Enum.GetName((ActionGeneral)action);

            Console.WriteLine($"Received packet: Type={TypeName}, Action={headerAction}");
            ushort type = (ushort)HeaderType.GENERAL;
            switch ((ActionGeneral)action)
            {
                case ActionGeneral.GET_PROPERTY:
                    General.GetProperty(type, action, packetBody, stream);
                    break;
                case ActionGeneral.GIFT_FROM:
                    General.GetGiftFrom(type, action, stream);
                    break;
                case ActionGeneral.GIFT_TO:
                    General.GetGiftTo(type, action, stream);
                    break;
                case ActionGeneral.UNKNOWN_1:
                    General.GetAvatars(type, action, packetBody, stream);
                    break;
                case ActionGeneral.UNKNOWN_2:
                    General.Unknown2(type, action, packetBody, stream);
                    break;
                case ActionGeneral.UNKNOWN_3:
                    General.Unknown3(type, action, stream);
                    break;
                case ActionGeneral.UNKNOWN_4:
                    General.Unknown4(type, action, stream);
                    break;
                case ActionGeneral.AVATAR_LIST:
                    General.GetAvatarList(type, action, packetBody, stream);
                    break;
                case ActionGeneral.GET_GRADE:
                    General.GetGrade(type, action, stream);
                    break;
                case ActionGeneral.TO_INFO:
                    General.MyInfo(type, action, stream);
                    break;
                case ActionGeneral.QUESTS:
                    General.Quests(type, action, packetBody, stream);
                    break;
                case ActionGeneral.BUYING:
                    General.Unknown(type, action, stream);
                    break;
                case ActionGeneral.BEGIN_MATCH:
                    General.BeginMatch(type, action, packetBody, stream);
                    break;
                case ActionGeneral.EQUIP_ITEM:
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
                case ActionGeneral.GET_USER_INFO_RENEWAL:
                    General.GetUserInfoRenewal(type, action, packetBody, stream);
                    break;
                case ActionGeneral.GEM_UNKNOWN_A:
                    General.GemUnknownA(type, action, packetBody, stream);
                    break;
                case ActionGeneral.GEM_UNKNOWN_1:
                    General.GemUnknown1(type, action, packetBody, stream);
                    break;
                case ActionGeneral.GEM_UNKNOWN_2:
                    General.GemUnknown2(type, action, packetBody, stream);
                    break;
                case ActionGeneral.GEM_UNKNOWN_3:
                    General.GemUnknown3(type, action, packetBody, stream);
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

            Console.WriteLine($"Received packet: Type={TypeName}, Action={headerAction}");

            ushort type = (ushort)HeaderType.INVENTORY;

            switch ((ActionInventory)action)
            {
                case ActionInventory.OLD_RF:
                    General.Unknown(type, action, stream);
                    break;
                case ActionInventory.CASH:
                    Inventory.GetCash((ushort)(HeaderType.INVENTORY), action, packetBody, stream);
                    break;
                case ActionInventory.ADD_ITEM:
                    Inventory.AddItem((ushort)(HeaderType.INVENTORY), action, packetBody, stream);
                    break;
                case ActionInventory.BUY_ITEM:
                    Inventory.BuyItem(type, action, packetBody, stream);
                    break;
                case ActionInventory.OPEN_BOX:
                    Inventory.OpenBox(type, action, packetBody, stream);
                    break;
                case ActionInventory.ENCHANT:
                    Inventory.Enchant(type, action, packetBody, stream);
                    break;
                case ActionInventory.UNKNOWN_10:
                    break;
                case ActionInventory.WIN_MEDAL:
                    Inventory.WinMedal(type, action, packetBody, stream);
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

            Console.WriteLine($"Received packet: Type={TypeName}, Action={headerAction}");
            ushort type = (ushort)(HeaderType.GAMEGUARD);

            switch ((ActionGG)action)
            {
                case ActionGG.GAMEGUARD_START:
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

            Console.WriteLine($"Received packet: Type={TypeName}, Action={headerAction}");

            ushort type = (ushort)(HeaderType.QUERY);

            switch ((ActionQuery)action)
            {
                case ActionQuery.EQUIPPED_AVATAR:
                    Query.GetEquippedAvatar(type, action, packetBody, stream);
                    break;
                case ActionQuery.CLEAR_AVATAR_SLOT:
                    Query.ClearAvatarSlot(type, action, packetBody, stream);
                    break;
                case ActionQuery.UPDATE_AVATAR:
                    Query.UpdateAvatar(type, action, packetBody, stream);
                   break;
                case ActionQuery.GET_ROOM:
                    Query.GetRoomList(type, action, packetBody, stream);
                    break;
                case ActionQuery.CREATE_ROOM:
                    Query.HandleCreateRoom(type, action, packetBody, stream);
                    break;
                case ActionQuery.JOIN_ROOM:
                    Query.JoinRoom(type, action, packetBody, stream);
                    break;
                case ActionQuery.JOIN_ROOM_GET_PLAYERS:
                    Query.JoinGetPlayers(type, action, packetBody, stream);
                    break;
                case ActionQuery.USER_JOINED_ROOM:
                    break;
                case ActionQuery.USER_READY:
                    Query.UserReady(type, action, packetBody, stream);
                    break;
                case ActionQuery.LEAVE_ROOM:
                    Query.LeaveRoom(type, action, packetBody, stream);
                    break;
                case ActionQuery.CHANGE_MAP:
                    Query.ChangeMap(type, action, packetBody, stream);
                    break;
                case ActionQuery.START_GAME:
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
                    Query.GetMatchReward(type, action, packetBody, stream);
                    break;
                case ActionQuery.CHAT:
                    Query.Chat(type, action, packetBody, stream);
                    break;
                case ActionQuery.CHANGE_AVATAR:
                    Query.ChangeAvatar(type, action, packetBody, stream);
                    break;
                case ActionQuery.FIN:
                    Query.FIN(type, action, packetBody, stream);
                    break;
                case ActionQuery.SELECT_TEAM:
                    Query.SelectTeam(type, action, packetBody, stream);
                    break;
                default:
                    Console.WriteLine($"Unknown action for Query packet: {action}");
                    break;
            }
        }
    }
}

