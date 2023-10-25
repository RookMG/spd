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


        static IDictionary<string, List<Action<object>>> pl_dict = new Dictionary<string, List<Action<object>>>();

        //등록 또는 덮어쓰기
        static public void Register(string token, Action<object> callback)
        {
            if (!pl_dict.ContainsKey(token))
            {
                var list = new List<Action<object>>();
                list.Add(callback);
                pl_dict.Add(token, list);
            }
            else
            {
                bool found = false;
                foreach (var item in pl_dict[token])
                    if (item.Method.ToString() == callback.Method.ToString())
                        found = true;
                if (!found)
                    pl_dict[token].Add(callback);
            }
        }

        //해제
        static public void Unregister(string token, Action<object> callback)
        {
            if (pl_dict.ContainsKey(token))
                pl_dict[token].Remove(callback);
        }

        //호출하기
        static public void NotifyColleagues(string token, object args)
        {
            MessageBox.Show("Debug : "+token+" 함수 실행");
            if (pl_dict.ContainsKey(token))
                foreach (var callback in pl_dict[token])
                    callback(args);
        }


    }


}
