using System;
using System.Collections.Generic;
using WordsDBModelView;

namespace tdic.IWordsRepository
{
    public interface IWordsRepository
    {
        void CreateSqliteFie();

        void CreateWord(Words word, List<Phonetics> phonetics, List<Meanings> meanings, List<Definitions> definitions);
        
        void CreatePhonetic(Phonetics phonetic);

        void CreateMeaning(Meanings meaning);

        void CreateDefinition(Definitions definition);


        List<Words> ReadWords();

        Words ReadWord(string WordID);

        List<Phonetics> ReadPhonetics(string WordID);

        List<Meanings> ReadMeanings(string WordID);

        List<Definitions> ReadDefinitions(string MeaningID);

        List<Words> ReadLimit100Words();
        
        bool WordExist(string Word);

        List<Words> ReadWordByFilter(string PartOfSpeech);


        void UpdateWord(Words word);

        void UpdatePhonetics(List<Phonetics> phonetics);

        void UpdateMeanings(List<Meanings> meanings);

        void UpdateDefinitions(List<Definitions> definitions);

        void UpdateWordRate(byte Rate, string WordID);


        void DeleteWord(string WordID);

        void DeletePhonetics(List<Phonetics> phonetics);

        void DeleteMeanings(List<Meanings> meanings);

        void DeleteDefinitions(List<Definitions> definitions);

    }
}
