using System.Data;
using Dapper;
using WordsDBModelView;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using WordsListedModelView;

namespace tdic.WordsRepository
{
    public class WordsRepository : IWordsRepository.IWordsRepository
    {
        readonly IDbConnection db;
        public WordsRepository(IDbConnection dbConnection)
        {
            this.db = dbConnection;
            db.Open();
        }

        #region Create

        public void CreateSqliteFie()
        {
            // جدول Words
            db.Execute(
                @"CREATE TABLE IF NOT EXISTS Words (
                    WordID TEXT PRIMARY KEY,
                    English TEXT,
                    Translation TEXT
	                Rate TEXT,
	                SourceUrl TEXT)");

            // جدول Phonetics
            db.Execute(
                @"CREATE TABLE IF NOT EXISTS Phonetics (
                    WordID TEXT,
                    PhoneticID TEXT,
                    Text TEXT,
                    Audio TEXT,
                    Language TEXT)");

            // جدول Meanings
            db.Execute(
                @"CREATE TABLE IF NOT EXISTS Meanings (
                    WordID TEXT,
                    PartOfSpeech TEXT,
                    MeaningID TEXT)");

            // جدول Definitions
            db.Execute(
                @"CREATE TABLE IF NOT EXISTS Definitions (
                    MeaningID TEXT,
                    DefinitionID TEXT
                    Text TEXT,
                    Example TEXT)");

        }

        public void CreateWord(Words word, List<Phonetics> phonetics, List<Meanings> meanings, List<Definitions> definitions)
        {
            db.Execute("INSERT INTO Words (WordID, English, Translation, SourceUrl) VALUES (@WordID, @English, @Translation, @SourceUrl)", word);
            db.Execute("INSERT INTO Phonetics (WordID, PhoneticID, Text, Audio, Language) VALUES (@WordID, @PhoneticID, @Text, @Audio, @Language)", phonetics);
            db.Execute("INSERT INTO Meanings (WordID, PartOfSpeech, MeaningID) VALUES (@WordID, @PartOfSpeech, @MeaningID)", meanings);
            db.Execute("INSERT INTO Definitions (MeaningID, DefinitionID, Text, Example) VALUES (@MeaningID, @DefinitionID,  @Text, @Example)", definitions);
        }

        public void CreatePhonetic(Phonetics phonetic)
        {
            db.Execute("INSERT INTO Phonetics (WordID, PhoneticID, Text, Audio, Language) VALUES (@WordID, @PhoneticID, @Text, @Audio, @Language)", phonetic);
        }

        public void CreateMeaning(Meanings meaning)
        {
            db.Execute("INSERT INTO Meanings (WordID, PartOfSpeech, MeaningID) VALUES (@WordID, @PartOfSpeech, @MeaningID)", meaning);
        }

        public void CreateDefinition(Definitions definition)
        {
            db.Execute("INSERT INTO Definitions (MeaningID, DefinitionID, Text, Example) VALUES (@MeaningID, @DefinitionID, @Text, @Example)", definition);
        }

        #endregion

        #region Read

        public List<Words> ReadWords()
        {
            var words = db.Query<Words>("SELECT * FROM Words");

            return words.ToList();
        }

        public List<Words> ReadLimit100Words()
        {
            var words = db.Query<Words>("SELECT * FROM Words LIMIT 100");

            return words.ToList();
        }

        public Words ReadWord(string WordID)
        {
            var word = db.Query<Words>("SELECT * FROM Words WHERE WordID = @WordID", new { WordID });

            return word.AsList()[0];
        }

        public List<Phonetics> ReadPhonetics(string WordID)
        {
            var Phonetics = db.Query<Phonetics>("SELECT * FROM Phonetics WHERE WordID = @WordID", new { WordID });

            return Phonetics.AsList();
        }

        public List<Meanings> ReadMeanings(string WordID)
        {
            var Meanings = db.Query<Meanings>("SELECT * FROM Meanings WHERE WordID = @WordID", new { WordID });

            return Meanings.AsList();
        }

        public List<Definitions> ReadDefinitions(string MeaningID)
        {
            var Definitions = db.Query<Definitions>("SELECT * FROM Definitions WHERE MeaningID = @MeaningID", new { MeaningID });

            return Definitions.AsList();
        }

        public bool WordExist(string Word)
        {
            int state = db.QueryFirstOrDefault<int>("SELECT CASE WHEN EXISTS (SELECT 1 FROM Words WHERE English = @Word) THEN 1 ELSE 0 END AS Result;", new { Word });

            bool isExist = Convert.ToBoolean(state);

            return isExist;
        }

        public List<Words> ReadWordByFilter(string PartOfSpeech)
        {
            var WordIDs = db.Query<string>("SELECT WordID FROM Meanings WHERE PartOfSpeech = @PartOfSpeech", new { PartOfSpeech });
            List<Words> words = new();
            foreach (var WordID in WordIDs)
            {
                var word = db.Query<Words>("SELECT * FROM Words WHERE WordID = @WordID", new { WordID });
                words.Add(word.ToList()[0]);
            }

            return words;
        }

        public int[] ReadCounts(string WordID)
        {
            int[] counts = new int[3];

            counts[0] = db.ExecuteScalar<int>("SELECT COUNT(*) FROM Phonetics WHERE WordID = @WordID", new { WordID });

            var meanings =ReadMeanings(WordID);

            counts[1] = meanings.Count;

            foreach (var meaning in meanings)
            {
                counts[2] += db.ExecuteScalar<int>("SELECT COUNT(*) FROM Definitions WHERE MeaningID = @MeaningID", new { meaning.MeaningID }); 
            }

            return counts;
        }

        #endregion

        #region Update

        public void UpdateWord(Words word)
        {
            db.Query("UPDATE Words SET English = @English,Translation = @Translation WHERE WordID = @WordID", new { word.English, word.Translation, word.WordID });
        }

        public void UpdatePhonetics(List<Phonetics> phonetics)
        {
            if (phonetics != null)
            {
                List<Phonetics> SPhonetics = ReadPhonetics(phonetics[0].WordID);

                if (SPhonetics != null)
                {
                    var EPhonetics = SPhonetics.Where(s => !phonetics.Any(p => p.PhoneticID == s.PhoneticID)).ToList();

                    DeletePhonetics(EPhonetics);

                    foreach (var phonetic in phonetics)
                    {
                        bool isPhoneticExist = SPhonetics.Exists(ph => ph.PhoneticID == phonetic.PhoneticID);
                        if (isPhoneticExist)
                        {
                            db.Query("UPDATE Phonetics SET Text = @Text,Audio = @Audio WHERE PhoneticID = @PhoneticID", new { phonetic.Text, phonetic.Audio, phonetic.PhoneticID });
                        }
                        else
                        {
                            CreatePhonetic(phonetic);
                        }
                    }
                }
            }
        }

        public void UpdateMeanings(List<Meanings> meanings)
        {
            if (meanings != null)
            {
                List<Meanings> SMeanings = ReadMeanings(meanings[0].WordID);

                if (SMeanings != null)
                {
                    var EMeanings = SMeanings.Where(s => !meanings.Any(p => p.MeaningID == s.MeaningID)).ToList();

                    DeleteMeanings(EMeanings);
                }

                foreach (var meaning in meanings)
                {
                    if (SMeanings != null)
                    {
                        bool isPhoneticExist = SMeanings.Exists(m => m.MeaningID == meaning.MeaningID);
                        if (!isPhoneticExist)
                        {
                            CreateMeaning(meaning);
                        }
                    }
                    else
                    {
                        CreateMeaning(meaning);
                    }
                }
            }
        }

        public void UpdateDefinitions(List<Definitions> definitions)
        {
            if (definitions != null)
            {
                foreach (var definition in definitions)
                {
                    List<Definitions> SDefinitions = ReadDefinitions(definition.MeaningID);

                    var EDefinitions = SDefinitions.Where(s => !definitions.Any(p => p.DefinitionID == s.DefinitionID)).ToList();

                    DeleteDefinitions(EDefinitions);

                    bool isPhoneticExist = SDefinitions.Exists(ph => ph.DefinitionID == definition.DefinitionID);
                    if (isPhoneticExist)
                    {
                        db.Query("UPDATE Definitions SET Text = @Text,Example = @Example WHERE DefinitionID = @DefinitionID", new { definition.Text, definition.Example, definition.DefinitionID });
                    }
                    else
                    {
                        CreateDefinition(definition);
                    }
                }
            }
        }

        public void UpdateWordRate(byte Rate, string WordID)
        {
            db.Query("UPDATE Words SET Rate = @Rate WHERE WordID = @WordID", new { Rate, WordID });
        }
        #endregion

        #region Delete

        public void DeleteWord(string WordID)
        {
            db.Query("DELETE FROM Words WHERE WordID = @WordID", new { WordID });

            var phonetics = ReadPhonetics(WordID);
            DeletePhonetics(phonetics);

            var meanings = ReadMeanings(WordID);
            foreach (var meaning in meanings)
            {
                db.Query("DELETE FROM Definitions WHERE MeaningID = @MeaningID", new { meaning.MeaningID });
            }
            db.Query("DELETE FROM Meanings WHERE WordID = @WordID", new { WordID });
        }

        public void DeletePhonetics(List<Phonetics> phonetics)
        {
            foreach (var phonetic in phonetics)
            {
                db.Query("DELETE FROM Phonetics WHERE PhoneticID = @PhoneticID", new { phonetic.PhoneticID });
                if (File.Exists(phonetic.Audio))
                {
                    File.Delete(phonetic.Audio);
                }
            }
        }

        public void DeleteMeanings(List<Meanings> meanings)
        {
            foreach (var meaning in meanings)
            {
                db.Query("DELETE FROM Meanings WHERE MeaningID = @MeaningID", new { meaning.MeaningID });
                List<Definitions> definitions = ReadDefinitions(meaning.MeaningID);
                DeleteDefinitions(definitions);
            }
        }

        public void DeleteDefinitions(List<Definitions> definitions)
        {
            foreach (var definition in definitions)
            {
                db.Query("DELETE FROM Definitions WHERE DefinitionID = @DefinitionID", new { definition.DefinitionID });
            }
        }

        #endregion

    }
}
