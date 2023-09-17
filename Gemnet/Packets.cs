using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemnet.Packets.Enums
{
    internal class Packets
    {
        
        public enum HeaderType
        {
            LOGIN = 0x10,
            GENERAL = 0x30,
            INVENTORY = 0x31,
            GAMEGUARD = 0x48,
            QUERY = 0x40,
            UNKNOWN = 0x33
        }

        public enum ActionLogin
        {
            // LOGIN (0x10)
            VERSION_CHECK = 0x90,
            SERVER_TIME = 0x92,
            CRED_CHECK = 0x84,
            ACCOUNT_CREATION = 0x88,
            CASH_UNKNOWN = 0xd0,
            BUDDY_LIST = 0xa8,
            TO_LOBBY = 0xb6,
        }

        public enum ActionGG
        {
            // GAMEGUARD (0x48)
            GAMEGUARD_START = 0x80,
        }
        
        public enum ActionGeneral
        {
            // GENERAL (0x30)
            GET_PROPERTY = 0x84,
            GIFT_FROM = 0x92,
            GIFT_TO = 0x90,
            UNKNOWN_1 = 0x86,
            UNKNOWN_2 = 0xb4,
            UNKNOWN_3 = 0xb2,
            UNKNOWN_4 = 0xba,
            AVATAR_LIST = 0x88,
            GET_GRADE = 0xa0,
            TO_INFO = 0xbc,
            QUESTS = 0xcc,
            BUYING = 0xce,
            BEGIN_MATCH = 0x80,
            EQUIP_ITEM = 0xd6,
            JOIN_ROOM_BS = 0xa0,
            GET_ZM_STATS = 0xd4,
            GET_ZM_STATS_2 = 0xd2,

        }

        public enum ActionInventory
        {
            // INVENTORY (0x31)
            OLD_RF = 0x8c,
            CASH = 0x88,
            UNKNOWN_6 = 0x8a,
            BUY_ITEM = 0x80,
            OPEN_BOX = 0x9a,

        }

        public enum ActionQuery
        {
            // QUERY (0x40)
            EQUIPPED_AVATAR = 0xa0,
            UNKNOWN_8 = 0xa6,
            UNKNOWN_9 = 0xa4,
            GET_ROOM = 0x84,
            CREATE_ROOM = 0x86,
            JOIN_ROOM = 0x80,
            JOIN_ROOM_GET_PLAYERS = 0x88,
            USER_JOINED_ROOM = 0x10,
            USER_READY = 0x60,
            LEAVE_ROOM = 0x82,
            CHANGE_MAP = 0xa8,
            START_GAME = 0x61,
            LOADING_GAME_1 = 0x43,
            EQUIPPING = 0xa4,
            END_MATCH = 0xb0,

        }  

    }
}
