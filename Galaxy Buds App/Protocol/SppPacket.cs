using System;

namespace Galaxy_Buds_App.Protocol
{
    public enum SppProtocolKind
    {
        Legacy,
        Modern
    }

    public enum SppMessageType : byte
    {
        Request = 0,
        Response = 1
    }

    public sealed class SppPacket
    {
        public SppPacket(byte messageId, SppMessageType messageType, byte[] payload)
        {
            if (payload == null)
                throw new ArgumentNullException("payload");

            MessageId = messageId;
            MessageType = messageType;
            Payload = (byte[])payload.Clone();
        }

        public byte MessageId { get; private set; }
        public SppMessageType MessageType { get; private set; }
        public byte[] Payload { get; private set; }

        public static byte[] Encode(SppPacket packet, SppProtocolKind protocol)
        {
            if (packet == null)
                throw new ArgumentNullException("packet");
            if (packet.MessageType != SppMessageType.Request && packet.MessageType != SppMessageType.Response)
                throw new ArgumentOutOfRangeException("packet");

            int size = packet.Payload.Length + 3;
            if (size > 1023)
                throw new ArgumentOutOfRangeException("packet", "The Buds message exceeds the protocol size limit.");

            bool legacy = protocol == SppProtocolKind.Legacy;
            byte[] encoded = new byte[size + (legacy ? 4 : 4)];
            encoded[0] = legacy ? (byte)0xFE : (byte)0xFD;

            if (legacy)
            {
                encoded[1] = (byte)packet.MessageType;
                encoded[2] = (byte)size;
                encoded[3] = packet.MessageId;
                Array.Copy(packet.Payload, 0, encoded, 4, packet.Payload.Length);
                WriteChecksum(encoded, 3, packet.Payload.Length, encoded.Length - 3);
                encoded[encoded.Length - 1] = 0xEE;
            }
            else
            {
                int header = size | (packet.MessageType == SppMessageType.Response ? 0x1000 : 0);
                encoded[1] = (byte)(header & 0xFF);
                encoded[2] = (byte)((header >> 8) & 0xFF);
                encoded[3] = packet.MessageId;
                Array.Copy(packet.Payload, 0, encoded, 4, packet.Payload.Length);
                WriteChecksum(encoded, 3, packet.Payload.Length, encoded.Length - 3);
                encoded[encoded.Length - 1] = 0xDD;
            }

            return encoded;
        }

        public static SppPacket Decode(byte[] data, SppProtocolKind protocol)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length < 7)
                throw new FormatException("The SPP frame is truncated.");

            bool legacy = protocol == SppProtocolKind.Legacy;
            byte start = legacy ? (byte)0xFE : (byte)0xFD;
            byte end = legacy ? (byte)0xEE : (byte)0xDD;
            if (data[0] != start)
                throw new FormatException("The SPP start marker is invalid.");

            int size;
            SppMessageType type;
            int messageOffset;
            if (legacy)
            {
                if (data[1] > 1)
                    throw new FormatException("The legacy SPP message type is invalid.");
                type = (SppMessageType)data[1];
                size = data[2];
                messageOffset = 3;
            }
            else
            {
                int header = data[1] | (data[2] << 8);
                size = header & 0x03FF;
                type = (header & 0x1000) != 0 ? SppMessageType.Response : SppMessageType.Request;
                if ((header & 0xC000) != 0)
                    throw new FormatException("The modern SPP frame uses unsupported header flags.");
                if ((header & 0x2000) != 0)
                    throw new FormatException("Fragmented SPP messages are not supported by this client.");
                messageOffset = 3;
            }

            if (size < 3)
                throw new FormatException("The SPP message length is invalid.");
            if (size + 4 != data.Length)
                throw new FormatException("The SPP frame length does not match its header.");
            if (data[data.Length - 1] != end)
                throw new FormatException("The SPP end marker is invalid.");

            int payloadLength = size - 3;
            byte[] payload = new byte[payloadLength];
            Array.Copy(data, messageOffset + 1, payload, 0, payloadLength);
            ushort actual = (ushort)(data[data.Length - 3] | (data[data.Length - 2] << 8));
            ushort expected = Crc16Ccitt.ComputeMessage(data[messageOffset], payload);
            if (actual != expected)
                throw new FormatException("The SPP checksum is invalid.");

            return new SppPacket(data[messageOffset], type, payload);
        }

        private static void WriteChecksum(byte[] encoded, int messageOffset, int payloadLength, int count)
        {
            ushort checksum = Crc16Ccitt.Compute(encoded, messageOffset, payloadLength + 1);
            int checksumOffset = messageOffset + payloadLength + 1;
            encoded[checksumOffset] = (byte)(checksum & 0xFF);
            encoded[checksumOffset + 1] = (byte)(checksum >> 8);
        }
    }

    public static class Crc16Ccitt
    {
        public static ushort Compute(byte[] data, int offset, int count)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (offset < 0 || count < 0 || offset > data.Length - count)
                throw new ArgumentOutOfRangeException("count");

            int crc = 0;
            for (int i = offset; i < offset + count; i++)
            {
                crc = Table[((crc >> 8) ^ data[i]) & 0xFF] ^ (crc << 8);
            }
            return (ushort)(crc & 0xFFFF);
        }

        public static ushort ComputeMessage(byte messageId, byte[] payload)
        {
            if (payload == null)
                throw new ArgumentNullException("payload");
            byte[] bytes = new byte[payload.Length + 1];
            bytes[0] = messageId;
            Array.Copy(payload, 0, bytes, 1, payload.Length);
            return Compute(bytes, 0, bytes.Length);
        }

        private static readonly ushort[] Table = new ushort[]
        {
            0x0000,0x1021,0x2042,0x3063,0x4084,0x50A5,0x60C6,0x70E7,0x8108,0x9129,0xA14A,0xB16B,0xC18C,0xD1AD,0xE1CE,0xF1EF,
            0x1231,0x0210,0x3273,0x2252,0x52B5,0x4294,0x72F7,0x62D6,0x9339,0x8318,0xB37B,0xA35A,0xD3BD,0xC39C,0xF3FF,0xE3DE,
            0x2462,0x3443,0x0420,0x1401,0x64E6,0x74C7,0x44A4,0x5485,0xA56A,0xB54B,0x8528,0x9509,0xE5EE,0xF5CF,0xC5AC,0xD58D,
            0x3653,0x2672,0x1611,0x0630,0x76D7,0x66F6,0x5695,0x46B4,0xB75B,0xA77A,0x9719,0x8738,0xF7DF,0xE7FE,0xD79D,0xC7BC,
            0x48C4,0x58E5,0x6886,0x78A7,0x0840,0x1861,0x2802,0x3823,0xC9CC,0xD9ED,0xE98E,0xF9AF,0x8948,0x9969,0xA90A,0xB92B,
            0x5AF5,0x4AD4,0x7AB7,0x6A96,0x1A71,0x0A50,0x3A33,0x2A12,0xDBFD,0xCBDC,0xFBBF,0xEB9E,0x9B79,0x8B58,0xBB3B,0xAB1A,
            0x6CA6,0x7C87,0x4CE4,0x5CC5,0x2C22,0x3C03,0x0C60,0x1C41,0xEDAE,0xFD8F,0xCDEC,0xDDCD,0xAD2A,0xBD0B,0x8D68,0x9D49,
            0x7E97,0x6EB6,0x5ED5,0x4EF4,0x3E13,0x2E32,0x1E51,0x0E70,0xFF9F,0xEFBE,0xDFDD,0xCFFC,0xBF1B,0xAF3A,0x9F59,0x8F78,
            0x9188,0x81A9,0xB1CA,0xA1EB,0xD10C,0xC12D,0xF14E,0xE16F,0x1080,0x00A1,0x30C2,0x20E3,0x5004,0x4025,0x7046,0x6067,
            0x83B9,0x9398,0xA3FB,0xB3DA,0xC33D,0xD31C,0xE37F,0xF35E,0x02B1,0x1290,0x22F3,0x32D2,0x4235,0x5214,0x6277,0x7256,
            0xB5EA,0xA5CB,0x95A8,0x8589,0xF56E,0xE54F,0xD52C,0xC50D,0x34E2,0x24C3,0x14A0,0x0481,0x7466,0x6447,0x5424,0x4405,
            0xA7DB,0xB7FA,0x8799,0x97B8,0xE75F,0xF77E,0xC71D,0xD73C,0x26D3,0x36F2,0x0691,0x16B0,0x6657,0x7676,0x4615,0x5634,
            0xD94C,0xC96D,0xF90E,0xE92F,0x99C8,0x89E9,0xB98A,0xA9AB,0x5844,0x4865,0x7806,0x6827,0x18C0,0x08E1,0x3882,0x28A3,
            0xCB7D,0xDB5C,0xEB3F,0xFB1E,0x8BF9,0x9BD8,0xABBB,0xBB9A,0x4A75,0x5A54,0x6A37,0x7A16,0x0AF1,0x1AD0,0x2AB3,0x3A92,
            0xFD2E,0xED0F,0xDD6C,0xCD4D,0xBDAA,0xAD8B,0x9DE8,0x8DC9,0x7C26,0x6C07,0x5C64,0x4C45,0x3CA2,0x2C83,0x1CE0,0x0CC1,
            0xEF1F,0xFF3E,0xCF5D,0xDF7C,0xAF9B,0xBFBA,0x8FD9,0x9FF8,0x6E17,0x7E36,0x4E55,0x5E74,0x2E93,0x3EB2,0x0ED1,0x1EF0
        };
    }
}
