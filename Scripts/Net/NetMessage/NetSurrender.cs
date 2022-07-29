using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class NetSurrender : NetMessage
{
    public int teamId;
    public byte wantSurrender;

    public NetSurrender()
    {
        Code = OpCode.SURRENDER;
    }
    public NetSurrender(DataStreamReader reader)
    {
        Code = OpCode.SURRENDER;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(teamId);
        writer.WriteByte(wantSurrender);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        teamId = reader.ReadInt();
        wantSurrender = reader.ReadByte();
    }

    public override void ReceiveOnClient()
    {
        NetUtility.C_SURRENDER?.Invoke(this);
    }
    public override void ReceiveOnServer(NetworkConnection cnn)
    {
        NetUtility.S_SURRENDER?.Invoke(this, cnn);
    }
}
