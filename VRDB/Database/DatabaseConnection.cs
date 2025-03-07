using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using VRDB.Models;

namespace VRDB
{
    public class DatabaseConnection : IDisposable
    {
        private readonly SqlConnection sqlConn;
        private readonly int timeout = 60;

        public Logger Logger { get; set; }

        #region Data Methods

        public int RowCount()
        {
            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Enter");

            var rowCount = 0;
            try
            {
                using (var cmd = new SqlCommand("spRowCount", sqlConn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    var obj = cmd.ExecuteScalar();
                    if (obj != null)
                    {
                        rowCount = (int)obj;
                    }
                    else
                        throw new Exception($"{nameof(RowCount)} failed\nsql={cmd.CommandText}.");
                }
            }
            catch (Exception ex)
            {
                Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Exception {ex.Message}");
                throw new Exception($"{nameof(RowCount)} failed.", ex);
            }

            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Leave");
            return rowCount;
        }

        public void CompareSearch(List<SearchVM> list, string[] addrDirections, string[] addrTypes, BackgroundWorker bw = null, DoWorkEventArgs bwe = null)
        {
            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Enter");

            try
            {
                var args = (WorkerArgs)bwe.Argument;
                var includeGender = (args.Level & 1) == 1 ? true : false;
                var includeFullFirstName = (args.Level & 2) == 2 ? true : false;
                var includeMiddleInitial = (args.Level & 4) == 4 ? true : false;

                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    using (var cmd = new SqlCommand("spCompareSearch", sqlConn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@lastName", item.LastName.ToSqlString());
                        cmd.Parameters.AddWithValue("@birthDate", item.BirthDate);
                        //if (DateTime.TryParse(item.BirthDate, out DateTime birthdate))
                        //{
                        //    cmd.Parameters.AddWithValue("@birthDate", birthdate);
                        //}
                        if (includeFullFirstName)
                        {
                            cmd.Parameters.AddWithValue("@firstName", item.FirstName.ToSqlString());
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@firstName", item.FirstName.ToSqlString().Substring(0, 1));
                        }
                        if (includeGender)
                        {
                            cmd.Parameters.AddWithValue("@gender", item.Gender);
                        }
                        if (includeMiddleInitial)
                        {
                            cmd.Parameters.AddWithValue("@middleName", item.MiddleName.ToSqlString());
                        }

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader != null)
                            {
                                item.Compare = Constants.LabelMissing;
                                while (reader.Read())
                                {
                                    var mbrAddress = new Address(item.Address, addrDirections, addrTypes);
                                    var mbrCity = item.City;
                                    var mbrState = item.State;
                                    var mbrZip = item.Zip;
                                    var regAddress = new Address(reader);
                                    item.Status = reader["StatusCode"].FromSqlString();
                                    var compare = Address.Compare(mbrAddress, regAddress);
                                    item.Compare = (compare == 0 && item.Status == "Active") ? Constants.LabelSame : Constants.LabelDifferent;
                                    item.Address = regAddress.ToString();
                                    item.City = reader["RegCity"].FromSqlString();
                                    item.State = reader["RegState"].FromSqlString();
                                    item.Zip = reader["RegZipCode"].FromSqlString();
                                    item.RegistrationDate = (DateTime)reader["RegistrationDate"];
                                    //if (DateTime.TryParse(reader["RegistrationDate"].ToString(), out DateTime d2))
                                    //{
                                    //    item.RegistrationDate = d2.ToShortDateString();
                                    //}
                                    item.LastVoted = (DateTime)reader["LastVoted"];
                                    //if (DateTime.TryParse(reader["LastVoted"].ToString(), out DateTime d3))
                                    //{
                                    //    item.LastVoted = d3.ToShortDateString();
                                    //}
                                    Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Compare:{compare}\n" +
                                        $"\t{item.LastName}, {item.FirstName} {item.MiddleName}\n" +
                                        $"\tmember: {mbrAddress}, {mbrCity} {mbrState} {mbrZip}\n" +
                                        $"\tregdb: {regAddress}, {item.City} {item.State} {item.Zip}");
                                }
                                if (bw != null && bw.CancellationPending)
                                {
                                    bwe.Cancel = true;
                                    return;
                                }
                                bw.ReportProgress(i * (int)args.MaxProgressValue / list.Count, i);
                            }
                            else
                            {
                                if (!reader.IsClosed) reader.Close();
                                throw new Exception($"{nameof(Search)} failed: reader={reader}\nsql={cmd.CommandText}.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Exception {ex.Message}");
                throw new Exception($"{nameof(CompareSearch)} failed.", ex);
            }

            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        public void Search(string lastName, string firstName, string birthYear, string gender, List<SearchVM> results)
        {
            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Enter");

            try
            {
                using (var cmd = new SqlCommand("spSearch", sqlConn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@lastName", lastName.Trim());
                    if (!string.IsNullOrEmpty(firstName))
                    {
                        cmd.Parameters.AddWithValue("@firstName", firstName.Trim());
                    }
                    if (int.TryParse(birthYear, out int param1))
                    {
                        cmd.Parameters.AddWithValue("@birthYear", param1);
                    }
                    if (char.TryParse(gender, out char param2))
                    {
                        cmd.Parameters.AddWithValue("@gender", param2);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader != null)
                        {
                            while (reader.Read())
                            {
                                var result = new SearchVM
                                {
                                    FirstName = reader["Fname"].FromSqlString(),
                                    MiddleName = reader["MName"].FromSqlString(),
                                    LastName = reader["LName"].FromSqlString(),
                                    Address = new Address(reader).ToString(),
                                    City = reader["RegCity"].FromSqlString(),
                                    State = reader["RegState"].FromSqlString(),
                                    Zip = reader["RegZipCode"].FromSqlString(),
                                    Status = reader["StatusCode"].FromSqlString()
                                };
                                result.BirthDate = (DateTime)reader["BirthDate"];
                                //if (DateTime.TryParse(reader["BirthDate"].ToString(), out DateTime d1))
                                //{
                                //    result.BirthDate = d1.ToShortDateString();
                                //}
                                if (char.TryParse(reader["Gender"].ToString(), out char c))
                                {
                                    result.Gender = c;
                                }
                                result.RegistrationDate = (DateTime)reader["RegistrationDate"];
                                //if (DateTime.TryParse(reader["RegistrationDate"].ToString(), out DateTime d2))
                                //{
                                //    result.RegistrationDate = (d2.Year == 1) ? "--" : d2.ToShortDateString();
                                //}
                                result.LastVoted = (DateTime)reader["LastVoted"];
                                //if (DateTime.TryParse(reader["LastVoted"].ToString(), out DateTime d3))
                                //{
                                //    result.LastVoted = (d3.Year == 1) ? "--" : d3.ToShortDateString();
                                //}
                                results.Add(result);
                            }
                        }
                        else
                        {
                            if (!reader.IsClosed) reader.Close();
                            throw new Exception($"{nameof(Search)} failed: reader={reader}\nsql={cmd.CommandText}.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Exception {ex.Message}");
                throw new Exception($"{nameof(Search)} failed: Search={lastName},{firstName},{birthYear},{gender}.", ex);
            }

            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        public void RegistrationInsert(Registration reg)
        {
            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Enter");

            try
            {
                using (var cmd = new SqlCommand("spRegistrationInsert", sqlConn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@stateVoterID", reg.StateVoterId);
                    cmd.Parameters.AddWithValue("@fname", reg.FName.ToSqlString());
                    cmd.Parameters.AddWithValue("@mname", reg.MName.ToSqlString());
                    cmd.Parameters.AddWithValue("@lname", reg.LName.ToSqlString());
                    cmd.Parameters.AddWithValue("@nameSuffix", reg.NameSuffix.ToSqlString());
                    cmd.Parameters.AddWithValue("@birthDate", reg.BirthDate.ToShortDateString());
                    cmd.Parameters.AddWithValue("@gender", reg.Gender);
                    cmd.Parameters.AddWithValue("@regStNum", reg.RegStNum.ToSqlString());
                    cmd.Parameters.AddWithValue("@regStFrac", reg.RegStFrac.ToSqlString());
                    cmd.Parameters.AddWithValue("@regStName", reg.RegStName.ToSqlString());
                    cmd.Parameters.AddWithValue("@regStType", reg.RegStType.ToSqlString());
                    cmd.Parameters.AddWithValue("@regUnitType", reg.RegUnitType.ToSqlString());
                    cmd.Parameters.AddWithValue("@regStPreDir", reg.RegStPreDirection.ToSqlString());
                    cmd.Parameters.AddWithValue("@regStPostDir", reg.RegStPostDirection.ToSqlString());
                    cmd.Parameters.AddWithValue("@regStUnitNum", reg.RegStUnitNum.ToSqlString());
                    cmd.Parameters.AddWithValue("@regCity", reg.RegCity.ToSqlString());
                    cmd.Parameters.AddWithValue("@regState", reg.RegState.ToSqlString());
                    cmd.Parameters.AddWithValue("@regZipCode", reg.RegZipCode.ToSqlString());
                    cmd.Parameters.AddWithValue("@countyCode", reg.CountyCode.ToSqlString());
                    cmd.Parameters.AddWithValue("@precinctCode", reg.PrecinctCode.ToSqlString());
                    cmd.Parameters.AddWithValue("@precinctPart", reg.PrecinctPart.ToSqlString());
                    cmd.Parameters.AddWithValue("@legislativeDistrict", reg.LegislativeDistrict);
                    cmd.Parameters.AddWithValue("@congressionalDistrict", reg.CongressionalDistrict);
                    cmd.Parameters.AddWithValue("@mail1", reg.Mail1.ToSqlString());
                    cmd.Parameters.AddWithValue("@mail2", reg.Mail2.ToSqlString());
                    cmd.Parameters.AddWithValue("@mail3", reg.Mail3.ToSqlString());
                    cmd.Parameters.AddWithValue("@mail4", reg.Mail4.ToSqlString());
                    cmd.Parameters.AddWithValue("@mailCity", reg.MailCity.ToSqlString());
                    cmd.Parameters.AddWithValue("@mailZip", reg.MailZip.ToSqlString());
                    cmd.Parameters.AddWithValue("@mailState", reg.MailState.ToSqlString());
                    cmd.Parameters.AddWithValue("@mailCountry", reg.MailCountry.ToSqlString());
                    cmd.Parameters.AddWithValue("@registrationDate", reg.RegistrationDate.ToShortDateString());
                    cmd.Parameters.AddWithValue("@absenteeType", reg.AbsenteeType.ToSqlString());
                    cmd.Parameters.AddWithValue("@lastVoted", reg.LastVoted.ToShortDateString());
                    cmd.Parameters.AddWithValue("@statusCode", reg.StatusCode.ToSqlString());
                    var obj = cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Exception {ex.Message}");
                throw new Exception($"{nameof(RegistrationInsert)} failed: StateVoterId='{reg.StateVoterId}'.", ex);
            }

            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        public bool RegistrationExists(int id)
        {
            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Enter");

            var check = false;

            try
            {
                using (var cmd = new SqlCommand("spRegistrationRead", sqlConn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@stateVoterID", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader != null)
                        {
                            check = reader.HasRows;
                        }
                        else
                        {
                            if (!reader.IsClosed) reader.Close();
                            throw new Exception($"{nameof(RegistrationExists)} failed: reader={reader}\nsql={cmd.CommandText}.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Exception {ex.Message}");
                throw new Exception($"{nameof(RegistrationExists)} failed: StateVoterId={id}.", ex);
            }

            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Leave");
            return check;
        }

        #endregion

        #region Status Methods

        public void StatusUpdate(string filename, long ticks)
        {
            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Enter");

            try
            {
                using (var cmd = new SqlCommand("spStatusUpdate", sqlConn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@filename", filename);
                    cmd.Parameters.AddWithValue("@ticks", ticks);
                    var obj = cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Exception {ex.Message}");
                throw new Exception($"{nameof(StatusUpdate)} failed: filename={filename}, ticks={ticks}.", ex);
            }

            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        public void StatusRead(ImportStatus status)
        {
            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Enter");

            try
            {
                using (var cmd = new SqlCommand("spStatusRead", sqlConn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader != null)
                        {
                            while (reader.Read())
                            {
                                status.ImportFilename = reader["ImportFilename"].FromSqlString();
                                if (DateTime.TryParse(reader["ImportDateTime"].ToString(), out DateTime importDatetime))
                                {
                                    status.ImportDateTime = importDatetime;
                                }
                                if (long.TryParse(reader["TimeSpanTicks"].ToString(), out long ticks))
                                {
                                    status.TimeSpanTicks = ticks;
                                }
                            }
                        }
                        else
                        {
                            if (!reader.IsClosed) reader.Close();
                            throw new Exception($"{nameof(RegistrationExists)} failed: reader={reader}\nsql={cmd.CommandText}.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Exception {ex.Message}");
                throw new Exception($"{nameof(StatusRead)} failed.", ex);
            }

            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Leave");
        }


        #endregion

        #region Helper Methods

        private static Registration LoadRegistration(SqlDataReader reader)
        {
            var reg = new Registration
            {
                StateVoterId = (int)reader["StateVoterId"],
                FName = reader["Fname"].FromSqlString(),
                MName = reader["MName"].FromSqlString(),
                LName = reader["LName"].FromSqlString(),
                NameSuffix = reader["NameSuffix"].FromSqlString(),
                RegStNum = reader["RegStNum"].FromSqlString(),
                RegStFrac = reader["RegStFrac"].FromSqlString(),
                RegStName = reader["RegStName"].FromSqlString(),
                RegStType = reader["RegStType"].FromSqlString(),
                RegUnitType = reader["RegUnitType"].FromSqlString(),
                RegStPreDirection = reader["RegStPreDirection"].FromSqlString(),
                RegStPostDirection = reader["RegStPostDirection"].FromSqlString(),
                RegStUnitNum = reader["RegStUnitNum"].FromSqlString(),
                RegCity = reader["RegCity"].FromSqlString(),
                RegState = reader["RegState"].FromSqlString(),
                RegZipCode = reader["RegZipCode"].FromSqlString(),
                CountyCode = reader["CountyCode"].FromSqlString(),
                PrecinctCode = reader["PrecinctCode"].FromSqlString(),
                PrecinctPart = reader["PrecinctPart"].FromSqlString(),
                LegislativeDistrict = (int)reader["LegislativeDistrict"],
                CongressionalDistrict = (int)reader["CongressionalDistrict"],
                Mail1 = reader["Mail1"].FromSqlString(),
                Mail2 = reader["Mail2"].FromSqlString(),
                Mail3 = reader["Mail3"].FromSqlString(),
                // Mail4 = reader["Mail4"].FromSqlString(), // v2.3.0 - no longer supplied in 2025 extract
                MailCity = reader["MailCity"].FromSqlString(),
                MailZip = reader["MailZip"].FromSqlString(),
                MailState = reader["MailState"].FromSqlString(),
                MailCountry = reader["MailCountry"].FromSqlString(),
                // AbsenteeType = reader["AbsenteeType"].FromSqlString(), // v2.3.0 - no longer supplied in 2025 extract
                StatusCode = reader["StatusCode"].FromSqlString()
            };
            if (DateTime.TryParse(reader["BirthDate"].ToString(), out DateTime d1))
            {
                reg.BirthDate = d1;
            }
            if (char.TryParse(reader["Gender"].ToString(), out char c))
            {
                reg.Gender = c;
            }
            if (DateTime.TryParse(reader["RegistrationDate"].ToString(), out DateTime d2))
            {
                reg.RegistrationDate = d2;
            }
            if (DateTime.TryParse(reader["LastVoted"].ToString(), out DateTime d3))
            {
                reg.LastVoted = d3;
            }

            return reg;
        }

        #endregion

        #region Administration

        public DatabaseConnection(string path)
        {
            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Enter");
#if DEBUG
            // Use DB from user
            sqlConn = new SqlConnection($@"Data Source=(LocalDB)\ProjectsV13;AttachDbFilename=C:\USERS\RODBA\APPDATA\ROAMING\ADVANCED APPLICATIONS\VRDB\VRDB.mdf;Integrated Security = True;Connect Timeout={timeout}");

            // Use DB from Debug
            //sqlConn = new SqlConnection($@"Data Source=(LocalDB)\ProjectsV13;AttachDbFilename={path}\VRDB.mdf;Integrated Security = True;Connect Timeout={timeout}");
#else
            sqlConn = new SqlConnection($@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={path}\VRDB.mdf;Integrated Security = True;Connect Timeout={timeout}");
#endif
            sqlConn.Open();

            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        void IDisposable.Dispose() => sqlConn.Close();

        public void ClearData()
        {
            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Enter");

            try
            {
                var cmd = new SqlCommand("spDatabaseClear", sqlConn);
                var rtn = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Exception {ex.Message}");
                throw new Exception($"{nameof(ClearData)} failed.", ex);
            }

            Logger?.Write(Logger.LogLevel.Debug, $"{typeof(DatabaseConnection).Name}.{Utility.GetCurrentMethod()}:Leave");
        }

        #endregion
    }
}
