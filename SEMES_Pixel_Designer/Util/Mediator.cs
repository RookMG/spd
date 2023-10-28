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

        static IDictionary<string, Action<object>> callback_dict = new Dictionary<string, Action<object>>();

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
    }



}
