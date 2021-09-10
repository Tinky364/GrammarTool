using System.Collections.Generic;
using UnityEngine;

namespace Grammar.Core
{
    public class Sentence
    {
        public List<Word> Words { get; }

        public string FullString { get; private set; } = "";

        public Sentence()
        {
            Words = new List<Word>();
        }

        public Sentence(string fullString)
        {
            FullString = fullString;
            Words = new List<Word>();
            Words = SplitSentenceIntoWords(fullString, null);
        }

        public void AddWord(Word word)
        {
            Words.Add(word);
            FullString += word.Value;
        }

        public void AddWords(List<Word> words)
        {
            Words.AddRange(words);
            foreach (Word word in words)
            {
                FullString += word.Value;
            }
        }

        public static List<Word> SplitSentenceIntoWords(string str, Word parentWord)
        {
            List<Word> words = new List<Word>();
            string value = "";
            bool wordStarted = false;

            int i = 0;
            while (i < str.Length) // loop all chars in the sentence
            {
                char curChar = str[i];

                // comment char
                if (curChar.Equals('/'))
                {
                    return words;
                }

                // start word and move to the next char
                if (curChar.Equals('<'))
                {
                    if (!wordStarted)
                        wordStarted = true;
                    else // word started before
                    {
                        Debug.LogError("Grammar Mistake: A new word is started before closing the word !");
                        return words;
                    }

                    value += curChar;
                    i++;
                    continue;
                }
                //

                // if the first char of the word is not the '<', then there is a mistake in the grammar rules. 
                if (!wordStarted)
                {
                    if (char.IsWhiteSpace(curChar))
                    {
                        i++;
                        continue;
                    }
                    Debug.LogError("Grammar Mistake: The first char of the word is not '<' !");
                    return words;
                }
                //

                // add all chars different than the '>' and move to the next char
                if (!curChar.Equals('>'))
                {
                    value += curChar;
                    i++;
                    continue;
                }
                //

                // there is no other char that can be added other than the '>'. 
                value += curChar; // add the '>' to the word
                //

                Word curWord = new Word(value, parentWord);
                words.Add(curWord); // add word to the words list
                wordStarted = false;
                value = "";
                i++;
            }

            return words;
        }
    }
}
