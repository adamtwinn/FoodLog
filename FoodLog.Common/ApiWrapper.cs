using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FoodLog.DTOs;
using Newtonsoft.Json;

namespace FoodLog.Common
{
    public class ApiWrapper : IApiWrapper
    {
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private static readonly HttpClient _client = new HttpClient();

        private const string Server = "http://lonhapp02.ttint.com:5000";
        
        public async Task <bool> Delete(EntryViewModel entryViewModel)
        {
            if (entryViewModel.EntryId==0)
                return false;

            try
            {
                await _lock.WaitAsync();

                var response = await _client.DeleteAsync($"{Server}/api/entries/{entryViewModel.EntryId}");

                return response.IsSuccessStatusCode;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<IList<EntryViewModel>> GetEntries(bool forceRefresh = false)
        {
            var uri = $"{Server}/api/entries";

            var result = await Get<List<EntryDTO>>(uri, CancellationToken.None);

            return result.Select(c => EntryMapper.Map(c, new EntryViewModel(), e => e.Updated = false)).ToList();
        }

        public async Task<bool> Save(EntryViewModel entryViewModel)
        {
            var serializedItem = JsonConvert.SerializeObject(EntryMapper.Map(entryViewModel, new EntryDTO()));

            if (entryViewModel.EntryId == 0)
            {
                try
                {
                    await _lock.WaitAsync();

                    var response = await _client.PostAsync($"{Server}/api/entries", new StringContent(serializedItem, Encoding.UTF8, "application/json"));

                    return response.IsSuccessStatusCode;
                }
                finally
                {
                    _lock.Release();
                }
            }

            try
            {
                await _lock.WaitAsync();

                var response = await _client.PutAsync($"{Server}/api/entries/{entryViewModel.EntryId}", new StringContent(serializedItem, Encoding.UTF8, "application/json"));

                return response.IsSuccessStatusCode;
            }
            finally
            {
                _lock.Release();
            }
        }

        private static async Task<T> Get<T>(string uri, CancellationToken token)
        {
            try
            {
                Messenger.Instance.NotifyColleagues("Log", new LogEvent("Waiting for Get...", new Dictionary<string, string> {{"Uri", uri}}));
                await _lock.WaitAsync(token);
                Messenger.Instance.NotifyColleagues("Log", new LogEvent("Running Get...", new Dictionary<string, string> { { "Uri", uri } }));

                var response = await _client.GetAsync(uri, token);

                Messenger.Instance.NotifyColleagues("Log", new LogEvent("Get Responded...", new Dictionary<string, string> { { "Uri", uri } }));

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
                }

                Report(response.StatusCode + ": " + uri);

                return default(T);
            }
            finally
            {
                _lock.Release();
                Messenger.Instance.NotifyColleagues("Log", new LogEvent("Get Unlocked...", new Dictionary<string, string> { { "Uri", uri } }));
            }
        }

        private static void Report(string message, [CallerMemberName] string propertyName = null)
        {
            Messenger.Instance.NotifyColleagues("Exception", new Exception("WebAPI Exception: " + message + "; from " + propertyName));
        }
    }
}
