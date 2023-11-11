using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Globalization;
using SEMES_Pixel_Designer.ViewModel;

namespace SEMES_Pixel_Designer
{
    class TcpIp
    {
        public class AsyncObject
        {
            public Byte[] Buffer;
            public Socket WorkingSocket;
            public AsyncObject(Int32 bufferSize)
            {
                this.Buffer = new Byte[bufferSize];
            }
        }

        // 비동기 변수
        private Socket m_ConnectedClient = null;
        private Socket m_ServerSocket = null;
        private AsyncCallback m_fnReceiveHandler;
        private AsyncCallback m_fnSendHandler;
        private AsyncCallback m_fnAcceptHandler;

        // 데이터 저장할 queue
        Queue<byte[]> messageQueue = new Queue<byte[]>();
        private AsyncCallback m_data_queue_process;
        private CancellationTokenSource cancellationTokenSource;

        // system.ini 데이터 추출
        private static string iniFilePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.ini"));
        Dictionary<string, string> iniData = ReadIniFile(iniFilePath);

        public TcpIp()
        {
            Utils.Mediator.Register("TcpIp.TcpConnection", TcpConnection);
            TcpIpLogViewModel.Instance.LogMessageList.Add("통신 초기화...");
        }

        // system.ini 데이터 파싱
        static Dictionary<string, string> ReadIniFile(string filePath)
        {
            Dictionary<string, string> iniData = new Dictionary<string, string>();

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedLine) && !trimmedLine.StartsWith(";"))
                    {
                        string[] parts = trimmedLine.Split('=');
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string value = parts[1].Trim();
                            iniData[key] = value;
                        }
                    }
                }
            }
            catch
            {
                System.Windows.MessageBox.Show("system.ini 파일을 실행 파일 경로에 추가해주세요");
                TcpIpLogViewModel.Instance.LogMessageList.Add("system.ini 파일을 실행 파일 경로에 추가해주세요");
            }

            return iniData;
        }


        #region TCPIP 관련 함수들


        // TCP 연결 대기
        public void TcpConnection(object obj)
        {
            //TODO : 구현
            //저장하지 않은 내용 확인 필수!!

            // TCP 통신을 위한 소켓을 생성합니다.
            m_ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            
            // 특정 포트에서 모든 주소로부터 들어오는 연결을 받기 위해 포트를 바인딩합니다.
            if (iniData.Count != 0)
            {
                if(iniData["IP"] == null || iniData["port"] == null)
                {
                    AsyncCallback m_wait_ip_port_set = new AsyncCallback(wait_ip_port_set);
                    IAsyncResult result = m_wait_ip_port_set.BeginInvoke(null, null, null);
                }
                else
                {
                    m_ServerSocket.Bind(new IPEndPoint(IPAddress.Parse(iniData["IP"]), int.Parse(iniData["port"])));

                    // 연결 요청을 받기 시작합니다.
                    m_ServerSocket.Listen(5);

                    // 비동기 작업에 사용될 대리자를 초기화합니다.
                    m_fnReceiveHandler = new AsyncCallback(handleDataReceive);
                    m_fnSendHandler = new AsyncCallback(handleDataSend);
                    m_fnAcceptHandler = new AsyncCallback(handleClientConnectionRequest);
                    m_data_queue_process = new AsyncCallback(data_queue_process);

                    // queue 처리 비동기 작업 시작
                    IAsyncResult result = m_data_queue_process.BeginInvoke(null, null, null);

                    // BeginAccept 메서드를 이용해 들어오는 연결 요청을 비동기적으로 처리합니다.
                    // 연결 요청을 처리하는 함수는 handleClientConnectionRequest 입니다.
                    m_ServerSocket.BeginAccept(m_fnAcceptHandler, null);
                }                
            }
            else
            {
                AsyncCallback m_ini_file_check = new AsyncCallback(ini_file_check);
                IAsyncResult result = m_ini_file_check.BeginInvoke(null, null, null);
            }
        }

        // ip, port 셋팅 될 때 까지 wait
        private void wait_ip_port_set(IAsyncResult ar)
        {
            while (true)
            {
                iniData = ReadIniFile(iniFilePath);
                if (iniData["IP"] != null || iniData["port"] != null)
                {
                    break;
                }
            }
            TcpConnection(null);
        }

        // system.ini가 경로에 없을 때
        private void ini_file_check(IAsyncResult ar)
        {
            while (true)
            {
                if (File.Exists(iniFilePath))
                {
                    break;
                }
            }
            iniData = ReadIniFile(iniFilePath);
            TcpConnection(null);
        }

        private void data_queue_process(IAsyncResult ar)
        {
            // queue 데이터 모두 처리
            while (true)
            {
                if(messageQueue.Count > 0)
                {
                    if (iniData["default_path"] == null)
                    {
                        System.Windows.MessageBox.Show("system.ini에 default_path를 추가해주세요");
                        TcpIpLogViewModel.Instance.LogMessageList.Add("system.ini에 default_path를 추가해주세요");
                        return;
                    }

                    Byte[] tmp_msgByte;
                    lock (messageQueue)
                    {
                        tmp_msgByte = messageQueue.Dequeue();
                    }

                    // 데이터 파싱
                    string now_data = Encoding.Unicode.GetString(tmp_msgByte);
                    string[] parts = now_data.Split(';');

                    if (parts[0].Trim() == "GetCADFile")
                    {
                        // default 경로 관리
                        string default_Path = iniData["default_path"]; // System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "CadFile"));
                        string[] pathParts = Directory.GetDirectories(default_Path);
                        TcpIpLogViewModel.Instance.LogMessageList.Add($"default path: {default_Path}");

                        for (int i = 0; i < pathParts.Length; i++)
                        {
                            string[] pathtmp = pathParts[i].Split(Path.DirectorySeparatorChar);
                            pathParts[i] = pathtmp[pathtmp.Length - 1];
                        }

                        if (parts[1].StartsWith("Type="))
                        {
                            string getType = parts[1].Substring(5);
                            bool chk = false;
                            for (int i = 0; i < pathParts.Length; i++)
                            {
                                if (getType == pathParts[i])
                                {
                                    default_Path += ("\\" + getType);

                                    // 파일 목록 가져오기
                                    string[] files = Directory.GetFiles(default_Path);

                                    if (files.Length > 0)
                                    {
                                        // 최신 날짜 파싱
                                        var mostRecentFile = files
                                            .Select(filePath => new
                                            {
                                                FilePath = filePath,
                                                DatePart = Path.GetFileNameWithoutExtension(filePath)
                                            })
                                            .Where(fileInfo => fileInfo.DatePart != null) // null 값 제외
                                            .OrderByDescending(fileInfo =>
                                            {
                                                DateTime parsedDate;
                                                if (DateTime.TryParseExact(fileInfo.DatePart, "yyMMdd_HHmmss", null, DateTimeStyles.None, out parsedDate))
                                                {
                                                    return parsedDate;
                                                }
                                                return DateTime.MinValue; // 올바르지 않은 경우 MinValue를 반환
                                        })
                                            .First();

                                        chk = true;
                                        // System.Windows.MessageBox.Show("가장 최근 파일: " + mostRecentFile.FilePath);
                                        var msgACK = "GetCADFile;ACK;Path=" + mostRecentFile.FilePath;
                                        SendMessage(msgACK);
                                        TcpIpLogViewModel.Instance.LogMessageList.Add(msgACK);
                                    }
                                    break;
                                }
                            }
                            if (!chk)
                            {
                                var msgNAK = "GetCADFile;NAK;";
                                SendMessage(msgNAK);
                                TcpIpLogViewModel.Instance.LogMessageList.Add(msgNAK);
                            }
                        }
                    }
                }
            }
        }

        private void handleDataReceive(IAsyncResult ar)
        {
            // 클라이언트 연결 체크
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            
            if(m_ConnectedClient.Poll(1000, SelectMode.SelectRead) && m_ConnectedClient.Available == 0)
            {
                m_ConnectedClient.Close();
                m_ServerSocket.Close();
                TcpConnection(null);
                return;
            }

            // 넘겨진 추가 정보를 가져옵니다.
            // AsyncState 속성의 자료형은 Object 형식이기 때문에 형 변환이 필요합니다~!
            AsyncObject ao = (AsyncObject)ar.AsyncState;

            // 받은 바이트 수 저장할 변수 선언
            Int32 recvBytes;

            try
            {
                // 자료를 수신하고, 수신받은 바이트를 가져옵니다.
                recvBytes = ao.WorkingSocket.EndReceive(ar);
            }
            catch(Exception ex)
            {
                // 예외가 발생하면 함수 종료!
                TcpIpLogViewModel.Instance.LogMessageList.Add(ex.Message);
                return;
            }

            // 수신받은 자료의 크기가 1 이상일 때에만 자료 처리
            if (recvBytes > 0)
            {
                // 공백 문자들이 많이 발생할 수 있으므로, 받은 바이트 수 만큼 배열을 선언하고 복사한다.
                Byte[] msgByte = new Byte[recvBytes];
                Array.Copy(ao.Buffer, msgByte, recvBytes);

                // 데이터 queue에 추가
                lock (messageQueue)
                {
                    messageQueue.Enqueue(msgByte);
                }
            }

            try
            {
                // 비동기적으로 들어오는 자료를 수신하기 위해 BeginReceive 메서드 사용!
                ao.WorkingSocket.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnReceiveHandler, ao);
            }
            catch (Exception ex)
            {
                // 예외가 발생하면 예외 정보 출력 후 함수를 종료한다.
                System.Windows.MessageBox.Show($"자료 수신 대기 도중 오류 발생!\n메세지: {ex.Message}");
                TcpIpLogViewModel.Instance.LogMessageList.Add($"자료 수신 대기 도중 오류 발생!\n메세지: {ex.Message}");
                return;
            }
        }

        private void handleDataSend(IAsyncResult ar)
        {

            // 넘겨진 추가 정보를 가져옵니다.
            AsyncObject ao = (AsyncObject)ar.AsyncState;

            // 보낸 바이트 수를 저장할 변수 선언
            Int32 sentBytes;

            try
            {
                // 자료를 전송하고, 전송한 바이트를 가져옵니다.
                sentBytes = ao.WorkingSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                // 예외가 발생하면 예외 정보 출력 후 함수를 종료한다.
                System.Windows.MessageBox.Show($"자료 송신 도중 오류 발생!\n메세지: {ex.Message}");
                TcpIpLogViewModel.Instance.LogMessageList.Add($"자료 송신 도중 오류 발생!\n메세지: {ex.Message}");
                return;
            }

            if (sentBytes > 0)
            {
                // 여기도 마찬가지로 보낸 바이트 수 만큼 배열 선언 후 복사한다.
                Byte[] msgByte = new Byte[sentBytes];
                Array.Copy(ao.Buffer, msgByte, sentBytes);

                System.Windows.MessageBox.Show($"메세지 보냄: {Encoding.Unicode.GetString(msgByte)}");
                TcpIpLogViewModel.Instance.LogMessageList.Add($"메세지 보냄: {Encoding.Unicode.GetString(msgByte)}");
            }
        }

        private void handleClientConnectionRequest(IAsyncResult ar)
        {
            Socket sockClient;
            try
            {
                // 클라이언트의 연결 요청을 수락합니다.
                sockClient = m_ServerSocket.EndAccept(ar);
                System.Windows.MessageBox.Show("연결 성공 !");
                TcpIpLogViewModel.Instance.LogMessageList.Add($"연결 성공 !");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"연결 수락 도중 오류 발생! 메세지: {ex.Message}");
                TcpIpLogViewModel.Instance.LogMessageList.Add($"연결 수락 도중 오류 발생! 메세지: {ex.Message}");
                return;
            }

            // 4096 바이트의 크기를 갖는 바이트 배열을 가진 AsyncObject 클래스 생성
            AsyncObject ao = new AsyncObject(4096);

            // 작업 중인 소켓을 저장하기 위해 sockClient 할당
            ao.WorkingSocket = sockClient;

            // 클라이언트 소켓 저장
            m_ConnectedClient = sockClient;

            try
            {
                // 비동기적으로 들어오는 자료를 수신하기 위해 BeginReceive 메서드 사용!
                sockClient.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnReceiveHandler, ao);
            }
            catch (Exception ex)
            {
                // 예외가 발생하면 예외 정보 출력 후 함수를 종료한다.
                System.Windows.MessageBox.Show($"자료 수신 대기 도중 오류 발생!\n메세지: {ex.Message}");
                TcpIpLogViewModel.Instance.LogMessageList.Add($"자료 수신 대기 도중 오류 발생!\n메세지: {ex.Message}");
                return;
            }
        }
        public void SendMessage(String message)
        {
            // 추가 정보를 넘기기 위한 변수 선언
            // 크기를 설정하는게 의미가 없습니다.
            // 왜냐하면 바로 밑의 코드에서 문자열을 유니코드 형으로 변환한 바이트 배열을 반환하기 때문에
            // 최소한의 크기르 배열을 초기화합니다.
            AsyncObject ao = new AsyncObject(1);

            // 문자열을 바이트 배열으로 변환
            ao.Buffer = Encoding.Unicode.GetBytes(message);

            ao.WorkingSocket = m_ConnectedClient;

            // 전송 시작!
            try
            {
                m_ConnectedClient.BeginSend(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnSendHandler, ao);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"전송 중 오류 발생!\n메세지: {ex.Message}");
                TcpIpLogViewModel.Instance.LogMessageList.Add($"전송 중 오류 발생!\n메세지: {ex.Message}");
            }
        }
        #endregion
    }
}
