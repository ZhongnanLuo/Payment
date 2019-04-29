using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;

namespace Essensoft.AspNetCore.Payment.PayPal
{
    public class PaypalClient: IPaypalClient
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IOptionsSnapshot<PaypalOptions> _optionsSnapshotAccessor;

        public PaypalClient(ILogger<PaypalClient> logger,
            IHttpClientFactory clientFactory,
            IOptionsSnapshot<PaypalOptions> optionsAccessor)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _optionsSnapshotAccessor = optionsAccessor;
        }
        #region Members

        public async Task<T> ExecuteAsync<T>(IPaypalRequest<T> request) where T : PaypalResponse
        {
            return await ExecuteAsync(request, null);
        }

        public async Task<T> ExecuteAsync<T>(IPaypalRequest<T> request, string optionsName) where T : PaypalResponse
        {
            return await ExecuteAsync(request, optionsName, null, null);
        }

        public async Task<T> ExecuteAsync<T>(IPaypalRequest<T> request, string optionsName, string accessToken) where T : PaypalResponse
        {
            return await ExecuteAsync(request, optionsName, accessToken, null);
        }

        public async Task<T> ExecuteAsync<T>(IPaypalRequest<T> request, string optionsName, string accessToken, string appAuthToken) where T : PaypalResponse
        {
            var options = _optionsSnapshotAccessor.Get(optionsName);
            throw new System.NotImplementedException();
        }
        #endregion

    }
}