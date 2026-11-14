using System;
using System.Threading.Tasks;

namespace Galaxy_Buds_App.Bluetooth
{
    public enum TransportState
    {
        Disconnected,
        Connecting,
        Connected,
        Disconnecting
    }

    public sealed class TransportConnectionChangedEventArgs : EventArgs
    {
        public TransportState State { get; private set; }
        public string Error { get; private set; }

        public TransportConnectionChangedEventArgs(TransportState state, string error = null)
        {
            State = state;
            Error = error;
        }
    }

    public sealed class DeviceDiscoveredEventArgs : EventArgs
    {
        public string DeviceId { get; private set; }
        public string Name { get; private set; }
        public Guid ServiceUuid { get; private set; }

        public DeviceDiscoveredEventArgs(string deviceId, string name, Guid serviceUuid)
        {
            DeviceId = deviceId;
            Name = name;
            ServiceUuid = serviceUuid;
        }
    }

    public interface IBluetoothTransport
    {
        event EventHandler<TransportConnectionChangedEventArgs> ConnectionChanged;
        event EventHandler<DeviceDiscoveredEventArgs> DeviceDiscovered;
        event EventHandler<byte[]> DataReceived;

        TransportState State { get; }

        Task DiscoverAsync();
        Task ConnectAsync(string deviceId, Guid serviceUuid);
        Task WriteAsync(byte[] data);
        Task DisconnectAsync();
    }
}
