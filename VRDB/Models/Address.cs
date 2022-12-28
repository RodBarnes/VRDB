using Common;
using System;
using System.Data.SqlClient;
using System.Text;

namespace VRDB.Models
{
    public class Address
    {
        public string Number { get; set; }
        public string Fraction { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string UnitId { get; set; }
        public string UnitType { get; set; }
        public string PostDirection { get; set; }
        public string PreDirection { get; set; }

        public Address() { }

        // Populate and Address from data in the database
        public Address(SqlDataReader reader) : this()
        {
            Number = reader["RegStNum"].FromSqlString();
            Fraction = reader["RegStFrac"].FromSqlString();
            Name = reader["RegStName"].FromSqlString();
            Type = reader["RegStType"].FromSqlString();
            UnitType = reader["RegUnitType"].FromSqlString();
            PreDirection = reader["RegStPreDirection"].FromSqlString();
            PostDirection = reader["RegStPostDirection"].FromSqlString();
            UnitId = reader["RegStUnitNum"].FromSqlString();
        }

        // Populate an address by parsing a string
        public Address(string address, string[] directions, string[] types) : this()
        {
            // NUMBER [FRACTION] [PREDIRECTION] NAME TYPE [POSTDIRECTION][, [UNIT TYPE] [UNIT ID]]
        
            var parts = address.Replace(".","").Split(' ');
            var ptr = 0;

            Number = parts[ptr++];
            Fraction = (parts.Length > ptr && parts[ptr].Contains("/")) ? parts[ptr++] : "";
            PreDirection = (parts.Length > ptr && Array.IndexOf(directions, parts[ptr]) > -1) ? parts[ptr++] : "";
            Name = (parts.Length > ptr) ? parts[ptr++] : "";

            while (parts.Length > ptr && Array.IndexOf(types, parts[ptr].Replace(",", "")) == -1)
            {
                Name += $" {parts[ptr++]}";
            }
            Type = (parts.Length > ptr) ? parts[ptr++] : "";
            PostDirection = (parts.Length > ptr && Array.IndexOf(directions, parts[ptr]) > -1) ? parts[ptr++].Replace(",", "") : "";
            UnitType = (parts.Length > ptr) ? parts[ptr++] : "";
            UnitId = (parts.Length > ptr) ? parts[ptr++] : "";
        }

        public static int Compare(Address addr1, Address addr2)
        {
            int rtn = 0;

            if (addr1.Number != addr2.Number) rtn += 1;
            if (addr1.Fraction != addr2.Fraction) rtn += 2;
            if (addr1.PreDirection != addr2.PreDirection) rtn += 4;
            if (addr1.Name != addr2.Name) rtn += 8;
            if (addr1.Type != addr2.Type) rtn += 16;
            if (addr1.PostDirection != addr2.PostDirection) rtn += 32;
            if (addr1.UnitType != addr2.UnitType) rtn += 64;
            if (addr1.UnitId != addr2.UnitId) rtn+= 128;

            return rtn;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            // Street Number
            if (Number.Length > 0)
            {
                sb.Append(Number);
            }
            // Street Number Fraction
            if (Fraction.Length > 0)
            {
                sb.Append(Fraction);
            }
            // Street Pre Direction
            if (PreDirection.Length > 0)
            {
                sb.Append($" {PreDirection}");
            }
            // Street Name
            if (Name.Length > 0)
            {
                sb.Append($" {Name}");
            }
            // Street Type
            if (Type.Length > 0)
            {
                sb.Append($" {Type}");
            }
            // Street Post Direction
            if (PostDirection.Length > 0)
            {
                sb.Append($" {PostDirection}");
            }
            // Street Unit Type
            if (UnitType.Length > 0)
            {
                sb.Append($", {UnitType}");
            }
            // Street Unit Number
            if (UnitId.Length > 0)
            {
                if (UnitType.Length > 0)
                {
                    if (UnitType == "#")
                    {
                        sb.Append(UnitId);
                    }
                    else
                    {
                        sb.Append($" {UnitId}");
                    }
                }
                else
                {
                    sb.Append($", {UnitId}");
                }
            }

            return sb.ToString();
        }
    }
}
