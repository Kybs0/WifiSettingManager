using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using WifiSetting.Bussiness;

namespace WifiSettingManager
{
    /// <summary>
    /// WifiPasswardInputWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WifiPasswardInputWindow : Window
    {
        public WifiPasswardInputWindow()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty WlanNameProperty = DependencyProperty.Register(
            "WlanName", typeof(string), typeof(WifiPasswardInputWindow), new PropertyMetadata(default(string)));

        public string WlanName
        {
            get { return (string)GetValue(WlanNameProperty); }
            set { SetValue(WlanNameProperty, value); }
        }

        public static readonly DependencyProperty IsNeedPasswordProperty = DependencyProperty.Register(
            "IsNeedPassword", typeof(bool), typeof(WifiPasswardInputWindow), new PropertyMetadata(default(bool)));

        public bool IsNeedPassword
        {
            get { return (bool) GetValue(IsNeedPasswordProperty); }
            set { SetValue(IsNeedPasswordProperty, value); }
        }

        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
            "Password", typeof(string), typeof(WifiPasswardInputWindow), new PropertyMetadata(default(string)));

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public static readonly DependencyProperty IsAutoConnectProperty = DependencyProperty.Register(
            "IsAutoConnect", typeof(bool), typeof(WifiPasswardInputWindow), new PropertyMetadata(default(bool)));

        public bool IsAutoConnect
        {
            get { return (bool)GetValue(IsAutoConnectProperty); }
            set { SetValue(IsAutoConnectProperty, value); }
        }

        public WlanInfoItem WlanInfoItem { get; set; }

        public event EventHandler Comfirmed;
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (IsNeedPassword&&string.IsNullOrEmpty(Password))
            {
                MessageBox.Show(this, "密码不能为空");
                return;
            }

            this.Close();
            Comfirmed?.Invoke(this, EventArgs.Empty);
        }
    }
}
