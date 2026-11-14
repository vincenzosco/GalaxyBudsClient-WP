using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Galaxy_Buds_App.Models;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Galaxy_Buds_App.Bluetooth
{
    // WP8.1 Silverlight RFCOMM transport.
    // Uses Windows.Devices.Bluetooth.Rfcomm and Windows.Networking.Sockets,
    // which are available in the WP8.1 Silverlight app model.
    public sealed class Wp81RfcommTransport : IBluetoothTransport
    {
        private StreamSocket _socket;
        private DataWriter _writer;
        private DataReader _reader;
        private TransportState _state = TransportState.Disconnected;
        private readonly object _lock = new object();

        public event EventHandler<TransportConnectionChangedEventArgs> ConnectionChanged;
        public event EventHandler<DeviceDiscoveredEventArgs> DeviceDiscovered;
        public event EventHandler<byte[]> DataReceived;

        public TransportState State
        {
            get { return _state; }
        }

        public async Task DiscoverAsync()
        {
            var seen = new HashSet<string>();
            foreach (Guid uuid in DeviceSpecCatalog.ServiceUuids)
            {
                var selector = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.FromUuid(uuid));
                var devices = await DeviceInformation.FindAllAsync(selector);
                foreach (var device in devices)
                {
                    if (!seen.Add(device.Id))
                        continue;
                    if (DeviceDiscovered != null)
                        DeviceDiscovered(this, new DeviceDiscoveredEventArgs(device.Id, device.Name, uuid));
                }
            }
        }

        public async Task ConnectAsync(string deviceId, Guid serviceUuid)
        {
            SetState(TransportState.Connecting);
            try
            {
                var serviceId = RfcommServiceId.FromUuid(serviceUuid);
                var selector = RfcommDeviceService.GetDeviceSelector(serviceId);
                var devices = await DeviceInformation.FindAllAsync(selector);
                DeviceInformation target = null;
                foreach (var d in devices)
                {
                    if (d.Id == deviceId) { target = d; break; }
                }
                if (target == null)
                    throw new InvalidOperationException("Device not found.");

                var service = await RfcommDeviceService.FromIdAsync(target.Id);
                if (service == null)
                    throw new InvalidOperationException("RFCOMM service not available.");

                _socket = new StreamSocket();
                await _socket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName);

                _writer = new DataWriter(_socket.OutputStream);
                _reader = new DataReader(_socket.InputStream);
                _reader.InputStreamOptions = InputStreamOptions.Partial;

                SetState(TransportState.Connected);
                var _ignored = ReadLoopAsync();
            }
            catch (Exception ex)
            {
                CleanupSocket();
                SetState(TransportState.Disconnected, ex.Message);
                throw;
            }
        }

        public async Task WriteAsync(byte[] data)
        {
            if (_writer == null)
                throw new InvalidOperationException("Not connected.");
            _writer.WriteBytes(data);
            await _writer.StoreAsync();
        }

        public async Task DisconnectAsync()
        {
            SetState(TransportState.Disconnecting);
            try
            {
                CleanupSocket();
            }
            finally
            {
                SetState(TransportState.Disconnected);
            }
            await Task.FromResult(0);
        }

        private void CleanupSocket()
        {
            if (_reader != null) { _reader.Dispose(); _reader = null; }
            if (_writer != null) { _writer.Dispose(); _writer = null; }
            if (_socket != null) { _socket.Dispose(); _socket = null; }
        }

        private async Task ReadLoopAsync()
        {
            var buffer = new byte[4096];
            try
            {
                while (_reader != null && _state == TransportState.Connected)
                {
                    uint bytesRead = await _reader.LoadAsync((uint)buffer.Length);
                    if (bytesRead == 0)
                        break;
                    var chunk = new byte[bytesRead];
                    Array.Copy(buffer, 0, chunk, 0, (int)bytesRead);
                    if (DataReceived != null)
                        DataReceived(this, chunk);
                }
            }
            catch
            {
                // Stream closed or device disconnected.
            }
            if (_state == TransportState.Connected)
                SetState(TransportState.Disconnected);
        }

        private void SetState(TransportState state, string error = null)
        {
            _state = state;
            if (ConnectionChanged != null)
                ConnectionChanged(this, new TransportConnectionChangedEventArgs(state, error));
        }
    }
}
