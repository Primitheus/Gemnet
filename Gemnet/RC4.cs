using System;

namespace Gemnet.Security
{
    public class RC4
    {
        private readonly int[] sBox;

        public RC4(string key)
        {
            sBox = InitializeSBox(key);
        }

        private int[] InitializeSBox(string key)
        {
            int[] S = new int[1029]; // S[0-255] + [0x400] + [0x404]
            int keyLength = key.Length;
            int j = 0;

            // Fill S-box with identity permutation
            for (int i = 0; i < 256; i++)
                S[i] = i;

            // Key Scheduling Algorithm (KSA)
            for (int i = 0; i < 256; i++)
            {
                j = (j + S[i] + key[i % keyLength]) % 256;
                (S[i], S[j]) = (S[j], S[i]);
            }

            // Initialize state registers
            S[0x400] = 0;
            S[0x404] = 0;

            return S;
        }

        public byte[] Encrypt(byte[] data)
        {
            int state1 = sBox[0x400];
            int state2 = sBox[0x404];

            byte[] result = new byte[data.Length];

            for (int index = 0; index < data.Length; index++)
            {
                state1 = (state1 + 1) & 0xFF;
                int temp1 = sBox[state1];
                int temp2 = temp1 & 0xFF;
                state2 = (state2 + temp2) & 0xFF;
                int temp3 = sBox[state2];

                // Swap S[state1] and S[state2]
                sBox[state1] = temp3;
                sBox[state2] = temp1;

                result[index] = (byte)(data[index] ^ sBox[(temp3 + temp2) & 0xFF]);
            }

            sBox[0x400] = state1;
            sBox[0x404] = state2;

            return result;
        }

        public byte[] Encrypt(byte[] data, int bytesRead)
        {
            int state1 = sBox[0x400];
            int state2 = sBox[0x404];

            byte[] result = new byte[bytesRead];

            for (int index = 0; index < bytesRead; index++)
            {
                state1 = (state1 + 1) & 0xFF;
                int temp1 = sBox[state1];
                int temp2 = temp1 & 0xFF;
                state2 = (state2 + temp2) & 0xFF;
                int temp3 = sBox[state2];

                // Swap S[state1] and S[state2]
                sBox[state1] = temp3;
                sBox[state2] = temp1;

                result[index] = (byte)(data[index] ^ sBox[(temp3 + temp2) & 0xFF]);
            }

            sBox[0x400] = state1;
            sBox[0x404] = state2;

            return result;
        }


        public byte[] Decrypt(byte[] data, int bytesRead)
        {
            // RC4 encryption and decryption are the same
            return Encrypt(data, bytesRead);
        }
    }


    public class ClientCipherState
    {
        public RC4 Encryptor { get; set; }
        public RC4 Decryptor { get; set; }

        public ClientCipherState(string key)
        {
            Encryptor = new RC4(key);
            Decryptor = new RC4(key);
        }
    }



}
