namespace Lzma
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SevenZip;
    using SevenZip.Compression.LZMA;

    public class LzmaCompressionParameters
    {
        private bool _readonly;

        private int _dictionary = 23;
        /// <summary>
        /// Gets or sets the dictionary.
        /// Possible values 0-29 (dictionary size is 2^n)
        /// Defaults to 23
        /// </summary>
        /// <value>
        /// The dictionary.
        /// </value>
        public int Dictionary
        {
            get { return _dictionary; }
            set { CheckWrite(); _dictionary = Between(value, 0, 29); }
        }

        private int _posStateBits = 2;
        /// <summary>
        /// Gets or sets number of position bits.
        /// Possible values 0-4
        /// Defaults to 2
        /// </summary>
        /// <value>
        /// The position state bits.
        /// </value>
        public int PosStateBits
        {
            get { return _posStateBits; }
            set { CheckWrite(); _posStateBits = Between(value, 0, 4); }
        }

        private int _litContextBits = 3;
        /// <summary>
        /// Gets or sets the literal context bits.
        /// Possible values 0-8
        /// Defaults to 3
        /// </summary>
        /// <value>
        /// The lit context bit.
        /// </value>
        public int LitContextBits
        {
            get { return _litContextBits; }
            set { CheckWrite(); _litContextBits = Between(value, 0, 8); }
        }

        private int _litPosBits = 0;
        /// <summary>
        /// Gets or sets the literal context bits.
        /// Possible values 0-4
        /// Defaults to 0
        /// </summary>
        /// <value>
        /// The lit context bit.
        /// </value>
        public int LitPosBits
        {
            get { return _litPosBits; }
            set { CheckWrite(); _litPosBits = Between(value, 0, 4); }
        }

        private int _algorithm = 1;

        /// <summary>
        /// Gets or sets the algorithm (compression mode).
        /// Values between 0-1
        /// Defaults to 1 (max)
        /// Fun fact: unused by current (19) implementation
        /// </summary>
        /// <remarks>Unused</remarks>
        /// <value>
        /// The algorithm.
        /// </value>
        public int Algorithm
        {
            get { return _algorithm; }
            set { CheckWrite(); _algorithm = Between(value, 0, 1); }
        }

        private int _numFastBytes = 128;

        /// <summary>
        /// Gets or sets the number of fast bytes.
        /// Values between 5-273
        /// Defaults to 128
        /// </summary>
        /// <value>
        /// The number fast bytes.
        /// </value>
        public int NumFastBytes
        {
            get { return _numFastBytes; }
            set { CheckWrite(); _numFastBytes = Between(value, 5, 273); }
        }

        private string _mf = "bt4";

        /// <summary>
        /// Gets or sets the match finder algorithm.
        /// Values: bt2 ro bt4
        /// defaults to bt4
        /// </summary>
        /// <value>
        /// The mf.
        /// </value>
        public string Mf
        {
            get { return _mf; }
            set { CheckWrite(); _mf = From(value, "bt2", "bt4"); }
        }

        private bool _eos = true;
        /// <summary>
        /// Gets or sets the end of stream marker
        /// Defaults to false (assumes this is the end of the stream)
        /// </summary>
        /// <value>
        ///   <c>true</c> if eos; otherwise, <c>false</c>.
        /// </value>
        public bool Eos
        {
            get { return _eos; }
            set { CheckWrite(); _eos = value; }
        }

        private static int Between(int value, int min, int max)
        {
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException($"value must be between {min} ane {max}");
            return value;
        }

        private static string From(string value, params string[] allowed)
        {
            if (!allowed.Contains(value))
                throw new ArgumentOutOfRangeException($"value must be one of {string.Join(", ", allowed)}");
            return value;
        }

        private void CheckWrite()
        {
            if (_readonly)
                throw new InvalidOperationException();
        }

        private LzmaCompressionParameters ReadOnly()
        {
            _readonly = true;
            return this;
        }

        public LzmaCompressionParameters Clone()
        {
            var clone = (LzmaCompressionParameters)MemberwiseClone();
            clone._readonly = false;
            return clone;
        }

        internal void SetEncoderProperties(Encoder encoder)
        {
            var properties = new Dictionary<CoderPropID, object>
            {
                {CoderPropID.DictionarySize, 1 << Dictionary},
                {CoderPropID.PosStateBits,PosStateBits},
                {CoderPropID.LitContextBits,LitContextBits},
                {CoderPropID.LitPosBits,LitPosBits},
                {CoderPropID.Algorithm,Algorithm},
                {CoderPropID.NumFastBytes,NumFastBytes},
                {CoderPropID.MatchFinder,Mf},
                {CoderPropID.EndMarker,Eos}
            };

            encoder.SetCoderProperties(properties.Keys.ToArray(), properties.Values.ToArray());
        }

        public static readonly LzmaCompressionParameters Fast = new LzmaCompressionParameters
        {
            Dictionary = 16,
            LitContextBits = 0,
            LitPosBits = 0,
            NumFastBytes = 5,
            PosStateBits = 4,
            Mf = "bt2"
        }.ReadOnly();

        public static readonly LzmaCompressionParameters Default = new LzmaCompressionParameters().ReadOnly();

        public static readonly LzmaCompressionParameters Optimal = new LzmaCompressionParameters
        {
            Dictionary = 29,
            LitContextBits = 8,
            LitPosBits = 0,
            NumFastBytes = 273,
            PosStateBits = 0,
            Mf = "bt4"
        }.ReadOnly();
    }
}
