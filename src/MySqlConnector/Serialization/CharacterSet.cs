﻿namespace MySql.Data.Serialization
{
	/// <summary>
	/// MySQL character set codes.
	/// </summary>
	/// <remarks>Obtained from <c>SELECT id, collation_name FROM information_schema.collations ORDER BY id;</c> on MySQL 5.7.11.</remarks>
	internal enum CharacterSet : ushort
	{
		None = 0,
		Big5ChineseCaseInsensitive = 1,
		Latin2CzechCaseSensitive = 2,
		Dec8SwedishCaseInsensitive = 3,
		Cp850GeneralCaseInsensitive = 4,
		Latin1German1CaseInsensitive = 5,
		Hp8EnglishCaseInsensitive = 6,
		Koi8rGeneralCaseInsensitive = 7,
		Latin1SwedishCaseInsensitive = 8,
		Latin2GeneralCaseInsensitive = 9,
		Swe7SwedishCaseInsensitive = 10,
		AsciiGeneralCaseInsensitive = 11,
		UjisJapaneseCaseInsensitive = 12,
		SjisJapaneseCaseInsensitive = 13,
		Cp1251BulgarianCaseInsensitive = 14,
		Latin1DanishCaseInsensitive = 15,
		HebrewGeneralCaseInsensitive = 16,
		Tis620ThaiCaseInsensitive = 18,
		EuckrKoreanCaseInsensitive = 19,
		Latin7EstonianCaseSensitive = 20,
		Latin2HungarianCaseInsensitive = 21,
		Koi8uGeneralCaseInsensitive = 22,
		Cp1251UkrainianCaseInsensitive = 23,
		Gb2312ChineseCaseInsensitive = 24,
		GreekGeneralCaseInsensitive = 25,
		Cp1250GeneralCaseInsensitive = 26,
		Latin2CroatianCaseInsensitive = 27,
		GbkChineseCaseInsensitive = 28,
		Cp1257LithuanianCaseInsensitive = 29,
		Latin5TurkishCaseInsensitive = 30,
		Latin1German2CaseInsensitive = 31,
		Armscii8GeneralCaseInsensitive = 32,
		Utf8GeneralCaseInsensitive = 33,
		Cp1250CzechCaseSensitive = 34,
		Ucs2GeneralCaseInsensitive = 35,
		Cp866GeneralCaseInsensitive = 36,
		Keybcs2GeneralCaseInsensitive = 37,
		MacceGeneralCaseInsensitive = 38,
		MacromanGeneralCaseInsensitive = 39,
		Cp852GeneralCaseInsensitive = 40,
		Latin7GeneralCaseInsensitive = 41,
		Latin7GeneralCaseSensitive = 42,
		MacceBinary = 43,
		Cp1250CroatianCaseInsensitive = 44,
		Utf8Mb4GeneralCaseInsensitive = 45,
		Utf8Mb4Binary = 46,
		Latin1Binary = 47,
		Latin1GeneralCaseInsensitive = 48,
		Latin1GeneralCaseSensitive = 49,
		Cp1251Binary = 50,
		Cp1251GeneralCaseInsensitive = 51,
		Cp1251GeneralCaseSensitive = 52,
		MacromanBinary = 53,
		Utf16GeneralCaseInsensitive = 54,
		Utf16Binary = 55,
		Utf16leGeneralCaseInsensitive = 56,
		Cp1256GeneralCaseInsensitive = 57,
		Cp1257Binary = 58,
		Cp1257GeneralCaseInsensitive = 59,
		Utf32GeneralCaseInsensitive = 60,
		Utf32Binary = 61,
		Utf16leBinary = 62,
		Binary = 63,
		Armscii8Binary = 64,
		AsciiBinary = 65,
		Cp1250Binary = 66,
		Cp1256Binary = 67,
		Cp866Binary = 68,
		Dec8Binary = 69,
		GreekBinary = 70,
		HebrewBinary = 71,
		Hp8Binary = 72,
		Keybcs2Binary = 73,
		Koi8rBinary = 74,
		Koi8uBinary = 75,
		Latin2Binary = 77,
		Latin5Binary = 78,
		Latin7Binary = 79,
		Cp850Binary = 80,
		Cp852Binary = 81,
		Swe7Binary = 82,
		Utf8Binary = 83,
		Big5Binary = 84,
		EuckrBinary = 85,
		Gb2312Binary = 86,
		GbkBinary = 87,
		SjisBinary = 88,
		Tis620Binary = 89,
		Ucs2Binary = 90,
		UjisBinary = 91,
		Geostd8GeneralCaseInsensitive = 92,
		Geostd8Binary = 93,
		Latin1SpanishCaseInsensitive = 94,
		Cp932JapaneseCaseInsensitive = 95,
		Cp932Binary = 96,
		EucjpmsJapaneseCaseInsensitive = 97,
		EucjpmsBinary = 98,
		Cp1250PolishCaseInsensitive = 99,
		Utf16UnicodeCaseInsensitive = 101,
		Utf16IcelandicCaseInsensitive = 102,
		Utf16LatvianCaseInsensitive = 103,
		Utf16RomanianCaseInsensitive = 104,
		Utf16SlovenianCaseInsensitive = 105,
		Utf16PolishCaseInsensitive = 106,
		Utf16EstonianCaseInsensitive = 107,
		Utf16SpanishCaseInsensitive = 108,
		Utf16SwedishCaseInsensitive = 109,
		Utf16TurkishCaseInsensitive = 110,
		Utf16CzechCaseInsensitive = 111,
		Utf16DanishCaseInsensitive = 112,
		Utf16LithuanianCaseInsensitive = 113,
		Utf16SlovakCaseInsensitive = 114,
		Utf16Spanish2CaseInsensitive = 115,
		Utf16RomanCaseInsensitive = 116,
		Utf16PersianCaseInsensitive = 117,
		Utf16EsperantoCaseInsensitive = 118,
		Utf16HungarianCaseInsensitive = 119,
		Utf16SinhalaCaseInsensitive = 120,
		Utf16German2CaseInsensitive = 121,
		Utf16CroatianCaseInsensitive = 122,
		Utf16Unicode520CaseInsensitive = 123,
		Utf16VietnameseCaseInsensitive = 124,
		Ucs2UnicodeCaseInsensitive = 128,
		Ucs2IcelandicCaseInsensitive = 129,
		Ucs2LatvianCaseInsensitive = 130,
		Ucs2RomanianCaseInsensitive = 131,
		Ucs2SlovenianCaseInsensitive = 132,
		Ucs2PolishCaseInsensitive = 133,
		Ucs2EstonianCaseInsensitive = 134,
		Ucs2SpanishCaseInsensitive = 135,
		Ucs2SwedishCaseInsensitive = 136,
		Ucs2TurkishCaseInsensitive = 137,
		Ucs2CzechCaseInsensitive = 138,
		Ucs2DanishCaseInsensitive = 139,
		Ucs2LithuanianCaseInsensitive = 140,
		Ucs2SlovakCaseInsensitive = 141,
		Ucs2Spanish2CaseInsensitive = 142,
		Ucs2RomanCaseInsensitive = 143,
		Ucs2PersianCaseInsensitive = 144,
		Ucs2EsperantoCaseInsensitive = 145,
		Ucs2HungarianCaseInsensitive = 146,
		Ucs2SinhalaCaseInsensitive = 147,
		Ucs2German2CaseInsensitive = 148,
		Ucs2CroatianCaseInsensitive = 149,
		Ucs2Unicode520CaseInsensitive = 150,
		Ucs2VietnameseCaseInsensitive = 151,
		Ucs2GeneralMySql500CaseInsensitive = 159,
		Utf32UnicodeCaseInsensitive = 160,
		Utf32IcelandicCaseInsensitive = 161,
		Utf32LatvianCaseInsensitive = 162,
		Utf32RomanianCaseInsensitive = 163,
		Utf32SlovenianCaseInsensitive = 164,
		Utf32PolishCaseInsensitive = 165,
		Utf32EstonianCaseInsensitive = 166,
		Utf32SpanishCaseInsensitive = 167,
		Utf32SwedishCaseInsensitive = 168,
		Utf32TurkishCaseInsensitive = 169,
		Utf32CzechCaseInsensitive = 170,
		Utf32DanishCaseInsensitive = 171,
		Utf32LithuanianCaseInsensitive = 172,
		Utf32SlovakCaseInsensitive = 173,
		Utf32Spanish2CaseInsensitive = 174,
		Utf32RomanCaseInsensitive = 175,
		Utf32PersianCaseInsensitive = 176,
		Utf32EsperantoCaseInsensitive = 177,
		Utf32HungarianCaseInsensitive = 178,
		Utf32SinhalaCaseInsensitive = 179,
		Utf32German2CaseInsensitive = 180,
		Utf32CroatianCaseInsensitive = 181,
		Utf32Unicode520CaseInsensitive = 182,
		Utf32VietnameseCaseInsensitive = 183,
		Utf8UnicodeCaseInsensitive = 192,
		Utf8IcelandicCaseInsensitive = 193,
		Utf8LatvianCaseInsensitive = 194,
		Utf8RomanianCaseInsensitive = 195,
		Utf8SlovenianCaseInsensitive = 196,
		Utf8PolishCaseInsensitive = 197,
		Utf8EstonianCaseInsensitive = 198,
		Utf8SpanishCaseInsensitive = 199,
		Utf8SwedishCaseInsensitive = 200,
		Utf8TurkishCaseInsensitive = 201,
		Utf8CzechCaseInsensitive = 202,
		Utf8DanishCaseInsensitive = 203,
		Utf8LithuanianCaseInsensitive = 204,
		Utf8SlovakCaseInsensitive = 205,
		Utf8Spanish2CaseInsensitive = 206,
		Utf8RomanCaseInsensitive = 207,
		Utf8PersianCaseInsensitive = 208,
		Utf8EsperantoCaseInsensitive = 209,
		Utf8HungarianCaseInsensitive = 210,
		Utf8SinhalaCaseInsensitive = 211,
		Utf8German2CaseInsensitive = 212,
		Utf8CroatianCaseInsensitive = 213,
		Utf8Unicode520CaseInsensitive = 214,
		Utf8VietnameseCaseInsensitive = 215,
		Utf8GeneralMySql500CaseInsensitive = 223,
		Utf8Mb4UnicodeCaseInsensitive = 224,
		Utf8Mb4IcelandicCaseInsensitive = 225,
		Utf8Mb4LatvianCaseInsensitive = 226,
		Utf8Mb4RomanianCaseInsensitive = 227,
		Utf8Mb4SlovenianCaseInsensitive = 228,
		Utf8Mb4PolishCaseInsensitive = 229,
		Utf8Mb4EstonianCaseInsensitive = 230,
		Utf8Mb4SpanishCaseInsensitive = 231,
		Utf8Mb4SwedishCaseInsensitive = 232,
		Utf8Mb4TurkishCaseInsensitive = 233,
		Utf8Mb4CzechCaseInsensitive = 234,
		Utf8Mb4DanishCaseInsensitive = 235,
		Utf8Mb4LithuanianCaseInsensitive = 236,
		Utf8Mb4SlovakCaseInsensitive = 237,
		Utf8Mb4Spanish2CaseInsensitive = 238,
		Utf8Mb4RomanCaseInsensitive = 239,
		Utf8Mb4PersianCaseInsensitive = 240,
		Utf8Mb4EsperantoCaseInsensitive = 241,
		Utf8Mb4HungarianCaseInsensitive = 242,
		Utf8Mb4SinhalaCaseInsensitive = 243,
		Utf8Mb4German2CaseInsensitive = 244,
		Utf8Mb4CroatianCaseInsensitive = 245,
		Utf8Mb4Unicode520CaseInsensitive = 246,
		Utf8Mb4VietnameseCaseInsensitive = 247,
		Gb18030ChineseCaseInsensitive = 248,
		Gb18030Binary = 249,
		Gb18030Unicode520CaseInsensitive = 250,
		Utf8Mb4Uca900AccentInsensitiveCaseInsensitive = 255,
		Utf8Mb4GermanPhonebookUca900AccentInsensitiveCaseInsensitive = 256,
		Utf8Mb4IcelandicUca900AccentInsensitiveCaseInsensitive = 257,
		Utf8Mb4LatvianUca900AccentInsensitiveCaseInsensitive = 258,
		Utf8Mb4RomanianUca900AccentInsensitiveCaseInsensitive = 259,
		Utf8Mb4SlovenianUca900AccentInsensitiveCaseInsensitive = 260,
		Utf8Mb4PolishUca900AccentInsensitiveCaseInsensitive = 261,
		Utf8Mb4EstonianUca900AccentInsensitiveCaseInsensitive = 262,
		Utf8Mb4SpanishUca900AccentInsensitiveCaseInsensitive = 263,
		Utf8Mb4SwedishUca900AccentInsensitiveCaseInsensitive = 264,
		Utf8Mb4TurkishUca900AccentInsensitiveCaseInsensitive = 265,
		Utf8Mb4CaseSensitiveUca900AccentInsensitiveCaseInsensitive = 266,
		Utf8Mb4DanishUca900AccentInsensitiveCaseInsensitive = 267,
		Utf8Mb4LithuanianUca900AccentInsensitiveCaseInsensitive = 268,
		Utf8Mb4SlovakUca900AccentInsensitiveCaseInsensitive = 269,
		Utf8Mb4TraditionalSpanishUca900AccentInsensitiveCaseInsensitive = 270,
		Utf8Mb4LatinUca900AccentInsensitiveCaseInsensitive = 271,
		Utf8Mb4EsperantoUca900AccentInsensitiveCaseInsensitive = 273,
		Utf8Mb4HungarianUca900AccentInsensitiveCaseInsensitive = 274,
		Utf8Mb4CroatianUca900AccentInsensitiveCaseInsensitive = 275,
		Utf8Mb4VietnameseUca900AccentInsensitiveCaseInsensitive = 277,
		Utf8Mb4Uca900AccentSensitiveCaseSensitive = 278,
		Utf8Mb4GermanPhonebookUca900AccentSensitiveCaseSensitive = 279,
		Utf8Mb4IcelandicUca900AccentSensitiveCaseSensitive = 280,
		Utf8Mb4LatvianUca900AccentSensitiveCaseSensitive = 281,
		Utf8Mb4RomanianUca900AccentSensitiveCaseSensitive = 282,
		Utf8Mb4SlovenianUca900AccentSensitiveCaseSensitive = 283,
		Utf8Mb4PolishUca900AccentSensitiveCaseSensitive = 284,
		Utf8Mb4EstonianUca900AccentSensitiveCaseSensitive = 285,
		Utf8Mb4SpanishUca900AccentSensitiveCaseSensitive = 286,
		Utf8Mb4SwedishUca900AccentSensitiveCaseSensitive = 287,
		Utf8Mb4TurkishUca900AccentSensitiveCaseSensitive = 288,
		Utf8Mb4CaseSensitiveUca900AccentSensitiveCaseSensitive = 289,
		Utf8Mb4DanishUca900AccentSensitiveCaseSensitive = 290,
		Utf8Mb4LithuanianUca900AccentSensitiveCaseSensitive = 291,
		Utf8Mb4SlovakUca900AccentSensitiveCaseSensitive = 292,
		Utf8Mb4TraditionalSpanishUca900AccentSensitiveCaseSensitive = 293,
		Utf8Mb4LatinUca900AccentSensitiveCaseSensitive = 294,
		Utf8Mb4EsperantoUca900AccentSensitiveCaseSensitive = 296,
		Utf8Mb4HungarianUca900AccentSensitiveCaseSensitive = 297,
		Utf8Mb4CroatianUca900AccentSensitiveCaseSensitive = 298,
		Utf8Mb4VietnameseUca900AccentSensitiveCaseSensitive = 300,
		Utf8Mb4JapaneseUca900AccentSensitiveCaseSensitive = 303,
	}

	internal static partial class Extensions
	{
		public static int MaxLength(this CharacterSet character)
		{
			switch (character)
			{
			case CharacterSet.Big5Binary:
			case CharacterSet.Big5ChineseCaseInsensitive:
			case CharacterSet.SjisBinary:
			case CharacterSet.SjisJapaneseCaseInsensitive:
			case CharacterSet.EuckrBinary:
			case CharacterSet.EuckrKoreanCaseInsensitive:
			case CharacterSet.Gb2312Binary:
			case CharacterSet.Gb2312ChineseCaseInsensitive:
			case CharacterSet.GbkBinary:
			case CharacterSet.GbkChineseCaseInsensitive:
			case CharacterSet.Ucs2Binary:
			case CharacterSet.Ucs2UnicodeCaseInsensitive:
			case CharacterSet.Ucs2IcelandicCaseInsensitive:
			case CharacterSet.Ucs2LatvianCaseInsensitive:
			case CharacterSet.Ucs2RomanianCaseInsensitive:
			case CharacterSet.Ucs2SlovenianCaseInsensitive:
			case CharacterSet.Ucs2PolishCaseInsensitive:
			case CharacterSet.Ucs2EstonianCaseInsensitive:
			case CharacterSet.Ucs2SpanishCaseInsensitive:
			case CharacterSet.Ucs2SwedishCaseInsensitive:
			case CharacterSet.Ucs2TurkishCaseInsensitive:
			case CharacterSet.Ucs2CzechCaseInsensitive:
			case CharacterSet.Ucs2DanishCaseInsensitive:
			case CharacterSet.Ucs2LithuanianCaseInsensitive:
			case CharacterSet.Ucs2SlovakCaseInsensitive:
			case CharacterSet.Ucs2Spanish2CaseInsensitive:
			case CharacterSet.Ucs2RomanCaseInsensitive:
			case CharacterSet.Ucs2PersianCaseInsensitive:
			case CharacterSet.Ucs2EsperantoCaseInsensitive:
			case CharacterSet.Ucs2HungarianCaseInsensitive:
			case CharacterSet.Ucs2SinhalaCaseInsensitive:
			case CharacterSet.Ucs2German2CaseInsensitive:
			case CharacterSet.Ucs2CroatianCaseInsensitive:
			case CharacterSet.Ucs2Unicode520CaseInsensitive:
			case CharacterSet.Ucs2VietnameseCaseInsensitive:
			case CharacterSet.Ucs2GeneralMySql500CaseInsensitive:
			case CharacterSet.Ucs2GeneralCaseInsensitive:
			case CharacterSet.Cp932Binary:
			case CharacterSet.Cp932JapaneseCaseInsensitive:
				return 2;
			case CharacterSet.UjisBinary:
			case CharacterSet.UjisJapaneseCaseInsensitive:
			case CharacterSet.EucjpmsBinary:
			case CharacterSet.EucjpmsJapaneseCaseInsensitive:
			case CharacterSet.Utf8Binary:
			case CharacterSet.Utf8GeneralCaseInsensitive:
			case CharacterSet.Utf8UnicodeCaseInsensitive:
			case CharacterSet.Utf8IcelandicCaseInsensitive:
			case CharacterSet.Utf8LatvianCaseInsensitive:
			case CharacterSet.Utf8RomanianCaseInsensitive:
			case CharacterSet.Utf8SlovenianCaseInsensitive:
			case CharacterSet.Utf8PolishCaseInsensitive:
			case CharacterSet.Utf8EstonianCaseInsensitive:
			case CharacterSet.Utf8SpanishCaseInsensitive:
			case CharacterSet.Utf8SwedishCaseInsensitive:
			case CharacterSet.Utf8TurkishCaseInsensitive:
			case CharacterSet.Utf8CzechCaseInsensitive:
			case CharacterSet.Utf8DanishCaseInsensitive:
			case CharacterSet.Utf8LithuanianCaseInsensitive:
			case CharacterSet.Utf8SlovakCaseInsensitive:
			case CharacterSet.Utf8Spanish2CaseInsensitive:
			case CharacterSet.Utf8RomanCaseInsensitive:
			case CharacterSet.Utf8PersianCaseInsensitive:
			case CharacterSet.Utf8EsperantoCaseInsensitive:
			case CharacterSet.Utf8HungarianCaseInsensitive:
			case CharacterSet.Utf8SinhalaCaseInsensitive:
			case CharacterSet.Utf8German2CaseInsensitive:
			case CharacterSet.Utf8CroatianCaseInsensitive:
			case CharacterSet.Utf8Unicode520CaseInsensitive:
			case CharacterSet.Utf8VietnameseCaseInsensitive:
			case CharacterSet.Utf8GeneralMySql500CaseInsensitive:
				return 3;
			case CharacterSet.Utf8Mb4UnicodeCaseInsensitive:
			case CharacterSet.Utf8Mb4IcelandicCaseInsensitive:
			case CharacterSet.Utf8Mb4LatvianCaseInsensitive:
			case CharacterSet.Utf8Mb4RomanianCaseInsensitive:
			case CharacterSet.Utf8Mb4SlovenianCaseInsensitive:
			case CharacterSet.Utf8Mb4PolishCaseInsensitive:
			case CharacterSet.Utf8Mb4EstonianCaseInsensitive:
			case CharacterSet.Utf8Mb4SpanishCaseInsensitive:
			case CharacterSet.Utf8Mb4SwedishCaseInsensitive:
			case CharacterSet.Utf8Mb4TurkishCaseInsensitive:
			case CharacterSet.Utf8Mb4CzechCaseInsensitive:
			case CharacterSet.Utf8Mb4DanishCaseInsensitive:
			case CharacterSet.Utf8Mb4LithuanianCaseInsensitive:
			case CharacterSet.Utf8Mb4SlovakCaseInsensitive:
			case CharacterSet.Utf8Mb4Spanish2CaseInsensitive:
			case CharacterSet.Utf8Mb4RomanCaseInsensitive:
			case CharacterSet.Utf8Mb4PersianCaseInsensitive:
			case CharacterSet.Utf8Mb4EsperantoCaseInsensitive:
			case CharacterSet.Utf8Mb4HungarianCaseInsensitive:
			case CharacterSet.Utf8Mb4SinhalaCaseInsensitive:
			case CharacterSet.Utf8Mb4German2CaseInsensitive:
			case CharacterSet.Utf8Mb4CroatianCaseInsensitive:
			case CharacterSet.Utf8Mb4Unicode520CaseInsensitive:
			case CharacterSet.Utf8Mb4VietnameseCaseInsensitive:
			case CharacterSet.Gb18030ChineseCaseInsensitive:
			case CharacterSet.Gb18030Binary:
			case CharacterSet.Gb18030Unicode520CaseInsensitive:
			case CharacterSet.Utf8Mb4Uca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4GermanPhonebookUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4IcelandicUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4LatvianUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4RomanianUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4SlovenianUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4PolishUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4EstonianUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4SpanishUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4SwedishUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4TurkishUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4CaseSensitiveUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4DanishUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4LithuanianUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4SlovakUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4TraditionalSpanishUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4LatinUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4EsperantoUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4HungarianUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4CroatianUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4VietnameseUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4Uca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4GermanPhonebookUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4IcelandicUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4LatvianUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4RomanianUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4SlovenianUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4PolishUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4EstonianUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4SpanishUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4SwedishUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4TurkishUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4CaseSensitiveUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4DanishUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4LithuanianUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4SlovakUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4TraditionalSpanishUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4LatinUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4EsperantoUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4HungarianUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4CroatianUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4VietnameseUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4JapaneseUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf16GeneralCaseInsensitive:
			case CharacterSet.Utf16Binary:
			case CharacterSet.Utf16leBinary:
			case CharacterSet.Utf16leGeneralCaseInsensitive:
			case CharacterSet.Utf16UnicodeCaseInsensitive:
			case CharacterSet.Utf16IcelandicCaseInsensitive:
			case CharacterSet.Utf16LatvianCaseInsensitive:
			case CharacterSet.Utf16RomanianCaseInsensitive:
			case CharacterSet.Utf16SlovenianCaseInsensitive:
			case CharacterSet.Utf16PolishCaseInsensitive:
			case CharacterSet.Utf16EstonianCaseInsensitive:
			case CharacterSet.Utf16SpanishCaseInsensitive:
			case CharacterSet.Utf16SwedishCaseInsensitive:
			case CharacterSet.Utf16TurkishCaseInsensitive:
			case CharacterSet.Utf16CzechCaseInsensitive:
			case CharacterSet.Utf16DanishCaseInsensitive:
			case CharacterSet.Utf16LithuanianCaseInsensitive:
			case CharacterSet.Utf16SlovakCaseInsensitive:
			case CharacterSet.Utf16Spanish2CaseInsensitive:
			case CharacterSet.Utf16RomanCaseInsensitive:
			case CharacterSet.Utf16PersianCaseInsensitive:
			case CharacterSet.Utf16EsperantoCaseInsensitive:
			case CharacterSet.Utf16HungarianCaseInsensitive:
			case CharacterSet.Utf16SinhalaCaseInsensitive:
			case CharacterSet.Utf16German2CaseInsensitive:
			case CharacterSet.Utf16CroatianCaseInsensitive:
			case CharacterSet.Utf16Unicode520CaseInsensitive:
			case CharacterSet.Utf16VietnameseCaseInsensitive:
			case CharacterSet.Utf32GeneralCaseInsensitive:
			case CharacterSet.Utf32Binary:
			case CharacterSet.Utf32UnicodeCaseInsensitive:
			case CharacterSet.Utf32IcelandicCaseInsensitive:
			case CharacterSet.Utf32LatvianCaseInsensitive:
			case CharacterSet.Utf32RomanianCaseInsensitive:
			case CharacterSet.Utf32SlovenianCaseInsensitive:
			case CharacterSet.Utf32PolishCaseInsensitive:
			case CharacterSet.Utf32EstonianCaseInsensitive:
			case CharacterSet.Utf32SpanishCaseInsensitive:
			case CharacterSet.Utf32SwedishCaseInsensitive:
			case CharacterSet.Utf32TurkishCaseInsensitive:
			case CharacterSet.Utf32CzechCaseInsensitive:
			case CharacterSet.Utf32DanishCaseInsensitive:
			case CharacterSet.Utf32LithuanianCaseInsensitive:
			case CharacterSet.Utf32SlovakCaseInsensitive:
			case CharacterSet.Utf32Spanish2CaseInsensitive:
			case CharacterSet.Utf32RomanCaseInsensitive:
			case CharacterSet.Utf32PersianCaseInsensitive:
			case CharacterSet.Utf32EsperantoCaseInsensitive:
			case CharacterSet.Utf32HungarianCaseInsensitive:
			case CharacterSet.Utf32SinhalaCaseInsensitive:
			case CharacterSet.Utf32German2CaseInsensitive:
			case CharacterSet.Utf32CroatianCaseInsensitive:
			case CharacterSet.Utf32Unicode520CaseInsensitive:
			case CharacterSet.Utf32VietnameseCaseInsensitive:
				return 4;
			default:
				return 1;
			}
		}
	}
}
