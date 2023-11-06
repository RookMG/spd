using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace WpfApp3
{
    class tcpip
    {
        private Socket m_ServerSocket = null;
        private AsyncCallback m_fnAcceptHandler = new AsyncCallback(handleClientConnectionRequest);

        public void startServer()
        {
            // TCP 통신을 위한 소켓을 생성합니다.
            m_ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            // 특정 포트에서 모든 주소로부터 들어오는 연결을 받기 위해 포트를 바인딩합니다.
            // 사용한 포트: 1234
            m_ServerSocket.Bind(new IPEndPoint(IPAddress.Any, 1234));

            // 연결 요청을 받기 시작합니다.
            m_ServerSocket.Listen(5);

            // BeginAccept 메서드를 이용해 들어오는 연결 요청을 비동기적으로 처리합니다.
            // 연결 요청을 처리하는 함수는 handleClientConnectionRequest 입니다.
            m_ServerSocket.BeginAccept(m_fnAcceptHandler, null);
        }

        private void handleClientConnectionRequest(IAsyncResult ar)
        {
        }
    }
}
