using Sharp7;

public class PlcCommunicator
{
    private S7Client _plcClient;

    public PlcCommunicator()
    {
        _plcClient = new S7Client();
    }

    public bool Connect(string ip, int rack, int slot)
    {
        int result = _plcClient.ConnectTo(ip, rack, slot);
        return result == 0; 
    }

    public void Disconnect()
    {
        _plcClient.Disconnect();
    }
     public object ReadPLCData( DataMapping mapping)
        {
            byte[] buffer;

            switch (mapping.DataType)
            {
                case "bool":
                    buffer = new byte[1];
                    _plcClient.DBRead(mapping.DbNumber, mapping.ByteOffset, buffer.Length, buffer);
                    return Sharp7.S7.GetBitAt(buffer, 0, mapping.BitOffset);

                case "int":
                    buffer = new byte[2];
                    _plcClient.DBRead(mapping.DbNumber, mapping.ByteOffset, buffer.Length, buffer);
                    return Sharp7.S7.GetIntAt(buffer, 0);

                case "dint":
                    buffer = new byte[4];
                    _plcClient.DBRead(mapping.DbNumber, mapping.ByteOffset, buffer.Length, buffer);
                    return Sharp7.S7.GetDIntAt(buffer, 0);

                case "real":
                    buffer = new byte[4];
                    _plcClient.DBRead(mapping.DbNumber, mapping.ByteOffset, buffer.Length, buffer);
                    return Sharp7.S7.GetRealAt(buffer, 0);


                default:
                    return null;
            }
        }


        public void WritePLCData(DataMapping mapping, object value)
        {
            byte[] buffer;

            switch (mapping.DataType)
            {
                case "bool":
                    buffer = new byte[1];
                    Sharp7.S7.SetBitAt(ref buffer, 0, mapping.BitOffset, (bool)value);
                    break;

                case "int":
                    buffer = new byte[2];
                    Sharp7.S7.SetIntAt(buffer, 0, (short)value);
                    break;

                case "dint":
                    buffer = new byte[4];
                    Sharp7.S7.SetDIntAt(buffer, 0, (int)value);
                    break;

                case "real":
                    buffer = new byte[4];
                    Sharp7.S7.SetRealAt(buffer, 0, (float)value);
                    break;

                default:
                    throw new NotSupportedException($"Data type {mapping.DataType} not supported for writing.");
            }

            _plcClient.DBWrite(mapping.DbNumber, mapping.ByteOffset, buffer.Length, buffer);

            

        }

                public object ConvertValueToPLCData(string value, string dataType)
                {
                    return dataType switch
                    {
                        "bool" => bool.Parse(value),
                        "int" => short.Parse(value),
                        "dint" => int.Parse(value),
                        "real" => float.Parse(value),
                        _ => throw new NotSupportedException($"Data type {dataType} not supported for conversion."),
                    };
                }



}
