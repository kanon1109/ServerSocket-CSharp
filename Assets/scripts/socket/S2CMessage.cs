//消息体
public class S2CMessage
{
    //协议号
    public string pId;
    //内容数据
    public Buffer buffer;
    public S2CMessage(string pId, Buffer buffer)
    {
        this.pId = pId;
        this.buffer = buffer;
    }
}