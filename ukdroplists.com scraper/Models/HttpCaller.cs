using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace scrapingTemplateV51.Models
{
    public class HttpCaller
    {
        HttpClient _httpClient;
        readonly HttpClientHandler _httpClientHandler = new HttpClientHandler()
        {
            CookieContainer = new CookieContainer(),
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        public HttpCaller()
        {
            _httpClient = new HttpClient(_httpClientHandler){Timeout = TimeSpan.FromMinutes(5)};
            //_httpClient.DefaultRequestHeaders.Add("Host", "ukdroplists.com");
            //_httpClient.DefaultRequestHeaders.Add("X-Requested-With","XMLHttpRequest");
            //_httpClient.DefaultRequestHeaders.Add("Referer","https://ukdroplists.com");
            //_httpClient.DefaultRequestHeaders.Add("Accept","*/*");
            //_httpClient.DefaultRequestHeaders.Add("Connection","keep-alive");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36");
        }
        public async Task<HtmlDocument> GetDoc(string url, int maxAttempts = 1)
        {
            var html = await GetHtml(url, maxAttempts);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }
        public async Task<string> GetHtml(string url, int maxAttempts = 1)
        {
            int tries = 0;
            do
            {
                try
                {
                    var response = await _httpClient.GetAsync(url);
                    byte[] byteArray = await response.Content.ReadAsByteArrayAsync();
                    string html = Encoding.UTF8.GetString(byteArray);
                    return html;
                }
                catch (WebException ex)
                {
                    var errorMessage = "";
                    try
                    {
                        errorMessage = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    }
                    catch (Exception)
                    {
                    }
                    tries++;
                    if (tries == maxAttempts)
                    {
                        throw new Exception(ex.Status + " " + ex.Message + " " + errorMessage);
                    }
                    await Task.Delay(2000);
                }
            } while (true);
        }
        public async Task<Stream> GetStream(string url, int maxAttempts = 5)
        {
            int tries = 0;
            do
            {
                try
                {
                    var response = await _httpClient.GetAsync(url);
                    //byte[] byteArray = await response.Content.ReadAsByteArrayAsync();
                    //string html = Encoding.UTF8.GetString(byteArray);
                    return await response.Content.ReadAsStreamAsync();
                }
                catch (WebException ex)
                {
                    var errorMessage = "";
                    try
                    {
                        errorMessage = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    }
                    catch (Exception)
                    {
                    }
                    tries++;
                    if (tries == maxAttempts)
                    {
                        throw new Exception(ex.Status + " " + ex.Message + " " + errorMessage);
                    }
                    await Task.Delay(2000);
                }
            } while (true);
        }
        public async Task<string> PostJson(string url, string json, int maxAttempts = 1)
        {
            int tries = 0;
            do
            {
                try
                {
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    // content.Headers.Add("x-appeagle-authentication", Token);
                    var r = await _httpClient.PostAsync(url, content);
                    var s = await r.Content.ReadAsStringAsync();
                    return (s);
                }
                catch (WebException ex)
                {
                    var errorMessage = "";
                    try
                    {
                        errorMessage = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    }
                    catch (Exception)
                    {
                    }
                    tries++;
                    if (tries == maxAttempts)
                    {
                        throw new Exception(ex.Status + " " + ex.Message + " " + errorMessage);
                    }
                    await Task.Delay(2000);
                }
            } while (true);

        }
        public async Task<string> PostFormData(string url, List<KeyValuePair<string, string>> formData, int maxAttempts = 1)
        {
            var formContent = new FormUrlEncodedContent(formData);
            int tries = 0;
            do
            {
                try
                {
                    var response = await _httpClient.PostAsync(url, formContent);
                    byte[] byteArray = await response.Content.ReadAsByteArrayAsync();
                    string html = Encoding.UTF8.GetString(byteArray);
                    return html;
                }
                catch (WebException ex)
                {
                    var errorMessage = "";
                    try
                    {
                        errorMessage = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    }
                    catch (Exception)
                    {
                    }
                    tries++;
                    if (tries == maxAttempts)
                    {
                        throw new Exception(ex.Status + " " + ex.Message + " " + errorMessage);
                    }
                    await Task.Delay(2000);
                }
            } while (true);
        }
    }
}
