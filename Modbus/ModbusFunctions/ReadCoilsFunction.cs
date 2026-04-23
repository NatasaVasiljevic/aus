using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc/>
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
            // Length (6 bytes follow)
            request[4] = 0;
            request[5] = 6;
            // Unit ID
            request[6] = p.UnitId;
            // Function code (1 = Read Coils)
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

            if (response[7] == p.FunctionCode + 0x80)
            {
                HandeException(response[8]);
            }

            byte byteCount = response[8];

            for (int i = 0; i < p.Quantity; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;

                if (9 + byteIndex >= response.Length)
                    break;

                ushort value = (ushort)((response[9 + byteIndex] >> bitIndex) & 1);
                result.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, (ushort)(p.StartAddress + i)), value);
            }

            return result;
        }
    }
}