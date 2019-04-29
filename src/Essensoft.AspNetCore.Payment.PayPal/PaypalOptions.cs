using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Essensoft.AspNetCore.Payment.PayPal
{
    /// <summary>
    /// PayPal 配置选项
    /// </summary>
    public class PaypalOptions
    {
        /// <summary>
        /// 服务地址
        /// </summary>
        public string ServerUrl { get; set; } = "https://api.sandbox.paypal.com";

        /// <summary>
        /// 环境模式
        /// </summary>
        public string Mode { get; set; } = "sandbox";

        /// <summary>
        /// 超时时间 second
        /// </summary>
        public int ConnectionTimeout { get; set; } = 120000;

        /// <summary>
        /// 重试
        /// </summary>
        public int RequestRetries { get; set; } = 1;

        /// <summary>
        /// 客户端ID
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// 客户端密钥
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// 日志等级
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }
}
