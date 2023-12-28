using WordsListedModelView;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System;

namespace tdic.InternetConnectionLayer.WebAPI
{
    public static class OnlineDictionary
    {
        public static async Task<List<Word>> GetWordFromAPI(string word)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync("https://api.dictionaryapi.dev/api/v2/entries/en/" + word);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        
                        List<Root> roots = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Root>>(jsonResponse);

                        List<Word> words = roots.Select(root => new Word
                        {
                            English = root.word,
                            Translation = "",
                            SourceUrl = root.sourceUrls.FirstOrDefault(s => s.Contains("en.wiktionary.org")),
                            Phonetics = root.phonetics.Select(SearchedPhonetic =>
                            {
                                var phonetic = new Phonetic
                                {
                                    Text = SearchedPhonetic.text,
                                    Audio = SearchedPhonetic.audio,
                                    Language = string.Empty
                                };

                                if (SearchedPhonetic.audio.EndsWith("uk.mp3"))
                                {
                                    phonetic.Language = "uk";
                                }
                                else if (SearchedPhonetic.audio.EndsWith("us.mp3"))
                                {
                                    phonetic.Language = "us";
                                }
                                else if (SearchedPhonetic.audio.EndsWith("au.mp3"))
                                {
                                    phonetic.Language = "au";
                                }

                                return phonetic;
                            }).ToList(),
                            Meanings = root.meanings.Select(meaning => new Meaning
                            {
                                PartOfSpeech = meaning.partOfSpeech,
                                Definitions = meaning.definitions.Select(def => new Definition
                                {
                                    definition = def.definition,
                                    Example = def.example
                                }).ToList()
                            }).ToList()
                        }).ToList();

                        return words;
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
    }

    #region Root Class

    class Definitionr
    {
        public string definition { get; set; }
        public List<object> synonyms { get; set; }
        public List<object> antonyms { get; set; }
        public string example { get; set; }
    }
    class License
    {
        public string name { get; set; }
        public string url { get; set; }
    }
    class Meaningr
    {
        public string partOfSpeech { get; set; }
        public List<Definitionr> definitions { get; set; }
        public List<string> synonyms { get; set; }
        public List<object> antonyms { get; set; }
    }
    class Phoneticr
    {
        public string text { get; set; }
        public string audio { get; set; }
        public string sourceUrl { get; set; }
        public License license { get; set; }
    }
    class Root
    {
        public string word { get; set; }
        public string phonetic { get; set; }
        public List<Phoneticr> phonetics { get; set; }
        public List<Meaningr> meanings { get; set; }
        public License license { get; set; }
        public List<string> sourceUrls { get; set; }
    }

    #endregion

}