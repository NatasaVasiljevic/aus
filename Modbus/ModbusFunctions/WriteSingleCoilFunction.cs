using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write coil functions/requests.
    /// </summary>
    public class WriteSingleCoilFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleCoilFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleCoilFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters p = (ModbusWriteCommandParameters)CommandParameters;
            byte[] request = new byte[12];

            // Transaction ID
            request[0] = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)p.TransactionId))[0];
            request[1] = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)p.TransactionId))[1];
            // Protocol ID (0)
            request[2] = 0;
            request[3] = 0;
            // Length
            request[4] = 0;
            request[5] = 6;
            // Unit ID
            request[6] = p.UnitId;
            // Function code (5 = Write Single Coil)
            request[7] = p.FunctionCode;
            // Output address
            request[8] = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)p.OutputAddress))[0];
            request[9] = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)p.OutputAddress))[1];
            // Value: 0xFF00 = ON, 0x0000 = OFF
            ushort coilValue = (p.Value == 1) ? (ushort)0xFF00 : (ushort)0x0000;
            request[10] = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)coilValue))[0];
            request[11] = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)coilValue))[1];

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusWriteCommandParameters p = (ModbusWriteCommandParameters)CommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response.Length < 12)
                return result;

            if (response[7] == p.FunctionCode + 0x80)
            {
                HandeException(response[8]);
            }

            ushort address = (ushort)IPAddress.NetworkToHostOrder((short)((response[8] << 8) | response[9]));
            ushort rawValue = (ushort)IPAddress.NetworkToHostOrder((short)((response[10] << 8) | response[11]));
            ushort value = (rawValue == 0xFF00) ? (ushort)1 : (ushort)0;

            result.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, address), value);

            return result;
        }
    }
}