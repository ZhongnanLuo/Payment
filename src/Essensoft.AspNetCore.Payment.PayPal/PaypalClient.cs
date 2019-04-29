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

        #region IAlipayClient Members

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
            var apiVersion = string.IsNullOrEmpty(request.GetApiVersion()) ? options.Version : request.GetApiVersion();

            // 添加协议级请求参数
            var txtParams = new AlipayDictionary(request.GetParameters())
            {
                { METHOD, request.GetApiName() },
                { VERSION, apiVersion },
                { APP_ID, options.AppId },
                { FORMAT, options.Format },
                { TIMESTAMP, DateTime.Now },
                { ACCESS_TOKEN, accessToken },
                { SIGN_TYPE, options.SignType },
                { TERMINAL_TYPE, request.GetTerminalType() },
                { TERMINAL_INFO, request.GetTerminalInfo() },
                { PROD_CODE, request.GetProdCode() },
                { CHARSET, options.Charset }
            };

            // 序列化BizModel
            txtParams = SerializeBizModel(txtParams, request);

            if (!string.IsNullOrEmpty(request.GetNotifyUrl()))
            {
                txtParams.Add(NOTIFY_URL, request.GetNotifyUrl());
            }

            if (!string.IsNullOrEmpty(appAuthToken))
            {
                txtParams.Add(APP_AUTH_TOKEN, appAuthToken);
            }

            if (request.GetNeedEncrypt())
            {

                if (string.IsNullOrEmpty(txtParams[BIZ_CONTENT]))
                {
                    throw new AlipayException("api request Fail ! The reason: encrypt request is not supported!");
                }

                if (string.IsNullOrEmpty(options.EncyptKey) || string.IsNullOrEmpty(options.EncyptType))
                {
                    throw new AlipayException("encryptType or encryptKey must not null!");
                }

                if (!"AES".Equals(options.EncyptType))
                {
                    throw new AlipayException("api only support Aes!");

                }

                var encryptContent = AES.Encrypt(txtParams[BIZ_CONTENT], options.EncyptKey, AlipaySignature.AES_IV, CipherMode.CBC, PaddingMode.PKCS7);
                txtParams.Remove(BIZ_CONTENT);
                txtParams.Add(BIZ_CONTENT, encryptContent);
                txtParams.Add(ENCRYPT_TYPE, options.EncyptType);
            }

            // 添加签名参数
            var signContent = AlipaySignature.GetSignContent(txtParams);
            txtParams.Add(SIGN, AlipaySignature.RSASignContent(signContent, options.PrivateRSAParameters, options.SignType));

            var query = AlipayUtility.BuildQuery(txtParams);
            _logger.Log(options.LogLevel, "Request:{query}", query);

            // 是否需要上传文件
            var body = string.Empty;
            using (var client = _clientFactory.CreateClient())
            {
                if (request is IAlipayUploadRequest<T> uRequest)
                {
                    var fileParams = AlipayUtility.CleanupDictionary(uRequest.GetFileParameters());

                    body = await client.DoPostAsync(options.ServerUrl, txtParams, fileParams);
                }
                else
                {
                    body = await client.DoPostAsync(options.ServerUrl, query);
                }
            }

            _logger.Log(options.LogLevel, "Response:{body}", body);

            T rsp = null;
            IAlipayParser<T> parser = null;
            if ("xml".Equals(options.Format))
            {
                parser = new AlipayXmlParser<T>();
                rsp = parser.Parse(body);
            }
            else
            {
                parser = new AlipayJsonParser<T>();
                rsp = parser.Parse(body);
            }

            var item = ParseRespItem(request, body, parser, options.EncyptKey, options.EncyptType);
            rsp = parser.Parse(item.RealContent);

            CheckResponseSign(request, item.RespContent, rsp.IsError, parser, options);

            return rsp;
        }

        private ResponseParseItem ParseRespItem<T>(IAlipayRequest<T> request, string respBody, IAlipayParser<T> parser, string encryptKey, string encryptType) where T : AlipayResponse
        {
            string realContent = null;

            if (request.GetNeedEncrypt())
            {
                realContent = parser.EncryptSourceData(request, respBody, encryptType, encryptKey);
            }
            else
            {
                realContent = respBody;
            }

            var item = new ResponseParseItem
            {
                RealContent = realContent,
                RespContent = respBody
            };
            return item;

        }

        private void CheckResponseSign<T>(IAlipayRequest<T> request, string responseBody, bool isError, IAlipayParser<T> parser, AlipayOptions options) where T : AlipayResponse
        {
            var signItem = parser.GetSignItem(request, responseBody);
            if (signItem == null)
            {
                throw new AlipayException("sign check fail: Body is Empty!");
            }

            if (!isError || isError && !string.IsNullOrEmpty(signItem.Sign))
            {
                var rsaCheckContent = AlipaySignature.RSACheckContent(signItem.SignSourceDate, signItem.Sign, options.PublicRSAParameters, options.SignType);
                if (!rsaCheckContent)
                {
                    if (!string.IsNullOrEmpty(signItem.SignSourceDate) && signItem.SignSourceDate.Contains("\\/"))
                    {
                        var srouceData = signItem.SignSourceDate.Replace("\\/", "/");
                        var jsonCheck = AlipaySignature.RSACheckContent(srouceData, signItem.Sign, options.PublicRSAParameters, options.SignType);
                        if (!jsonCheck)
                        {
                            throw new AlipayException("sign check fail: check Sign and Data Fail JSON also");
                        }
                    }
                    else
                    {
                        throw new AlipayException("sign check fail: check Sign and Data Fail!");
                    }
                }
            }
        }

        #endregion

    }
}