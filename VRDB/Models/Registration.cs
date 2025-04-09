using System;
using System.Globalization;

namespace VRDB.Models
{
    public class Registration
    {
        public int StateVoterId;
        public string FName;
        public string MName;
        public string LName;
        public string NameSuffix;
        public DateTime BirthDate;
        public char Gender;
        public string RegStNum;
        public string RegStFrac;
        public string RegStName;
        public string RegStType;
        public string RegUnitType;
        public string RegStPreDirection;
        public string RegStPostDirection;
        public string RegStUnitNum;
        public string RegCity;
        public string RegState;
        public string RegZipCode;
        public string CountyCode;
        public string PrecinctCode;
        public string PrecinctPart;
        public int LegislativeDistrict;
        public int CongressionalDistrict;
        public string Mail1;
        public string Mail2;
        public string Mail3;
        public string Mail4;
        public string MailCity;
        public string MailZip;
        public string MailState;
        public string MailCountry;
        public DateTime RegistrationDate;
        public string AbsenteeType;
        public DateTime LastVoted;
        public string StatusCode;

        public Registration() { }

        public Registration(string line)
        {
            var cols = line.Split('|');

            if (!int.TryParse(cols[0], out StateVoterId))
            {
                StateVoterId = 0;
            }
            FName = cols[1];
            MName = cols[2];
            LName = cols[3];
            NameSuffix = cols[4];
            if (!DateTime.TryParse(cols[5], out BirthDate))
            {
                // If it isn't a full date, try parsing just for the year value.
                if (!DateTime.TryParseExact(cols[5], "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out BirthDate))
                {
                    BirthDate = new DateTime();
                }
            }
            if (!char.TryParse(cols[6], out Gender))
            {
                Gender = 'X';
            }
            RegStNum = cols[7];
            RegStFrac = cols[8];
            RegStName = cols[9];
            RegStType = cols[10];
            RegUnitType = cols[11];
            RegStPreDirection = cols[12];
            RegStPostDirection = cols[13];
            RegStUnitNum = cols[14];
            RegCity = cols[15];
            RegState = cols[16];
            RegZipCode = cols[17];
            CountyCode = cols[18];
            PrecinctCode = cols[19];
            PrecinctPart = cols[20];
            if (!int.TryParse(cols[21], out LegislativeDistrict))
            {
                LegislativeDistrict = 0;
            }
            if (!int.TryParse(cols[22], out CongressionalDistrict))
            {
                CongressionalDistrict = 0;
            }
            Mail1 = cols[23];
            Mail2 = cols[24];
            Mail3 = cols[25];
            /* v2.3.0
             * Mail4 and AbsenteeType are not provided in extracts for 2025
             * Setting those columns to null rather (instead of a value from the extract)
             * and reordering the following properties' column positions to
             * accomodate that these columns are missing in the extract
             */
            Mail4 = null;
            MailCity = cols[26];
            MailZip = cols[27];
            MailState = cols[28];
            MailCountry = cols[29];
            if (!DateTime.TryParse(cols[30], out RegistrationDate))
            {
                RegistrationDate = new DateTime();
            }
            AbsenteeType = null;
            if (!DateTime.TryParse(cols[31], out LastVoted))
            {
                LastVoted = new DateTime();
            }
            StatusCode = cols[32];
        }

        public override string ToString()
        {
            return $"" +
                $"{StateVoterId}, " +
                $"{LName}, " +
                $"{MName}, " +
                $"{FName}, " +
                $"{NameSuffix}, " +
                $"{BirthDate}, " +
                $"{Gender}, " +
                $"{RegStNum}, " +
                $"{RegStFrac}, " +
                $"{RegStType}, " +
                $"{RegUnitType}, " +
                $"{RegStPreDirection}, " +
                $"{RegStPostDirection}, " +
                $"{RegStUnitNum}, " +
                $"{RegCity}, " +
                $"{RegState}, " +
                $"{RegZipCode}, " +
                $"{CountyCode}, " +
                $"{PrecinctCode}, " +
                $"{PrecinctPart}, " +
                $"{LegislativeDistrict}, " +
                $"{CongressionalDistrict}, " +
                $"{Mail1}, " +
                $"{Mail2}, " +
                $"{Mail3}, " +
                $"{Mail4}, " +
                $"{MailCity}, " +
                $"{MailZip}, " +
                $"{MailState}, " +
                $"{MailCountry}, " +
                $"{RegistrationDate}, " +
                $"{AbsenteeType}, " +
                $"{LastVoted}, " +
                $"{StatusCode}";
        }
    }
}
