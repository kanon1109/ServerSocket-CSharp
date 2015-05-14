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
    //包最小长度
    private const int MIN_LENGTH = 4;
    //缓冲区最大长度
    private const int MAX_LENGTH = 8192;
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
    //缓冲区
    private Buffer buffer = new Buffer();
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
            //讲先读出的数据放入缓冲区
            this.buffer.writeBytes(so.buffer);
            //MonoBehaviour.print("socket.Available " + socket.Available);
            while (socket.Available > 0)
            {
                //写入的长度
                int size;
                if (socket.Available + this.buffer.length() > MAX_LENGTH)
                    size = MAX_LENGTH - this.buffer.length();
                else
                    size = socket.Available;
                //将socket缓冲区的数据放入自定义的缓冲区
                Byte[] bytes = new Byte[size];
                //MonoBehaviour.print("bytes.Length " + bytes.Length);
                //MonoBehaviour.print("prev this.buffer.length() " + this.buffer.length());
                socket.Receive(bytes, size, SocketFlags.None);
                this.buffer.writeBytes(bytes);
                //MonoBehaviour.print("next this.buffer.length() " + this.buffer.length());
                //从buffer中取消息数据
                this.getMessageFromBuffer();
            }
        }
        //重新建立监听
        this.socket.BeginReceive(so.buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(receiveSocketHandler), so);
    }

    /// <summary>
    /// 从buffer中取消息数据
    /// </summary>
    /// <returns></returns>
    private void getMessageFromBuffer()
    {
        //消息结构  {消息实体的长度(int)4位 消息实体body[协议号(int)4位 + 内容]}
        int length = this.buffer.length();
        //长度大于一条消息的最小值，取数据
        while (length >= MIN_LENGTH)
        {
            //归0位才能从头开始读取
            this.buffer.position = 0;
            //这里的read 并不会将缓冲区数据读走 只是改变buffer的postion
            //消息体长度
            this.bodyLength = this.buffer.readInt();
            //如果缓冲区剩余长度不足一条消息的长度则跳出，下次重新读去bodyLength(因为buffer的长度不会变，所以从头开始重新读);
            if (length < MIN_LENGTH + this.bodyLength)
            {
                break;
            }

            Byte[] body = this.buffer.readBytesByLength(this.bodyLength);

            Buffer bodyBuffer = new Buffer();
            bodyBuffer.writeBytes(body);
            bodyBuffer.position = 0;
            //协议号
            int pId = bodyBuffer.readInt();

            //MonoBehaviour.print("协议号: " + pId);

            NotificationCenter.getInstance().postNotification(pId.ToString(), bodyBuffer);

            //从缓冲区中删除已经读取的数据
            this.buffer.removeBytesByLength(MIN_LENGTH + bodyLength);
            length -= MIN_LENGTH + bodyLength;
        }
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
        this.socket.Send(headBuffer.readStream().GetBuffer(), headBuffer.length(), SocketFlags.None);
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
