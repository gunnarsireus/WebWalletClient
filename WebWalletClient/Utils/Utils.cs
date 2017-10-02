using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace WebWalletClient
{
	public static class Utils
	{
		public static async Task<List<T>> GetItems<T>(HttpClient client, string url)
		{
			var response = await client.GetAsync(url);
			var items = new List<T>();
			if (response.IsSuccessStatusCode)
				items = await response.Content.ReadAsAsync<List<T>>();

			return items;
		}


		public static async Task<T> GetItem<T>(HttpClient client, string url)
		{
			var response = await client.GetAsync(url);
			var item = default(T);
			if (response.IsSuccessStatusCode)
				item = await response.Content.ReadAsAsync<T>();

			return item;
		}

		// INSERT
		public static async Task<T> Post<T>(HttpClient client, string url, object data)
		{
			var httpContent = new StringContent(JsonConvert.SerializeObject(data));
			httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			var response = await client.PostAsync(url, httpContent);
			var content = await response.Content.ReadAsStringAsync();
			return await Task.Run(() => JsonConvert.DeserializeObject<T>(content));
		}
		public static async Task<T> Put<T>(HttpClient client, string url, object data)
		{
			var httpContent = new StringContent(JsonConvert.SerializeObject(data));
			httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			var response = await client.PutAsync(url, httpContent);
			var content = await response.Content.ReadAsStringAsync();
			return await Task.Run(() => JsonConvert.DeserializeObject<T>(content));
		}

		public static async Task<T> Delete<T>(HttpClient client, string url)
		{
			var response = await client.DeleteAsync(url);
			var content = await response.Content.ReadAsStringAsync();
			return await Task.Run(() => JsonConvert.DeserializeObject<T>(content));
		}

	}
}