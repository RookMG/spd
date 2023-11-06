/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 2014-09-28
 * Time: 오전 10:18
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace ChatClient
{
	class Program {
		public static void Main(string[] args) {
		enterHostName:
			String hostName;
			UInt16 hostPort;
			
			Console.Write("서버 주소 입력: ");
			
			// 키를 입력받는다.
			hostName = Console.ReadLine().Trim();
			
			// null 값이 입력됬다면 (아무것도 입력되지 않았다면)
			if ( String.IsNullOrEmpty(hostName) ) {
				Console.WriteLine("다시 입력하세요");
				goto enterHostName;
			}
			
		enterHostPort:
			Console.Write("서버 포트 입력: ");
			
			try {
				// 포트를 입력받고, UInt16 형으로 변환을 시도한다.
				hostPort = UInt16.Parse(Console.ReadLine().Trim());
			} catch {
				Console.WriteLine("다시 입력하세요");
				goto enterHostPort;
			}
			
			ChatClient cc = new ChatClient();
			Console.WriteLine("접속 중...");
			
			cc.ConnectToServer(hostName, hostPort);
			if ( !cc.Connected ) {
				Console.WriteLine("접속 중 오류 발생!");
				Console.Write("아무 키나 누르면 종료합니다.");
				Console.ReadKey(true);
				return;
			}
			
			// 무한반복
			while ( true ) {
				String msg;
				Console.Write("보낼 메세지 (종료키: X): ");
				msg = Console.ReadLine().Trim();
				
				// 입력받은 문자열이 null 인 경우, 다시 반복문의 처음으로 돌아간다.
				if ( String.IsNullOrEmpty(msg) )
					continue;
				
				// 입력받은 문자열이 X 인 경우, 프로그램을 종료한다.
				if ( msg.Equals("X") ) {
					cc.StopClient();
					return;
				}
				
				// 그 외의 경우엔 메세지를 보낸다.
				cc.SendMessage(msg);
			}
		}
	}
}