﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using System.Numerics;

namespace s3i_lib
{
    /// <summary>
    /// Semantic Version 2.0 implementation
    /// See https://semver.org/
    /// Also: https://regex101.com/r/vkijKf/1/
    /// </summary>
    public class SemanticVersion : IComparable<SemanticVersion>
    {
        public static Regex Regex { get; } = new Regex(@"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$", RegexOptions.Compiled);
        // ^(?P<major>0|[1-9]\d*)\.(?P<minor>0|[1-9]\d*)\.(?P<patch>0|[1-9]\d*)(?:-(?P<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?P<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$
        public BigInteger Major { get; set; }
        public BigInteger Minor { get; set; }
        public BigInteger Patch { get; set; }
        public string Prerelease { get; set; }
        public string Metadata { get; set; }
        public static bool TryParse(string s, out SemanticVersion semver)
        {
            semver = null;
            var m = Regex.Match(s);
            if (m.Success)
            {
                if (!BigInteger.TryParse(m.Groups[1].Value, out var major)) return false;
                if (!BigInteger.TryParse(m.Groups[2].Value, out var minor)) return false;
                if (!BigInteger.TryParse(m.Groups[3].Value, out var patch)) return false;
                semver = new SemanticVersion
                {
                    Major = major,
                    Minor = minor,
                    Patch = patch,
                    Prerelease = 4 < m.Groups.Count && m.Groups[4].Success ? m.Groups[4].Value : null,
                    Metadata = 5 < m.Groups.Count && m.Groups[5].Success ? m.Groups[5].Value : null,
                };
                return true;
            }
            return false;
        }
        /// <summary>
        /// Compare semantic verions using precedence rules from https://semver.org/#spec-item-11
        /// </summary>
        /// <param name="other">Semantic version to compare to</param>
        /// <returns>-1, if this precedes other, +1 if other precedes this, or 0 if no precedence could be determined</returns>
        public int CompareTo(SemanticVersion other)
        {
            var major = Major.CompareTo(other.Major);
            if (0 != major) return major;
            var minor = Minor.CompareTo(other.Minor);
            if (0 != minor) return minor;
            var patch = Patch.CompareTo(other.Patch);
            if (0 != patch) return patch;
            // pre-release is tricky, null to be considered greater than any value
            var thisPrereleaseEmpty = string.IsNullOrEmpty(Prerelease);
            var otherPrereleaseEmpty = string.IsNullOrEmpty(other.Prerelease);
            if(thisPrereleaseEmpty || otherPrereleaseEmpty)
            {
                return !thisPrereleaseEmpty ? -1 : otherPrereleaseEmpty ? 0 : +1;
            }
            var prerelease = Prerelease.CompareTo(other.Prerelease);
            if (0 != prerelease) return prerelease;
            // we do not compare metadata, per standard
            return 0;
        }
        /// <summary>
        /// Convert ProductVersion to SemanticVersion:
        ///  Major = product.Major
        ///  Minor = product.Minor
        ///  Path = product.Build
        ///  Prerelease = null
        ///  Metadata = product.Patch
        /// </summary>
        /// <param name="productVersion">ProductVersion to convert</param>
        /// <returns>SemanticVersion</returns>
        public static SemanticVersion From(ProductVersion productVersion)
        {
            // product version patch to be ignored in semver compare, so we push it to metadata, keeping prerelease empty
            return new SemanticVersion { Major = productVersion.Major, Minor = productVersion.Minor, Patch = productVersion.Build, Prerelease = null, Metadata = productVersion.Patch.ToString(), };
        }
        public override string ToString()
        {
            return $"{Major}.{Minor}.{Patch}{(string.IsNullOrEmpty(Prerelease) ? "" : $"-{Prerelease}")}{(string.IsNullOrEmpty(Metadata) ? "" : $"+{Metadata}")}";
        }
    }
}