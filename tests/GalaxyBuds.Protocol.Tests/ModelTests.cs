using System;
using Galaxy_Buds_App.Models;
using Galaxy_Buds_App.Protocol;
using Galaxy_Buds_App.Protocol.Messages;

internal static class ModelTests
{
    private static int _passed;

    private static void Main()
    {
        Run("model catalog returns known specs", ModelCatalog);
        Run("unknown model name returns null", UnknownModel);
        Run("legacy Buds uses FE framing", LegacyProtocol);
        Run("modern models use FD framing", ModernProtocol);
        Run("equalizer Buds legacy encoding", EqualizerBuds);
        Run("equalizer modern encoding", EqualizerModern);
        Run("ambient volume clamps to max", AmbientVolumeClamp);
        Run("lock touchpad encoding", LockTouchpad);
        Run("find my earbuds start/stop", FindMyEarbuds);
        Run("reset encoding", ResetCommand);
        Run("firmware revision gating", FirmwareGating);
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
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL " + name + ": " + ex.Message);
            Environment.ExitCode = 1;
        }
    }

    private static void ModelCatalog()
    {
        Assert(DeviceSpecCatalog.FindByModel(BudsModel.Buds) != null, "Buds spec exists");
        Assert(DeviceSpecCatalog.FindByModel(BudsModel.Buds2Pro) != null, "Buds2Pro spec exists");
        Assert(DeviceSpecCatalog.FindByModel(BudsModel.Buds3Pro) != null, "Buds3Pro spec exists");
        Assert(DeviceSpecCatalog.FindByModel(BudsModel.Unknown) == null, "Unknown model has no spec");
    }

    private static void UnknownModel()
    {
        Assert(DeviceSpecCatalog.FindByDeviceName("") == null, "Empty name returns null");
        Assert(DeviceSpecCatalog.FindByDeviceName(null) == null, "Null name returns null");
        Assert(DeviceSpecCatalog.FindByDeviceName("Random Device") == null, "Random name returns null");
    }

    private static void LegacyProtocol()
    {
        DeviceSpec spec = DeviceSpecCatalog.FindByModel(BudsModel.Buds);
        Assert(spec.StartOfMessage == 0xFE, "Buds uses legacy SOM");
        Assert(spec.EndOfMessage == 0xEE, "Buds uses legacy EOM");
    }

    private static void ModernProtocol()
    {
        DeviceSpec spec = DeviceSpecCatalog.FindByModel(BudsModel.Buds2);
        Assert(spec.StartOfMessage == 0xFD, "Buds2 uses modern SOM");
        Assert(spec.EndOfMessage == 0xDD, "Buds2 uses modern EOM");
    }

    private static void EqualizerBuds()
    {
        byte[] frame = CommandEncoder.EncodeEqualizer(BudsModel.Buds, true, 3);
        Assert(frame[0] == 0xFE, "Legacy frame start");
        Assert(frame[3] == (byte)MsgIds.Equalizer, "EQ message ID");
        Assert(frame[4] == 1, "EQ enabled");
        Assert(frame[5] == 8, "Preset 3 maps to 8 for Buds (3+5)");
    }

    private static void EqualizerModern()
    {
        byte[] frame = CommandEncoder.EncodeEqualizer(BudsModel.Buds2, true, 2);
        Assert(frame[0] == 0xFD, "Modern frame start");
        Assert(frame[3] == (byte)MsgIds.Equalizer, "EQ message ID");
        Assert(frame[4] == 3, "Preset 2 maps to 3 for modern (2+1)");
    }

    private static void AmbientVolumeClamp()
    {
        byte[] frame = CommandEncoder.EncodeAmbientVolume(BudsModel.Buds2, 99);
        int vol = frame[4] - 1;
        Assert(vol == 2, "Volume clamped to 2 for Buds2");
    }

    private static void LockTouchpad()
    {
        byte[] frame = CommandEncoder.EncodeLockTouchpad(BudsModel.Buds2, true);
        Assert(frame[3] == (byte)MsgIds.LockTouchpad, "Lock message ID");
        Assert(frame[4] == 0, "LockAll sends 0 for first byte");
    }

    private static void FindMyEarbuds()
    {
        byte[] start = CommandEncoder.EncodeFindMyEarbuds(BudsModel.Buds2, true);
        Assert(start[3] == (byte)MsgIds.FindMyEarbudsStart, "Start ID");
        byte[] stop = CommandEncoder.EncodeFindMyEarbuds(BudsModel.Buds2, false);
        Assert(stop[3] == (byte)MsgIds.FindMyEarbudsStop, "Stop ID");
    }

    private static void ResetCommand()
    {
        byte[] frame = CommandEncoder.EncodeReset(BudsModel.Buds2);
        Assert(frame[3] == (byte)MsgIds.Reset, "Reset ID");
    }

    private static void FirmwareGating()
    {
        DeviceSpec plus = DeviceSpecCatalog.FindByModel(BudsModel.BudsPlus);
        Assert(!plus.SupportsFirmwareUpdates(7), "Buds+ rev 7 blocked");
        Assert(plus.SupportsFirmwareUpdates(8), "Buds+ rev 8 allowed");
        DeviceSpec pro = DeviceSpecCatalog.FindByModel(BudsModel.BudsPro);
        Assert(pro.SupportsFirmwareUpdates(0), "BudsPro rev 0 allowed");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
