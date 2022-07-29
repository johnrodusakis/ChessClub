using Unity.Networking.Transport;

public class NetTimer : NetMessage
{
    public int teamId;
    public float timer;
    public byte stop_start_Timer; // 0. stop || 1. start

    public NetTimer()
    {
        Code = OpCode.TIMER;
    }
    public NetTimer(DataStreamReader reader)
    {
        Code = OpCode.TIMER;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(teamId);
        writer.WriteFloat(timer);
        writer.WriteByte(stop_start_Timer);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        teamId = reader.ReadInt();
        timer = reader.ReadFloat();
        stop_start_Timer = reader.ReadByte();
    }

    public override void ReceiveOnClient()
    {
        NetUtility.C_TIMER?.Invoke(this);
    }
    public override void ReceiveOnServer(NetworkConnection cnn)
    {
        NetUtility.S_TIMER?.Invoke(this, cnn);
    }
}
