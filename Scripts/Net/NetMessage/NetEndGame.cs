using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class NetEndGame : NetMessage
{

    public NetEndGame()
    {
        Code = OpCode.END_GAME;
    }
    public NetEndGame(DataStreamReader reader)
    {
        Code = OpCode.END_GAME;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
    }
    public override void Deserialize(DataStreamReader reader)
    {

    }

    public override void ReceiveOnClient()
    {
        NetUtility.C_END_GAME?.Invoke(this);
    }
    public override void ReceiveOnServer(NetworkConnection cnn)
    {
        NetUtility.S_END_GAME?.Invoke(this, cnn);
    }
}
