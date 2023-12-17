using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace tdic.InternetConnectionLayer.WebAPI
{
    public class OnlineTranslate
    {
        public static async Task<string> Translate(string text, string toLanguage)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    //string uri = "https://api.mymemory.translated.net/get?q=" + text + "&langpair=" + fromLanguage + "|" + toLanguage;
                    
                    string Token = "756529:6564bdf9f21c1";
                    string action = "google";

                    string uri = "https://one-api.ir/translate/?token=" + Token + "&action=" + action + "&lang=" + toLanguage + "&q=" + text;

                    HttpResponseMessage response = await client.GetAsync(uri);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        Root root = JsonConvert.DeserializeObject<Root>(jsonResponse);

                        return root.result;
                    }
                    else
                    {
                        throw new Exception("Error: " + response.StatusCode);
                    }
                }
                catch (HttpRequestException e)
                {
                    if (e.Message == "The remote server returned an error: (404) Not Found.")
                    {
                        throw new Exception("Not Found");
                    }
                    else if (e.Message == "The remote name could not be resolved: 'api.dictionaryapi.dev'")
                    {
                        throw new Exception("The remote name could not be resolved");
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }

        #region root classes

        public class Root
        {
            public int status { get; set; }
            public string result { get; set; }
        }


        #endregion
    }
}