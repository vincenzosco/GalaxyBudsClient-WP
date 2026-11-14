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
                _discoveredDevices.Add(new DeviceInfo { Id = e.DeviceId, DisplayName = e.Name, Status = AppResources.DeviceStatusPaired });
                DeviceListBox.ItemsSource = null;
                DeviceListBox.ItemsSource = _discoveredDevices;
            });
        }

        private void OnConnectionChanged(object sender, TransportConnectionChangedEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
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
                        ConnectionStatusText.Text = AppResources.NotConnected;
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
            foreach (var d in _discoveredDevices)
            {
                if (d.Id == _selectedDeviceId) { name = d.DisplayName; break; }
            }
            DeviceSpec spec = DeviceSpecCatalog.FindByDeviceName(name);
            if (spec != null)
                _session.SetDeviceSpec(spec);
            try
            {
                await _session.ConnectAsync(_selectedDeviceId, spec != null ? spec.ServiceUuid : DeviceSpecCatalog.SppStandard);
            }
            catch (Exception ex)
            {
                ConnectionStatusText.Text = ex.Message;
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
        public string Status { get; set; }
    }
}
