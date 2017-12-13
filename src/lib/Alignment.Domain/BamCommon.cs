﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Alignment.Domain.Sequencing
{
    public static class BamUtilities
    {
        /// <summary>
        ///     recreate the behavior of List &lt;T&gt;.Contains() for arrays:
        /// </summary>
        public static bool ArrayContains<T, TU>(T[] array, TU query)
        {
            foreach (var item in array)
            {
                if (item.Equals(query)) return true;
            }
            return false;
        }
    }

    public static class Constants
    {
        public const int FastqOffset = 33;
    }

    public class BamAlignment
    {
        // variables

        private const uint Paired = 1;
        private const uint ProperPair = 2;
        private const uint Unmapped = 4;
        private const uint MateUnmapped = 8;
        private const uint ReverseStrand = 16;
        private const uint MateReverseStrand = 32;
        private const uint Mate1 = 64;
        private const uint Mate2 = 128;
        private const uint Secondary = 256;
        private const uint FailedQC = 512;
        private const uint Duplicate = 1024;
        private const uint Supplementary = 2048;

        public uint AlignmentFlag; // Alignment bit-flag - see Is<something>() methods to query this value, SetIs<something>() methods to manipulate

        public string Bases; // 'Original' sequence (as reported from sequencing machine) <- This comment seems like a lie
        public uint Bin; // Bin in BAM file where this alignment resides
        public CigarAlignment CigarData; // CIGAR operations for this alignment
        public int FragmentLength; // Read fragment length
        public uint MapQuality; // Mapping quality score
        public int MatePosition; // Position (0-based) where alignment's mate starts
        public int MateRefID; // ID number for reference sequence where alignment's mate was aligned
        public string Name; // Read name
        public int Position; // Position (0-based) where alignment starts
        public byte[] Qualities; // Phred qualities
        public int RefID; // ID number for reference sequence
        public byte[] TagData; // Tag data (accessors will pull the requested information out)

        // constructor
        public BamAlignment()
        {
			MatePosition = -1;
			MateRefID = -1;
			CigarData = new CigarAlignment();
        }


	    // Copy constructor.
        public BamAlignment(BamAlignment a)
        {
            AlignmentFlag = a.AlignmentFlag;
            Bases = a.Bases;
            Bin = a.Bin;
            CigarData = new CigarAlignment(a.CigarData);
            FragmentLength = a.FragmentLength;
            MapQuality = a.MapQuality;
            MatePosition = a.MatePosition;
            MateRefID = a.MateRefID;
            Name = a.Name;
            Position = a.Position;
            Qualities = new byte[a.Qualities.Length];
            Array.Copy(a.Qualities, Qualities, Qualities.Length);

            RefID = a.RefID;
            TagData = a.TagData;
            if (TagData != null)
                Array.Copy(a.TagData, TagData, TagData.Length);
        }

        /// <summary>
        ///     appends the supplied bytes to the end of the tag data byte array
        /// </summary>
        public void AppendTagData(byte[] b)
        {
            int newTagDataLen = TagData.Length + b.Length;
            byte[] newTagData = new byte[newTagDataLen];
            Array.Copy(TagData, newTagData, TagData.Length);
            Array.Copy(b, 0, newTagData, TagData.Length, b.Length);
            TagData = newTagData;
        }

        // calculates alignment end position, based on starting position and CIGAR operations
        public int GetEndPosition()
        {
            // initialize alignment end to starting position
            int alignEnd = Position + (int)CigarData.GetReferenceSpan();

            return alignEnd;
        }

        // retrieves the string data associated with the specified tag
        public string GetStringTag(string s)
        {
            return TagUtils.GetStringTag(TagData, s);
        }

        public int? GetIntTag(string s)
        {
            return TagUtils.GetIntTag(TagData, s);
        }

        // accessors
        public bool IsDuplicate()
        {
            return ((AlignmentFlag & Duplicate) != 0);
        }

        public bool IsFailedQC()
        {
            return ((AlignmentFlag & FailedQC) != 0);
        }

        public bool IsFirstMate()
        {
            return ((AlignmentFlag & Mate1) != 0);
        }

        public bool IsMapped()
        {
            return ((AlignmentFlag & Unmapped) == 0);
        }

        public bool IsMateMapped()
        {
            return ((AlignmentFlag & MateUnmapped) == 0);
        }

        public bool IsMateReverseStrand()
        {
            return ((AlignmentFlag & MateReverseStrand) != 0);
        }

        public bool IsPaired()
        {
            return ((AlignmentFlag & Paired) != 0);
        }

        /// <summary>
        /// Return true if this alignment is neither secondary nor supplementary.  If we're counting # of aligned reads,
        /// *these* records are the ones to count.
        /// </summary>
        public bool IsMainAlignment()
        {
            return (AlignmentFlag & (Supplementary + Secondary)) == 0;
        }

        public bool IsPrimaryAlignment()
        {
            return ((AlignmentFlag & Secondary) == 0);
        }

        public bool IsSupplementaryAlignment()
        {
            return ((AlignmentFlag & Supplementary) != 0);
        }

        public bool IsProperPair()
        {
            return ((AlignmentFlag & ProperPair) != 0);
        }

        public bool IsReverseStrand()
        {
            return ((AlignmentFlag & ReverseStrand) != 0);
        }

        public bool IsSecondMate()
        {
            return ((AlignmentFlag & Mate2) != 0);
        }

        public void SetIsDuplicate(bool b)
        {
            if (b) AlignmentFlag |= Duplicate;
            else AlignmentFlag &= ~Duplicate;
        }

        public void SetIsFailedQC(bool b)
        {
            if (b) AlignmentFlag |= FailedQC;
            else AlignmentFlag &= ~FailedQC;
        }

        public void SetIsFirstMate(bool b)
        {
            if (b) AlignmentFlag |= Mate1;
            else AlignmentFlag &= ~Mate1;
        }

        public void SetIsMateUnmapped(bool b)
        {
            if (b) AlignmentFlag |= MateUnmapped;
            else AlignmentFlag &= ~MateUnmapped;
        }

        public void SetIsMateReverseStrand(bool b)
        {
            if (b) AlignmentFlag |= MateReverseStrand;
            else AlignmentFlag &= ~MateReverseStrand;
        }

        public void SetIsPaired(bool b)
        {
            if (b) AlignmentFlag |= Paired;
            else AlignmentFlag &= ~Paired;
        }

        public void SetIsProperPair(bool b)
        {
            if (b) AlignmentFlag |= ProperPair;
            else AlignmentFlag &= ~ProperPair;
        }

        public void SetIsReverseStrand(bool b)
        {
            if (b) AlignmentFlag |= ReverseStrand;
            else AlignmentFlag &= ~ReverseStrand;
        }

        public void SetIsSecondaryAlignment(bool b)
        {
            if (b) AlignmentFlag |= Secondary;
            else AlignmentFlag &= ~Secondary;
        }

        public void SetIsSecondMate(bool b)
        {
            if (b) AlignmentFlag |= Mate2;
            else AlignmentFlag &= ~Mate2;
        }

        public void SetIsUnmapped(bool b)
        {
            if (b) AlignmentFlag |= Unmapped;
            else AlignmentFlag &= ~Unmapped;
        }
    }

    public static class BinaryIO
    {
        /// <summary>
        ///     Adds the bytes from the specified unsigned integer into a byte array
        /// </summary>
        public static void AddUIntBytes(ref byte[] b, ref int offset, uint num)
        {
            b[offset++] = (byte)num;
            b[offset++] = (byte)(num >> 8);
            b[offset++] = (byte)(num >> 16);
            b[offset++] = (byte)(num >> 24);
        }

        /// <summary>
        ///     Adds the bytes from the specified integer into a byte array
        /// </summary>
        public static void AddIntBytes(ref byte[] b, ref int offset, int num)
        {
            b[offset++] = (byte)num;
            b[offset++] = (byte)(num >> 8);
            b[offset++] = (byte)(num >> 16);
            b[offset++] = (byte)(num >> 24);
        }

        /// <summary>
        ///     Adds the bytes from the specified string into a byte array
        /// </summary>
        public static void AddNullTerminatedString(ref byte[] b, ref int offset, string s)
        {
            Encoding.ASCII.GetBytes(s, 0, s.Length, b, offset);
            offset += s.Length;
            b[offset++] = 0;
        }
    }

    // CIGAR operation codes
    internal enum CigarOpCodes
    {
        MatchAndMismatch = 0,
        Insertion = 1,
        Deletion = 2,
        SkippedRefRegion = 3,
        SoftClipping = 4,
        HardClipping = 5,
        Padding = 6
    };

    // CIGAR operation data type
    // This is a struct because as a class it was causing a large imapct
    // on performance through excessive memory allocations. As a struct,
    // there is no garbage collection.
    public struct CigarOp
    {
        private uint _length; // Operation length (number of bases)
        private char _type; // Operation type (MIDNSHP)

        // The CigarOp is read only because it is a struct. Structs are not
        // passed by reference and so setting the values may result in
        // unexpected behavior.
        public uint Length {  get { return _length; } }
        public char Type { get { return _type; } }

        public CigarOp(char type, uint length)
        {
            _type = type;
            _length = length;
        }

        public override bool Equals(Object obj)
        {
            // If parameter is null return false.
            if (obj == null) return false;

            // Throws an exception if the cast fails.
            CigarOp co = (CigarOp)obj;

            // Return true if the fields match:
            return (Type == co.Type) && (Length == co.Length);
        }

        public override int GetHashCode()
        {
            return ((Type << 24) | (int)Length);
        }

        public override string ToString()
        {
            return String.Format("{0}{1}", Length, Type);
        }

        public CigarOp DeepCopy()
        {
            return (CigarOp)MemberwiseClone();
        }

        public bool IsReferenceSpan()
        {
            switch (Type)
            {
                case 'M':
                case 'D':
                case 'N':
                case '=':
                case 'X':
                    return true;
                default:
                    return false;
            }
        }

        public bool IsReadSpan()
        {
            switch (Type)
            {
                case 'M':
                case 'I':
                case 'S':
                case '=':
                case 'X':
                    return true;
                default:
                    return false;
            }
        }
    }


    /// <summary>
    ///     subset of alignment information which is represented in a SAM/BAM CIGAR string
    /// </summary>
    public class CigarAlignment : IEnumerable
    {
        private readonly List<CigarOp> _data;

        public CigarAlignment()
        {
            _data = new List<CigarOp>();
        }

        public CigarAlignment(CigarAlignment other)
        {
            _data = new List<CigarOp>(other._data);
        }

        /// <summary>
        ///     initialize from SAM CIGAR string:
        /// </summary>
        public CigarAlignment(string cigarString)
            : this()
        {
            if (String.IsNullOrEmpty(cigarString) || (cigarString == "*")) return;

            int head = 0;
            for (int i = 0; i < cigarString.Length; ++i)
            {
                if (Char.IsDigit(cigarString, i)) continue;
                if (!BamUtilities.ArrayContains(BamConstants.CigarTypes, cigarString[i]))
                {
                    throw new InvalidDataException(string.Format("ERROR: Unexpected format in character {0} of CIGAR string: {1}",
                                                      (i + 1), cigarString));
                 
                }
                var length = uint.Parse(cigarString.Substring(head, i - head));
                var op = new CigarOp(cigarString[i], length);
                _data.Add(op);
                head = i + 1;
            }
            if (head != cigarString.Length)
            {
                throw new InvalidDataException(string.Format("ERROR: Unexpected format in CIGAR string: {0}", cigarString));

            }
        }

        public void Insert(int index, CigarOp op)
        {
            _data.Insert(index, op);
        }

        public int Count
        {
            get { return _data.Count; }
        }

        public IEnumerator GetEnumerator()
        {
            foreach (CigarOp op in _data)
            {
                yield return op;
            }
        }

        public CigarOp this[int i]
        {
            get
            {
                return _data[i];
            }
            set
            {
                _data[i] = value;
            }
        }

        public void Clear()
        {
            _data.Clear();
        }

        public void Add(CigarOp op)
        {
            _data.Add(op);
        }

        public void Reverse()
        {
            _data.Reverse();
        }

        /// <summary>
        ///     reference distance spanned by alignment.
        /// </summary>
        public uint GetReferenceSpan()
        {
            uint length = 0;
            foreach (CigarOp op in _data)
            {
                if (op.IsReferenceSpan()) length += op.Length;
            }
            return length;
        }


        /// <summary>
        ///     read distance spanned by the alignmnet
        /// </summary>
        public uint GetReadSpan()
        {
            uint length = 0;
            foreach (CigarOp op in _data)
            {
                if (op.IsReadSpan()) length += op.Length;
            }
            return length;
        }

        public uint GetCigarSpan()
        {
            uint length = 0;
            foreach (CigarOp op in _data)
            {
                length += op.Length;
            }
            return length;
        }


        /// <summary>
        ///     provide the total leading soft-clip
        /// </summary>
        public uint GetPrefixClip()
        {
            uint length = 0;
            foreach (CigarOp op in _data)
            {
                if (op.Type == 'S')
                {
                    length += op.Length;
                }
                else if (op.Type != 'H')
                {
                    break;
                }
            }
            return length;
        }

        /// <summary>
        ///     provide the total trailing soft-clip
        /// </summary>
        public uint GetSuffixClip()
        {
            uint length = 0;

            for (int index = (_data.Count - 1); index >= 0; index--)
            {
                CigarOp op = _data[index];
                if (op.Type == 'S')
                {
                    length += op.Length;
                }
                else if (op.Type != 'H')
                {
                    break;
                }
            }
            return length;
        }

        /// <summary>
        /// Number of bases 'matched' in alignment
        /// </summary>
        public int CountMatches()
        {
            return CountOperations('M');
        }

        /// <summary>
        /// Number of bases 'matched' in alignment
        /// </summary>
        public int CountOperations(char operationType)
        {
            int matches = 0;
            foreach (CigarOp op in this)
                if (op.Type == operationType) matches += (int)op.Length;
            return matches;
        }

        /// <summary>
        ///  Given a read okkk
        /// </summary>
        /// <param name="readOffset"></param>
        /// <returns></returns>
        public int TranslateReadToReferenceOffset(int readOffset)
        {
            int result = 0;
            foreach (var cigarOp in _data)
            {
                if (readOffset >= 0)
                {
                    if (cigarOp.IsReadSpan())
                    {
                        result += cigarOp.IsReferenceSpan() ? (int)Math.Min(readOffset, cigarOp.Length) : 0;
                        readOffset -= (int)cigarOp.Length;
                    }
                    else
                    {
                        result += (int)cigarOp.Length;
                    }
                }
                else
                {
                    return result;
                }
            }
            return result;
        }
        /// <summary>
        ///     given an offset in the reference sequence from the start
        ///     of the alignment, return the corresponding read offset. Return
        ///     -1 if reference offset has no mapped read position
        /// </summary>
        public int TranslateReferenceToReadOffset(int refOffset)
        {
            const int noMapping = -1;

            int refPos = 0;
            int nextRefPos = 0;
            int readPos = 0;
            foreach (CigarOp op in _data)
            {
                if (refPos > refOffset) return noMapping;
                if (op.IsReferenceSpan()) nextRefPos += (int)op.Length;
                // target segment:
                if (nextRefPos > refOffset)
                {
                    if (op.IsReadSpan())
                    {
                        return readPos + (refOffset - refPos);
                    }
                    return noMapping;
                }

                refPos = nextRefPos;
                if (op.IsReadSpan()) readPos += (int)op.Length;
            }
            return noMapping;
        }

        /// <summary>
        ///     if duplicated adjacent tags are present, reduce them to one copy,
        ///     also reduce adjacent insertion/deletion tags to a single pair
        /// </summary>
        /// <returns>true if cigar was altered by compression</returns>
        public bool Compress()
        {
            for (int segmentIndex = 0; segmentIndex < _data.Count; ++segmentIndex)
            {
                if (_data[segmentIndex].Length == 0) continue;
                for (int j = (segmentIndex + 1); j < _data.Count; ++j)
                {
                    if (_data[j].Length == 0) continue;
                    if (_data[segmentIndex].Type != _data[j].Type) break;
                    _data[segmentIndex] = new CigarOp(_data[segmentIndex].Type, _data[segmentIndex].Length + _data[j].Length);
                    _data[j] = new CigarOp(_data[j].Type, 0);
                }
            }

            int insertIndex = -1;
            int deleteIndex = -1;
            for (int segmentIndex = 0; segmentIndex < _data.Count; ++segmentIndex)
            {
                if (_data[segmentIndex].Length == 0) continue;
                if (_data[segmentIndex].Type == 'I')
                {
                    if (insertIndex >= 0 && deleteIndex >= 0)
                    {
                        _data[insertIndex] = new CigarOp(_data[insertIndex].Type, _data[insertIndex].Length + _data[segmentIndex].Length);
                        _data[segmentIndex] = new CigarOp(_data[segmentIndex].Type, 0);
                    }
                    if (insertIndex == -1) insertIndex = segmentIndex;
                }
                else if (_data[segmentIndex].Type == 'D')
                {
                    if (insertIndex >= 0 && deleteIndex >= 0)
                    {
                        _data[deleteIndex] = new CigarOp(_data[deleteIndex].Type, _data[deleteIndex].Length + _data[segmentIndex].Length);
                        _data[segmentIndex] = new CigarOp(_data[segmentIndex].Type, 0);
                    }
                    if (deleteIndex == -1) deleteIndex = segmentIndex;
                }
                else
                {
                    insertIndex = -1;
                    deleteIndex = -1;
                }
            }
            int numberRemoved = _data.RemoveAll(op => (op.Length == 0));
            return (numberRemoved != 0);
        }

        /// <summary>
        ///     Convert to SAM CIGAR format:
        /// </summary>
        public override string ToString()
        {
            StringBuilder val = new StringBuilder();
            foreach (CigarOp op in _data)
            {
                val.Append(op);
            }
            return val.ToString();
        }

        public CigarAlignment DeepCopy()
        {
            CigarAlignment val = new CigarAlignment();
            foreach (CigarOp op in _data)
            {
                val.Add(op.DeepCopy());
            }
            return val;
        }
    }

    // commonly used constants
    public static class BamConstants
    {
        public const uint CoreAlignmentDataLen = 32;
        public const int CigarShift = 4;
        public const uint CigarMask = ((1 << CigarShift) - 1);
        public const int MaxReadLength = 2048;
        public const int BlockHeaderLength = 18;
        public const int BlockFooterLength = 8;
        public const ushort GzipMagicNumber = 35615;
        public const string MagicNumber = "BAM\x1";
        public const string BaiMagicNumber = "BAI\x1";
        public const uint LutError = 255;
        public const int BestCompression = 9;
        public const int DefaultCompression = -1;
        public const int BestSpeed = 1;
        public static char[] CigarTypes = { 'M', 'I', 'D', 'N', 'S', 'H', 'P', '=', 'X' };
    }

    public class TagUtils
    {
        private readonly List<byte> _mByteList;

        // constructor
        public TagUtils()
        {
            _mByteList = new List<byte>();
        }

        // adds a string tag
        public void AddStringTag(string key, string value)
        {
            foreach (char c in key) _mByteList.Add((byte)c);
            _mByteList.Add(90); // Z
            foreach (char c in value) _mByteList.Add((byte)c);
            _mByteList.Add(0); // null termination
        }

        // adds an integer tag
        public void AddIntTag(string key, int value)
        {
            foreach (char c in key) _mByteList.Add((byte)c);
            _mByteList.Add(105); // i
            _mByteList.Add((byte)value);
            _mByteList.Add((byte)(value >> 8));
            _mByteList.Add((byte)(value >> 16));
            _mByteList.Add((byte)(value >> 24));
        }

        // adds a char tag
        public void AddCharTag(string key, char value)
        {
            foreach (char c in key) _mByteList.Add((byte)c);
            _mByteList.Add(65); // A
            _mByteList.Add((byte)value);
        }

        // returns the byte array
        public byte[] ToBytes()
        {
            return _mByteList.ToArray();
        }

        /// <summary>
        /// Returns the index for the first byte of the tag, including the tagKey
        /// </summary>
        /// <returns>the index, or -1 if not found</returns>
        private static int GetTagBeginIndex(byte[] tagData, string tagKey)
        {
            // convert the string into bytes
            byte[] tagNameBytes = new byte[2];
            tagNameBytes[0] = (byte)tagKey[0];
            tagNameBytes[1] = (byte)tagKey[1];

            int tagDataBegin = -1;
            char dataType;

            int tagIndex = 0;
            while (tagIndex < tagData.Length)
            {
                // check the current tag
                if ((tagData[tagIndex] == tagNameBytes[0]) && (tagData[tagIndex + 1] == tagNameBytes[1]))
                {
                    tagDataBegin = tagIndex;
                    break;
                }

                // skip to the next tag
                tagIndex += 2;
                dataType = Char.ToUpper((char)tagData[tagIndex]);
                tagIndex++;

                switch (dataType)
                {
                    case 'A':
                    case 'C':
                        tagIndex++;
                        break;

                    case 'S':
                        tagIndex += 2;
                        break;

                    case 'I':
                    case 'F':
                        tagIndex += 4;
                        break;

                    case 'Z':
                    case 'H':
                        while (tagData[tagIndex] != 0) tagIndex++;
                        tagIndex++;
                        break;

                    default:
                        throw new InvalidDataException(
                            string.Format("Found an unexpected BAM tag data type: [{0}] while looking for a tag ({1})",
                                          dataType, tagKey));
                }
            }
            return tagDataBegin;
        }

        #region Getting Values
        // retrieves the integer data associated with the specified tag
        public static int? GetIntTag(byte[] tagData, string tagKey)
        {
            int tagDataBegin = GetTagBeginIndex(tagData, tagKey);

            if (tagDataBegin < 0) return null;

            // grab the value
            int ret = 0;
            char dataType = Char.ToUpper((char)tagData[tagDataBegin + 2]);

            switch (dataType)
            {
                // signed and unsigned int8
                case 'C':
                    ret = tagData[tagDataBegin + 3];
                    break;

                // signed and unsigned int16
                case 'S':
                    ret = BitConverter.ToInt16(tagData, tagDataBegin + 3);
                    break;

                // signed and unsigned int32
                case 'I':
                    ret = BitConverter.ToInt32(tagData, tagDataBegin + 3);
                    break;

                default:
                    throw new InvalidDataException(
                        string.Format(
                            "Found an unexpected integer BAM tag data type: [{0}] while looking for a tag ({1})",
                            dataType, tagKey));
            }

            return ret;
        }

        // retrieves the string data associated with the specified tag
        public static string GetStringTag(byte[] tagData, string tagKey)
        {
            int tagDataBegin = GetTagBeginIndex(tagData, tagKey);

            if (tagDataBegin < 0) return null;

            // grab the value
            char dataType = Char.ToUpper((char)tagData[tagDataBegin + 2]);
            tagDataBegin += 3;    // the beginning of the tag value
            string ret = null;

            switch (dataType)
            {
                // null terminated and hex strings
                case 'Z':
                case 'H':
                    int len = 0;
                    while (tagData[tagDataBegin + len] != 0) len++;
                    ret = Encoding.ASCII.GetString(tagData, tagDataBegin, len);
                    break;
                case 'A':
                case 'C':
                    // single character
                    ret += Encoding.ASCII.GetChars(tagData, tagDataBegin, 1)[0];
                    break;
                default:
                    throw new InvalidDataException(
                        string.Format(
                            "Found an unexpected string BAM tag data type: [{0}] while looking for a tag ({1})",
                            dataType, tagKey));
            }

            return ret;
        }
        #endregion

    }
}