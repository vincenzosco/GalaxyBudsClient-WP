using System;
using Galaxy_Buds_App.Models;
using Galaxy_Buds_App.Protocol;

namespace Galaxy_Buds_App.Protocol.Messages
{
    // Command IDs derived from upstream GalaxyBudsClient (GPL-3.0).
    // See docs/PROTOCOL-CREDITS.md for source attribution.
    public enum MsgIds : byte
    {
        Equalizer = 134,
        SetAmbientMode = 128,
        AmbientVolume = 132,
        LockTouchpad = 144,
        SetTouchpadOption = 146,
        SetTouchAndHoldNoiseControls = 121,
        FindMyEarbudsStart = 160,
        FindMyEarbudsStop = 161,
        MuteEarbud = 162,
        StatusUpdated = 96,
        ExtendedStatusUpdated = 97,
        ManagerInfo = 136,
        UpdateTime = 167,
        Reset = 80,
        VersionInfo = 99
    }

    public static class CommandEncoder
    {
        public static byte[] EncodeEqualizer(BudsModel model, bool enabled, int preset)
        {
            byte[] payload;
            if (model == BudsModel.Buds)
            {
                int rawPreset = enabled ? preset + 5 : 0;
                payload = new byte[] { enabled ? (byte)1 : (byte)0, (byte)rawPreset };
            }
            else
            {
                payload = new byte[] { enabled ? (byte)(preset + 1) : (byte)0 };
            }
            return SppPacket.Encode(new SppPacket((byte)MsgIds.Equalizer, SppMessageType.Request, payload), GetProtocol(model));
        }

        public static byte[] EncodeAmbientMode(BudsModel model, bool enabled)
        {
            return SppPacket.Encode(new SppPacket((byte)MsgIds.SetAmbientMode, SppMessageType.Request,
                new byte[] { enabled ? (byte)1 : (byte)0 }), GetProtocol(model));
        }

        public static byte[] EncodeAmbientVolume(BudsModel model, int volume)
        {
            if (volume < 1)
                volume = 1;
            DeviceSpec spec = DeviceSpecCatalog.FindByModel(model);
            if (spec != null && volume > spec.MaximumAmbientVolume)
                volume = spec.MaximumAmbientVolume;
            return SppPacket.Encode(new SppPacket((byte)MsgIds.AmbientVolume, SppMessageType.Request,
                new byte[] { (byte)(volume + 1) }), GetProtocol(model));
        }

        public static byte[] EncodeLockTouchpad(BudsModel model, bool lockAll)
        {
            byte[] payload = new byte[] { lockAll ? (byte)0 : (byte)1, (byte)1, (byte)1, (byte)1, (byte)1 };
            return SppPacket.Encode(new SppPacket((byte)MsgIds.LockTouchpad, SppMessageType.Request, payload), GetProtocol(model));
        }

        public static byte[] EncodeFindMyEarbuds(BudsModel model, bool start)
        {
            byte id = start ? (byte)MsgIds.FindMyEarbudsStart : (byte)MsgIds.FindMyEarbudsStop;
            return SppPacket.Encode(new SppPacket(id, SppMessageType.Request, new byte[0]), GetProtocol(model));
        }

        public static byte[] EncodeReset(BudsModel model)
        {
            return SppPacket.Encode(new SppPacket((byte)MsgIds.Reset, SppMessageType.Request, new byte[0]), GetProtocol(model));
        }

        public static byte[] EncodeManagerInfo(BudsModel model)
        {
            return SppPacket.Encode(new SppPacket((byte)MsgIds.ManagerInfo, SppMessageType.Request,
                new byte[] { 1, 2, 33 }), GetProtocol(model));
        }

        private static SppProtocolKind GetProtocol(BudsModel model)
        {
            DeviceSpec spec = DeviceSpecCatalog.FindByModel(model);
            if (spec != null && spec.StartOfMessage == 0xFE)
                return SppProtocolKind.Legacy;
            return SppProtocolKind.Modern;
        }
    }
}
