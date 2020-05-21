using System;
using System.Collections.Generic;

namespace WifiSetting.Bussiness
{
    public interface IWlanService
    {
        List<WlanInfoItem> GetAllWlanList();
        void DisConnectToWlan(WlanInfoItem wlanInfoItem);
        bool ConnectToWlan(WlanInfoItem wlanInfoItem, string password, bool isAutoConnect, out string errorMsg);
    }

    public class WlanInfoItem
    {
        /// <summary>
        /// wifi名称，或者SSID
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 信号强度
        /// </summary>
        public int SignalQuality { get; set; }
        /// <summary>
        /// 是否安全
        /// </summary>
        public bool IsSecurityEnabled { get; set; }
        /// <summary>
        /// 是否有Profile
        /// 来自Flag参数（另，ProfileName是否有值，也可以判断）
        /// </summary>
        public bool HasProfile { get; set; }
        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected { get; set; }
        /// <summary>
        /// 网络认证算法类型
        /// </summary>
        public Wlan.Dot11AuthAlgorithm AuthAlgorithmType { get; set; }
        /// <summary>
        /// 网络加密解密算法类型
        /// </summary>
        public Wlan.Dot11CipherAlgorithm CipherAlgorithmType { get; set; }
        /// <summary>
        /// 标识
        /// </summary>
        public Wlan.Dot11Ssid SSid { get; set; }
        /// <summary>
        /// WlanInterface
        /// </summary>
        public WlanClient.WlanInterface WlanInterface { get; set; }
    }
    
    public class WlanProfileItem
    {
        public string ProfileName { get; set; }
        public string XmlContent { get; set; }
    }
}