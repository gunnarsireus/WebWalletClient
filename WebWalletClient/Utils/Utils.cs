using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebWalletClient
{
	public static class Utils
	{
		public static async Task<T> Get<T>(HttpClient client, string url)
		{
			var response = await client.GetAsync(url);
			var content = await response.Content.ReadAsStringAsync();
			return await Task.Run(() => JsonConvert.DeserializeObject<T>(content));
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

		// UPDATE
		public static async Task<T> Put<T>(HttpClient client, string url, object data)
		{
			var httpContent = new StringContent(JsonConvert.SerializeObject(data));
			httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			var response = await client.PutAsync(url, httpContent);
			var content = await response.Content.ReadAsStringAsync();
			return await Task.Run(() => JsonConvert.DeserializeObject<T>(content));
		}

		// DELETE

		public static async Task<T> Delete<T>(HttpClient client, string url)
		{
			var response = await client.DeleteAsync(url);
			var content = await response.Content.ReadAsStringAsync();
			return await Task.Run(() => JsonConvert.DeserializeObject<T>(content));
		}
	}
}