using UnityEngine;
using support;
using UnityEngine.UI;
using System;

public class SocketTest : MonoBehaviour 
{
    ServerSocket ss;
    public Button btn;
	// Use this for initialization
	void Start () 
    {
        //NotificationCenter.getInstance().addObserver("11", testHandler1);
        //NotificationCenter.getInstance().addObserver("22", testHandler2);
        NotificationCenter.getInstance().addObserver("S2C", s2cHandler);
        ss = new ServerSocket();
        ss.connect("127.0.0.1", 8000);
        btn.onClick.AddListener(btnClickHandler);
	}

    public void connect()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ss.update();
    }

    private void s2cHandler(object param)
    {
        S2CMessage message = param as S2CMessage;
        print("pId: " + message.pId);
        Buffer buffer;
        switch(message.pId)
        {
            case "11":
                buffer = message.buffer;
                String str = buffer.readLengthAndString();
                int i = buffer.readInt();
                print("i:" + i);
                break;
            case "22":
                buffer = message.buffer;
                print(buffer.readLengthAndString());
                print(buffer.readInt());
                //可以访问主线程的实例
                btn.gameObject.SetActive(false);
                break;
        }
    }

    private void btnClickHandler()
    {
        Buffer buffer = new Buffer();
        buffer.wirteInt(33);
        buffer.writeChar((char)6);
        ss.send(buffer);
    }

    /*private void testHandler2(object param)
    {
        Buffer buffer = param as Buffer;
        print(buffer.readLengthAndString());
        print(buffer.readInt());


    }

    private void testHandler1(object param)
    {
        Buffer buffer = param as Buffer;
        String str = buffer.readLengthAndString();
        int i = buffer.readInt();
        print(str);
        print(i);
        //无法访问主线程的实例
        btn.gameObject.SetActive(false);
    }*/
}
