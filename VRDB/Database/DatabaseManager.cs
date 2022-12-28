using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using VRDB.Models;

namespace VRDB
{
    public static class DatabaseManager
    {
        #region Properties

        public static Logger Logger { get; set; } = null;

        private static string databasePath;
        public static string DatabasePath
        {
            get => databasePath;
            set => databasePath = value;
        }

        public static bool HasData
        {
            get => RowCount > 0;
        }

        private static int rowCount;
        public static int RowCount
        {
            get => rowCount;
            private set => rowCount = value;
        }

        public static string[] AddressDirections { get; set; }
        public static string[] AddressTypes { get; set; }

        #endregion

        #region Record Methods

        public static void CompareSearch(List<SearchVM> list, BackgroundWorker bw = null, DoWorkEventArgs bwe = null)
        {
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(DatabaseManager).Name}.{Utility.GetCurrentMethod()}:Enter");

            using (var db = new DatabaseConnection(DatabasePath))
            {
                if (Logger != null) { db.Logger = Logger; }
                db.CompareSearch(list, AddressDirections, AddressTypes, bw, bwe);
            }

            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(DatabaseManager).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        public static List<SearchVM> Search(string lastName, string firstName, string birthYear, string gender)
        {
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(DatabaseManager).Name}.{Utility.GetCurrentMethod()}:Enter");

            if (string.IsNullOrEmpty(DatabasePath))
            {
                throw new Exception("DatabasePath must be set before calling any methods.");
            }

            var list = new List<SearchVM>();

            using (var db = new DatabaseConnection(DatabasePath))
            {
                if (Logger != null) { db.Logger = Logger; }
                db.Search(lastName, firstName, birthYear, gender, list);
            }

            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(DatabaseManager).Name}.{Utility.GetCurrentMethod()}:Leave");
            return list;
        }

        public static void RegistrationInsert(Registration item)
        {
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(DatabaseManager).Name}.{Utility.GetCurrentMethod()}:Enter");

            if (string.IsNullOrEmpty(DatabasePath))
            {
                throw new Exception("DatabasePath must be set before calling any methods.");
            }

            using (var db = new DatabaseConnection(DatabasePath))
            {
                if (Logger != null) { db.Logger = Logger; }
                db.RegistrationInsert(item);
            }
            
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(DatabaseManager).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        public static bool RegistrationExists(int id)
        {
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(DatabaseManager).Name}.{Utility.GetCurrentMethod()}:Enter");

            if (string.IsNullOrEmpty(DatabasePath))
            {
                throw new Exception("DatabasePath must be set before calling any methods.");
            }

            var check = false;
            using (var db = new DatabaseConnection(DatabasePath))
            {
                if (Logger != null) { db.Logger = Logger; }
                check = db.RegistrationExists(id);
            }

            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(DatabaseManager).Name}.{Utility.GetCurrentMethod()}:Leave");
            return check;
        }

        public static void StatusUpdate(string filename, long ticks)
        {
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(DatabaseManager).Name}.{Utility.GetCurrentMethod()}:Enter");

            if (string.IsNullOrEmpty(DatabasePath))
            {
                throw new Exception("DatabasePath must be set before calling any methods.");
            }

            using (var db = new DatabaseConnection(DatabasePath))
            {
                if (Logger != null) { db.Logger = Logger; }
                db.StatusUpdate(filename, ticks);
                RowCount = db.RowCount();
            }

            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(DatabaseManager).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        public static void StatusRead(ImportStatus status)
        {
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(DatabaseManager).Name}.{Utility.GetCurrentMethod()}:Enter");

            if (string.IsNullOrEmpty(DatabasePath))
            {
                throw new Exception("DatabasePath must be set before calling any methods.");
            }

            using (var db = new DatabaseConnection(DatabasePath))
            {
                if (Logger != null) { db.Logger = Logger; }
                db.StatusRead(status);
                RowCount = db.RowCount();
            }

            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(DatabaseManager).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        #endregion

        #region Administration

        private static int RegistrationRowCount()
        {
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(DatabaseManager).Name}.{Utility.GetCurrentMethod()}:Enter");

            if (string.IsNullOrEmpty(DatabasePath))
            {
                throw new Exception("DatabasePath must be set before calling any methods.");
            }

            var rowCount = 0;
            using (var db = new DatabaseConnection(DatabasePath))
            {
                if (Logger != null) { db.Logger = Logger; }
                rowCount = db.RowCount();
            }

            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(DatabaseManager).Name}.{Utility.GetCurrentMethod()}:Leave");
            return rowCount;
        }

        public static void Clear()
        {
            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(DatabaseManager).Name}.{Utility.GetCurrentMethod()}:Enter");

            if (string.IsNullOrEmpty(DatabasePath))
            {
                throw new Exception("DatabasePath must be set before calling any methods.");
            }

            using (var db = new DatabaseConnection(DatabasePath))
            {
                if (Logger != null) { db.Logger = Logger; }
                db.ClearData();
                RowCount = db.RowCount();
            }

            Logger?.Write(Logger.LogLevel.Trace, $"{typeof(DatabaseManager).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        #endregion
    }
}
