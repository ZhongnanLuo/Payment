using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Essensoft.AspNetCore.Payment.PayPal
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAlipay(
           this IServiceCollection services)
        {
            services.AddPayPal(null);
        }

        public static void AddPayPal(
            this IServiceCollection services,
            Action<PaypalOptions> setupAction)
        {
            services.AddScoped<IPaypalClient, PaypalClient>();
            services.AddScoped<IPaypalNotifyClient, PaypalNotifyClient>();
            if (setupAction != null)
            {
                services.Configure(setupAction);
            }
        }
    }
}
