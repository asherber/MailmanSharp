using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MailmanSharp
{
    public class MailmanVersion : IEquatable<MailmanVersion>, IComparable<MailmanVersion>
    {
        private string _value;
        private static Regex _re = new Regex(@"^(?<major>\d+)\.(?<minor>\d+)\.?(?<build>\d*)(?<patch>.*)$");

        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Build { get; private set; }
        public string Patch { get; private set; } = String.Empty;

        public MailmanVersion()
        {
        }

        public MailmanVersion(string value)
        {
            _value = value ?? throw new ArgumentException("The input value cannot be null.");

            var match = _re.Match(value);
            if (match.Success)
            {
                Major = Convert.ToInt32(match.Groups["major"].Value);
                Minor = Convert.ToInt32(match.Groups["minor"].Value);
                Build = Convert.ToInt32(match.Groups["build"].Value);
                Patch = match.Groups["patch"].Value?.Trim();
            }
            else
            {
                Major = Minor = Build = 0;
                Patch = String.Empty;
            }
        }

        public override string ToString()
        {
            return _value;
        }

        #region IEquatable<T>
        public bool Equals(MailmanVersion other)
        {
            return this.ToString() == other?.ToString();
        }

        public override bool Equals(Object obj)
        {
            return this.Equals(obj as MailmanVersion);
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public static bool operator ==(MailmanVersion version1, MailmanVersion version2)
        {
            if ((object)version1 == null || (object)version2 == null)
                return Object.Equals(version1, version2);
            else
                return version1.Equals(version2);
        }

        public static bool operator !=(MailmanVersion version1, MailmanVersion version2)
        {
            return !(version1 == version2);
        }
        #endregion

        #region IComparable<T>
        public int CompareTo(MailmanVersion other)
        {
            if (other == null)
                return 1;
            else if (Major != other.Major)
                return Major.CompareTo(other.Major);
            else if (Minor != other.Minor)
                return Minor.CompareTo(other.Minor);
            else if (Build != other.Build)
                return Build.CompareTo(other.Build);
            else
                return Patch.CompareTo(other.Patch);
        }

        public static bool operator >(MailmanVersion version1, MailmanVersion version2)
        {
            return version1?.CompareTo(version2) == 1;
        }

        public static bool operator <(MailmanVersion version1, MailmanVersion version2)
        {
            return version2?.CompareTo(version1) == 1;
        }

        public static bool operator >=(MailmanVersion version1, MailmanVersion version2)
        {
            return version1 == version2 || version1 > version2;
        }

        public static bool operator <=(MailmanVersion version1, MailmanVersion version2)
        {
            return version1 == version2 || version1 < version2;
        }
        #endregion
    }
}
