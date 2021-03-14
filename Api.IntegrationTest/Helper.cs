using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Services.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WebFramework.Api;

namespace Api.IntegrationTest
{
    static public class Helper
    {
        public static StringContent GetStringContent(object obj)
            => new StringContent(JsonConvert.SerializeObject(obj), Encoding.Default, "application/json");

        public static HttpClient AddAuth(this HttpClient Client, string token)
        {
            //Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return Client;
        }

        public static HttpRequestMessage AddJwtToken(this HttpRequestMessage request, string token)
        {
            request.Headers.Add("Cookie", new CookieHeaderValue("JWT_TOKEN", token).ToString());
            return request;
        }
    }

    public class BaseIntegrationTest
    {
        private readonly HttpClient client;

        public BaseIntegrationTest(HttpClient client)
        {
            this.client = client;
        }

        public async Task<string> GetJwtToken(string email = "haseli2684@gmail.com", string password = "123456A1!")
        {
            var loginRequest = new
            {
                Url = "/api/v1/user/token",
                Body = new LoginModel
                {
                    Email = email,
                    Password = password
                }
            };

            var loginResponse = await client.PostAsync(loginRequest.Url, Helper.GetStringContent(loginRequest.Body));
            var content = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonConvert.DeserializeObject<ApiResult<string>>(content);

            if (loginResult != null)
            {
                if (loginResult.IsSuccess) return loginResult.Data;

                throw new Exception($"Login Failed!{loginResult.Message}");
            }

            throw new Exception("Login Failed!");
        }
        public async Task<HttpRequestMessage> Login(string username, string password)
        {
            var loginRequest = new
            {
                Url = "/api/v1/user/token",
                Body = new LoginModel
                {
                    Email = username,
                    Password = password
                }
            };

            var loginResponse = await client.PostAsync(loginRequest.Url, Helper.GetStringContent(loginRequest.Body));
            var content = await loginResponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ApiResult<string>>(content);

            if (!result.IsSuccess)
            {
                throw new Exception($"Login Failed,{System.Environment.NewLine}{result.Message}");
            }

            var request = new HttpRequestMessage();
            request.Headers.Add("Cookie", new CookieHeaderValue("JWT_TOKEN", result.Data).ToString());
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {result.Data}");

            return request;
        }
    }
}
