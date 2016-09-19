using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace desktop
{
    enum PacketOpcodes : byte
    {
        OP_NULL = 0x00,
        OP_LIST_DELETE = 0x08,
        OP_LOGIN = 0x10,
        OP_LIST_ADD = 0x20,
        OP_LIST_GET = 0x40,
        OP_LIST_SET = 0x80
    };

    class NetworkPacket
    {
        /// <summary>
        /// three bytes for signature
        /// one byte for opcode
        /// on byte for subcode
        /// </summary>
        public static int minPacketSize = 5;


        public PacketOpcodes opcode = PacketOpcodes.OP_NULL;
        public byte subcode = 0x00;
        public string data = String.Empty;
    }
}
