using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SEMES_Pixel_Designer.Utils
{
    static public class Mediator
    {

        static private IDictionary<string, Action<object>> callback_dict = new Dictionary<string, Action<object>>();

        // Deque처럼 사용
        static private List<UndoableAction> UndoStack = new List<UndoableAction>(), RedoStack = new List<UndoableAction>();
        static private readonly int UNDO_LIMIT = 50;
        static public int FileChangeCount = 0;

        //등록 또는 덮어쓰기
        static public void Register(string token, Action<object> callback)
        {
            if (callback_dict.ContainsKey(token))
            {
                MessageBox.Show("Debug : " + token + " 이미 등록됨");
                return;
            }
            callback_dict.Add(token, callback);
        }

        //해제
        static public void Unregister(string token, Action<object> callback)
        {
            callback_dict.Remove(token);
        }

        //호출하기
        static public void NotifyColleagues(string token, object args)
        {
            // MessageBox.Show("Debug : " + token + " 함수 실행");
            if (callback_dict.ContainsKey(token)) callback_dict[token](args);
        }


        static public void ExecuteUndoableAction(UndoableAction action)
        {
            action.InitAction();
            for (int i = RedoStack.Count - 1; i >= 0; i--)
            {
                RedoStack[i].PopAction();
                RedoStack.RemoveAt(i);
            }
            UndoStack.Add(action);
            if (UndoStack.Count > UNDO_LIMIT)
            {
                UndoStack[0].PopAction();
                UndoStack.RemoveAt(0);
            }
            FileChangeCount++;
        }

        static public void Undo()
        {
            if (UndoStack.Count == 0) return;
            UndoStack[UndoStack.Count - 1].UndoAction();
            RedoStack.Add(UndoStack[UndoStack.Count - 1]);
            UndoStack.RemoveAt(UndoStack.Count - 1);
            FileChangeCount--;
        }

        static public void Redo()
        {
            if (RedoStack.Count == 0) return;
            RedoStack[RedoStack.Count - 1].RedoAction();
            UndoStack.Add(RedoStack[RedoStack.Count - 1]);
            RedoStack.RemoveAt(RedoStack.Count - 1);
            FileChangeCount++;
        }

        public class UndoableAction
        {
            public Action InitAction { get; set; }
            public Action UndoAction { get; set; }
            public Action RedoAction { get; set; }
            public Action PopAction { get; set; }


            public UndoableAction(Action initAction, Action undoAction, Action redoAction, Action popAction)
            {
                InitAction = initAction;
                UndoAction = undoAction;
                RedoAction = redoAction;
                PopAction = popAction;
            }

            public UndoableAction(Action undoAction, Action redoAction, Action popAction) : this(redoAction, undoAction, redoAction, popAction)
            {
            }

        }

    }



}
