using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using bd_lite;
using System.Data.SQLite;
using static System.Data.Entity.Infrastructure.Design.Executor;
using static System.Net.Mime.MediaTypeNames;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ManagePeople.create();

            ManagePeople.Insert();
            var list = ManagePeople.GetAllWords();
            foreach (var word in list)
            {
                Console.WriteLine($"ID: {word.WordID}, English: {word.English}, Persian: {word.Persian}");
            }

            Console.ReadKey();
        }
    }

    public class ManagePeople
    {
        public static string connectionString = "Data Source=Words.db;Version=3;";

        public static void create()
        {
            using (IDbConnection dbConnection = new SQLiteConnection(connectionString))
            {
                dbConnection.Open();

                // جدول Words
                dbConnection.Execute(
                    @"CREATE TABLE IF NOT EXISTS Words (
                    WordID TEXT PRIMARY KEY,
                    English TEXT,
                    Persian TEXT)");

                // جدول Phonetics
                dbConnection.Execute(
                    @"CREATE TABLE IF NOT EXISTS Phonetics (
                    WordID TEXT,
                    Text TEXT,
                    Audio TEXT,
                    Language TEXT,
                    PRIMARY KEY(WordID),
                    FOREIGN KEY(WordID) REFERENCES Words(WordID)
                        ON DELETE CASCADE ON UPDATE CASCADE)");

                // جدول Meanings
                dbConnection.Execute(
                    @"CREATE TABLE IF NOT EXISTS Meanings (
                    WordID TEXT,
                    PartOfSpeech TEXT,
                    MeaningID TEXT,
                    PRIMARY KEY(WordID, MeaningID),
                    FOREIGN KEY(WordID) REFERENCES Words(WordID)
                        ON DELETE CASCADE ON UPDATE CASCADE)");

                // جدول Definitions
                dbConnection.Execute(
                    @"CREATE TABLE IF NOT EXISTS Definitions (
                    MeaningID TEXT PRIMARY KEY,
                    Text TEXT,
                    Example TEXT,
                    FOREIGN KEY(MeaningID) REFERENCES Meanings(MeaningID)
                        ON DELETE CASCADE ON UPDATE CASCADE)");

            }
        }

        public static List<Words> GetAllWords()
        {
            var words = new List<Words>();
            using (IDbConnection dbConnection = new SQLiteConnection(connectionString))
            {
                dbConnection.Open();

                // اجرای کوئری برای انتخاب تمام رکوردها از جدول Words
                words = dbConnection.Query<Words>("SELECT * from Words").AsList();
            }

            return words;
        }

        public static void Insert()
        {
            using (IDbConnection dbConnection = new SQLiteConnection(connectionString))
            {
                dbConnection.Open();
                var words = new Words
                {
                    WordID = Guid.NewGuid().ToString(),
                    English = "close",
                    Persian = "بسته"
                };
                dbConnection.Execute("INSERT INTO Words (WordID, English, Persian) VALUES (@WordID, @English, @Persian)", words);

            }
        }
    }
}
