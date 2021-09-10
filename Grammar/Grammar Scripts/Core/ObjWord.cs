using Grammar.Helper;
using UnityEngine;

namespace Grammar.Core
{
    [AddComponentMenu("ObjWord")]
    public class ObjWord : MonoBehaviour
    {
        public Word word;

        #region OBJWORD EVENTS

        public virtual void OnSpawn()
        {

        }

        protected virtual void OnEndOfExecution(GrammarExecutor grammarExecutor)
        {
            //
            grammarExecutor.OnEndOfExecution -= OnEndOfExecution;
        }

        #endregion

        public void RegisterToOnEndOfExecution(GrammarExecutor grammarExecutor)
        {
            grammarExecutor.OnEndOfExecution += OnEndOfExecution;
        }

        #region METHOD WORDS

        /// <summary>
        /// It calls corresponding methods with methodWord.
        /// </summary>
        /// <param name="methodWord">Method type word that will call corresponding method.</param>
        /// <returns>If it finds the corresponding method, returns true.</returns>
        public virtual bool CallCorrespondingMethod(Word methodWord)
        {
            bool called = true;
            switch (methodWord.Name)
            {
                case "SetPos":
                    SetPos(methodWord);
                    break;
                case "SetRot":
                    SetRot(methodWord);
                    break;
                case "SetParent":
                    SetParent(methodWord);
                    break;
                case "Move":
                    Move(methodWord);
                    break;
                default:
                    called = false;
                    break;
            }
            return called;
        }

        protected void SetPos(Word methodWord)
        {
            Vector3 newPos = methodWord.Args.StringToVector3();
            transform.position = newPos;
        }

        protected void SetRot(Word methodWord)
        {
            Vector3 newRot = methodWord.Args.StringToVector3();
            transform.Rotate(newRot, Space.World);
        }

        protected void SetParent(Word methodWord)
        {
            if (word.Parent == null) return;
            if (word.Parent.objWord == null) return;
            string[] args = methodWord.Args.SplitArgs();

            if (args.Length < 2)
            {
                transform.SetParent(word.Parent.objWord.transform, args[0].StringToBool());
                return;
            }

            Word curWord = word;
            int i = 0;
            int deep = args[1].ToLower().Equals("root") || args[1].ToLower().Equals("r")
                ? int.MaxValue : args[1].StringToInt();
            while (i < deep)
            {
                if (curWord.Parent == null)
                    break;

                curWord = curWord.Parent;
                i++;
            }

            transform.SetParent(curWord.objWord.transform, args[0].StringToBool());
        }

        protected void Move(Word methodWord)
        {
            Vector3 newPos = methodWord.Args.StringToVector3();
            transform.localPosition += newPos;
        }

        #endregion
    }
}
