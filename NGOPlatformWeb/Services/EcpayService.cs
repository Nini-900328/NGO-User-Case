using NGOPlatformWeb.Models.Entity;
using System.Text.Json;
using System.Text;
using System.Security.Cryptography;
using System.Net;

namespace NGOPlatformWeb.Services
{
    public class EcpayService
    {
        private readonly NGODbContext _context;
        private readonly IConfiguration _configuration;

        // ECPay 測試環境設定
        private readonly string _merchantId = "2000132"; // 測試商店代號
        private readonly string _hashKey = "5294y06JbISpM5x9"; // 測試 HashKey
        private readonly string _hashIv = "v77hoKGq4kWxNNIS"; // 測試 HashIV
        private readonly string _paymentUrl = "https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5"; // 測試環境

        public EcpayService(NGODbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public string CreatePayment(UserOrder order, string returnUrl, string clientBackUrl)
        {
            try
            {
                // 建立 ECPay 交易記錄
                var ecpayTransaction = new EcpayTransaction
                {
                    UserOrderId = order.UserOrderId,
                    Status = "Pending",
                    CreatedDateTime = DateTime.Now
                };
                
                _context.EcpayTransactions.Add(ecpayTransaction);
                _context.SaveChanges();

            // 建立付款參數
            var parameters = new Dictionary<string, string>
            {
                ["MerchantID"] = _merchantId,
                ["MerchantTradeNo"] = order.OrderNumber!,
                ["MerchantTradeDate"] = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                ["PaymentType"] = "aio",
                ["TotalAmount"] = ((int)order.TotalPrice).ToString(),
                ["TradeDesc"] = $"NGO物資認購-{order.OrderNumber}",
                ["ItemName"] = "物資認購",
                ["ReturnURL"] = returnUrl,
                ["ChoosePayment"] = "ALL",
                ["ClientBackURL"] = clientBackUrl,
                ["NeedExtraPaidInfo"] = "N",
                ["EncryptType"] = "1"
            };

            // 生成檢查碼
            var checkMacValue = GenerateCheckMacValue(parameters, _hashKey, _hashIv);
            parameters["CheckMacValue"] = checkMacValue;

            // 產生 HTML 表單
            var formHtml = GeneratePaymentForm(parameters);

                // 更新 ECPay 交易記錄
                ecpayTransaction.EcpayTradeNo = order.OrderNumber;
                ecpayTransaction.Status = "Processing";
                _context.SaveChanges();

                return formHtml;
            }
            catch (Exception ex)
            {
                throw new Exception($"建立付款時發生錯誤: {ex.Message}", ex);
            }
        }

        public bool ProcessCallback(Dictionary<string, string> parameters)
        {
            try
            {
                var merchantTradeNo = parameters.GetValueOrDefault("MerchantTradeNo");
                var rtnCode = parameters.GetValueOrDefault("RtnCode");
                var tradeNo = parameters.GetValueOrDefault("TradeNo");

                if (string.IsNullOrEmpty(merchantTradeNo))
                    return false;

                // 查找訂單和交易記錄
                var order = _context.UserOrders.FirstOrDefault(o => o.OrderNumber == merchantTradeNo);
                var ecpayTransaction = _context.EcpayTransactions.FirstOrDefault(e => e.EcpayTradeNo == merchantTradeNo);

                if (order == null || ecpayTransaction == null)
                    return false;

                // 更新交易記錄
                ecpayTransaction.ResponseData = JsonSerializer.Serialize(parameters);
                
                if (rtnCode == "1") // 付款成功
                {
                    ecpayTransaction.Status = "Success";
                    order.PaymentStatus = "已付款";
                }
                else // 付款失敗
                {
                    ecpayTransaction.Status = "Failed";
                    order.PaymentStatus = "付款失敗";
                }

                _context.SaveChanges();
                return rtnCode == "1";
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GenerateCheckMacValue(Dictionary<string, string> parameters, string hashKey, string hashIv)
        {
            // 移除 CheckMacValue 和空值參數
            var filteredParams = parameters
                .Where(p => p.Key != "CheckMacValue" && !string.IsNullOrEmpty(p.Value))
                .OrderBy(p => p.Key)
                .ToList();

            // 組合字串
            var paramString = string.Join("&", filteredParams.Select(p => $"{p.Key}={p.Value}"));
            var rawString = $"HashKey={hashKey}&{paramString}&HashIV={hashIv}";

            // URL 編碼
            rawString = WebUtility.UrlEncode(rawString)?.ToLower() ?? string.Empty;

            // MD5 雜湊
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(rawString));
                return BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }
        }

        private string GeneratePaymentForm(Dictionary<string, string> parameters)
        {
            var formBuilder = new StringBuilder();
            formBuilder.AppendLine("<form id='ecpayForm' method='post' action='" + _paymentUrl + "'>");
            
            foreach (var param in parameters)
            {
                formBuilder.AppendLine($"<input type='hidden' name='{param.Key}' value='{param.Value}' />");
            }
            
            formBuilder.AppendLine("<input type='submit' value='前往付款' style='padding: 10px 20px; background: #007bff; color: white; border: none; border-radius: 5px; cursor: pointer;' />");
            formBuilder.AppendLine("</form>");
            
            return formBuilder.ToString();
        }
    }
}