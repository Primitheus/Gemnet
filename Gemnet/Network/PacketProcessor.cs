using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Gemnet.Packets;
using Gemnet.Packets.Enums;
using static Gemnet.Packets.Enums.Packets;
using Gemnet.PacketProcessors;
using Gemnet.Network.Packets;
using Gemnet.Network;
using Microsoft.Extensions.Logging;
using Gemnet.PacketProcessors.Extra;

namespace Gemnet.Network
{
    public class PacketProcessor
    {
        private readonly ILogger _logger;
        private readonly ConnectionManager _connectionManager;
        private readonly PlayerManager _playerManager;
        private readonly GameManager _gameManager;

        public PacketProcessor(
            ILogger logger,
            ConnectionManager connectionManager,
            PlayerManager playerManager,
            GameManager gameManager)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _playerManager = playerManager;
            _gameManager = gameManager;
        }

        public async Task ProcessPacketAsync(byte[] buffer, int bytesRead, NetworkStream stream)
        {
            var connection = _connectionManager.GetConnection(stream);
            if (connection == null)
            {
                _logger.LogWarning("Received packet from unknown connection");
                return;
            }

            connection.UpdateActivity();

            try
            {
                int offset = 0;
                byte[] packet = new byte[bytesRead];

                while (offset < bytesRead)
                {
                    if (offset + 6 > bytesRead) // PacketHeaderSize = 6
                    {
                        _logger.LogDebug("Incomplete packet header received");
                        break;
                    }

                    // Decrypt if encryption is enabled
                    if (connection.CipherState != null)
                    {
                        packet = connection.CipherState.Decryptor.Decrypt(buffer, bytesRead);
                    }
                    else
                    {
                        packet = buffer;
                    }

                    // Parse packet header
                    ushort type = (ushort)((packet[offset + 0] << 8) | packet[offset + 0 + 1]);
                    ushort length = (ushort)((packet[offset + 2] << 8) | packet[offset + 3]);
                    ushort action = BitConverter.ToUInt16(packet, offset + 4);

                    _logger.LogDebug("Parsed packet: Type=0x{Type:X4}, Action=0x{Action:X4}, Length={Length}", type, action, length);

                    if (length > bytesRead - offset)
                    {
                        _logger.LogWarning($"Invalid packet length: {length}, available: {bytesRead - offset}");
                        break;
                    }

                    byte[] packetBody = new byte[length];
                    Buffer.BlockCopy(buffer, offset + 6, packetBody, 0, length);

                    // Process the packet
                    await ProcessPacketByType(type, action, packet, stream);

                    offset += 6 + length;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing packet from {RemoteEndPoint}", connection.RemoteEndPoint);
                
                // Disconnect the client on critical errors
                if (ex is ObjectDisposedException || ex is IOException)
                {
                    connection.Disconnect();
                }
            }
        }

        private async Task ProcessPacketByType(ushort type, ushort action, byte[] packetBody, NetworkStream stream)
        {
            try
            {
                switch ((HeaderType)type)
                {
                    case HeaderType.LOGIN:
                        ProcessLoginPacket(action, packetBody, stream);
                        break;
                    case HeaderType.GENERAL:
                        ProcessGeneralPacket(action, packetBody, stream);
                        break;
                    case HeaderType.GAMEGUARD:
                        ProcessGameGuardPacket(action, packetBody, stream);
                        break;
                    case HeaderType.INVENTORY:
                        ProcessInventoryPacket(action, packetBody, stream);
                        break;
                    case HeaderType.QUERY:
                        ProcessQueryPacket(action, packetBody, stream);
                        break;
                    case HeaderType.GUILD:
                        ProcessGuildPacket(action, packetBody, stream);
                        break;
                    default:
                        _logger.LogWarning($"Unknown packet type: {type:X4}");
                        
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing packet type {PacketType}, action {Action}", type, action);
            }
        }

        private void ProcessLoginPacket(ushort action, byte[] packetBody, NetworkStream stream)
        {
            var actionName = Enum.GetName((ActionLogin)action);
            _logger.LogDebug($"Processing LOGIN packet: {actionName}");

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
                    Login.GetBuddyList((ushort)HeaderType.LOGIN, action, packetBody, stream);
                    break;
                case ActionLogin.TO_LOBBY:
                    Login.TO_LOBBY((ushort)HeaderType.LOGIN, action, packetBody, stream);
                    break;
                case ActionLogin.SET_OPTION_INV:
                    Login.SetOptionInventory((ushort)HeaderType.LOGIN, action, packetBody, stream);
                    break;
                case ActionLogin.ADD_BUDDY:
                    Login.ADD_BUDDY((ushort)HeaderType.LOGIN, action, packetBody, stream);
                    break;
                case ActionLogin.AGREE_BUDDY:
                    Login.AGREE_BUDDY((ushort)HeaderType.LOGIN, action, packetBody, stream);
                    break;
                case ActionLogin.DELETE_BUDDY :
                    Login.DELETE_BUDDY((ushort)HeaderType.LOGIN, action, packetBody, stream);
                    break;
                case ActionLogin.USE_MEGAPHONE:
                    Login.UseMegaphone((ushort)HeaderType.LOGIN, action, packetBody, stream);
                    break;
                case ActionLogin.CHANGE_NICKNAME:
                    Login.ChangeNickname((ushort)HeaderType.LOGIN, action, packetBody, stream);
                    break;
                case ActionLogin.ADD_BUDDY_ID:
                    Login.AddBuddyID((ushort)HeaderType.LOGIN, action, packetBody, stream);
                    break;
                case ActionLogin.GEM_LOGIN:
                    Login.GemLogin((ushort)HeaderType.LOGIN, action, packetBody, stream);
                    break;
                case ActionLogin.GEM_UNKNOWN_1:
                    Login.GemUnknown1((ushort)HeaderType.LOGIN, action, packetBody, stream);
                    break;
                default:
                    _logger.LogWarning($"Unknown LOGIN action: {action:X4}");
                    Util.GenericFail((ushort)HeaderType.GENERAL, action, stream);

                    break;
            }
        }

        private void ProcessGeneralPacket(ushort action, byte[] packetBody, NetworkStream stream)
        {
            var actionName = Enum.GetName((ActionGeneral)action);
            _logger.LogDebug($"Processing GENERAL packet: {actionName}");

            switch ((ActionGeneral)action)
            {
                case ActionGeneral.GET_PROPERTY:
                    General.GetProperty((ushort)HeaderType.GENERAL, action, packetBody, stream);
                    break;
                case ActionGeneral.GIFT_FROM:
                    General.GetGiftFrom((ushort)HeaderType.GENERAL, action, stream);
                    break;
                case ActionGeneral.GIFT_TO:
                    General.GetGiftTo((ushort)HeaderType.GENERAL, action, stream);
                    break;
                case ActionGeneral.UNKNOWN_1:
                    General.GetAvatars((ushort)HeaderType.GENERAL, action, packetBody, stream);
                    break;
                case ActionGeneral.UNKNOWN_2:
                    General.Unknown2((ushort)HeaderType.GENERAL, action, packetBody, stream);
                    break;
                case ActionGeneral.UNKNOWN_3:
                    General.Unknown3((ushort)HeaderType.GENERAL, action, stream);
                    break;
                case ActionGeneral.UNKNOWN_4:
                    General.Unknown4((ushort)HeaderType.GENERAL, action, stream);
                    break;
                case ActionGeneral.AVATAR_LIST:
                    General.GetAvatarList((ushort)HeaderType.GENERAL, action, packetBody, stream);
                    break;
                case ActionGeneral.GET_GRADE:
                    General.GetGrade((ushort)HeaderType.GENERAL, action, stream);
                    break;
                case ActionGeneral.TO_INFO:
                    General.MyInfo((ushort)HeaderType.GENERAL, action, stream);
                    break;
                case ActionGeneral.QUESTS:
                    General.Quests((ushort)HeaderType.GENERAL, action, packetBody, stream);
                    break;
                case ActionGeneral.BUYING:
                    General.Unknown((ushort)HeaderType.GENERAL, action, stream);
                    break;
                case ActionGeneral.BEGIN_MATCH:
                    General.BeginMatch((ushort)HeaderType.GENERAL, action, packetBody, stream);
                    break;
                case ActionGeneral.EQUIP_ITEM:
                    General.Equip((ushort)HeaderType.GENERAL, action, packetBody, stream);
                    break;
                case ActionGeneral.GET_ZM_STATS:
                    General.GetZMStats((ushort)HeaderType.GENERAL, action, packetBody, stream);
                    break;
                case ActionGeneral.GET_ZM_STATS_2:
                    General.GetZMStats2((ushort)HeaderType.GENERAL, action, packetBody, stream);
                    break;
                case ActionGeneral.CHAT:
                    General.GlobalChat((ushort)HeaderType.GENERAL, action, packetBody, stream);
                    break;
                case ActionGeneral.GET_USER_INFO_RENEWAL:
                    General.GetUserInfoRenewal((ushort)HeaderType.GENERAL, action, packetBody, stream);
                    break;
                case ActionGeneral.GEM_UNKNOWN_A:
                    General.GemUnknownA((ushort)HeaderType.GENERAL, action, packetBody, stream);
                    break;
                case ActionGeneral.GEM_UNKNOWN_1:
                    General.GemUnknown1((ushort)HeaderType.GENERAL, action, packetBody, stream);
                    break;
                case ActionGeneral.GEM_UNKNOWN_2:
                    General.GemUnknown2((ushort)HeaderType.GENERAL, action, packetBody, stream);
                    break;
                case ActionGeneral.GEM_UNKNOWN_3:
                    General.GemUnknown3((ushort)HeaderType.GENERAL, action, packetBody, stream);
                    break;
                default:
                    _logger.LogWarning($"Unknown GENERAL action: {action:X4}");
                    Util.GenericFail((ushort)HeaderType.GENERAL, action, stream);
                    break;
            }
        }

        private void ProcessGameGuardPacket(ushort action, byte[] packetBody, NetworkStream stream)
        {
            var actionName = Enum.GetName((ActionGG)action);
            _logger.LogDebug($"Processing GAMEGUARD packet: {actionName}");

            switch ((ActionGG)action)
            {
                case ActionGG.GAMEGUARD_START:
                    GameGuard.SendGameGuard((ushort)HeaderType.GAMEGUARD, action, stream);
                    break;
                default:
                    _logger.LogWarning($"Unknown GAMEGUARD action: {action:X4}");
                    Util.GenericFail((ushort)HeaderType.GENERAL, action, stream);

                    break;
            }
        }

        private void ProcessInventoryPacket(ushort action, byte[] packetBody, NetworkStream stream)
        {
            var actionName = Enum.GetName((ActionInventory)action);
            _logger.LogDebug($"Processing INVENTORY packet: {actionName}");

            switch ((ActionInventory)action)
            {
                case ActionInventory.OLD_RF:
                    General.Unknown((ushort)HeaderType.INVENTORY, action, stream);
                    break;
                case ActionInventory.CASH:
                    Inventory.GetCash((ushort)HeaderType.INVENTORY, action, packetBody, stream);
                    break;
                case ActionInventory.ADD_ITEM:
                    Inventory.AddItem((ushort)HeaderType.INVENTORY, action, packetBody, stream);
                    break;
                case ActionInventory.BUY_ITEM:
                    Inventory.BuyItem((ushort)HeaderType.INVENTORY, action, packetBody, stream);
                    break;
                case ActionInventory.OPEN_BOX:
                    Inventory.OpenBox((ushort)HeaderType.INVENTORY, action, packetBody, stream);
                    break;
                case ActionInventory.ENCHANT:
                    Inventory.Enchant((ushort)HeaderType.INVENTORY, action, packetBody, stream);
                    break;
                case ActionInventory.WIN_MEDAL:
                    Inventory.WinMedal((ushort)HeaderType.INVENTORY, action, packetBody, stream);
                    break;
                default:
                    _logger.LogWarning($"Unknown INVENTORY action: {action:X4}");
                    Util.GenericFail((ushort)HeaderType.GENERAL, action, stream);

                    break;
            }
        }

        private void ProcessQueryPacket(ushort action, byte[] packetBody, NetworkStream stream)
        {
            var actionName = Enum.GetName((ActionQuery)action);
            _logger.LogDebug($"Processing QUERY packet: {actionName}");

            switch ((ActionQuery)action)
            {
                case ActionQuery.EQUIPPED_AVATAR:
                    Query.GetEquippedAvatar((ushort)HeaderType.QUERY, action, packetBody, stream);
                    break;
                case ActionQuery.CLEAR_AVATAR_SLOT:
                    Query.ClearAvatarSlot((ushort)HeaderType.QUERY, action, packetBody, stream);
                    break;
                case ActionQuery.UPDATE_AVATAR:
                    Query.UpdateAvatar((ushort)HeaderType.QUERY, action, packetBody, stream);
                    break;
                case ActionQuery.GET_ROOM:
                    Query.GetRoomList((ushort)HeaderType.QUERY, action, packetBody, stream);
                    break;
                case ActionQuery.CREATE_ROOM:
                    Query.HandleCreateRoom((ushort)HeaderType.QUERY, action, packetBody, stream);
                    break;
                case ActionQuery.JOIN_ROOM:
                    Query.JoinRoom((ushort)HeaderType.QUERY, action, packetBody, stream);
                    break;
                case ActionQuery.JOIN_ROOM_GET_PLAYERS:
                    Query.JoinGetPlayers((ushort)HeaderType.QUERY, action, packetBody, stream);
                    break;
                case ActionQuery.USER_READY:
                    Query.UserReady((ushort)HeaderType.QUERY, action, packetBody, stream);
                    break;
                case ActionQuery.LEAVE_ROOM:
                    Query.LeaveRoom((ushort)HeaderType.QUERY, action, packetBody, stream);
                    break;
                case ActionQuery.CHANGE_MAP:
                    Query.ChangeMap((ushort)HeaderType.QUERY, action, packetBody, stream);
                    break;
                case ActionQuery.START_GAME:
                    Query.StartMatch((ushort)HeaderType.QUERY, action, packetBody, stream);
                    break;
                case ActionQuery.LOADING_GAME_1:
                    Query.LoadGame1((ushort)HeaderType.QUERY, action, packetBody, stream);
                    break;
                case ActionQuery.LOADING_GAME_2:
                    Query.LoadGame2((ushort)HeaderType.QUERY, action, packetBody, stream);
                    break;
                case ActionQuery.END_MATCH:
                    Query.GetReward((ushort)HeaderType.QUERY, action, packetBody, stream);
                    break;
                case ActionQuery.MATCH_REWARD:
                    Query.GetMatchReward((ushort)HeaderType.QUERY, action, packetBody, stream);
                    break;
                case ActionQuery.CHAT:
                    Query.Chat((ushort)HeaderType.QUERY, action, packetBody, stream);
                    break;
                case ActionQuery.CHANGE_AVATAR:
                    Query.ChangeAvatar((ushort)HeaderType.QUERY, action, packetBody, stream);
                    break;
                case ActionQuery.FIN:
                    Query.FIN((ushort)HeaderType.QUERY, action, packetBody, stream);
                    break;
                case ActionQuery.SELECT_TEAM:
                    Query.SelectTeam((ushort)HeaderType.QUERY, action, packetBody, stream);
                    break;
                default:
                    _logger.LogWarning($"Unknown QUERY action: {action:X4}");
                    Util.GenericFail((ushort)HeaderType.GENERAL, action, stream);
                    break;
            }
        }

        private void ProcessGuildPacket(ushort action, byte[] packetBody, NetworkStream stream)
        {
            var actionName = Enum.GetName((ActionGuild)action);
            _logger.LogDebug($"Processing GUILD packet: {actionName}");

            switch ((ActionGuild)action)
            {
                case ActionGuild.GET_GUILD_RANK:
                    Guild.GetGuildRank((ushort)HeaderType.GUILD, action, packetBody, stream);
                    break;
                case ActionGuild.GET_GUILD_RANK_LIST:
                    Guild.GetGuildRankList((ushort)HeaderType.GUILD, action, packetBody, stream);
                    break;
                default:
                    _logger.LogWarning($"Unknown GUILD action: {action:X4}");
                    Util.GenericFail((ushort)HeaderType.GENERAL, action, stream);
                    break;
            }
        }
    }
} 