
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AliyunWooyang
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }
        private string lastIp = "";
        private string accessKeyId = "";
        private string accessKeySecret = "";
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                try
                {
                    var ip = GetMyIP();
                    _logger.LogInformation($"��ǰip�ǣ�{GetMyIP()}");
                    if (!lastIp.Equals(ip))
                    {
                        lastIp = ip;
                        Cdn("www.zl771.cn", 10086);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
                await Task.Delay(10*60*1000, stoppingToken);
            }
        }
        private void Cdn(string domain,int port)
        {
            var ccc = "[{\"content\":\"" + lastIp + "\",\"type\":\"ipaddr\",\"priority\":\"20\",\"port\":" + port + ",\"weight\":\"15\"}]";

            AlibabaCloud.OpenApiClient.Models.Config config = new AlibabaCloud.OpenApiClient.Models.Config
            {
                // ����AccessKey ID
                AccessKeyId = accessKeyId,
                // ����AccessKey Secret
                AccessKeySecret = accessKeySecret,
            };
            // ���ʵ�����
            config.Endpoint = "cdn.aliyuncs.com";
            var client= new AlibabaCloud.SDK.Cdn20180510.Client(config);
            AlibabaCloud.SDK.Cdn20180510.Models.ModifyCdnDomainRequest modifyCdnDomainRequest = new AlibabaCloud.SDK.Cdn20180510.Models.ModifyCdnDomainRequest
            {
                DomainName = domain,
                Sources = ccc,
            };
            // ���ƴ������������д�ӡ API �ķ���ֵ
            var rsp=client.ModifyCdnDomain(modifyCdnDomainRequest);
            _logger.LogInformation(rsp.Body.ToString());
        }
        /// <summary>
        /// ��html��ͨ�������ҵ�ip��Ϣ(ֻ֧��ipv4��ַ)
        /// </summary>
        /// <param name="pageHtml"></param>
        /// <returns></returns>
        public static string GetMyIP()
        {
            HttpClient client = new HttpClient();
            
           var stream = client.GetStreamAsync("https://pv.sohu.com/cityjson").Result;
            var pageHtml = "";
            using (var sr = new StreamReader(stream, Encoding.GetEncoding("iso-8859-1")))
            {
                pageHtml = sr.ReadToEnd();
            }
            //��֤ipv4��ַ
            string reg = @"(?:(?:(25[0-5])|(2[0-4]\d)|((1\d{2})|([1-9]?\d)))\.){3}(?:(25[0-5])|(2[0-4]\d)|((1\d{2})|([1-9]?\d)))";
            string ip = "";
            Match m = Regex.Match(pageHtml, reg);
            if (m.Success)
            {
                ip = m.Value;
            }
            return ip;
        }
    }
}
