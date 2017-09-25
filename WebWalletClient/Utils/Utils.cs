using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebWalletClient
{
    public static class Utils
    {
        public static async Task<List<T>> GetItems<T>(HttpResponseMessage response)
        {
            var items = new List<T>();
            if (response.IsSuccessStatusCode)
                items = await response.Content.ReadAsAsync<List<T>>();

            return items;
        }


        public static async Task<T> GetItem<T>(HttpResponseMessage response)
        {
            var item = default(T);
            if (response.IsSuccessStatusCode)
                item = await response.Content.ReadAsAsync<T>();

            return item;
        }
    }
}