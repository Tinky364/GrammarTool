using System;
using System.Collections.Generic;
using Grammar.Editor;
using Grammar.Helper;
using UnityEngine;

namespace Grammar.Core
{
    /// <summary>
    /// The main component that executes the grammar rule.
    /// </summary>
    [AddComponentMenu("Grammar Executor")]
    public class GrammarExecutor : MonoBehaviour
    {
        #region INSPECTOR VARIABLES

        [SerializeField, Tooltip("If it is true Iteration Count will be ignored.")]
        private bool endlessIteration = false;

        [SerializeField, Min(0), Tooltip("It only works when the Endless Iteration variable is false.")]
        private int iterationCount = 1;

        [SerializeField, TextArea(1, 20), Tooltip("The first sentence to be executed.")]
        private string startingSentence = "";

        [SerializeField, Expandable, Tooltip("To create Grammar Rule Data = Right click on Project window -> Create/Grammar/Grammar Rule Data")]
        private GrammarRuleData grammarRuleData = default;

        [SerializeField, Expandable, Tooltip("To create Grammar Link Data = Right click on Project window -> Create/Grammar/Grammar Link Data")]
        private GrammarLinkData grammarLinkData = default;

        #endregion

        // helper components. they might be null.
        private GrammarIterator grammarIterator;
        private GrammarSpace grammarSpace;
        //

        public event Action<GrammarExecutor> OnEndOfExecution;
        private int IterationCount => endlessIteration ? int.MaxValue : iterationCount; // if endless iteration is true, it returns endless loop.
        private Sentence curSentence; // keeps the result of every grammar iteration.
        private Sentence preSentence; // keeps the previous result of every grammar iteration.
        private Transform holder;  // holder is the root GameObject of all spawned objWords in one execution.
        private List<Word> allSpawnedObjWordsInLastIteration; // it holds all object words spawned in the last sentence.

        // Space Error Data
        private bool spaceErrorFlag; // error flag
        private Word errorWord; // parent object of the last object that returned an error
        private List<int> errorIndexes; // index of the last object that returned an error
        //

        /// <summary>
        /// It is the starter method for executing grammar rules.
        /// It can be called from the editor by clicking the Execute Button in the Grammar Executor component.
        /// </summary>
        public void Execute()
        {
            // find if these components attached to this GameObject.
            if (grammarSpace == null)
            {
                if (TryGetComponent(out GrammarSpace space))
                    grammarSpace = space;
            }
            if (grammarIterator == null)
            {
                if (TryGetComponent(out GrammarIterator iterator))
                    grammarIterator = iterator;
            }
            //

            // start executing sentence
            if (grammarIterator != null) // if this GameObject has a GrammarIterator component.
            {
                for (int i = 0; i < grammarIterator.executeCount; i++)
                    ExecuteGrammar();
            }
            else // if this GameObject does not have a GrammarIterator component.
                ExecuteGrammar();
            //
        }

        /// <summary>
        /// It executes the starter sentence and resulting sentences of the starter sentence.
        /// </summary>
        private void ExecuteGrammar()
        {
            // Keep track of the execution time.
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            //

            allSpawnedObjWordsInLastIteration = new List<Word>(); // initialize list.

            curSentence = new Sentence(startingSentence); // initialize curSentence as the startingSentence in the editor.
            preSentence = curSentence; // initialize preSentence as the curSentence just for the first iteration.
            // start iterate sentences
            for (int i = 0; i < IterationCount; i++)
            {
                // print the resulting sentence of every iteration to the console.
                Debug.Log(i == 0
                    ? $"    Start Sentence: {curSentence.FullString}"
                    : $"    Sentence {i}: {curSentence.FullString}");

                // read the curSentence and return its resulting sentence to itself.
                Sentence resultingSentence = ExecuteSentenceAndReturnResultingSentence(curSentence);
                while (spaceErrorFlag) // if above method returns error 
                {
                    spaceErrorFlag = false;
                    // return a new sentence from the previous sentence because current sentence returns an error.
                    // current sentence will be a new returned sentence from the previous sentence.
                    curSentence = ReturnNewResultingSentenceWithoutErrorCheck(preSentence); 
                    resultingSentence = ExecuteSentenceAndReturnResultingSentence(curSentence); // execute and read the new current sentence.
                    // if the new returned sentence do not return a new error, print the resulting sentence to the console. 
                    if (!spaceErrorFlag)
                        Debug.Log($"    New Sentence {i}: {curSentence.FullString}"); // print the new current sentence to the console.
                    // if the new returned sentence returns a new error, iterate again.
                }
                preSentence = curSentence; // the previous sentence is the current sentence
                curSentence = resultingSentence; // the current sentence is the resulting sentence

                // stop iterating if there is no word to read any more in the current sentence
                if (curSentence.Words.Count <= 0)
                {
                    Debug.Log("-- End of Grammar Execution --");
                    break;
                }
            }

            OnEndOfExecution?.Invoke(this); // call On End of Execution Event

            // holder is the root GameObject of all spawned objWords.
            if (grammarIterator != null && holder != null)
            {
                holder.position = grammarIterator.ReturnPositionFromArea();
            }
            holder = null;
            //

            // Get the execution time and print the information for this run.
            stopWatch.Stop();
            Debug.Log($"-- Total Execution Time: {stopWatch.ElapsedMilliseconds}ms --");
            //
        }

        /// <summary>
        /// It reads sentence and returns resulting sentence of it.
        /// </summary>
        /// <param name="sentence">Sentence to be read</param>
        /// <returns>Resulting sentence</returns>
        private Sentence ExecuteSentenceAndReturnResultingSentence(Sentence sentence)
        {
            allSpawnedObjWordsInLastIteration.Clear();
            Sentence resultingSentence = new Sentence(); // sentence that will be returned at the end.
            Word lastObjTypeWord = null; // the last object type word to call methods of it if any in the sentence.
            foreach (Word word in sentence.Words) // loop all words in the sentence.
            {
                // start reading the word and return resulting sentence of it.
                switch (word.Type)
                {
                    // if the word type is object, search the word in the grammar rules and spawn it.
                    case WordType.Object:
                    {
                        // check whether the last spawned object will return error or not
                        if (lastObjTypeWord != null)
                        {
                            if (CheckSpaceError(ref lastObjTypeWord))
                            {
                                CreateSpaceErrorData(lastObjTypeWord);
                                return null; // stop reading
                            }
                        }
                        //

                        lastObjTypeWord = word; // the last object type word is this.
                        SpawnObjWord(ref lastObjTypeWord); // spawn it in the game
                        allSpawnedObjWordsInLastIteration.Add(lastObjTypeWord);
                        
                        // read the word to find its resulting sentence
                        RuleOutputIndex output = new RuleOutputIndex() { outputSentence = "Null" };
                        if (grammarRuleData != null)
                        {
                            output = grammarRuleData.ReturnRuleOutput(word);
                            word.childSentenceIndex = output.index;
                        }
                        if (output.outputSentence.Equals("Null")) continue; // if there is no rule for this word continue to next word.
                        resultingSentence.AddWords(Sentence.SplitSentenceIntoWords(output.outputSentence, word));
                        //

                        break;
                    }
                    // if it is rule type word, search the word in the grammar rules.
                    case WordType.Rule:
                    {
                        // check whether the last spawned object will return error or not
                        if (lastObjTypeWord != null)
                        {
                            if (CheckSpaceError(ref lastObjTypeWord))
                            {
                                CreateSpaceErrorData(lastObjTypeWord);
                                return null; // stop reading
                            }
                        }
                        //

                        lastObjTypeWord = null; // last read word is not an object type word now

                        // read the word to find its resulting sentence
                        RuleOutputIndex output = new RuleOutputIndex() {outputSentence = "Null"};
                        if (grammarRuleData != null)
                        {
                            output = grammarRuleData.ReturnRuleOutput(word);
                            word.childSentenceIndex = output.index;
                        }
                        if (output.outputSentence.Equals("Null")) continue; // if there is no rule for this word continue to next word.
                        resultingSentence.AddWords(Sentence.SplitSentenceIntoWords(output.outputSentence, word));
                        //

                        break;
                    }
                    // if the word type is method, search the word in the grammar methods of
                    // the last object type word.
                    case WordType.Method when lastObjTypeWord != null:
                    {
                        if (!CallCorrespondingMethod(word, lastObjTypeWord))
                        {
                            Debug.LogWarning($"Could not find the corresponding method word {word.Value}" +
                                             $" for the objWord word {lastObjTypeWord.Value}!");
                        }
                        break;
                    }
                    // if the last object type word is null, print error
                    case WordType.Method:
                    {
                        Debug.LogError($"Method words {word} should have been placed after objWord!");
                        break;
                    }
                }
                //
            }

            // check whether the last spawned object will return error or not
            if (lastObjTypeWord != null)
            {
                if (CheckSpaceError(ref lastObjTypeWord))
                {
                    CreateSpaceErrorData(lastObjTypeWord);
                    return null; // stop reading
                }
            }
            //

            return resultingSentence;
        }

        /// <summary>
        /// It calls the corresponding methodWord word`s method from the lastObjTypeWord word
        /// if it has the method.
        /// </summary>
        /// <param name="methodWord">Method type word to be called</param>
        /// <param name="lastObjTypeWord">Object type word that will call the method</param>
        /// <returns>It returns true if the method called.</returns>
        private static bool CallCorrespondingMethod(Word methodWord, Word lastObjTypeWord)
        {
            if (lastObjTypeWord.objWord == null)
                Debug.LogError($"ObjWord of word {lastObjTypeWord.Value} should have been spawned first!");

            return lastObjTypeWord.objWord.CallCorrespondingMethod(methodWord);
        }

        /// <summary>
        /// It spawns the object type word`s prefab from the grammar link data.
        /// </summary>
        /// <param name="objTypeWord">Object type word to be spawned</param>
        private void SpawnObjWord(ref Word objTypeWord)
        {
            // spawn the holder when the first time an objWord spawns
            // holder is the root GameObject of all spawned objWords.
            if (holder == null && grammarSpace != null)
            {
                holder = new GameObject(objTypeWord.Name + " Holder").transform;
                holder.position = grammarSpace.SpawnStartPosition;
            }
            //

            // read linked prefab index of the objWord from its parameters and spawn accordingly.
            // note: if the objWord does not have any parameters and the index variable is -1,
            // grammarLinkData returns a random prefab.
            int index = -1;
            string str = objTypeWord.Args;
            if (!string.IsNullOrWhiteSpace(str))
                index = str.StringToInt();

            // find corresponding prefab with the word and the index of the prefab from the grammar link data.
            // note: if the index is -1, grammarLinkData returns a random prefab.
            ObjWord prefab = grammarLinkData.ReturnPrefab(objTypeWord, index);
            if (prefab != null) // if the word has a linked prefab, spawn it
            {
                // spawn and initialize objWord variable of this word 
                objTypeWord.objWord = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity); 
                objTypeWord.objWord.name = objTypeWord.Name;
                objTypeWord.objWord.transform.SetParent(holder, false);
            }
            else // if the word does not have a linked prefab, spawn an empty GameObject with an ObjWord component.
            {
                // spawn and initialize objWord variable of this word 
                objTypeWord.objWord = new GameObject(objTypeWord.Name).AddComponent<ObjWord>();
                objTypeWord.objWord.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                objTypeWord.objWord.transform.SetParent(holder, false);
            }

            objTypeWord.objWord.word = objTypeWord; // initialize the word variable of the spawned objWord to this objWord word.
            objTypeWord.objWord.RegisterToOnEndOfExecution(this); // register spawned objWord to the OnEndOfExecution event of this grammar executor  
            objTypeWord.objWord.OnSpawn(); // call OnSpawn method of spawned objWord

            //Debug.Log($"        Method: SpawnObjWord() -> GameObject: {objWord.objWord.name}", objWord.objWord.gameObject);
        }

        #region GRAMMAR SPACE

        /// <summary>
        /// It reads sentence and returns result sentence of it.
        /// </summary>
        /// <param name="sentence">Sentence to be read</param>
        /// <returns>Resulting sentence</returns>
        private Sentence ReturnNewResultingSentenceWithoutErrorCheck(Sentence sentence)
        {
            Sentence resultingSentence = new Sentence(); // sentence that will be returned at the end.
            foreach (Word word in sentence.Words) // loop all words in the sentence.
            {
                // start reading the word and return resulting sentence of it.
                switch (word.Type)
                {
                    // if it is object or rule type word, search the word in the grammar rules.
                    case WordType.Object:
                    case WordType.Rule:
                    {
                        if (word == errorWord || grammarSpace.DoNotCheckSameErrorForOtherObjects)
                        {
                            // read the word to find its resulting sentence
                            RuleOutputIndex output = new RuleOutputIndex() { outputSentence = "Null" };
                            if (grammarRuleData != null)
                            {
                                output = grammarRuleData.ReturnRuleOutput(word, errorIndexes);
                                word.childSentenceIndex = output.index;
                            }
                            if (output.outputSentence.Equals("Null")) continue; // if there is no rule for this word continue to next word.
                            resultingSentence.AddWords(Sentence.SplitSentenceIntoWords(output.outputSentence, word));
                            //
                        }
                        else
                        {
                            // read the word to find its resulting sentence
                            RuleOutputIndex output = new RuleOutputIndex() { outputSentence = "Null" };
                            if (grammarRuleData != null)
                            {
                                output = grammarRuleData.ReturnRuleOutput(word);
                                word.childSentenceIndex = output.index;
                            }
                            if (output.outputSentence.Equals("Null")) continue;  // if there is no rule for this word continue to next word.
                            resultingSentence.AddWords(Sentence.SplitSentenceIntoWords(output.outputSentence, word));
                            //
                        }
                        break;
                    }
                }
                //
            }
            return resultingSentence;
        }

        /// <summary>
        /// It checks the last object type word spawned is in the space created in GrammarSpace component.
        /// If it is not in the space, return an error and delete the object words spawned from the last sentence.
        /// To use this method properly GameObject that has Grammar Executor component has to have a Grammar Space component.  
        /// </summary>
        /// <param name="objTypeWord">Object type word to be checked</param>
        /// <returns>It returns true if there is an error</returns>
        private bool CheckSpaceError(ref Word objTypeWord)
        {
            // check error for the last spawned object before passing to the new object.
            if (grammarSpace == null) return false;
            if (!objTypeWord.objWord.TryGetComponent(out ObjTile objTile)) return false;
            if (!grammarSpace.CheckSpaceError(objTile)) return false;

            // an error occurred, print to the console.
            grammarSpace.PrintError(objTypeWord);
            // destroy all objWords spawned in the last iteration.
            foreach (Word word in allSpawnedObjWordsInLastIteration)
            {
                if (Application.isPlaying) // in play
                    Destroy(word.objWord.gameObject);
                else // in editor
                    DestroyImmediate(word.objWord.gameObject);
            }
            return true;
        }

        /// <summary>
        /// It creates space error data.
        /// </summary>
        /// <param name="objWord">Object word returning an error</param>
        private void CreateSpaceErrorData(Word objWord)
        {
            if (errorWord == null) // if this is the first error.
            {
                errorWord = objWord.Parent; // initialize errorWord.
                // add its child rule index that return error to a new error index list.
                errorIndexes = new List<int>();
                if (errorWord != null)
                    errorIndexes.Add(errorWord.childSentenceIndex);
            }
            else if (errorWord == objWord.Parent) // if this is the same errorWord as the last one.
            {
                // add its child rule index that return error to the error index list.
                errorIndexes.Add(objWord.Parent.childSentenceIndex);
            }
            else // if this is not the first error or neither the same errorWord as the last one.
            {
                errorWord = objWord.Parent; // initialize errorWord.
                // add its child rule index that return error to a new error index list.
                errorIndexes = new List<int>();
                errorIndexes.Add(objWord.Parent.childSentenceIndex);
            }
            spaceErrorFlag = true;
        }

        #endregion
    }
}
