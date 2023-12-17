using System;
using System.Data;
using System.Data.SQLite;

namespace tdic.DataContext
{
    public class UnitOfWork : IDisposable
    {
        #region ConnectionString

        protected static string connectionString = "Data Source=Words.db;Version=3;";

        #endregion

        IDbConnection dbConnection = new SQLiteConnection(connectionString);
         
        private IWordsRepository.IWordsRepository _wordsRepository;

        public IWordsRepository.IWordsRepository WordsRepository
        {
            get
            {
                if(_wordsRepository == null)
                {
                    _wordsRepository = new WordsRepository.WordsRepository(dbConnection);
                }

                return _wordsRepository;
            }
        }

        public void Dispose()
        {
            dbConnection.Dispose();
        }
    }
}
