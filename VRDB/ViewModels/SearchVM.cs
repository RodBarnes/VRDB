using Common;
using Common.ViewModels;
using System;

namespace VRDB.Models
{
    public class SearchVM : BaseVM
    {
        private readonly string[] months = new string[12] { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };

        private string lastName;
        public string LastName
        {
            get => lastName;
            set
            {
                lastName = value;
                NotifyPropertyChanged();
            }
        }

        private string firstName;
        public string FirstName
        {
            get => firstName;
            set
            {
                firstName = value;
                NotifyPropertyChanged();
            }
        }

        private string middleName;
        public string MiddleName
        {
            get => middleName;
            set
            {
                middleName = value;
                NotifyPropertyChanged();
            }
        }

        private DateTime birthDate;
        public DateTime BirthDate
        {
            get => birthDate;
            set
            {
                birthDate = value;
                NotifyPropertyChanged();
            }
        }

        private char gender;
        public char Gender
        {
            get => gender;
            set
            {
                gender = value;
                NotifyPropertyChanged();
            }
        }

        private string address;
        public string Address
        {
            get => address;
            set
            {
                address = value;
                NotifyPropertyChanged();
            }
        }

        private string city;
        public string City
        {
            get => city;
            set
            {
                city = value;
                NotifyPropertyChanged();
            }
        }

        private string state;
        public string State
        {
            get => state;
            set
            {
                string code;
                if (value.Length > 2)
                {
                    code = Utility.StateCodeFromName(value);
                }
                else
                {
                    code = value;
                }
                state = code;
                NotifyPropertyChanged();
            }
        }

        private string zip;
        public string Zip
        {
            get => zip;
            set
            {
                zip = value;
                NotifyPropertyChanged();
            }
        }

        private DateTime registrationDate;
        public DateTime RegistrationDate
        {
            get => registrationDate;
            set
            {
                registrationDate = value;
                NotifyPropertyChanged();
            }
        }

        private DateTime lastVoted;
        public DateTime LastVoted
        {
            get => lastVoted;
            set
            {
                lastVoted = value;
                NotifyPropertyChanged();
            }
        }

        private string status;
        public string Status
        {
            get => status;
            set
            {
                status = value;
                NotifyPropertyChanged();
            }
        }

        private string compare;
        public string Compare
        {
            get => compare;
            set
            {
                compare = value;
                NotifyPropertyChanged();
            }
        }

        public SearchVM() { }

        public SearchVM(string content)
        {
            var data = content.Split('|');
            ParseNames(data[0]);
            BirthDate = ParseBirthdate(data[1]);
            if (data.Length > 2)
            {
                if (!char.TryParse(data[2], out char gender))
                {
                    gender = ' ';
                }
                Gender = gender;
            }
            if (data.Length > 3)
            {
                Address = data[3];
            }
            if (data.Length > 4)
            {
                City = data[4];
            }
            if (data.Length > 5)
            {
                State = data[5];
            }
            if (data.Length > 6)
            {
                Zip = data[6];
            }
        }

        private DateTime ParseBirthdate(string birthdate)
        {
            int month = 0;
            int day = 0;
            int year = 0;

            var parts = birthdate.Split(' ');
            if (!int.TryParse(parts[2], out year))
            {
                throw new FormatException($"Birth Date '{birthdate}' does not have a valid year value -- found '{parts[2]}'.");
            }
            if ((month = Array.IndexOf(months, parts[1]) + 1) == 0)
            {
                throw new FormatException($"Birth Date '{birthdate}' does not have a valid month value -- found '{parts[1]}'.");
            }
            if (!int.TryParse(parts[0], out day))
            {
                throw new FormatException($"Birth Date '{birthdate}' does not have a valid day value -- found '{parts[0]}'.");
            }

            return new DateTime(year, month, day);
        }

        private void ParseNames(string names)
        {
            var parts = names.Split(' ');
            LastName = parts[0].Replace(",", "");
            if (parts.Length > 1)
            {
                FirstName = parts[1];
            }
            if (parts.Length > 2)
            {
                MiddleName = parts[2];
            }
        }
    }
}
