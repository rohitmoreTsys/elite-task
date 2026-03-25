using LazyCache;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Elite.Common.Utilities.SecretVault
{
    public sealed class SecretVault : ISecretVault
    {
        private static readonly Lazy<SecretVault> lazy =
        new Lazy<SecretVault>(() => new SecretVault(), true);

        public static SecretVault Instance { get { return lazy.Value; } }

        private string _clientToken;
        private static readonly IAppCache appCache = new CachingService();

        private SecretVault()
        {   
            if (!appCache.TryGetValue<string>("token", out string token))
            {
                _clientToken = GetVaultToken().Result;
                AddValueToCache("token", _clientToken);
            }
        }

        private void AddValueToCache(string key, string value)
        {
            if (appCache != null)
                appCache.Add(key, value, DateTimeOffset.Now.AddDays(6));
        }

        private string GetValueFromCache(string key)
        {
            if (appCache != null)
                return appCache.Get<string>(key);
            else
            {
                if (string.Compare("token", key) == 0)
                {
                    return GetVaultToken().Result;
                }
            }
            return string.Empty;
        }

        public string GetValuesFromVault(string key)
        {
            var valuefromCache = GetValueFromCache(key);
            if (string.IsNullOrEmpty(valuefromCache))
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("X-Vault-Request", "true");
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetValueFromCache("token"));
                var response = httpClient.GetAsync(Environment.GetEnvironmentVariable("VaultUrl")).Result;
                dynamic res = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                var data = res.data[key];
                AddValueToCache(key, Convert.ToString(data));
                return data;
            }
            return valuefromCache;

        }

        public string GetValuesFromCustomVault(string key)
        {
            var valuefromCache = GetValueFromCache(key);
            if (string.IsNullOrEmpty(valuefromCache))
            {
                var vaultPath = Environment.GetEnvironmentVariable(key);
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("X-Vault-Request", "true");
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetValueFromCache("token"));
                //Getting vault data from Custom Vault
                var response = httpClient.GetAsync(Environment.GetEnvironmentVariable("CustomVaultUrl") + vaultPath).Result;
                dynamic res = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                var data = res.data[key];
                AddValueToCache(key, Convert.ToString(data));
                return data;
            }
            return valuefromCache;
        }

        public async Task<string> GetVaultToken()
        {
            HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("X-Vault-Request", "true");

            string vaultPassword = Encoding.UTF8.GetString(Convert.FromBase64String("ZUxpdGVEYWltbGVyJA=="));
            var requestData = new Dictionary<string, object>
            {
                {"password", vaultPassword }
            };

            var requestContent = requestData != null
                                ? new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8)
                                : null;

            var response = await httpClient.PostAsync(Environment.GetEnvironmentVariable("VaultTokenUrl"), requestContent);
            dynamic res = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
            return res.auth["client_token"];
        }

    }
    public interface ISecretVault
    {
        string GetValuesFromVault(string key);
    }
}
