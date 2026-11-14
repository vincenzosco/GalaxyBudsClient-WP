using System;
using Galaxy_Buds_App.Protocol;

internal static class ProtocolTests
{
    private static int _passed;

    private static void Main()
    {
        Run("CRC16-CCITT check vector", CrcCheckVector);
        Run("legacy packet round trip", LegacyRoundTrip);
        Run("modern packet round trip", ModernRoundTrip);
        Run("CRC mismatch rejected", RejectBadChecksum);
        Run("truncated packet rejected", RejectTruncatedPacket);
        Run("unknown start marker rejected", RejectBadStartMarker);
        Run("invalid message length rejected", RejectInvalidLength);
        Console.WriteLine("Passed: " + _passed);
    }

    private static void Run(string name, Action test)
    {
        try
        {
            test();
            _passed++;
            Console.WriteLine("PASS " + name);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine("FAIL " + name + ": " + exception.Message);
            Environment.ExitCode = 1;
        }
    }

    private static void CrcCheckVector()
    {
        Equal((ushort)0x31C3, Crc16Ccitt.Compute(new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, 0, 9));
    }

    private static void LegacyRoundTrip()
    {
        var original = new SppPacket(0x61, SppMessageType.Request, new byte[] { 0x02, 0x00, 0x4B });
        var bytes = SppPacket.Encode(original, SppProtocolKind.Legacy);
        Equal((byte)0xFE, bytes[0]);
        Equal((byte)0x00, bytes[1]);
        Equal((byte)0x06, bytes[2]);
        Equal(10, bytes.Length);
        var decoded = SppPacket.Decode(bytes, SppProtocolKind.Legacy);
        Equal(original.MessageId, decoded.MessageId);
        Equal(original.MessageType, decoded.MessageType);
        Bytes(original.Payload, decoded.Payload);
    }

    private static void ModernRoundTrip()
    {
        var original = new SppPacket(0x61, SppMessageType.Request, new byte[] { 0x02, 0x00, 0x4B });
        var bytes = SppPacket.Encode(original, SppProtocolKind.Modern);
        Equal((byte)0xFD, bytes[0]);
        var decoded = SppPacket.Decode(bytes, SppProtocolKind.Modern);
        Equal(original.MessageId, decoded.MessageId);
        Equal(original.MessageType, decoded.MessageType);
        Bytes(original.Payload, decoded.Payload);
    }

    private static void RejectBadChecksum()
    {
        var bytes = SppPacket.Encode(new SppPacket(0x61, SppMessageType.Request, new byte[] { 0x02 }), SppProtocolKind.Legacy);
        bytes[4] ^= 0x01;
        Throws<FormatException>(delegate { SppPacket.Decode(bytes, SppProtocolKind.Legacy); });
    }

    private static void RejectTruncatedPacket()
    {
        Throws<FormatException>(delegate { SppPacket.Decode(new byte[] { 0xFE, 0x00, 0x03 }, SppProtocolKind.Legacy); });
    }

    private static void RejectBadStartMarker()
    {
        Throws<FormatException>(delegate { SppPacket.Decode(new byte[] { 0x00, 0x00, 0x03, 0x61, 0, 0, 0, 0xEE }, SppProtocolKind.Legacy); });
    }

    private static void RejectInvalidLength()
    {
        Throws<FormatException>(delegate { SppPacket.Decode(new byte[] { 0xFE, 0x00, 0x02, 0x61, 0, 0, 0xEE }, SppProtocolKind.Legacy); });
    }

    private static void Equal<T>(T expected, T actual)
    {
        if (!Object.Equals(expected, actual))
            throw new InvalidOperationException("Expected " + expected + ", got " + actual);
    }

    private static void Bytes(byte[] expected, byte[] actual)
    {
        if (expected.Length != actual.Length)
            throw new InvalidOperationException("Byte array lengths differ");
        for (int i = 0; i < expected.Length; i++)
            Equal(expected[i], actual[i]);
    }

    private static void Throws<T>(Action action) where T : Exception
    {
        try
        {
            action();
        }
        catch (T)
        {
            return;
        }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }
}
