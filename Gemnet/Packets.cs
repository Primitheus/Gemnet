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
            GUILD = 0x33,
            UNKNOWN_2 = 0x0A,
            UNKNOWN_3 = 0x41,
            UNKNOWN_4 = 0x1f,
        }

        public enum Guild
        {
            UNKNOWN_A = 0x80,
            GET_GUILD_MARK = 0x82,
            UNKNOWN_B = 0x92,
            UNKNOWN_C = 0x96,
            IS_USER_GUILD_MEMEBER = 0x94,
            GET_GUILD_RANK = 0x90,
            GET_GUILD_MEMBER = 0x84,
            GET_GUILD_GREETINGS = 0x8A,
            GET_GUILD_RANK_LIST = 0x8E,
            

        }

        public enum Unknown3 
        {
            UNKNOWN_A = 0x40,
            UNKNOWN_B = 0x70,
            UNKNOWN_C = 0x84,
            UNKNOWN_D = 0x82,
            UNKNOWN_E = 0x88,
            UNKNOWN_F = 0xc2,
            UNKNOWN_G = 0xc6,
            UNKNOWN_H = 0xca,
            UNKNOWN_I = 0x8e,
            UNKNOWN_J = 0xc4,
            UNKNOWN_K = 0xa2,
            UNKNOWN_L = 0x86,
            UNKNOWN_M = 0x80,
            UNKNOWN_N = 0x42,
            UNKNOWN_O = 0x46,
            UNKNOWN_P = 0x40,
            UNKNOWN_Q = 0x41,
            UNKNOWN_R = 0x44,

        }
        public enum Unknown4 
        {
            UNKNOWN_A = 0x10,

        }

        public enum ActionLogin
        {
            // LOGIN (0x10)
            VERSION_CHECK = 0x90,
            SERVER_TIME = 0x92,
            CRED_CHECK = 0x84,
            CREATE_ACCOUNT = 0x88,
            CASH_UNKNOWN = 0xd0,
            BUDDY_LIST = 0xa8,
            TO_LOBBY = 0xb6,
            SHORT_MSG = 0x40,
            DELETE_BUDDY = 0xae,
            AGREE_BUDDY = 0xac,
            SET_OPTION_INV = 0xd2,
            UNKNOWN_E = 0xc3,
            UNKNOWN_F = 0xcd,
            UNKNOWN_G = 0xc9,
            UNKNOWN_H = 0xcb,
            UNKNOWN_I = 0xc1,
            UNKNOWN_J = 0x43,
            UNKNOWN_K = 0xcf,
            UNKNOWN_L = 0xc7,
            UNKNOWN_M = 0x9c,
            UNKNOWN_N = 0x96,
            UNKNOWN_O = 0x98,
            ENCHANT = 0xbe,
            UNKNOWN_Q = 0xaa,
            UNKNOWN_R = 0xb8,
            UNKNOWN_S = 0x8b,
            UNKNOWN_T = 0xb4,
            UNKNOWN_U = 0x44,
            
        }

        public enum ActionGG
        {
            // GAMEGUARD (0x48)
            GAMEGUARD_START = 0x80,
            ANHS_RESPONSE = 0x82,
        }
        
        public enum ActionGeneral
        {
            // GENERAL (0x30)
            GET_PROPERTY = 0x84,
            GIFT_FROM = 0x92,
            GIFT_TO = 0x90,
            UNKNOWN_1 = 0x86,
            UNKNOWN_2 = 0xb4,
            UNKNOWN_3 = 0xb2, // LOGIN_COMPLETE 
            UNKNOWN_4 = 0xba, // GET_RUNESTONE_INFO
            AVATAR_LIST = 0x88,
            GET_GRADE = 0xa0,
            TO_INFO = 0xbc, //GET_BUFF_INFO_CHECK
            QUESTS = 0xcc, // REQUEST_QUEST_SERVER
            BUYING = 0xce, // REQUEST_UPDATE_QUEST
            BEGIN_MATCH = 0x80, // GET_USER_INFO
            EQUIP_ITEM = 0xd6,
            JOIN_ROOM_BS = 0xa0,
            GET_ZM_STATS = 0xd4,
            GET_ZM_STATS_2 = 0xd2,
            CHAT = 0xD8,
            CHANGE_RESELL = 0xa8,
            SET_RESELL = 0xa6,
            GET_RESELL_PWD_STATE = 0xaa,
            STONE_CHECK = 0xbe,
            BUFF_UNUSABLE = 0xc2,
            RUNE_USE_CHECK = 0xc0,
            UNKNOWN_A = 0xc6,
            SET_INTRODUCTION = 0xb8, 
            IS_VALID_USER = 0x94,
            GET_USER_INFO_RENEWAL = 0xb6,
            START_USE_ITEM = 0x8e,
            UNKNOWN_F = 0xca,

        }

        public enum ActionInventory
        {
            // INVENTORY (0x31)
            OLD_RF = 0x8c,
            CASH = 0x88,
            ADD_ITEM = 0x8a, // Daily Login?
            BUY_ITEM = 0x80,
            OPEN_BOX = 0x9a,
            UNKNOWN_10 = 0xc6,
            UNKNOWN_A = 0x96,
            UNKNOWN_B = 0x94,
            UNKNOWN_C = 0xa8,
            UNKNOWN_D = 0xae,
            UNKNOWN_E = 0xac,
            UNKNOWN_F = 0xb4,
            UNKNOWN_G = 0xcc,
            UNKNOWN_H = 0xd6,
            UNKNOWN_I = 0xce,
            UNKNOWN_J = 0xd0,
            UNKNOWN_K = 0xd2,
            UNKNOWN_L = 0xa0,
            UNKNOWN_M = 0xb2,
            UNKNOWN_N= 0xd8,
            UNKNOWN_O = 0x82,
            UNKNOWN_P = 0xc2,
            SELL_ITEM = 0x84,
            RECALL_ITEM = 0x9e,
            GIVE_ITEM = 0xc0,
            UNKNOWN_Q = 0xc4,
            UNKNOWN_R = 0xd4,
            WIN_MEDAL = 0xb6,
            UNKNOWN_T = 0xb0,
            REQUEST_QUESTITEM = 0xca,
            //GRADE_UP = 0x8c,
            UNKNOWN_W = 0x8e,
            UNPACK = 0x86,
            UNKNOWN_Y = 0xc4,
            UNKNOWN_Z = 0xc4,

            ENCHANT = 0x92,

        }

        public enum ActionQuery
        {
            // QUERY (0x40)
            CHANGE_AVATAR = 0xa2,
            CHANGE_AI_INFO = 0xb6,
            CHANGE_HOST = 0x42,
            EQUIPPED_AVATAR = 0xa0,
            CLEAR_AVATAR_SLOT = 0xa6, // Equip Item
            UPDATE_AVATAR = 0xa4,
            GET_ROOM = 0x84,
            EXIT_ROOM = 0x82,
            FIND_ROOM = 0x8e,
            CREATE_ROOM = 0x86,
            JOIN_ROOM = 0x80,
            JOIN_ROOM_GET_PLAYERS = 0x88,
            USER_JOINED_ROOM = 0x10,
            USER_READY = 0x60,
            SELECT_TEAM = 0x45,
            LEAVE_ROOM = 0x82,
            CHANGE_MAP = 0xa8,
            START_GAME = 0x61,
            LOADING_GAME_1 = 0x43,
            LOADING_GAME_2 = 0x62,
            END_MATCH = 0xb0,
            FIN = 0xaa,
            MATCH_REWARD = 0x68,
            CHAT = 0x40,
            INVITE = 0x46,
            UNKNOWN_A = 0xb2,
            UNKNOWN_B = 0x61,
            UNKNOWN_C = 0x41,
            USE_ITEM_WRITE = 0x64,


        }  

    }
}
