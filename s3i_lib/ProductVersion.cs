using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace s3i_lib
{
    /// <summary>
    /// See https://docs.microsoft.com/en-us/windows/win32/msi/productversion
    /// </summary>
    public class ProductVersion : IComparable<ProductVersion>
    {
        public static Regex Regex { get; } = new Regex(@"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$", RegexOptions.Compiled);
        public byte Major { get; set; }
        public byte Minor { get; set; }
        public ushort Build { get; set; }
        public ushort Patch { get; set; }
        public static bool TryParse(string s, out ProductVersion version)
        {
            version = null;
            var m = Regex.Match(s);
            if (m.Success)
            {
                if (!byte.TryParse(m.Groups[1].Value, out byte major)) return false;
                if (!byte.TryParse(m.Groups[2].Value, out byte minor)) return false;
                if (!ushort.TryParse(m.Groups[3].Value, out ushort build)) return false;
                if (!ushort.TryParse(m.Groups[4].Value, out ushort patch)) return false;
                version = new ProductVersion { Major = major, Minor = minor, Build = build, Patch = patch, };
                return true;
            }
            return false;
        }
        public int CompareTo(ProductVersion other)
        {
            var major = Major.CompareTo(other.Major);
            if (0 != major) return major;
            var minor = Minor.CompareTo(other.Minor);
            if (0 != minor) return minor;
            var build = Build.CompareTo(other.Build);
            if (0 != build) return build;
            var patch = Patch.CompareTo(other.Patch);
            if (0 != patch) return patch;
            return 0;
        }
        public override string ToString()
        {
            return $"{Major}.{Minor}.{Build}.{Patch}";
        }
    }
}