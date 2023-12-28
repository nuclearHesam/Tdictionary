using System;
using System.Collections.Generic;
using System.Linq;
using WordsDBModelView;
using WordsListedModelView;

namespace tdic.WordsRepository
{
    /// <summary>
    /// Conver ListedModel to DbModel And conversely
    /// </summary>
    public static class DbModelConvertor
    {
        public static Words WordConvertor(this Word word)
        {
            Words dbWords = new()
            {
                WordID = word.WordID,
                English = word.English,
                Rate = word.Rate,
                SourceUrl = word.SourceUrl
            };

            if (word.Translation != null)
            {
                dbWords.Translation = word.Translation;
            }

            return dbWords;
        }

        public static List<Phonetics> PhoneticsConvertor(this List<Phonetic> phonetics)
        {
            List<Phonetics> dbPhonetics = new ();

            dbPhonetics = phonetics.Select(phonetic => new Phonetics
            {
                PhoneticID = phonetic.PhoneticID,
                WordID = phonetic.WordID,
                Text = phonetic.Text,
                Audio = phonetic.Audio,
                Language = phonetic.Language,
            }).ToList();

            return dbPhonetics;
        }

        public static List<Meanings> MeaningsConvertor(this List<Meaning> meanings)
        {
            List<Meanings> dbMeanings = new();
            Definitions = new List<Definitions>();

            foreach (var meaning in meanings)
            {
                var dbmeanings = new Meanings
                {
                    WordID = meaning.WordID,
                    PartOfSpeech = meaning.PartOfSpeech,
                    MeaningID = meaning.MeaningID,
                };
                dbMeanings.Add(dbmeanings);

                foreach (var definitions in meaning.Definitions)
                {
                    var dbdefinitions = new Definitions
                    {
                        DefinitionID = definitions.DefinitionID,
                        MeaningID = definitions.MeaningID,
                        Text = definitions.definition,
                        Example = definitions.Example
                    };
                    Definitions.Add(dbdefinitions);
                }
            }

            return dbMeanings;
        }

        public static  List<Definitions>? Definitions;

    }

    public static class ListedModelConvertor
    {
        public static Word WordsConvertor(Words word, List<Phonetics> phonetics, List<Meanings> meanings, List<Definitions> definitions)
        {
            Word listedWord = new()
            {
                WordID = word.WordID,
                English = word.English,
                Translation = word. Translation,
                Phonetics = phonetics.Select(phonetic => new Phonetic
                {
                    PhoneticID = phonetic.PhoneticID,
                    WordID = word.WordID,
                    Text = phonetic.Text,
                    Audio = phonetic.Audio,
                    Language = phonetic.Language,
                }).ToList(),
                Meanings = meanings.Select(meaning => new Meaning
                {
                    WordID = meaning.WordID,
                    MeaningID = meaning.MeaningID,
                    PartOfSpeech = meaning.PartOfSpeech,
                    Definitions = definitions.FindAll(definition => definition.MeaningID == meaning.MeaningID).ToList()
                    .Select(definition => new Definition
                    {
                        DefinitionID = definition.DefinitionID,
                        MeaningID = definition.MeaningID,
                        definition = definition.Text,
                        Example = definition.Example,
                    }).ToList(),
                }).ToList(),
                Rate = word.Rate,
                SourceUrl = word.SourceUrl,
            };

            return listedWord;
        }
    }
}
