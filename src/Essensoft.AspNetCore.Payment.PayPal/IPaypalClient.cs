using System.Threading.Tasks;

namespace Essensoft.AspNetCore.Payment.PayPal
{
    public interface IPaypalClient
    {
        /// <summary>
        /// 执行PayPal公开API请求。
        /// </summary>
        /// <typeparam name="T">领域对象</typeparam>
        /// <param name="request">具体的Alipay API请求</param>
        /// <returns>领域对象</returns>
        Task<T> ExecuteAsync<T>(IPaypalRequest<T> request) where T : PaypalResponse;
    }
}