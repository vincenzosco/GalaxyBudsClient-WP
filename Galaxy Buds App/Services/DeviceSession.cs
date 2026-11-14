using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Galaxy_Buds_App.Bluetooth;
using Galaxy_Buds_App.Models;
using Galaxy_Buds_App.Protocol;
using Galaxy_Buds_App.Protocol.Messages;

namespace Galaxy_Buds_App.Services
{
    public enum CommandResult
    {
        Success,
        Timeout,
        NotConnected,
        ProtocolError,
        Rejected
    }

    public sealed class DeviceStatus
    {
        public int? BatteryLeft { get; set; }
        public int? BatteryRight { get; set; }
        public int? BatteryCase { get; set; }
        public bool IsLeftCharging { get; set; }
        public bool IsRightCharging { get; set; }
        public bool IsCaseCharging { get; set; }
        public bool IsLeftWearing { get; set; }
        public bool IsRightWearing { get; set; }
        public bool TouchpadLocked { get; set; }
        public int EqualizerPreset { get; set; }
        public bool EqualizerEnabled { get; set; }
        public bool AmbientEnabled { get; set; }
        public int AmbientVolume { get; set; }
        public DateTime LastUpdatedUtc { get; set; }

        public DeviceStatus()
        {
            LastUpdatedUtc = DateTime.MinValue;
        }
    }

    public sealed class DeviceSession : IDisposable
    {
        private readonly IBluetoothTransport _transport;
        private DeviceSpec _spec;
        private readonly List<byte> _receiveBuffer = new List<byte>();
        private readonly object _lock = new object();

        public DeviceStatus Status { get; private set; }
        public BudsModel Model
        {
            get { return _spec != null ? _spec.Model : BudsModel.Unknown; }
        }
        public bool IsConnected
        {
            get { return _transport.State == TransportState.Connected; }
        }

        public event EventHandler<DeviceStatus> StatusUpdated;
        public event EventHandler<TransportConnectionChangedEventArgs> ConnectionChanged;

        public DeviceSession(IBluetoothTransport transport)
        {
            if (transport == null)
                throw new ArgumentNullException("transport");
            _transport = transport;
            Status = new DeviceStatus();
            _transport.DataReceived += OnDataReceived;
            _transport.ConnectionChanged += OnConnectionChanged;
        }

        public void SetDeviceSpec(DeviceSpec spec)
        {
            _spec = spec;
        }

        public async Task<CommandResult> ConnectAsync(string deviceId, Guid serviceUuid)
        {
            if (!IsConnected)
            {
                try
                {
                    await _transport.ConnectAsync(deviceId, serviceUuid);
                }
                catch
                {
                    return CommandResult.ProtocolError;
                }
                // Announce the manager to the buds (upstream parity) so they
                // start pushing status updates over the SPP link.
                await SendCommandAsync(CommandEncoder.EncodeManagerInfo(Model));
            }
            return CommandResult.Success;
        }

        public async Task<CommandResult> DisconnectAsync()
        {
            await _transport.DisconnectAsync();
            lock (_lock) { _receiveBuffer.Clear(); }
            return CommandResult.Success;
        }

        public async Task<CommandResult> SendCommandAsync(byte[] encodedFrame, bool nonIdempotent = false)
        {
            if (!IsConnected)
                return CommandResult.NotConnected;
            try
            {
                await _transport.WriteAsync(encodedFrame);
                return CommandResult.Success;
            }
            catch
            {
                return CommandResult.ProtocolError;
            }
        }

        public async Task<CommandResult> SetEqualizerAsync(bool enabled, int preset)
        {
            if (_spec == null || !_spec.Supports(BudsFeature.Equalizer))
                return CommandResult.Rejected;
            byte[] frame = CommandEncoder.EncodeEqualizer(_spec.Model, enabled, preset);
            return await SendCommandAsync(frame);
        }

        public async Task<CommandResult> SetAmbientModeAsync(bool enabled)
        {
            if (_spec == null || !_spec.Supports(BudsFeature.AmbientSound))
                return CommandResult.Rejected;
            byte[] frame = CommandEncoder.EncodeAmbientMode(_spec.Model, enabled);
            return await SendCommandAsync(frame);
        }

        public async Task<CommandResult> SetAmbientVolumeAsync(int volume)
        {
            if (_spec == null || !_spec.Supports(BudsFeature.AmbientSoundVolume))
                return CommandResult.Rejected;
            byte[] frame = CommandEncoder.EncodeAmbientVolume(_spec.Model, volume);
            return await SendCommandAsync(frame);
        }

        public async Task<CommandResult> SetTouchpadLockAsync(bool locked)
        {
            if (_spec == null || !_spec.Supports(BudsFeature.TouchpadLock))
                return CommandResult.Rejected;
            byte[] frame = CommandEncoder.EncodeLockTouchpad(_spec.Model, locked);
            return await SendCommandAsync(frame);
        }

        public async Task<CommandResult> FindMyEarbudsAsync(bool start)
        {
            if (_spec == null || !_spec.Supports(BudsFeature.FindMyEarbuds))
                return CommandResult.Rejected;
            byte[] frame = CommandEncoder.EncodeFindMyEarbuds(_spec.Model, start);
            return await SendCommandAsync(frame);
        }

        public async Task<CommandResult> FactoryResetAsync()
        {
            if (_spec == null)
                return CommandResult.Rejected;
            byte[] frame = CommandEncoder.EncodeReset(_spec.Model);
            return await SendCommandAsync(frame, nonIdempotent: true);
        }

        private void OnDataReceived(object sender, byte[] chunk)
        {
            lock (_lock)
            {
                _receiveBuffer.AddRange(chunk);
                TryParseBuffer();
            }
        }

        private void TryParseBuffer()
        {
            SppProtocolKind kind = SppProtocolKind.Modern;
            if (_spec != null && _spec.StartOfMessage == 0xFE)
                kind = SppProtocolKind.Legacy;

            byte som = kind == SppProtocolKind.Legacy ? (byte)0xFE : (byte)0xFD;
            byte eom = kind == SppProtocolKind.Legacy ? (byte)0xEE : (byte)0xDD;

            while (_receiveBuffer.Count >= 7)
            {
                // Find start marker
                int startIdx = _receiveBuffer.IndexOf(som);
                if (startIdx < 0) { _receiveBuffer.Clear(); return; }
                if (startIdx > 0) _receiveBuffer.RemoveRange(0, startIdx);

                int size;
                if (kind == SppProtocolKind.Legacy)
                {
                    if (_receiveBuffer.Count < 3) return;
                    size = _receiveBuffer[2];
                }
                else
                {
                    if (_receiveBuffer.Count < 3) return;
                    int header = _receiveBuffer[1] | (_receiveBuffer[2] << 8);
                    size = header & 0x03FF;
                }

                int totalLength = size + 4;
                if (_receiveBuffer.Count < totalLength) return;
                if (_receiveBuffer[totalLength - 1] != eom)
                {
                    _receiveBuffer.RemoveAt(0);
                    continue;
                }

                byte[] frameBytes = _receiveBuffer.GetRange(0, totalLength).ToArray();
                _receiveBuffer.RemoveRange(0, totalLength);

                try
                {
                    SppPacket packet = SppPacket.Decode(frameBytes, kind);
                    ParseStatusPacket(packet);
                }
                catch
                {
                    // Skip malformed frame.
                }
            }
        }

        private void ParseStatusPacket(SppPacket packet)
        {
            if (packet.MessageId == (byte)MsgIds.StatusUpdated || packet.MessageId == (byte)MsgIds.ExtendedStatusUpdated)
            {
                ParseStatusPayload(packet);
            }
        }

        private void ParseStatusPayload(SppPacket packet)
        {
            byte[] p = packet.Payload;
            DeviceStatus s = new DeviceStatus();
            bool legacy = _spec != null && _spec.Model == BudsModel.Buds;
            bool extended = packet.MessageId == (byte)MsgIds.ExtendedStatusUpdated;

            if (extended)
            {
                // ExtendedStatusUpdate: Revision[0], EarType[1], BatteryL[2],
                // BatteryR[3], Coupled[4], MainConnection[5], then
                // model-specific fields.
                if (p.Length < 6) return;
                s.BatteryLeft = p[2];
                s.BatteryRight = p[3];
                if (legacy)
                {
                    // WearState[6], AmbientEnabled[7], AmbientMode[8],
                    // AmbientVolume[9], EqualizerEnabled[10], EqualizerMode[11].
                    if (p.Length < 7) { PublishStatus(s); return; }
                    ApplyLegacyWear(s, p[6]);
                    if (p.Length >= 12)
                    {
                        s.AmbientEnabled = p[7] != 0;
                        s.AmbientVolume = p[9];
                        s.EqualizerEnabled = p[10] != 0;
                        s.EqualizerPreset = p[11];
                    }
                }
                else
                {
                    // Placement[6], BatteryCase[7], AmbientEnabled[8],
                    // AmbientVolume[9], AdjustSoundSync[10], EqualizerMode[11],
                    // TouchpadLock[12].
                    if (p.Length < 7) { PublishStatus(s); return; }
                    ApplyModernPlacement(s, p[6]);
                    if (p.Length >= 8 && _spec != null && _spec.Supports(BudsFeature.CaseBattery))
                        s.BatteryCase = p[7];
                    if (p.Length >= 13)
                    {
                        s.AmbientEnabled = p[8] != 0;
                        s.AmbientVolume = p[9];
                        s.EqualizerPreset = p[11];
                        s.TouchpadLocked = p[12] != 0;
                    }
                }
            }
            else
            {
                // StatusUpdate. Legacy: EarType[0], BatteryL[1], BatteryR[2],
                // Coupled[3], MainConnection[4], WearState[5]. Modern:
                // Revision[0], BatteryL[1], BatteryR[2], Coupled[3],
                // MainConnection[4], Placement[5], BatteryCase[6], Charging[7].
                if (p.Length < 6) return;
                s.BatteryLeft = p[1];
                s.BatteryRight = p[2];
                if (legacy)
                {
                    ApplyLegacyWear(s, p[5]);
                }
                else
                {
                    ApplyModernPlacement(s, p[5]);
                    if (p.Length >= 7 && _spec != null && _spec.Supports(BudsFeature.CaseBattery))
                        s.BatteryCase = p[6];
                    if (p.Length >= 8)
                    {
                        // Charging bits: left=4 (0x10), right=2 (0x04), case=0 (0x01).
                        s.IsLeftCharging = (p[7] & 0x10) != 0;
                        s.IsRightCharging = (p[7] & 0x04) != 0;
                        s.IsCaseCharging = (p[7] & 0x01) != 0;
                    }
                }
            }

            PublishStatus(s);
        }

        // LegacyWearStates: None=0, R=1, L=16, Both=17 (upstream mapping).
        private static void ApplyLegacyWear(DeviceStatus s, byte wear)
        {
            s.IsLeftWearing = wear == 16 || wear == 17;
            s.IsRightWearing = wear == 1 || wear == 17;
        }

        // PlacementStates: Wearing=1, Idle=2, Case=3; one nibble per side,
        // left in the high nibble.
        private static void ApplyModernPlacement(DeviceStatus s, byte placement)
        {
            s.IsLeftWearing = ((placement & 0xF0) >> 4) == 1;
            s.IsRightWearing = (placement & 0x0F) == 1;
        }

        private void PublishStatus(DeviceStatus s)
        {
            s.LastUpdatedUtc = DateTime.UtcNow;
            Status = s;
            if (StatusUpdated != null)
                StatusUpdated(this, s);
        }

        private void OnConnectionChanged(object sender, TransportConnectionChangedEventArgs e)
        {
            if (e.State == TransportState.Disconnected)
            {
                lock (_lock) { _receiveBuffer.Clear(); }
                Status = new DeviceStatus();
            }
            if (ConnectionChanged != null)
                ConnectionChanged(this, e);
        }

        public void Dispose()
        {
            _transport.DataReceived -= OnDataReceived;
            _transport.ConnectionChanged -= OnConnectionChanged;
        }
    }
}
