﻿using BGC.Study;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace BGC.Utility
{
    /// <summary>
    /// Sequence-based versioning parsing, managing, and comparing
    /// </summary>
    public readonly struct ApplicationVersion
    {
        private const string WILD_CARD = "*";
        private const ushort WILD_CARD_CEIL = ushort.MaxValue;
        private const ushort WILD_CARD_FLOOR = ushort.MinValue;
        private const char DELIM = '.';
        private static readonly string DELIM_STR = DELIM.ToString();

        private static readonly ushort[] nullVersion = new ushort[] { 0, 0, 0, 0 };
        private static readonly ushort[] maxVersion = new ushort[] { ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue };

        public static readonly ApplicationVersion NullVersion = new ApplicationVersion(nullVersion);
        public static readonly ApplicationVersion Max = new ApplicationVersion(maxVersion);

        private readonly ushort[] versions;

        public ushort this[int i]
        {
            get
            {
                if (i >= 4)
                {
                    throw new IndexOutOfRangeException($"ApplicationVersion: Length {4}, Index {i}");
                }

                if (i < Length)
                {
                    return versions[i];
                }

                return 0;
            }
        }

        public int Length => versions.Length;

        public ushort Major => this[0];
        public ushort Minor => this[1];
        public ushort Build => this[2];
        public ushort Revision => this[3];

        public ApplicationVersion(string version)
        {
            ushort[] parsedVersion = null;

            if (string.IsNullOrEmpty(version))
            {
                parsedVersion = nullVersion;
            }
            else
            {
                try
                {
                    parsedVersion = version.Split(DELIM).Select(ushort.Parse).ToArray();
                }
                catch (Exception)
                {
                    Debug.LogError($"Failed to parse version string: {version}");
                }
            }

            if (parsedVersion == null || parsedVersion.Length == 0)
            {
                parsedVersion = nullVersion;
            }

            if (parsedVersion.Length <= 4)
            {
                versions = parsedVersion;
            }
            else
            {
                Debug.LogError($"ApplicationVersion constructed by string with more than 4 fields: \"{version}\"");
                versions = new ushort[4]
                {
                    parsedVersion[0],
                    parsedVersion[1],
                    parsedVersion[2],
                    parsedVersion[3]
                };
            }
        }

        public ApplicationVersion(params ushort[] version)
        {
            if (version.Length <= 4)
            {
                versions = (ushort[])version.Clone();
            }
            else
            {
                Debug.LogError($"ApplicationVersion constructed by array with more than 4 fields: {{{string.Join(", ", version.Select(PrintVersionElement))}}}");
                versions = new ushort[4]
                {
                    version[0],
                    version[1],
                    version[2],
                    version[3]
                };
            }
        }

        /// <summary>
        /// Checks if this ApplicationVersion in the range [lowerBound, upperBound)
        /// </summary>
        /// <param name="lowerBound">Inclusive lowerBound</param>
        /// <param name="upperBound">Exclusive upperBound</param>
        public bool Between(in ApplicationVersion lowerBound, in ApplicationVersion upperBound)
        {
            if (lowerBound >= upperBound)
            {
                throw new ArgumentException($"lowerBound ({lowerBound}) exceeds upperBound ({upperBound})");
            }

            return (this < upperBound) && (this >= lowerBound);
        }



        /// <summary>
        /// Compares the version number to one that supports asterisk wildcards
        /// Unspecified elements are considered wildcards
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public bool MatchesPattern(string pattern)
        {
            try
            {
                string[] splitVersion = pattern.Split(DELIM);

                if (splitVersion.Length > 4)
                {
                    throw new ArgumentException(
                        message: $"Version pattern had too many version elements: {splitVersion.Length}",
                        paramName: nameof(pattern));
                }

                for (int i = 0; i < splitVersion.Length; i++)
                {
                    if (splitVersion[i] != WILD_CARD)
                    {
                        if (this[i] != ushort.Parse(splitVersion[i]))
                        {
                            return false;
                        }
                    }
                }

            }
            catch (Exception excp)
            {
                throw new ArgumentException(
                    message: $"Bad version parsing: {pattern}",
                    innerException: excp);
            }

            return true;
        }

        public static ApplicationVersion BuildFromWild(string version, bool upperBound)
        {
            ushort[] parsedVersion = null;
            try
            {
                string[] splitVersion = version.Split(DELIM);
                parsedVersion = new ushort[4];

                for (int i = 0; i < splitVersion.Length; i++)
                {
                    if (splitVersion[i] == WILD_CARD)
                    {
                        parsedVersion[i] = upperBound ? WILD_CARD_CEIL : WILD_CARD_FLOOR;
                    }
                    else
                    {
                        parsedVersion[i] = ushort.Parse(splitVersion[i]);
                    }
                }

                //End with Floor for unspecified elements
                //This way, the range "100.3" to "101.1" means [100.3.0.0 , 101.1.0.0)
                for (int i = splitVersion.Length; i < parsedVersion.Length; i++)
                {
                    parsedVersion[i] = 0;
                }

            }
            catch (Exception excp)
            {
                throw new ArgumentException(
                    message: $"Bad version parsing: {version}",
                    innerException: excp);
            }

            return new ApplicationVersion(parsedVersion);
        }

        public bool IsNull() => this == NullVersion;

        public static bool TryParse(string s, out ApplicationVersion result)
        {
            result = NullVersion;
            ushort[] parsedVersion = null;

            if (string.IsNullOrEmpty(s))
            {
                return false;
            }
            else
            {
                string[] splitString = s.Split(DELIM);
                parsedVersion = new ushort[splitString.Length];

                for (int i = 0; i < splitString.Length; i++)
                {
                    string segment = splitString[i];
                    if (!ushort.TryParse(segment, out ushort value))
                    {
                        return false;
                    }

                    parsedVersion[i] = value;
                }
            }

            if (parsedVersion.Length > 4)
            {
                return false;
            }

            result = new ApplicationVersion(parsedVersion);
            return true;
        }

        #region Object Overloads

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((ApplicationVersion)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 0;

                for (int i = 0; i < 4; i++)
                {
                    hashCode = (hashCode * 397) ^ this[i].GetHashCode();
                }

                return hashCode;
            }
        }

        public override string ToString() => string.Join(DELIM_STR, versions.Select(PrintVersionElement));

        #endregion Object Overloads
        #region Operators

        public bool Equals(in ApplicationVersion other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            for (int i = 0; i < 4; i++)
            {
                //Cascade down version significance
                //Only stop on a mismatch
                if (this[i] != other[i])
                {
                    return false;
                }
            }

            //Equal
            return true;
        }

        public static bool operator <(in ApplicationVersion lVersion, in ApplicationVersion rVersion)
        {
            for (int i = 0; i < 4; i++)
            {
                //Cascade down version significance
                //Only stop on a mismatch
                if (lVersion[i] != rVersion[i])
                {
                    return lVersion[i] < rVersion[i];
                }
            }

            //Equal
            return false;
        }

        public static bool operator >(in ApplicationVersion lVersion, in ApplicationVersion rVersion)
        {
            for (int i = 0; i < 4; i++)
            {
                //Cascade down version significance
                //Only stop on a mismatch
                if (lVersion[i] != rVersion[i])
                {
                    return lVersion[i] > rVersion[i];
                }
            }

            //Equal
            return false;
        }

        public static bool operator <=(in ApplicationVersion lVersion, in ApplicationVersion rVersion)
        {
            for (int i = 0; i < 4; i++)
            {
                //Cascade down version significance
                //Only stop on a mismatch
                if (lVersion[i] != rVersion[i])
                {
                    return lVersion[i] < rVersion[i];
                }
            }

            //Equal
            return true;
        }

        public static bool operator >=(in ApplicationVersion lVersion, in ApplicationVersion rVersion)
        {
            for (int i = 0; i < 4; i++)
            {
                //Cascade down version significance
                //Only stop on a mismatch
                if (lVersion[i] != rVersion[i])
                {
                    return lVersion[i] > rVersion[i];
                }
            }

            //Equal
            return true;
        }

        public static bool operator ==(in ApplicationVersion lVersion, in ApplicationVersion rVersion)
        {
            if (ReferenceEquals(lVersion, rVersion))
            {
                return true;
            }

            return lVersion.Equals(rVersion);
        }

        public static bool operator !=(in ApplicationVersion lVersion, in ApplicationVersion rVersion) =>
            !(lVersion == rVersion);

        public static implicit operator ApplicationVersion(string version) =>
            new ApplicationVersion(version);

        public static implicit operator string(in ApplicationVersion version) =>
            version.ToString();

        #endregion Operators
        #region Helper Methods

        private static string PrintVersionElement(ushort versionElement)
        {
            if (versionElement == WILD_CARD_CEIL)
            {
                return WILD_CARD;
            }

            return versionElement.ToString();
        }

        #endregion
    }

}
