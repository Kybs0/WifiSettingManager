using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WifiSetting.Bussiness;

namespace WifiSettingManager
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            WlanService = new WlanService();
            WlanService.WlanConnectionNotification += WlanService_WlanConnectionNotification;
            WlanService.WlanNotification += WlanService_WlanNotification;
            WlanService.WlanReasonNotification += WlanService_WlanReasonNotification;
        }

        private void WlanService_WlanReasonNotification(Wlan.WlanNotificationData notifyData, Wlan.WlanReasonCode reasonCode)
        {

        }

        private void WlanService_WlanNotification(Wlan.WlanNotificationData notifyData)
        {
            Application.Current.Dispatcher.InvokeAsync(() => { RefreshWlanList(); });
        }

        private void WlanService_WlanConnectionNotification(Wlan.WlanNotificationData notifyData, Wlan.WlanConnectionNotificationData connNotifyData)
        {
            var dot11Ssid = connNotifyData.dot11Ssid;
            var wlanNMAE = WlanService.GetWlanNameBySsid(dot11Ssid);
            Debug.WriteLine(wlanNMAE);
        }

        private WlanService WlanService { get; }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshWlanList();
        }

        private void RefreshWlanList()
        {
            var wlanInfoItems = WlanService.GetAllWlanList();
            WlanInfoItems = wlanInfoItems.OrderByDescending(i => i.IsConnected).ThenByDescending(i => i.SignalQuality).ToList();
            NoWlanTextBlock.Visibility = wlanInfoItems.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public static readonly DependencyProperty WlanInfoItemsProperty = DependencyProperty.Register(
            "WlanInfoItems", typeof(List<WlanInfoItem>), typeof(MainWindow), new PropertyMetadata(default(List<WlanInfoItem>)));

        public List<WlanInfoItem> WlanInfoItems
        {
            get { return (List<WlanInfoItem>)GetValue(WlanInfoItemsProperty); }
            set { SetValue(WlanInfoItemsProperty, value); }
        }

        private void ConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is WlanInfoItem wlanInfoItem)
            {
                var wifiPasswardInputWindow = new WifiPasswardInputWindow();
                wifiPasswardInputWindow.Owner = this;
                wifiPasswardInputWindow.WlanName = wlanInfoItem.Name;
                wifiPasswardInputWindow.IsNeedPassword = wlanInfoItem.IsSecurityEnabled;
                wifiPasswardInputWindow.WlanInfoItem = wlanInfoItem;
                wifiPasswardInputWindow.Comfirmed += WifiPasswardInputWindow_Comfirmed;
                wifiPasswardInputWindow.ShowDialog();
            }
        }

        private void WifiPasswardInputWindow_Comfirmed(object sender, EventArgs e)
        {
            if (sender is WifiPasswardInputWindow wifiInfoWindow)
            {
                var wlanInfoItem = wifiInfoWindow.WlanInfoItem;
                var connectSuccess = WlanService.ConnectToWlan(wlanInfoItem, wifiInfoWindow.Password, wifiInfoWindow.IsAutoConnect, out var errorMsg);
                if (!connectSuccess)
                {
                    MessageBox.Show(errorMsg);
                }
            }
        }

        private void DisconnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is WlanInfoItem wlanInfoItem)
            {
                WlanService.DisConnectToWlan(wlanInfoItem);
            }
        }

        private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            RefreshWlanList();
        }
    }
}
