using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write single register functions/requests.
    /// </summary>
    public class WriteSingleRegisterFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleRegisterFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleRegisterFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
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
            // Function code (6 = Write Single Register)
            request[7] = p.FunctionCode;
            // Output address
            request[8] = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)p.OutputAddress))[0];
            request[9] = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)p.OutputAddress))[1];
            // Value
            request[10] = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)p.Value))[0];
            request[11] = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)p.Value))[1];

            return request;
        }

        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusWriteCommandParameters p = (ModbusWriteCommandParameters)CommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            ushort address = (ushort)IPAddress.NetworkToHostOrder((short)((response[8] << 8) | response[9]));
            ushort value = (ushort)IPAddress.NetworkToHostOrder((short)((response[10] << 8) | response[11]));

            result.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, address), value);

            return result;
        }
    }
}