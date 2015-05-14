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
        NotificationCenter.getInstance().addObserver("11", testHandler1);
        NotificationCenter.getInstance().addObserver("22", testHandler2);

        ss = new ServerSocket();
        ss.connect("127.0.0.1", 8000);

        btn.onClick.AddListener(btnClickHandler);
	}

    public void connect()
    {
        
    }

    private void btnClickHandler()
    {
        Buffer buffer = new Buffer();
        buffer.wirteInt(33);
        buffer.writeChar((char)6);
        buffer.writeLengthAndString("nmbsssss");
        ss.send(buffer);
    }


    private void testHandler2(object param)
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
        //print(str);
        //print(i);
    }

	// Update is called once per frame
	void Update () 
    {
	    
	}
}
