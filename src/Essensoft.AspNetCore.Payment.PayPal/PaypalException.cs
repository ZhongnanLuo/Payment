using System;
using System.Collections.Generic;
using System.Text;

namespace Essensoft.AspNetCore.Payment.PayPal
{
    /// <summary>
    /// Alipay 异常。
    /// </summary>
    public class PaypalException : Exception
    {
        public PaypalException(string messages) : base(messages)
        {
        }
    }
}
