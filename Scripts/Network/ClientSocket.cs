using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using UnityEngine;

public abstract class ClientSocket : MonoBehaviour
{
    protected virtual string TAG => "Socket";

    protected Encrypt en = new Encrypt();
    protected const int BUFFER_SIZE = 5524;
    protected Socket socket;

    protected SocketAsyncEventArgs receiveSocketEvent;
    protected byte[] sendBuffer = new byte[BUFFER_SIZE];
    protected byte[] receiveBuffer = new byte[BUFFER_SIZE];
    protected int currentReceiveBuffer = 0;

    protected string currentIp;
    protected int currentPort;

    Queue<HS_PK_HEADER> pkQueue = new Queue<HS_PK_HEADER>();

    void log(string str)
    {
        Logger.Log(TAG + "::" + str);// Logger.
    }

    void error(string str)
    {
        Logger.Error(TAG + "::" + str);// Logger.
    }

    public void Awake()
    {
        receiveSocketEvent = new SocketAsyncEventArgs();
        receiveSocketEvent.Completed += OnReceive;
    }

    protected abstract void OnLoginProcess(Action<LMSC_LoginRes3.Code> result);
    protected abstract bool ReceivePacketHandler(HS_PK_HEADER packet);
    protected abstract void OnDisconnectHandler(FPDefineString.Define reason);

    protected bool bDisconnectNotify = false;
    protected bool bForcedDisconnect = false;
    public void Close()
    {
        bForcedDisconnect = true;
        socket.Close();
    }

    public void ForceReconnect()
    {
        socket.Close();
    }

    protected bool bReconnecting = false;

    protected virtual void Reconnect()
    {
        UIManager.a.OpenLoading(true);
        ReconnectRoutine();
        Invoke(nameof(OpenConnectFail), 20f);
    }

    void ReconnectRoutine()
    {
        if (bReconnecting)
            return;

        bReconnecting = true;
        Connect(currentIp, currentPort, re =>
        {
            bReconnecting = false;

            if (re == LMSC_LoginRes3.Code.ER_NO)
            {
                UIManager.a.CloseLoading();
                CancelInvoke(nameof(OpenConnectFail));
            }
            else
            {
                if (UIManager.a.FindPopup<UIPopup_OneButton>() == null)
                {
                    Invoke(nameof(ReconnectRoutine), 4f);
                }
            }
        });
    }

    public void OpenConnectFail()
    {
        var popup = UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.SYSTEM, Const.MSG_CONNECT_FAIL);
        popup.callback += () => SceneManager.LoadLoginScene();
    }

    public void Connect(string ip, int port, Action<LMSC_LoginRes3.Code> result)
    {
        en = new Encrypt();
        sendBuffer = new byte[BUFFER_SIZE];
        receiveBuffer = new byte[BUFFER_SIZE];
        currentReceiveBuffer = 0;
        currentIp = ip;
        currentPort = port;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Logger.Where($"Try Connect to {ip}:{port}");
        bDisconnectNotify = false;
        socket.BeginConnect(new IPEndPoint(System.Net.IPAddress.Parse(ip), port),
            ar =>
            {
                if (ar.IsCompleted && socket != null && socket.Connected)
                {
                    log("Connection Success");
                    socket.EndConnect(ar);
                    Receive();
                    Send<MPSC_SecretNumRes>(new MPCS_SecretNumReq(), p =>
                     {
                         en.recv_SetState(p.dwSecretNum[0], p.dwSecretNum[1], p.dwSecretNum[2], p.dwSecretNum[7]);
                         en.send_SetState(p.dwSecretNum[3], p.dwSecretNum[4], p.dwSecretNum[5], p.dwSecretNum[6]);

                         MPCS_SecretCLRPT packet_secret = new MPCS_SecretCLRPT();
                         Send(packet_secret);
                         OnLoginProcess(result);
                     });

                }
                else
                {
                    log($"Connection Failed :: {socket}, {socket.Connected}, {ar.IsCompleted}");
                    if (socket.Connected) Disconnect();
                    RunInMainThread(() => result.Invoke(LMSC_LoginRes3.Code.ER_CONNECTION_ERROR));
                    //TODO: update로
                    //Util.RunSTA(() =>
                    //{
                    //    result.Invoke(LMSC_LoginRes3.Code.ER_CONNECTION_ERROR);
                    //});
                }
                //result.Invoke(ar.IsCompleted);
                //result.Invoke(LMSC_LoginRes3.Code.ER_NO);

            }, socket);

        //TODO
        //if (timer_connection != null)
        //{
        //    timer_connection.Dispose();
        //}
        //else
        //{
        //    timer_connection = new Timer();
        //}
        //timer_connection.Interval = 2000;
        //timer_connection.Elapsed += timer_connection_Tick;
        //timer_connection.Start();
    }

    Queue<Action> mainThreadActions = new Queue<Action>();

    protected void RunInMainThread(Action action)
    {
        mainThreadActions.Enqueue(action);
    }

    private Dictionary<Type, IMessage> handler = new Dictionary<Type, IMessage>();

    public interface IMessage
    {
        void Invoke(HS_PK_HEADER p);
    }


    public class ReceiveHandlerContent<T> : IMessage where T : HS_PK_HEADER
    {
        private Action<T> action;

        public void Invoke(HS_PK_HEADER p)
        {
            action?.Invoke(p as T);
        }

        public void SetAction(Action<T> a)
        {
            action = a;
        }
    }


    public class ReceiveHandlerContent<T1, T2, T3> : IMessage where T1 : HS_PK_HEADER where T2 : HS_PK_HEADER where T3 : HS_PK_HEADER
    {
        private Action<T1, T2, T3> action;

        T1 t1 = null;
        T2 t2 = null;
        T3 t3 = null;

        public void Invoke(HS_PK_HEADER p)
        {
            switch (p)
            {
                case T1 c:
                    t1 = c;
                    break;
                case T2 c:
                    t2 = c;
                    break;
                case T3 c:
                    t3 = c;
                    break;
            }
            if (t1 != null && t2 != null && t3 != null)
                action?.Invoke(t1, t2, t3);
        }

        public void SetAction(Action<T1, T2, T3> a)
        {
            action = a;
        }
    }

    public class ReceiveHandlerContent<T1, T2> : IMessage where T1 : HS_PK_HEADER where T2 : HS_PK_HEADER
    {
        private Action<T1, T2> action;

        T1 t1 = null;
        T2 t2 = null;

        public void Invoke(HS_PK_HEADER p)
        {
            switch (p)
            {
                case T1 c:
                    t1 = c;
                    break;
                case T2 c:
                    t2 = c;
                    break;
            }
            if (t1 != null && t2 != null)
                action?.Invoke(t1, t2);
        }

        public void SetAction(Action<T1, T2> a)
        {
            action = a;
        }
    }

    public void Send<T>(HS_PK_HEADER data, Action<T> receiveHandler = null) where T : HS_PK_HEADER
    {
        ArrayList list = new ArrayList();
        data.WritePacket(list);
        var b = (byte[])list.ToArray(typeof(byte));

        uint size = en.send_ScramblePacket(b, (uint)b.Length, sendBuffer);
        byte[] sendData = new byte[size];
        Array.Copy(sendBuffer, sendData, sendData.Length);
        var type = typeof(T);
        if (type != null)
        {
            var i = new ReceiveHandlerContent<T>();
            i.SetAction(receiveHandler);
            if (handler.ContainsKey(type))
            {
                handler[type] = i;
            }
            else handler.Add(type, i);
        }

        log("SEND :" + data.GetType());
        Send(sendData);

    }

    public void Send<T1, T2>(HS_PK_HEADER data, Action<T1, T2> receiveHandler = null) where T1 : HS_PK_HEADER where T2 : HS_PK_HEADER
    {
        ArrayList list = new ArrayList();
        data.WritePacket(list);
        var b = (byte[])list.ToArray(typeof(byte));

        uint size = en.send_ScramblePacket(b, (uint)b.Length, sendBuffer);
        byte[] sendData = new byte[size];
        Array.Copy(sendBuffer, sendData, sendData.Length);

        var i = new ReceiveHandlerContent<T1, T2>();
        i.SetAction(receiveHandler);
        var type = typeof(T1);
        if (type != null)
        {
            if (handler.ContainsKey(type))
            {
                handler[type] = i;
            }
            else handler.Add(type, i);
        }
        type = typeof(T2);
        if (type != null)
        {
            if (handler.ContainsKey(type))
            {
                handler[type] = i;
            }
            else handler.Add(type, i);
        }

        log("SEND :" + data.GetType());
        Send(sendData);

    }

    public void Send<T1, T2, T3>(HS_PK_HEADER data, Action<T1, T2, T3> receiveHandler = null) where T1 : HS_PK_HEADER where T2 : HS_PK_HEADER where T3 : HS_PK_HEADER
    {
        ArrayList list = new ArrayList();
        data.WritePacket(list);
        var b = (byte[])list.ToArray(typeof(byte));

        uint size = en.send_ScramblePacket(b, (uint)b.Length, sendBuffer);
        byte[] sendData = new byte[size];
        Array.Copy(sendBuffer, sendData, sendData.Length);

        var i = new ReceiveHandlerContent<T1, T2, T3>();
        i.SetAction(receiveHandler);
        var type = typeof(T1);
        if (type != null)
        {
            if (handler.ContainsKey(type))
            {
                handler[type] = i;
            }
            else handler.Add(type, i);
        }
        type = typeof(T2);
        if (type != null)
        {
            if (handler.ContainsKey(type))
            {
                handler[type] = i;
            }
            else handler.Add(type, i);
        }

        type = typeof(T3);
        if (type != null)
        {
            if (handler.ContainsKey(type))
            {
                handler[type] = i;
            }
            else handler.Add(type, i);
        }

        log("SEND :" + data.GetType());
        Send(sendData);

    }

    public void Send(HS_PK_HEADER data)
    {
        ArrayList list = new ArrayList();
        data.WritePacket(list);
        var b = (byte[])list.ToArray(typeof(byte));

        uint size = en.send_ScramblePacket(b, (uint)b.Length, sendBuffer);
        byte[] sendData = new byte[size];
        Array.Copy(sendBuffer, sendData, sendData.Length);
        log("SEND :" + data.GetType());
        Send(sendData);

    }

    private void Send(byte[] message)
    {
        socket.BeginSend(message, 0, message.Length, SocketFlags.None, ar =>
        {
            Socket s = (Socket)ar.AsyncState;
            s.EndSend(ar, out var errorCode);
            if (errorCode != SocketError.Success) error("Send Result Failed reason=" + errorCode);

        }, socket);
    }

    protected void Receive()
    {
        receiveSocketEvent.SetBuffer(receiveBuffer, 0, receiveBuffer.Length - currentReceiveBuffer);

        if (!socket.ReceiveAsync(receiveSocketEvent))
        {
            OnReceive(socket, receiveSocketEvent);
        }

    }

    protected void OnReceive(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
        {
            en.recv_AddReadSize(receiveBuffer, e.BytesTransferred);

            lock (pkQueue)
            {
                while (en.recv_EndReadWaiting(out byte[] data))
                {
                    var protocol = FPFactory.CreatePacket(data);
                    if (protocol != null)
                    {
                        if (protocol.GetType() != typeof(HSD_AlivePing))
                        {
                            log(@"RECEIVE : " + protocol + "");
                        }
                        pkQueue.Enqueue(protocol);
                    }
                    else
                    {
                        var hw = BitConverter.ToUInt16(data, 2);
                        var lw = BitConverter.ToUInt16(data, 4);

                        error("Unknown Protocol! Hw=0x" + hw.ToString("X") + " ,Lw=0x" + lw.ToString("X"));
                    }
                }
                currentReceiveBuffer = en.ReceiveSize;
            }

            Receive();

            //TODO: update
            //Application.Current.Dispatcher.InvokeAsync(() =>
            //{
            //    while (pkQueue.Count > 0)
            //    {
            //        PacketHandler(pkQueue.Dequeue());
            //    }
            //});

        }
        else
        {
            error("Receive Error=" + (e.BytesTransferred <= 0 ? "zero len" : e.SocketError.ToString()));
            Disconnect();
        }
    }


    private void Update()
    {
        lock (mainThreadActions)
        {
            while (mainThreadActions.Count > 0) mainThreadActions.Dequeue()?.Invoke();
        }

        lock (pkQueue)
        {
            while (pkQueue.Count > 0)
            {
                var protocol = pkQueue.Dequeue();
                PacketHandler(protocol);
            }
        }

    }

    public void Disconnect(FPDefineString.Define reason = FPDefineString.Define.IDS_NONE, bool bForced = true)
    {
        if (socket == null) return;
        if (socket.Connected)
        {
            Debug.LogError("Disconnect from " + currentIp + ":" + currentPort + " by reason=" + reason);

            socket.Shutdown(SocketShutdown.Both);

            socket.BeginDisconnect(true, ar =>
             {
                 //TODO: 재접속시, 에러남
                 socket.EndDisconnect(ar);
                 socket.Close();
                 socket = null;
             }, socket);

        }

        if (bForced && !bDisconnectNotify)
        {
            bDisconnectNotify = true;
            RunInMainThread(() => OnDisconnectHandler(reason));
        }
    }

    void PacketHandler(HS_PK_HEADER protocol)
    {
        var t = protocol.GetType();
        if (handler.ContainsKey(t))
        {
            var target = handler[t];

            handler.Remove(t);
            target.Invoke(protocol);
        }
        else
        if (!ReceivePacketHandler(protocol))
        {
            switch (protocol)
            {
                case LMSC_TableOptionList table:
                    GameData.TableOptions.Clear();
                    if (table.table_option != null)
                        foreach (var to in table.table_option)
                            GameData.TableOptions.Add(to.wTableOptionNo, to);

                    break;
                case LMSC_GSList gsList:

                    GameData.GameServers.Clear();
                    if (gsList.gslist != null)
                        foreach (var gs in gsList.gslist) GameData.GameServers.Add(gs.bGSNo, gs);

                    break;

                case MPBT_SysError er:

                    error($"Receive Error {(er.dwCode)}");
                    if (er.bType == 0) Disconnect(er.dwCode);

                    //else
                    //{
                    //    MessagePopup.Show(GameManager.a.GetErrorString(er.dwCode));
                    //}

                    break;
            }

        }

    }

}
