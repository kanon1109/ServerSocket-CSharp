using System;
using System.IO;
using System.Text;
public class Buffer
{
    //数据流
    private MemoryStream ms;
     //位置
    public long position
    {
        get { return ms.Position; }
        set { ms.Position = value; }
    }
    public Buffer()
    {
        this.ms = new MemoryStream();
        this.ms.Position = 0;
    }

    /// <summary>
    /// 根据长度从头部开始删除数据
    /// </summary>
    /// <param name="len">长度</param>
    /// <returns></returns>
    public void removeBytesByLength(int len)
    {
        int length = (int)(this.ms.Length);
        //缓冲区长度小于等于删除的长度，释放全部缓冲区
        if (length <= len)
        {
            this.ms.Dispose();
            this.ms = new MemoryStream();
            return;
        }

        //删除后剩余的长度
        int count = length - len;

        //读出删除后剩余的数据
        Byte[] tempBytes = new Byte[length];
        this.ms.Read(tempBytes, len - 1, count);

        //保存新的缓冲区
        Byte[] bytes = new Byte[count];
        Array.Copy(tempBytes, len - 1, bytes, 0, count);

        this.ms.Dispose();
        this.ms = new MemoryStream();
        this.ms.Write(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// 读出流
    /// </summary>
    /// <param name="ms">数据流</param>
    /// <returns></returns>
    public void writeStream(MemoryStream ms)
    {
        if (this.ms != null) this.ms.Dispose();
        this.ms = ms;
    }

    /// <summary>
    /// 读取整型
    /// </summary>
    /// <returns></returns>
    public int readInt()
    {
        Byte[] bytes = new Byte[4];
        this.ms.Read(bytes, 0, 4);
        this.checkEndian(bytes);
        return BitConverter.ToInt32(bytes, 0);
    }

    /// <summary>
    ///  读取短整型
    /// </summary>
    /// <returns></returns>
    public short readShort()
    {
        Byte[] bytes = new Byte[2];
        this.ms.Read(bytes, 0, 2);
        this.checkEndian(bytes);
        return BitConverter.ToInt16(bytes, 0);
    }

    /// <summary>
    /// 读取单精度
    /// </summary>
    /// <returns></returns>
    public float readFloat()
    {
        Byte[] bytes = new Byte[4];
        this.ms.Read(bytes, 0, 4);
        this.checkEndian(bytes);
        return BitConverter.ToSingle(bytes, 0);
    }

    /// <summary>
    /// 读取双进度型
    /// </summary>
    /// <returns></returns>
    public double readDouble()
    {
        Byte[] bytes = new Byte[8];
        this.ms.Read(bytes, 0, 8);
        this.checkEndian(bytes);
        return BitConverter.ToDouble(bytes, 0);
    }

    /// <summary>
    /// 读取一个字节
    /// </summary>
    /// <returns></returns>
    public int readByte()
    {
        return this.ms.ReadByte();
    }

    /// <summary>
    /// 读取长整型
    /// </summary>
    /// <returns></returns>
    public long readLong()
    {
        Byte[] bytes = new Byte[8];
        this.ms.Read(bytes, 0, 8);
        this.checkEndian(bytes);
        return BitConverter.ToInt64(bytes, 0);
    }

    /// <summary>
    /// 读取字符串
    /// </summary>
    /// <param name=len>字符串长度</param>
    /// <returns></returns>
    public String readString(int len)
    {
        Byte[] bytes = new Byte[len];
        ms.Read(bytes, 0, len);
        String str = Encoding.UTF8.GetString(bytes);
        return str;
    }


    /// <summary>
    /// 根据长度读取一个bytes[]
    /// </summary>
    /// <param name="len">长度</param>
    /// <returns>一个bytes[]</returns>
    public Byte[] readBytesByLength(int len)
    {
        Byte[] bytes = new Byte[len];
        ms.Read(bytes, 0, len);
        return bytes;
    }

    /// <summary>
    ///  读取带长度的字符串
    /// </summary>
    /// <returns></returns>
    public String readLengthAndString()
    {
        return readString(readShort());
    }

    //将流取出
    public MemoryStream readStream()
    {
        return this.ms;
    }

    /// <summary>
    /// 写入一个int
    /// </summary>
    /// <param name="data">int数据</param>
    /// <returns></returns>
    public void wirteInt(int data)
    {
        Byte[] dataBytes = BitConverter.GetBytes(data);
        this.checkEndian(dataBytes);
        this.ms.Write(dataBytes, 0, dataBytes.Length);
    }
    
    /// <summary>
    /// 写入一个Char
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public void writeChar(char data)
    {
        Byte[] dataBytes = BitConverter.GetBytes(data);
        this.checkEndian(dataBytes);
        this.ms.Write(dataBytes, 0, dataBytes.Length);
    }

    /// <summary>
    /// 写长整型
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public void writeLong(long data)
    {
        Byte[] dataBytes = BitConverter.GetBytes(data);
        this.checkEndian(dataBytes);
        this.ms.Write(dataBytes, 0, dataBytes.Length);
    }

    /// <summary>
    /// 写短整型
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public void writeShort(short data)
    {
        Byte[] dataBytes = BitConverter.GetBytes(data);
        this.checkEndian(dataBytes);
        this.ms.Write(dataBytes, 0, dataBytes.Length);
    }

    /// <summary>
    /// 写浮点型
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public void writeFloat(float data)
    {
        Byte[] dataBytes = BitConverter.GetBytes(data);
        this.checkEndian(dataBytes);
        this.ms.Write(dataBytes, 0, dataBytes.Length);
    }

    /// <summary>
    /// 写双精度型
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public void writeDouble(double data)
    {
        Byte[] dataBytes = BitConverter.GetBytes(data);
        this.checkEndian(dataBytes);
        this.ms.Write(dataBytes, 0, dataBytes.Length);
    }

    /// <summary>
    /// 写bool
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public void writeBool(bool data)
    {
        Byte[] dataBytes = BitConverter.GetBytes(data);
        this.checkEndian(dataBytes);
        this.ms.Write(dataBytes, 0, dataBytes.Length);
    }

    /// <summary>
    /// 写字符串
    /// </summary>
    /// <param name="str">字符串</param>
    /// <returns></returns>
    public void writeStr(String str)
    {
        Byte[] bytes = Encoding.UTF8.GetBytes(str);
        this.ms.Write(bytes, 0, bytes.Length);
    }


    /// <summary>
    /// 写入一个带长度的字符串
    /// </summary>
    /// <param name="str">字符串</param>
    /// <returns></returns>
    public void writeLengthAndString(String str)
    {
        this.wirteInt(str.Length);
        Byte[] bytes = Encoding.UTF8.GetBytes(str);
        this.ms.Write(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// 将一个字节数组写入流中
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public void writeBytes(Byte[] bytes)
    {
        this.ms.Write(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// 写入一个buffer
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public void writeBuffer(Buffer buffer)
    {
        this.ms.Write(buffer.readStream().GetBuffer(), 0, buffer.length());
    }

    /// <summary>
    /// 写入一个byte
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public void writeByte(byte i)
    {
        this.ms.WriteByte(i);
    }

    //是否为空
    public bool empty()
    {
        if (this.ms == null || this.ms.Length == 0) 
            return true;
        return false;
    }

    /// <summary>
    /// 获取数据长度
    /// </summary>
    /// <returns></returns>
    public int length()
    {
        return (int)this.ms.Length;
    }

    /// <summary>
    /// 根据字节序反转数据
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    private void checkEndian(Byte[] bytes)
    {
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
    }
}
