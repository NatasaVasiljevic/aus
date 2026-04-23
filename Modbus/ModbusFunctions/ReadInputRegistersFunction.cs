using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read input registers functions/requests.
    /// </summary>
    public class ReadInputRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadInputRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadInputRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters p = (ModbusReadCommandParameters)CommandParameters;
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
            // Function code (4 = Read Input Registers)
            request[7] = p.FunctionCode;
            // Start address
            request[8] = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)p.StartAddress))[0];
            request[9] = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)p.StartAddress))[1];
            // Quantity
            request[10] = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)p.Quantity))[0];
            request[11] = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)p.Quantity))[1];

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusReadCommandParameters p = (ModbusReadCommandParameters)CommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            for (int i = 0; i < p.Quantity; i++)
            {
                byte high = response[9 + i * 2];
                byte low = response[10 + i * 2];
                ushort value = (ushort)((high << 8) | low);
                result.Add(new Tuple<PointType, ushort>(PointType.ANALOG_INPUT, (ushort)(p.StartAddress + i)), value);
            }

            return result;
        }
    }
}