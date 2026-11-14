using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Galaxy_Buds_App.Bluetooth;
using Galaxy_Buds_App.Models;
using Galaxy_Buds_App.Resources;
using Galaxy_Buds_App.Services;
using Microsoft.Phone.Controls;

namespace Galaxy_Buds_App
{
    public partial class MainPage : PhoneApplicationPage
    {
        private readonly Wp81RfcommTransport _transport;
        private DeviceSession _session;
        private readonly List<DeviceInfo> _discoveredDevices = new List<DeviceInfo>();
        private string _selectedDeviceId;
        private string _lastError;

        public MainPage()
        {
            InitializeComponent();
            _transport = new Wp81RfcommTransport();
            _transport.DeviceDiscovered += OnDeviceDiscovered;
            _transport.ConnectionChanged += OnConnectionChanged;
            _session = new DeviceSession(_transport);
            _session.StatusUpdated += OnStatusUpdated;
        }

        private void OnDeviceDiscovered(object sender, DeviceDiscoveredEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                _discoveredDevices.Add(new DeviceInfo { Id = e.DeviceId, DisplayName = e.Name, ServiceUuid = e.ServiceUuid, Status = AppResources.DeviceStatusPaired });
                DeviceListBox.ItemsSource = null;
                DeviceListBox.ItemsSource = _discoveredDevices;
            });
        }

        private void OnConnectionChanged(object sender, TransportConnectionChangedEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                if (!string.IsNullOrEmpty(e.Error))
                    _lastError = e.Error;
                switch (e.State)
                {
                    case TransportState.Connected:
                        ConnectionStatusText.Text = AppResources.Connected;
                        ConnectButton.IsEnabled = false;
                        DisconnectButton.IsEnabled = true;
                        OverviewPanel.Visibility = Visibility.Visible;
                        SoundPanel.Visibility = Visibility.Visible;
                        ControlsPanel.Visibility = Visibility.Visible;
                        ToolsPanel.Visibility = Visibility.Visible;
                        break;
                    case TransportState.Connecting:
                        ConnectionStatusText.Text = AppResources.Connecting;
                        ConnectButton.IsEnabled = false;
                        break;
                    case TransportState.Disconnected:
                        ConnectionStatusText.Text = string.IsNullOrEmpty(e.Error) ? AppResources.NotConnected : e.Error;
                        ConnectButton.IsEnabled = _selectedDeviceId != null;
                        DisconnectButton.IsEnabled = false;
                        OverviewPanel.Visibility = Visibility.Collapsed;
                        SoundPanel.Visibility = Visibility.Collapsed;
                        ControlsPanel.Visibility = Visibility.Collapsed;
                        ToolsPanel.Visibility = Visibility.Collapsed;
                        BatteryLeftText.Text = "--";
                        BatteryRightText.Text = "--";
                        break;
                }
            });
        }

        private void OnStatusUpdated(object sender, DeviceStatus status)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                BatteryLeftText.Text = status.BatteryLeft.HasValue ? status.BatteryLeft.Value + "%" : "--";
                BatteryRightText.Text = status.BatteryRight.HasValue ? status.BatteryRight.Value + "%" : "--";
            });
        }

        private async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            _discoveredDevices.Clear();
            DeviceListBox.ItemsSource = null;
            ConnectionStatusText.Text = AppResources.Scanning;
            try
            {
                await _transport.DiscoverAsync();
            }
            catch (Exception ex)
            {
                ConnectionStatusText.Text = ex.Message;
            }
            if (_discoveredDevices.Count == 0)
                ConnectionStatusText.Text = AppResources.NoDevicesFound;
        }

        private void DeviceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = DeviceListBox.SelectedItem as DeviceInfo;
            if (item != null)
            {
                _selectedDeviceId = item.Id;
                ConnectButton.IsEnabled = _transport.State == TransportState.Disconnected;
            }
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDeviceId == null) return;
            ConnectionStatusText.Text = AppResources.Connecting;
            var name = "";
            Guid discoveredUuid = Guid.Empty;
            foreach (var d in _discoveredDevices)
            {
                if (d.Id == _selectedDeviceId) { name = d.DisplayName; discoveredUuid = d.ServiceUuid; break; }
            }
            DeviceSpec spec = DeviceSpecCatalog.FindByDeviceName(name);
            if (spec != null)
                _session.SetDeviceSpec(spec);

            // Try the service UUID the device was discovered on first, then
            // the model's expected one, then every known SPP UUID.
            var candidates = new List<Guid>();
            if (discoveredUuid != Guid.Empty)
                candidates.Add(discoveredUuid);
            if (spec != null && !candidates.Contains(spec.ServiceUuid))
                candidates.Add(spec.ServiceUuid);
            foreach (Guid uuid in DeviceSpecCatalog.ServiceUuids)
            {
                if (!candidates.Contains(uuid))
                    candidates.Add(uuid);
            }

            _lastError = null;
            bool connected = false;
            foreach (Guid uuid in candidates)
            {
                var result = await _session.ConnectAsync(_selectedDeviceId, uuid);
                if (result == CommandResult.Success) { connected = true; break; }
            }
            if (!connected)
            {
                ConnectionStatusText.Text = string.IsNullOrEmpty(_lastError)
                    ? AppResources.NotConnected
                    : _lastError;
            }
        }

        private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            await _session.DisconnectAsync();
        }

        private async void EqualizerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = EqualizerListBox.SelectedIndex;
            if (idx < 0) return;
            await _session.SetEqualizerAsync(true, idx);
        }

        private async void TouchpadLockCheck_Changed(object sender, RoutedEventArgs e)
        {
            await _session.SetTouchpadLockAsync(TouchpadLockCheck.IsChecked == true);
        }

        private async void FindMyEarbudsButton_Click(object sender, RoutedEventArgs e)
        {
            await _session.FindMyEarbudsAsync(true);
        }
    }

    public class DeviceInfo
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public Guid ServiceUuid { get; set; }
        public string Status { get; set; }
    }
}
