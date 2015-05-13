using System;
using UnityEngine;
using System.IO;
using System.Net;
using System.Net.Sockets;
using support;
using System.Threading;
public class ServerSocket
{
    //ip
    private String host;
    //端口
    private int port;
    //套接字
    private Socket socket = null;
    //包头固定的长度
    private int headSize = 4;
    //包体的长度
    private int bodyLength = 0;
    //计时器
    private System.Timers.Timer timer = null;
    //重连间隔时间
    private double reconnetDelay = 15000;
    //socket连接成功
    public static String CONNECTED = "connected";
    //socket连接关闭
    public static String DISCONNECT = "disconnect";
    public ServerSocket()
    {
        
    }

    /// <summary>
    /// 链接服务器
    /// </summary>
    /// <param name="host">ip</param>
    /// <param name="port">端口</param>
    /// <returns></returns>
    public void connect(String host, int port)
    {
        if (this.socket != null && this.socket.Connected) return;
        this.host = host;
        this.port = port;
        if (this.socket == null)
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //采用TCP方式连接  
        IPAddress address = IPAddress.Parse(host);
        IPEndPoint endpoint = new IPEndPoint(address, port);
        //异步连接,连接成功调用connectCallback方法  
        this.socket.BeginConnect(endpoint, new AsyncCallback(connectCallback), this.socket);
    }

    /// <summary>
    /// 根据域名链接
    /// </summary>
    /// <param name="domain">域名</param>
    /// <param name="port">端口</param>
    /// <returns></returns>
    public void connectDomain(String domain, int port)
    {
        IPHostEntry ipHost = Dns.GetHostEntry(domain);
        this.connect(ipHost.AddressList[0].ToString(), port);
    }

    private void receiveSocketHandler(IAsyncResult ar)
    {
        StateObject so = (StateObject)ar.AsyncState;
        Socket socket = so.socket;
        if (!socket.Connected)
        {
            MonoBehaviour.print("socket断开");
            NotificationCenter.getInstance().postNotification(DISCONNECT);
            this.disconnect();
            this.startReconnetTimer();
            return;
        }
        if (socket.Poll(-1, SelectMode.SelectRead) == true)
        {
            int read = socket.EndReceive(ar);
            if (read == 0)
            {
                //socket 断开
                MonoBehaviour.print("socket断开");
                NotificationCenter.getInstance().postNotification(DISCONNECT);
                this.disconnect();
                this.startReconnetTimer();
                return;
            }
            //如果不足包头的长度
            if (read < this.headSize)
            {

            }
            //socket.Available 这里的缓冲区长度是去掉头部后的长度
            if (this.bodyLength == 0)
            {
                Buffer buffer = new Buffer();
                buffer.writeStream(new MemoryStream(so.buffer));
                this.bodyLength = buffer.readInt();
            }
            while (socket.Available > 0)
            {
                //||---头---||-----内容-----||;
                //读包头
                if (this.bodyLength == 0)
                {
                    byte[] headBuffer = new byte[this.headSize];
                    socket.Receive(headBuffer, 0, headBuffer.Length, SocketFlags.None);
                    Buffer headByteArray = new Buffer();
                    headByteArray.writeStream(new MemoryStream(headBuffer));
                    this.bodyLength = headByteArray.readInt();
                    MonoBehaviour.print("this.bodyLength " + this.bodyLength);
                }
                if (socket.Available < this.bodyLength)
                {
                    //缓冲区不足 讲读出的头还回去，并且跳出循环。
                    break;
                }
                if (this.bodyLength > 0 && this.socket.Available >= this.bodyLength)
                {
                    //读包体
                    byte[] bodyBuffer = new byte[this.bodyLength];
                    socket.Receive(bodyBuffer, 0, this.bodyLength, SocketFlags.None);
                    this.receiveCallback(new MemoryStream(bodyBuffer));
                    this.bodyLength = 0;
                }
                MonoBehaviour.print("socket.Available " + socket.Available);
            }
        }
        //重新建立监听
        this.socket.BeginReceive(so.buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(receiveSocketHandler), so);
    }

    /// <summary>
    /// 链接成功回调
    /// </summary>
    /// <param name="ar"></param>
    /// <returns></returns>
    private void connectCallback(IAsyncResult ar)
    {
        //结束挂起的异步连接请求。
        Socket socket = (Socket)ar.AsyncState;
        MonoBehaviour.print("socket.Connected " + socket.Connected);
        if (!socket.Connected)
        {
            //尝试重连
            this.startReconnetTimer();
        }
        else
        {
            NotificationCenter.getInstance().postNotification(CONNECTED);
            StateObject so = new StateObject();
            so.socket = this.socket;
            //与socket建立连接成功建立监听
            this.socket.BeginReceive(so.buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(receiveSocketHandler), so);
        }
        socket.EndConnect(ar);
    }

    /// <summary>
    /// 接收socket消息
    /// </summary>
    /// <returns></returns>
    /*private void receiveSocket()
    {
        
        if (!this.socket.Connected)
        {
            MonoBehaviour.print("Failed to clientSocket server.");
            this.socket.Close();
        }
        if (this.socket.Poll(-1, SelectMode.SelectRead) == true)
        {
            MonoBehaviour.print("receiveSocket socket.Available " + this.socket.Available);
            while (this.socket.Available >= this.headSize)
            {
                //||---头---||-----内容-----||;
                //读包头
                if (this.bodyLength == 0)
                {
                    byte[] headBuffer = new byte[this.headSize];
                    this.socket.Receive(headBuffer, 0, headBuffer.Length, SocketFlags.None);
                    Buffer headByteArray = new Buffer();
                    headByteArray.writeStream(new MemoryStream(headBuffer));
                    this.bodyLength = headByteArray.readInt();
                    MonoBehaviour.print("this.bodyLength " + this.bodyLength);
                }
                if (this.socket.Available < this.bodyLength)
                {
                    //缓冲区不足 讲读出的头还回去，并且跳出循环。
                    break;
                }
                if (this.bodyLength > 0 && this.socket.Available >= this.bodyLength)
                {
                    //读包体
                    byte[] bodyBuffer = new byte[this.bodyLength];
                    this.socket.Receive(bodyBuffer, 0, this.bodyLength, SocketFlags.None);
                    this.receiveCallback(new MemoryStream(bodyBuffer));
                    this.bodyLength = 0;
                }
                MonoBehaviour.print("socket.Available " + this.socket.Available);
            }
        }
    }*/

    /// <summary>
    /// 接收回调
    /// </summary>
    /// <param name="ms">数据流</param>
    /// <returns></returns>
    private void receiveCallback(MemoryStream ms)
    {
        //将ms中的包体的数据读出放入一个新的流中
        byte[] bodyByte = new byte[this.bodyLength];
        ms.Read(bodyByte, 0, this.bodyLength);
        MemoryStream bodyMs = new MemoryStream(bodyByte);
        bodyMs.Position = 0;
        //再存入buffer里面 为了保证下一条数据顺利的读出
        Buffer buffer = new Buffer();
        buffer.writeStream(bodyMs);
        int id = buffer.readInt(); //协议号
        MonoBehaviour.print("id = " + id);
        NotificationCenter.getInstance().postNotification(id.ToString(), buffer);
    }

    /// <summary>
    /// 发送数据
    /// </summary>
    /// <param name="buffer">字节数组</param>
    /// <returns></returns>
    public void send(Buffer buffer)
    {
        if (this.socket == null) return;
        if (!this.socket.Connected) return;
        if (buffer == null) return;
        if (buffer.empty()) return;
        //头长度
        Buffer headBuffer = new Buffer();
        headBuffer.wirteInt(buffer.length());
        //写入内容
        headBuffer.writeBytes(buffer.readStream().GetBuffer());
        this.socket.Send(headBuffer.readStream().GetBuffer(), SocketFlags.None);
    }

    /// <summary>
    /// 关闭socket
    /// </summary>
    /// <returns></returns>
    public void disconnect()
    {
        if (this.socket != null &&
            this.socket.Connected)
            this.socket.Disconnect(true);
    }

    /// <summary>
    /// 开启重连计时器
    /// </summary>
    /// <param name="delay">尝试重连的时间间隔</param>
    /// <returns></returns>
    private void startReconnetTimer()
    {
        if (this.timer == null)
        {
            this.timer = new System.Timers.Timer(this.reconnetDelay);
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(timeCompleteHandler);
        }
        this.timer.Start();
    }

    private void timeCompleteHandler(object sender, System.Timers.ElapsedEventArgs e)
    {
        this.timer.Stop();
        MonoBehaviour.print("重连");
        this.connect(this.host, this.port);
    }
}


public class StateObject
{
    public Socket socket = null;
    public const int BUFFER_SIZE = 4;
    public byte[] buffer = new byte[BUFFER_SIZE];
}
