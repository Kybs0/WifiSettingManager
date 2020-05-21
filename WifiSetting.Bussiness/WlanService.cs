using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WifiSetting.Bussiness
{
    public class WlanService : IWlanService
    {
        public WlanService()
        {
            Client = new WlanClient();
        }
        private WlanClient Client { get; }
        private List<WlanClient.WlanInterface> _wlanInterfaces;
        private List<WlanClient.WlanInterface> GetAllWlanInterfaces()
        {
            if (_wlanInterfaces == null || _wlanInterfaces.Count == 0)
            {
                _wlanInterfaces = Client.Interfaces.ToList();
                if (_wlanInterfaces != null)
                {
                    foreach (var wlanInterface in _wlanInterfaces)
                    {
                        wlanInterface.WlanConnectionNotification += WlanInterface_WlanConnectionNotification;
                        wlanInterface.WlanNotification += WlanInterface_WlanNotification;
                        wlanInterface.WlanReasonNotification += WlanInterface_WlanReasonNotification;
                    }
                }
            }
            return _wlanInterfaces;
        }

        public bool HasWifiModule
        {
            get { return GetAllWlanInterfaces()?.Count != 0; }
        }

        public List<WlanInfoItem> GetAllWlanList()
        {
            var wlanInfoItems = new List<WlanInfoItem>();
            try
            {
                var wlanInterfaces = GetAllWlanInterfaces();
                foreach (WlanClient.WlanInterface wlanIface in wlanInterfaces)
                {
                    Wlan.WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(0);
                    foreach (Wlan.WlanAvailableNetwork network in networks)
                    {
                        var wlanNameBySsid = GetWlanNameBySsid(network.dot11Ssid);
                        var wlanInfoItem = wlanInfoItems.FirstOrDefault(i => i.Name == wlanNameBySsid);
                        //wlanInfoItems.Add(new WlanInfoItem()
                        //{
                        //    WlanInterfaceGuid = wlanIface.InterfaceGuid,
                        //    Name = wlanNameBySsid,
                        //    SignalQuality = (int)network.wlanSignalQuality,
                        //    IsSecurityEnabled = network.securityEnabled,
                        //    AuthAlgorithmType = network.dot11DefaultAuthAlgorithm,
                        //    CipherAlgorithmType = network.dot11DefaultCipherAlgorithm,
                        //    //HasProfile = !string.IsNullOrEmpty(network.profileName),
                        //    HasProfile = (network.flags & Wlan.WlanAvailableNetworkFlags.HasProfile) != 0,
                        //    IsConnected = (network.flags & Wlan.WlanAvailableNetworkFlags.Connected) != 0
                        //});
                        //break;
                        if (wlanInfoItem == null)
                        {
                            wlanInfoItems.Add(new WlanInfoItem()
                            {
                                WlanInterface = wlanIface,
                                SSid = network.dot11Ssid,
                                Name = wlanNameBySsid,
                                SignalQuality = (int)network.wlanSignalQuality,
                                IsSecurityEnabled = network.securityEnabled,
                                AuthAlgorithmType = network.dot11DefaultAuthAlgorithm,
                                CipherAlgorithmType = network.dot11DefaultCipherAlgorithm,
                                //HasProfile = !string.IsNullOrEmpty(network.profileName),
                                HasProfile = (network.flags & Wlan.WlanAvailableNetworkFlags.HasProfile) != 0,
                                IsConnected = (network.flags & Wlan.WlanAvailableNetworkFlags.Connected) != 0
                            });
                        }
                        else
                        {
                            //连接状态下的wifi，会有俩条数据，此处补充连接状态。连接状态不同
                            if ((network.flags & Wlan.WlanAvailableNetworkFlags.Connected) != 0)
                            {
                                wlanInfoItem.IsConnected = true;
                            }
                            //有Profile缓存的wifi，会有俩条数据，此处补充Profile状态。是否有Profile
                            if ((network.flags & Wlan.WlanAvailableNetworkFlags.HasProfile) != 0)
                            {
                                //wlanInfoItem.HasProfile = !string.IsNullOrEmpty(network.profileName);
                                wlanInfoItem.HasProfile = true;
                            }
                        }
                    }
                    //获取当前网络连接协议信息
                    //foreach (Wlan.WlanProfileInfo profileInfo in wlanIface.GetProfiles())
                    //{
                    //    string name = profileInfo.profileName; // this is typically the network's SSID
                    //    string xml = wlanIface.GetProfileXml(profileInfo.profileName);
                    //    Debug.WriteLine($"profileName:{name},xml:{xml} ");
                    //}
                }
            }
            catch (Exception e)
            {
#if DEBUG
                //throw e;
#endif
            }

            return wlanInfoItems;
        }

        public bool ConnectToWlan(WlanInfoItem wlanItem, string key, bool isAutoConnect, out string errorMsg)
        {
            errorMsg = string.Empty;
            try
            {
                string authenticationType = GetAuthenticationType(wlanItem.AuthAlgorithmType);
                var cipherType = GetCipherType(wlanItem, out var keytype);

                if (wlanItem.IsSecurityEnabled && string.IsNullOrEmpty(key))
                {
                    errorMsg = ("无法连接网络，密码不能为空！");
                    return false;
                }

                string profileName = wlanItem.Name;
                string mac = GetMacByWlanName(profileName);
                string profileXml;
                if (!string.IsNullOrEmpty(key))
                {
                    profileXml = string.Format("<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\"><name>{0}</name><SSIDConfig><SSID><hex>{1}</hex><name>{0}</name></SSID></SSIDConfig><connectionType>ESS</connectionType><connectionMode>{2}</connectionMode><autoSwitch>false</autoSwitch><MSM><security><authEncryption><authentication>{3}</authentication><encryption>{4}</encryption><useOneX>false</useOneX></authEncryption><sharedKey><keyType>{5}</keyType><protected>false</protected><keyMaterial>{6}</keyMaterial></sharedKey><keyIndex>0</keyIndex></security></MSM></WLANProfile>", profileName, mac, isAutoConnect ? "auto" : " manual", authenticationType, cipherType, keytype, key);
                }
                else
                {
                    profileXml = string.Format("<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\"><name>{0}</name><SSIDConfig><SSID><hex>{1}</hex><name>{0}</name></SSID></SSIDConfig><connectionType>ESS</connectionType><connectionMode>{2}</connectionMode><autoSwitch>false</autoSwitch><MSM><security><authEncryption><authentication>{3}</authentication><encryption>{4}</encryption><useOneX>false</useOneX></authEncryption></security></MSM></WLANProfile>", profileName, mac, isAutoConnect ? "auto" : " manual", authenticationType, cipherType);
                }

                wlanItem.WlanInterface.SetProfile(Wlan.WlanProfileFlags.AllUser, profileXml, true);
                bool success = wlanItem.WlanInterface.ConnectSynchronously(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, profileName, 15000);
                return success;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return false;
            }
        }

        #region 内部代码

        private static string GetCipherType(WlanInfoItem wlanItem, out string keytype)
        {
            string cipher = string.Empty;
            keytype = string.Empty;

            switch (wlanItem.CipherAlgorithmType.ToString())
            {
                case "CCMP":
                    cipher = "AES";
                    keytype = "passPhrase";
                    break;
                case "TKIP":
                    cipher = "TKIP";
                    keytype = "passPhrase";
                    break;
                case "None":
                    cipher = "none";
                    keytype = "";
                    break;
                case "WWEP":
                    cipher = "WEP";
                    keytype = "networkKey";
                    break;
                case "WEP40":
                    cipher = "WEP";
                    keytype = "networkKey";
                    break;
                case "WEP104":
                    cipher = "WEP";
                    keytype = "networkKey";
                    break;
            }

            return cipher;
        }

        private string GetAuthenticationType(Wlan.Dot11AuthAlgorithm wlanItemAuthAlgorithmType)
        {
            string auth = string.Empty;
            switch (wlanItemAuthAlgorithmType.ToString())
            {
                case "IEEE80211_Open":
                    auth = "open"; break;
                case "RSNA":
                    auth = "WPA2PSK"; break;
                case "RSNA_PSK":
                    //Console.WriteLine("电子设计wifi：》》》");
                    auth = "WPA2PSK"; break;
                case "WPA":
                    auth = "WPAPSK"; break;
                case "WPA_None":
                    auth = "WPAPSK"; break;
                case "WPA_PSK":
                    auth = "WPAPSK"; break;
            }

            return auth;
        }

        /// <summary>
        /// 字符串转Hex
        /// </summary>
        /// <param name="wlanName"></param>
        /// <returns></returns>
        public static string GetMacByWlanName(string wlanName)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = Encoding.UTF8.GetBytes(wlanName); //默认是System.Text.Encoding.Default.GetBytes(str)
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(Convert.ToString(byStr[i], 16));
            }

            return (sb.ToString().ToUpper());
        }
        public string GetWlanNameBySsid(Wlan.Dot11Ssid ssid)
        {
            return Encoding.UTF8.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
        }

        /// <summary>
        /// Occurs when an event of any kind occurs on a WLAN interface.
        /// </summary>
        public event WlanClient.WlanInterface.WlanNotificationEventHandler WlanNotification;

        /// <summary>
        /// Occurs when a WLAN interface changes connection state.
        /// </summary>
        public event WlanClient.WlanInterface.WlanConnectionNotificationEventHandler WlanConnectionNotification;

        /// <summary>
        /// Occurs when a WLAN operation fails due to some reason.
        /// </summary>
        public event WlanClient.WlanInterface.WlanReasonNotificationEventHandler WlanReasonNotification;

        private void WlanInterface_WlanReasonNotification(Wlan.WlanNotificationData notifyData, Wlan.WlanReasonCode reasonCode)
        {
            WlanReasonNotification?.Invoke(notifyData, reasonCode);
        }

        private void WlanInterface_WlanNotification(Wlan.WlanNotificationData notifyData)
        {
            WlanNotification?.Invoke(notifyData);
        }

        private void WlanInterface_WlanConnectionNotification(Wlan.WlanNotificationData notifyData, Wlan.WlanConnectionNotificationData connNotifyData)
        {
            WlanConnectionNotification?.Invoke(notifyData, connNotifyData);
        }

        #endregion

        public List<WlanProfileItem> GetCurrentConnectedProfiles()
        {
            var list = new List<WlanProfileItem>();
            foreach (WlanClient.WlanInterface wlanIface in Client.Interfaces)
            {
                foreach (Wlan.WlanProfileInfo profileInfo in wlanIface.GetProfiles())
                {
                    string name = profileInfo.profileName; // this is typically the network's SSID
                    string xml = wlanIface.GetProfileXml(profileInfo.profileName);
                    Debug.WriteLine($"profileName:{name},xml:{xml} ");
                    list.Add(new WlanProfileItem()
                    {
                        ProfileName = name,
                        XmlContent = xml
                    });
                }
            }
            return list;
        }

        public void DisConnectToWlan(WlanInfoItem wlanInfoItem)
        {
            if (wlanInfoItem.IsConnected)
            {
                wlanInfoItem.WlanInterface.DeleteProfile(wlanInfoItem.Name);
            }
        }
    }

    public class WlanProfileItem
    {
        public string ProfileName { get; set; }
        public string XmlContent { get; set; }
    }
}
