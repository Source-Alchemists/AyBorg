using System.Security.Cryptography;
using System.Text;

internal static class RandomPasswordGenerator
{
        private enum CharType
        {
            Lowercase,
            Uppercase,
            Digit,
            Special
        }

        public static int Length { get; } = 12;
        public static int MinLowercases { get; } = 1;
        public static int MinUppercases { get; } = 1;
        public static int MinDigits { get; } = 1;
        public static int MinSpecials { get; } = 1;


        private static readonly Dictionary<CharType, string> _chars = new()
        {
            { CharType.Lowercase, "abcdefghijklmnopqrstuvwxyz" },
            { CharType.Uppercase, "ABCDEFGHIJKLMNOPQRSTUVWXYZ" },
            { CharType.Digit, "0123456789" },
            { CharType.Special, "!@#$%^&*()-_=+{}[]?<>.," }
        };

        private static readonly Dictionary<CharType, int> _outstandingChars = new();

        public static string Generate()
        {
            if (Length < MinLowercases + MinUppercases + MinDigits + MinSpecials)
            {
                throw new ArgumentException("Minimum requirements exceed password length.");
            }

            ResetOutstandings();

            var password = new StringBuilder();

            for (int i = 0; i < Length; i++)
            {
                if (_outstandingChars.Sum(x => x.Value) == Length - i)
                {
                    var outstanding = _outstandingChars.Where(x => x.Value > 0).Select(x => x.Key).ToArray();
                    password.Append(DrawChar(outstanding));
                }
                else
                {
                    password.Append(DrawChar());
                }
            }

            return password.ToString();
        }

        private static void ResetOutstandings()
        {
            _outstandingChars[CharType.Lowercase] = MinLowercases;
            _outstandingChars[CharType.Uppercase] = MinUppercases;
            _outstandingChars[CharType.Digit] = MinDigits;
            _outstandingChars[CharType.Special] = MinSpecials;
        }

        private static char DrawChar(params CharType[] types)
        {
            var filteredChars = types.Length == 0 ? _chars : _chars.Where(x => types.Contains(x.Key));
            int length = filteredChars.Sum(x => x.Value.Length);
            int index = RandomNumberGenerator.GetInt32(length);
            int offset = 0;

            foreach (var item in filteredChars)
            {
                if (index < offset + item.Value.Length)
                {
                    DecreaseOustanding(item.Key);
                    return item.Value[index - offset];
                }
                offset += item.Value.Length;
            }

            return new char();
        }

        private static void DecreaseOustanding(CharType type)
        {
            if (_outstandingChars[type] > 0)
            {
                _outstandingChars[type]--;
            }
        }
    }