using System;
using System.Collections.Generic;
using Grammar.Helper;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Grammar.Core
{
    [Serializable]
    public struct Rule
    {
        public string inputWord; // word name
        public string InputWord => inputWord.RemoveWhitespace();
        public RuleOutput[] outputSentences; // output Sentences
    }

    [Serializable]
    public struct RuleOutput
    {
        [Range(0, 1), Tooltip("The 0 means it is never called, but the 1 does not mean it is always called.")]
        public float probability;
        [TextArea(1, 20)]
        public string outputSentence; // output sentence
    }

    public struct RuleOutputIndex
    {
        public int index;
        public string outputSentence; // output sentence
    }

    /// <summary>
    /// To create = Right click on Project window -> Create/Grammar/Grammar Rule Data.
    /// It holds words and their resulting sentences data for one grammar rule set.
    /// </summary>
    [CreateAssetMenu(fileName = "Grammar Rule Data", menuName = "Grammar/Grammar Rule Data", order = 1)]
    public class GrammarRuleData : ScriptableObject
    {
        [SerializeField, Tooltip("List of all grammar rules.")]
        private List<Rule> rules = new List<Rule>();

        /// <summary>
        /// It searches the rules list to find the word if it finds returns its output string.
        /// It randomly decides which output it will return by calculating with their probability.
        /// </summary>
        /// <param name="word">Word to be found in the rules list</param>
        /// <param name="indexesException">Do not return this index</param>
        /// <returns>Output string of the word</returns>
        public RuleOutputIndex ReturnRuleOutput(Word word, List<int> indexesException)
        {
            // define output to be returned.
            RuleOutputIndex output = new RuleOutputIndex {outputSentence = "Null"}; 
            // compare every input word in the rules list.
            for (int i = 0; i < rules.Count; i++)
            {
                if (!word.Name.Equals(rules[i].InputWord)) continue; // if not equal, continue comparing.

                // return random index by calculating with probability of outputs.
                int randomIndex = RandomIndex(rules[i].outputSentences, indexesException); 

                if (randomIndex == -1) // if there is no output that can be returned.
                {
                    output.outputSentence = "Null"; // return Null string.
                    output.index = -1;
                    break;
                }

                output.outputSentence = rules[i].outputSentences[randomIndex].outputSentence; // return output with that chosen random index.
                output.index = randomIndex;
                if (string.IsNullOrWhiteSpace(output.outputSentence)) // if output is empty string return Null string.
                {
                    output.outputSentence = "Null";
                }
                break;
            }
            return output;
        }
        /// <summary>
        /// It searches the rules list to find the word if it finds returns its output string.
        /// It randomly decides which output it will return by calculating with their probability.
        /// </summary>
        /// <param name="word">Word to be found in the rules list</param>
        /// <returns>Output string of the word</returns>
        public RuleOutputIndex ReturnRuleOutput(Word word)
        {
            // define output to be returned.
            RuleOutputIndex output = new RuleOutputIndex { outputSentence = "Null" };
            // compare every input word in the rules list.
            for (int i = 0; i < rules.Count; i++)
            {
                if (!word.Name.Equals(rules[i].InputWord)) continue; // if not equal, continue comparing.

                // return random index by calculating with probability of outputs.
                int randomIndex = RandomIndex(rules[i].outputSentences, new List<int>());

                if (randomIndex == -1) // if there is no output that can be returned.
                {
                    output.outputSentence = "Null"; // return Null string.
                    output.index = -1;
                    break;
                }

                output.outputSentence = rules[i].outputSentences[randomIndex].outputSentence; // return output with that chosen random index.
                output.index = randomIndex;
                if (string.IsNullOrWhiteSpace(output.outputSentence)) // if output is empty string return Null string.
                {
                    output.outputSentence = "Null";
                }
                break;
            }
            return output;
        }

        /// <summary>
        /// It returns a random index with considering the probability of outputs.
        /// </summary>
        /// <param name="outputs">Outputs with a probability</param>
        /// <param name="indexesException">Do not return this index</param>
        /// <returns>Index of random chosen output</returns>
        private static int RandomIndex(RuleOutput[] outputs, List<int> indexesException)
        {
            // calculate total probability of this outputs array.
            float totalProbability = 0f;
            for (int t = 0; t < outputs.Length; t++)
            {
                if (indexesException.Contains(t))
                    continue;

                totalProbability += outputs[t].probability;
            }
            //

            // random float number between 0 and totalProbability.
            float diceRoll = Random.Range(0, totalProbability); 

            // compare diceRoll with every element`s cumulative probability.
            // if it is smaller than them, return that element index.
            float cumulative = 0f;
            for (int i = 0; i < outputs.Length; i++)
            {
                if (indexesException.Contains(i))
                    continue;

                cumulative += outputs[i].probability;
                if (diceRoll < cumulative)
                    return i;
            }
            //
            return -1;
        }
    }
}
