using System;
using System.Collections.Generic;

namespace Galaxy_Buds_App.Models
{
    public enum BudsModel
    {
        Unknown = 0,
        Buds = 1,
        BudsPlus = 2,
        BudsLive = 3,
        BudsPro = 4,
        Buds2 = 5,
        Buds2Pro = 6,
        BudsFe = 7,
        BudsCore = 8,
        Buds3 = 9,
        Buds3Pro = 10,
        Buds3Fe = 11,
        Buds4 = 12,
        Buds4Pro = 13
    }

    public enum BudsFeature
    {
        CaseBattery,
        BatteryType,
        AmbientSound,
        AmbientSoundVolume,
        AmbientVoiceFocus,
        AmbientSidetone,
        AmbientCustomize,
        Anc,
        NoiseControl,
        NoiseControlModeDualSide,
        Equalizer,
        CustomEqualizer,
        TouchpadLock,
        TouchpadAdvancedLock,
        TouchpadOptions,
        FindMyEarbuds,
        FitTest,
        FirmwareUpdates,
        Rename,
        UsageReport,
        BuildInfo,
        SpatialSensor,
        HeadTracking,
        GamingMode,
        SmartThingsFind
    }

    public sealed class DeviceSpec
    {
        private readonly HashSet<BudsFeature> _features;
        private readonly int _minimumRevisionForFirmwareUpdates;

        public BudsModel Model { get; private set; }
        public string DisplayName { get; private set; }
        public string DeviceBaseName { get; private set; }
        public Guid ServiceUuid { get; private set; }
        public byte StartOfMessage { get; private set; }
        public byte EndOfMessage { get; private set; }
        public int MaximumAmbientVolume { get; private set; }

        public DeviceSpec(BudsModel model, string displayName, string deviceBaseName,
            Guid serviceUuid, byte startOfMessage, byte endOfMessage,
            int maximumAmbientVolume, BudsFeature[] features,
            int minimumRevisionForFirmwareUpdates = int.MaxValue)
        {
            Model = model;
            DisplayName = displayName;
            DeviceBaseName = deviceBaseName;
            ServiceUuid = serviceUuid;
            StartOfMessage = startOfMessage;
            EndOfMessage = endOfMessage;
            MaximumAmbientVolume = maximumAmbientVolume;
            _features = new HashSet<BudsFeature>(features);
            _minimumRevisionForFirmwareUpdates = minimumRevisionForFirmwareUpdates;
        }

        public bool Supports(BudsFeature feature)
        {
            return _features.Contains(feature);
        }

        public bool SupportsFirmwareUpdates(int revision)
        {
            return revision >= _minimumRevisionForFirmwareUpdates;
        }
    }

    public static class DeviceSpecCatalog
    {
        public static readonly Guid SppLegacy = new Guid("00001102-0000-1000-8000-00805f9b34fd");
        public static readonly Guid SppStandard = new Guid("00001101-0000-1000-8000-00805F9B34FB");
        public static readonly Guid SppNew = new Guid("2e73a4ad-332d-41fc-90e2-16bef06523f2");

        // All SPP service UUIDs used by Galaxy Buds models, for discovery.
        public static readonly Guid[] ServiceUuids = new Guid[] { SppLegacy, SppStandard, SppNew };

        private static readonly DeviceSpec[] Specs = new DeviceSpec[]
        {
            new DeviceSpec(BudsModel.Buds, "Galaxy Buds", "Galaxy Buds (", SppLegacy, 0xFE, 0xEE, 4,
                new BudsFeature[] { BudsFeature.AmbientSound, BudsFeature.AmbientSoundVolume, BudsFeature.AmbientVoiceFocus, BudsFeature.Equalizer, BudsFeature.TouchpadLock, BudsFeature.TouchpadOptions, BudsFeature.FindMyEarbuds, BudsFeature.BatteryType, BudsFeature.BuildInfo, BudsFeature.UsageReport }),

            new DeviceSpec(BudsModel.BudsPlus, "Galaxy Buds+", "Galaxy Buds+ (", SppStandard, 0xFD, 0xDD, 2,
                new BudsFeature[] { BudsFeature.AmbientSound, BudsFeature.AmbientSoundVolume, BudsFeature.AmbientSidetone, BudsFeature.CaseBattery, BudsFeature.BatteryType, BudsFeature.Equalizer, BudsFeature.TouchpadLock, BudsFeature.TouchpadOptions, BudsFeature.FindMyEarbuds, BudsFeature.BuildInfo, BudsFeature.FirmwareUpdates, BudsFeature.UsageReport, BudsFeature.GamingMode },
                8),

            new DeviceSpec(BudsModel.BudsLive, "Galaxy Buds Live", "Buds Live", SppStandard, 0xFD, 0xDD, 0,
                new BudsFeature[] { BudsFeature.Anc, BudsFeature.CaseBattery, BudsFeature.Equalizer, BudsFeature.TouchpadLock, BudsFeature.TouchpadOptions, BudsFeature.FindMyEarbuds, BudsFeature.FirmwareUpdates, BudsFeature.SpatialSensor, BudsFeature.UsageReport, BudsFeature.GamingMode }),

            new DeviceSpec(BudsModel.BudsPro, "Galaxy Buds Pro", "Buds Pro", SppStandard, 0xFD, 0xDD, 3,
                new BudsFeature[] { BudsFeature.Anc, BudsFeature.NoiseControl, BudsFeature.AmbientSound, BudsFeature.AmbientSoundVolume, BudsFeature.AmbientSidetone, BudsFeature.AmbientCustomize, BudsFeature.CaseBattery, BudsFeature.Equalizer, BudsFeature.TouchpadLock, BudsFeature.TouchpadOptions, BudsFeature.FindMyEarbuds, BudsFeature.FirmwareUpdates, BudsFeature.SpatialSensor, BudsFeature.UsageReport, BudsFeature.GamingMode },
                0),

            new DeviceSpec(BudsModel.Buds2, "Galaxy Buds2", "Buds2", SppNew, 0xFD, 0xDD, 2,
                new BudsFeature[] { BudsFeature.Anc, BudsFeature.NoiseControl, BudsFeature.NoiseControlModeDualSide, BudsFeature.AmbientSound, BudsFeature.AmbientSoundVolume, BudsFeature.AmbientSidetone, BudsFeature.AmbientCustomize, BudsFeature.CaseBattery, BudsFeature.Equalizer, BudsFeature.TouchpadLock, BudsFeature.TouchpadAdvancedLock, BudsFeature.TouchpadOptions, BudsFeature.FindMyEarbuds, BudsFeature.FitTest, BudsFeature.FirmwareUpdates, BudsFeature.Rename, BudsFeature.SmartThingsFind, BudsFeature.UsageReport, BudsFeature.GamingMode }),

            new DeviceSpec(BudsModel.Buds2Pro, "Galaxy Buds2 Pro", "Buds2 Pro", SppNew, 0xFD, 0xDD, 2,
                new BudsFeature[] { BudsFeature.Anc, BudsFeature.NoiseControl, BudsFeature.NoiseControlModeDualSide, BudsFeature.AmbientSound, BudsFeature.AmbientSoundVolume, BudsFeature.AmbientSidetone, BudsFeature.AmbientCustomize, BudsFeature.CaseBattery, BudsFeature.Equalizer, BudsFeature.CustomEqualizer, BudsFeature.TouchpadLock, BudsFeature.TouchpadAdvancedLock, BudsFeature.TouchpadOptions, BudsFeature.FindMyEarbuds, BudsFeature.FitTest, BudsFeature.FirmwareUpdates, BudsFeature.Rename, BudsFeature.SmartThingsFind, BudsFeature.UsageReport, BudsFeature.GamingMode }),

            new DeviceSpec(BudsModel.BudsFe, "Galaxy Buds FE", "Buds FE", SppNew, 0xFD, 0xDD, 2,
                new BudsFeature[] { BudsFeature.Anc, BudsFeature.NoiseControl, BudsFeature.NoiseControlModeDualSide, BudsFeature.AmbientSound, BudsFeature.AmbientSoundVolume, BudsFeature.AmbientSidetone, BudsFeature.AmbientCustomize, BudsFeature.CaseBattery, BudsFeature.Equalizer, BudsFeature.TouchpadLock, BudsFeature.TouchpadAdvancedLock, BudsFeature.TouchpadOptions, BudsFeature.FindMyEarbuds, BudsFeature.FitTest, BudsFeature.FirmwareUpdates, BudsFeature.Rename, BudsFeature.SmartThingsFind, BudsFeature.UsageReport }),

            new DeviceSpec(BudsModel.BudsCore, "Galaxy Buds Core", "Buds Core", SppNew, 0xFD, 0xDD, 2,
                new BudsFeature[] { BudsFeature.Anc, BudsFeature.NoiseControl, BudsFeature.NoiseControlModeDualSide, BudsFeature.AmbientSound, BudsFeature.AmbientSoundVolume, BudsFeature.AmbientSidetone, BudsFeature.AmbientCustomize, BudsFeature.CaseBattery, BudsFeature.Equalizer, BudsFeature.TouchpadLock, BudsFeature.TouchpadAdvancedLock, BudsFeature.TouchpadOptions, BudsFeature.FindMyEarbuds, BudsFeature.FitTest, BudsFeature.FirmwareUpdates, BudsFeature.Rename, BudsFeature.SmartThingsFind, BudsFeature.UsageReport }),

            new DeviceSpec(BudsModel.Buds3, "Galaxy Buds3", "Buds3", SppNew, 0xFD, 0xDD, 2,
                new BudsFeature[] { BudsFeature.Anc, BudsFeature.NoiseControl, BudsFeature.NoiseControlModeDualSide, BudsFeature.AmbientSound, BudsFeature.AmbientSoundVolume, BudsFeature.AmbientSidetone, BudsFeature.AmbientCustomize, BudsFeature.CaseBattery, BudsFeature.Equalizer, BudsFeature.CustomEqualizer, BudsFeature.TouchpadLock, BudsFeature.TouchpadAdvancedLock, BudsFeature.TouchpadOptions, BudsFeature.FindMyEarbuds, BudsFeature.FitTest, BudsFeature.FirmwareUpdates, BudsFeature.Rename, BudsFeature.SmartThingsFind, BudsFeature.UsageReport }),

            new DeviceSpec(BudsModel.Buds3Pro, "Galaxy Buds3 Pro", "Buds3 Pro", SppNew, 0xFD, 0xDD, 2,
                new BudsFeature[] { BudsFeature.Anc, BudsFeature.NoiseControl, BudsFeature.NoiseControlModeDualSide, BudsFeature.AmbientSound, BudsFeature.AmbientSoundVolume, BudsFeature.AmbientSidetone, BudsFeature.AmbientCustomize, BudsFeature.CaseBattery, BudsFeature.Equalizer, BudsFeature.CustomEqualizer, BudsFeature.TouchpadLock, BudsFeature.TouchpadAdvancedLock, BudsFeature.TouchpadOptions, BudsFeature.FindMyEarbuds, BudsFeature.FitTest, BudsFeature.FirmwareUpdates, BudsFeature.Rename, BudsFeature.SmartThingsFind, BudsFeature.UsageReport, BudsFeature.HeadTracking }),

            new DeviceSpec(BudsModel.Buds3Fe, "Galaxy Buds3 FE", "Buds3 FE", SppNew, 0xFD, 0xDD, 2,
                new BudsFeature[] { BudsFeature.Anc, BudsFeature.NoiseControl, BudsFeature.NoiseControlModeDualSide, BudsFeature.AmbientSound, BudsFeature.AmbientSoundVolume, BudsFeature.AmbientSidetone, BudsFeature.AmbientCustomize, BudsFeature.CaseBattery, BudsFeature.Equalizer, BudsFeature.CustomEqualizer, BudsFeature.TouchpadLock, BudsFeature.TouchpadAdvancedLock, BudsFeature.TouchpadOptions, BudsFeature.FindMyEarbuds, BudsFeature.FitTest, BudsFeature.FirmwareUpdates, BudsFeature.Rename, BudsFeature.SmartThingsFind, BudsFeature.UsageReport }),

            new DeviceSpec(BudsModel.Buds4, "Galaxy Buds4", "Buds4", SppNew, 0xFD, 0xDD, 2,
                new BudsFeature[] { BudsFeature.Anc, BudsFeature.NoiseControl, BudsFeature.NoiseControlModeDualSide, BudsFeature.AmbientSound, BudsFeature.AmbientSoundVolume, BudsFeature.AmbientSidetone, BudsFeature.AmbientCustomize, BudsFeature.CaseBattery, BudsFeature.Equalizer, BudsFeature.CustomEqualizer, BudsFeature.TouchpadLock, BudsFeature.TouchpadAdvancedLock, BudsFeature.TouchpadOptions, BudsFeature.FindMyEarbuds, BudsFeature.FitTest, BudsFeature.FirmwareUpdates, BudsFeature.Rename, BudsFeature.SmartThingsFind, BudsFeature.UsageReport }),

            new DeviceSpec(BudsModel.Buds4Pro, "Galaxy Buds4 Pro", "Buds4 Pro", SppNew, 0xFD, 0xDD, 4,
                new BudsFeature[] { BudsFeature.Anc, BudsFeature.NoiseControl, BudsFeature.NoiseControlModeDualSide, BudsFeature.AmbientSound, BudsFeature.AmbientSoundVolume, BudsFeature.AmbientSidetone, BudsFeature.AmbientCustomize, BudsFeature.CaseBattery, BudsFeature.Equalizer, BudsFeature.CustomEqualizer, BudsFeature.TouchpadLock, BudsFeature.TouchpadAdvancedLock, BudsFeature.TouchpadOptions, BudsFeature.FindMyEarbuds, BudsFeature.FitTest, BudsFeature.FirmwareUpdates, BudsFeature.Rename, BudsFeature.SmartThingsFind, BudsFeature.UsageReport })
        };

        public static DeviceSpec FindByModel(BudsModel model)
        {
            foreach (var spec in Specs)
            {
                if (spec.Model == model)
                    return spec;
            }
            return null;
        }

        public static DeviceSpec FindByDeviceName(string deviceName)
        {
            if (string.IsNullOrEmpty(deviceName))
                return null;
            foreach (var spec in Specs)
            {
                if (deviceName.StartsWith(spec.DeviceBaseName, StringComparison.OrdinalIgnoreCase))
                    return spec;
            }
            return null;
        }
    }
}
