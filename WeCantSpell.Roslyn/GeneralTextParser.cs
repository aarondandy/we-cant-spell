﻿using System;
using System.Collections.Generic;

namespace WeCantSpell.Roslyn
{
    public static class GeneralTextParser
    {
        public static List<ParsedTextSpan> SplitWordParts(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var results = new List<ParsedTextSpan>();
            if (text.Length == 0)
            {
                return results;
            }

            var partStartIndex = 0;
            var prevChar = text[0];
            var prevCharType = ClassifyCharType(prevChar);

            char currChar;
            CharType currCharType;

            if (text.Length > 1)
            {
                currChar = text[1];
                currCharType = ClassifyCharType(currChar);
            }
            else
            {
                currChar = '\0';
                currCharType = prevCharType;
            }

            var prevEffectiveType = GetEffectiveCharType(prevChar, prevCharType, CharType.Unknown, currCharType);

            for (int searchIndex = 1, nextIndex = 2; searchIndex < text.Length; searchIndex = nextIndex++)
            {
                char nextChar;
                CharType nextCharType;
                if (nextIndex < text.Length)
                {
                    nextChar = text[nextIndex];
                    nextCharType = ClassifyCharType(nextChar);
                }
                else
                {
                    nextChar = '\0';
                    nextCharType = CharType.WhiteSpace;
                }

                var currEffectiveType = GetEffectiveCharType(currChar, currCharType, prevCharType, nextCharType);
                var currentIsWord = currEffectiveType == CharType.Word;
                var previousWasWord = prevEffectiveType == CharType.Word;

                if (currentIsWord != previousWasWord)
                {
                    results.Add(new ParsedTextSpan(text.Substring(partStartIndex, searchIndex - partStartIndex), partStartIndex, previousWasWord));

                    partStartIndex = searchIndex;
                }

                prevChar = currChar;
                prevCharType = currCharType;
                prevEffectiveType = currEffectiveType;

                currChar = nextChar;
                currCharType = nextCharType;
            }

            if (partStartIndex < text.Length)
            {
                results.Add(new ParsedTextSpan(text.Substring(partStartIndex, text.Length - partStartIndex), partStartIndex, prevEffectiveType == CharType.Word));
            }

            return results;
        }

        static CharType GetEffectiveCharType(char currChar, CharType currCharType, CharType prevCharType, CharType nextCharType) =>
            currCharType != CharType.Word
            && prevCharType == CharType.Word
            && nextCharType == CharType.Word
            && IsWordJoinChar(currChar)
            ? CharType.Word
            : currCharType;

        static CharType ClassifyCharType(char current)
        {
            if (char.IsLetterOrDigit(current))
            {
                return CharType.Word;
            }
            if (char.IsWhiteSpace(current))
            {
                return CharType.WhiteSpace;
            }
            if (char.IsPunctuation(current))
            {
                return CharType.Punctuation;
            }

            return CharType.Unknown;
        }

        static bool IsWordJoinChar(char c) =>
            IsHyphen(c) || IsApostrophe(c);

        static bool IsHyphen(char c) =>
            c == '-'
            || c == '\u2010'
            || c == '\u2212'
            || c == '\u2014'
            || c == '\u2013';

        static bool IsApostrophe(char c) =>
            c == '\''
            || c == '’' // U+2019 - RIGHT SINGLE QUOTATION MARK
            || c == 'ʼ' // U+02BC - MODIFIER LETTER APOSTROPHE
            || c == '＇'; // full width

        enum CharType : byte
        {
            Unknown = 0,
            Word = 1,
            WhiteSpace = 2,
            Punctuation = 4
        }
    }
}
