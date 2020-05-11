namespace Lzma
{
    using System;
    using System.IO.Compression;
    using System.Linq;
    using SevenZip;
    using SevenZip.Compression.LZMA;

    public class LzmaCompressionParameters
    {
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
            get => _dictionary;
            set => _dictionary = Between(value, 0, 29);
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
            get => _posStateBits;
            set => _posStateBits = Between(value, 0, 4);
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
            get => _litContextBits;
            set => _litContextBits = Between(value, 0, 8);
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
            get => _litPosBits;
            set => _litPosBits = Between(value, 0, 4);
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
            get => _algorithm;
            set => _algorithm = Between(value, 0, 1);
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
            get => _numFastBytes;
            set => _numFastBytes = Between(value, 5, 273);
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
            get => _mf;
            set => _mf = From(value, "bt2", "bt4");
        }

        /// <summary>
        /// Gets or sets the end of stream marker
        /// Defaults to false (assumes this is the end of the stream)
        /// </summary>
        /// <value>
        ///   <c>true</c> if eos; otherwise, <c>false</c>.
        /// </value>
        public bool Eos { get; set; } = true;

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

        public LzmaCompressionParameters Clone()
        {
            var clone = (LzmaCompressionParameters)MemberwiseClone();
            return clone;
        }

        internal void SetEncoderProperties(Encoder encoder)
        {
            CoderPropID[] propIDs =
            {
                CoderPropID.DictionarySize,
                CoderPropID.PosStateBits,
                CoderPropID.LitContextBits,
                CoderPropID.LitPosBits,
                CoderPropID.Algorithm,
                CoderPropID.NumFastBytes,
                CoderPropID.MatchFinder,
                CoderPropID.EndMarker
            };
            object[] properties =
            {
                1 << Dictionary,
                PosStateBits,
                LitContextBits,
                LitPosBits,
                Algorithm,
                NumFastBytes,
                Mf,
                Eos
            };

            encoder.SetCoderProperties(propIDs, properties);
        }

        public static readonly LzmaCompressionParameters Fast = new LzmaCompressionParameters
        {
            Dictionary = 20,
            //LitContextBits = 8,
            //LitPosBits = 4,
            NumFastBytes = 5,
            //PosStateBits = 4,
            Mf = "bt2"
        };

        public static readonly LzmaCompressionParameters Default = new LzmaCompressionParameters();

        public static readonly LzmaCompressionParameters Optimal = new LzmaCompressionParameters
        {
            Dictionary = 29,
            LitContextBits = 8,
            LitPosBits = 0,
            NumFastBytes = 273,
            PosStateBits = 0,
            Mf = "bt4"
        };
    }
}
