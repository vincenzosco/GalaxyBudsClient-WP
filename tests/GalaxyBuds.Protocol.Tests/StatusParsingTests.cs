using System;
using System.Threading.Tasks;
using Galaxy_Buds_App.Bluetooth;
using Galaxy_Buds_App.Models;
using Galaxy_Buds_App.Protocol;
using Galaxy_Buds_App.Protocol.Messages;
using Galaxy_Buds_App.Services;

// Regression tests for the device status parser. Payload layouts follow
// upstream GalaxyBudsClient StatusUpdateDecoder / ExtendedStatusUpdateDecoder.
internal static class StatusParsingTests
{
    private static int _passed;

    private static void Main()
    {
        Run("modern status offsets", ModernStatusOffsets);
        Run("modern status idle is not wearing", ModernStatusIdleNotWearing);
        Run("modern status charging flags", ModernStatusChargingFlags);
        Run("legacy wear left not swapped", LegacyWearLeft);
        Run("legacy wear right not swapped", LegacyWearRight);
        Run("legacy wear both", LegacyWearBoth);
        Run("extended modern offsets", ExtendedModernOffsets);
        Run("extended legacy offsets", ExtendedLegacyOffsets);
        Run("chunked frame delivery", ChunkedDelivery);
        Run("short payload is ignored", ShortPayloadIgnored);
        Run("garbage input does not throw", GarbageInput);
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

    private sealed class FakeTransport : IBluetoothTransport
    {
        public event EventHandler<TransportConnectionChangedEventArgs> ConnectionChanged;
        public event EventHandler<DeviceDiscoveredEventArgs> DeviceDiscovered;
        public event EventHandler<byte[]> DataReceived;

        public TransportState State { get { return TransportState.Connected; } }

        public Task DiscoverAsync() { return Task.FromResult(0); }
        public Task ConnectAsync(string deviceId, Guid serviceUuid) { return Task.FromResult(0); }
        public Task WriteAsync(byte[] data) { return Task.FromResult(0); }
        public Task DisconnectAsync() { return Task.FromResult(0); }

        public void RaiseData(byte[] chunk)
        {
            if (DataReceived != null)
                DataReceived(this, chunk);
        }

        public void RaiseConnection(TransportState state, string error)
        {
            if (ConnectionChanged != null)
                ConnectionChanged(this, new TransportConnectionChangedEventArgs(state, error));
        }

        public void RaiseDiscovered(string id, string name, Guid uuid)
        {
            if (DeviceDiscovered != null)
                DeviceDiscovered(this, new DeviceDiscoveredEventArgs(id, name, uuid));
        }
    }

    private static DeviceStatus Feed(BudsModel model, byte msgId, byte[] payload)
    {
        FakeTransport transport = new FakeTransport();
        DeviceSession session = new DeviceSession(transport);
        session.SetDeviceSpec(DeviceSpecCatalog.FindByModel(model));
        SppProtocolKind kind = model == BudsModel.Buds ? SppProtocolKind.Legacy : SppProtocolKind.Modern;
        byte[] frame = SppPacket.Encode(new SppPacket(msgId, SppMessageType.Request, payload), kind);
        transport.RaiseData(frame);
        return session.Status;
    }

    private static void ModernStatusOffsets()
    {
        // StatusUpdate: Revision[0], BatteryL[1], BatteryR[2], Coupled[3],
        // MainConnection[4], Placement[5], BatteryCase[6], Charging[7].
        byte[] payload = { 5, 80, 70, 1, 2, 0x12, 55, 0x00 };
        DeviceStatus status = Feed(BudsModel.Buds2, (byte)MsgIds.StatusUpdated, payload);
        Equal(80, status.BatteryLeft, "battery left");
        Equal(70, status.BatteryRight, "battery right");
        Equal(55, status.BatteryCase, "battery case");
        Equal(true, status.IsLeftWearing, "left wearing (placement 1)");
        Equal(false, status.IsRightWearing, "right idle is not wearing");
    }

    private static void ModernStatusIdleNotWearing()
    {
        // Placement states: Wearing=1, Idle=2, Case=3. Idle/Case must not
        // count as wearing.
        byte[] idle = { 5, 80, 70, 1, 2, 0x22, 55, 0x00 };
        DeviceStatus status = Feed(BudsModel.Buds2, (byte)MsgIds.StatusUpdated, idle);
        Equal(false, status.IsLeftWearing, "idle left is not wearing");
        Equal(false, status.IsRightWearing, "idle right is not wearing");

        byte[] inCase = { 5, 80, 70, 1, 2, 0x33, 55, 0x00 };
        status = Feed(BudsModel.Buds2, (byte)MsgIds.StatusUpdated, inCase);
        Equal(false, status.IsLeftWearing, "case left is not wearing");
        Equal(false, status.IsRightWearing, "case right is not wearing");
    }

    private static void ModernStatusChargingFlags()
    {
        // Charging bits: left=4 (0x10), right=2 (0x04), case=0 (0x01).
        byte[] payload = { 5, 80, 70, 1, 2, 0x11, 55, 0x11 };
        DeviceStatus status = Feed(BudsModel.Buds2, (byte)MsgIds.StatusUpdated, payload);
        Equal(true, status.IsLeftCharging, "left charging");
        Equal(false, status.IsRightCharging, "right not charging");
        Equal(true, status.IsCaseCharging, "case charging");
    }

    private static void LegacyWearLeft()
    {
        // LegacyWearStates: None=0, R=1, L=16, Both=17.
        byte[] payload = { 0, 90, 85, 1, 1, 16 };
        DeviceStatus status = Feed(BudsModel.Buds, (byte)MsgIds.StatusUpdated, payload);
        Equal(true, status.IsLeftWearing, "L=16 means left wearing");
        Equal(false, status.IsRightWearing, "L=16 means right idle");
    }

    private static void LegacyWearRight()
    {
        byte[] payload = { 0, 90, 85, 1, 1, 1 };
        DeviceStatus status = Feed(BudsModel.Buds, (byte)MsgIds.StatusUpdated, payload);
        Equal(false, status.IsLeftWearing, "R=1 means left idle");
        Equal(true, status.IsRightWearing, "R=1 means right wearing");
    }

    private static void LegacyWearBoth()
    {
        byte[] payload = { 0, 90, 85, 1, 1, 17 };
        DeviceStatus status = Feed(BudsModel.Buds, (byte)MsgIds.StatusUpdated, payload);
        Equal(true, status.IsLeftWearing, "Both=17 means left wearing");
        Equal(true, status.IsRightWearing, "Both=17 means right wearing");
    }

    private static void ExtendedModernOffsets()
    {
        // ExtendedStatusUpdate: Revision[0], EarType[1], BatteryL[2],
        // BatteryR[3], Coupled[4], MainConnection[5], Placement[6],
        // BatteryCase[7], AmbientEnabled[8], AmbientVolume[9],
        // AdjustSoundSync[10], EqualizerMode[11], TouchpadLock[12].
        byte[] payload = { 2, 1, 60, 55, 1, 2, 0x11, 40, 1, 3, 1, 4, 1 };
        DeviceStatus status = Feed(BudsModel.Buds2, (byte)MsgIds.ExtendedStatusUpdated, payload);
        Equal(60, status.BatteryLeft, "battery left at offset 2");
        Equal(55, status.BatteryRight, "battery right at offset 3");
        Equal(40, status.BatteryCase, "battery case at offset 7");
        Equal(true, status.IsLeftWearing, "left wearing");
        Equal(true, status.IsRightWearing, "right wearing");
        Equal(true, status.AmbientEnabled, "ambient enabled");
        Equal(3, status.AmbientVolume, "ambient volume");
        Equal(4, status.EqualizerPreset, "equalizer mode");
        Equal(true, status.TouchpadLocked, "touchpad locked");
    }

    private static void ExtendedLegacyOffsets()
    {
        // Legacy extended: Revision[0], EarType[1], BatteryL[2], BatteryR[3],
        // Coupled[4], MainConnection[5], WearState[6], ...
        byte[] payload = { 0, 0, 90, 85, 1, 1, 17, 1, 0, 2, 1, 3, 0 };
        DeviceStatus status = Feed(BudsModel.Buds, (byte)MsgIds.ExtendedStatusUpdated, payload);
        Equal(90, status.BatteryLeft, "battery left at offset 2");
        Equal(85, status.BatteryRight, "battery right at offset 3");
        Equal(true, status.IsLeftWearing, "both wearing");
        Equal(true, status.IsRightWearing, "both wearing");
    }

    private static void ChunkedDelivery()
    {
        byte[] payload = { 5, 80, 70, 1, 2, 0x11, 55, 0x00 };
        FakeTransport transport = new FakeTransport();
        DeviceSession session = new DeviceSession(transport);
        session.SetDeviceSpec(DeviceSpecCatalog.FindByModel(BudsModel.Buds2));
        byte[] frame = SppPacket.Encode(new SppPacket((byte)MsgIds.StatusUpdated, SppMessageType.Request, payload), SppProtocolKind.Modern);
        int half = frame.Length / 2;
        byte[] first = new byte[half];
        byte[] second = new byte[frame.Length - half];
        Array.Copy(frame, 0, first, 0, half);
        Array.Copy(frame, half, second, 0, second.Length);
        transport.RaiseData(first);
        Equal(null, session.Status.BatteryLeft, "no parse before frame is complete");
        transport.RaiseData(second);
        Equal(80, session.Status.BatteryLeft, "parsed after second chunk");
    }

    private static void ShortPayloadIgnored()
    {
        byte[] payload = { 2, 1 };
        DeviceStatus status = Feed(BudsModel.Buds2, (byte)MsgIds.ExtendedStatusUpdated, payload);
        Equal(null, status.BatteryLeft, "short payload keeps status empty");
    }

    private static void GarbageInput()
    {
        FakeTransport transport = new FakeTransport();
        DeviceSession session = new DeviceSession(transport);
        session.SetDeviceSpec(DeviceSpecCatalog.FindByModel(BudsModel.Buds2));
        transport.RaiseData(new byte[] { 0xFD, 0xFF, 0x03, 0x60, 0x00, 0xDD, 0x42 });
        transport.RaiseData(new byte[] { 0x00, 0x11, 0x22, 0x33 });
        transport.RaiseData(new byte[0]);
        Equal(null, session.Status.BatteryLeft, "garbage input keeps status empty");
    }

    private static void Equal(object expected, object actual, string what)
    {
        if (!object.Equals(expected, actual))
            throw new Exception(what + ": expected " + expected + ", got " + actual);
    }
}
