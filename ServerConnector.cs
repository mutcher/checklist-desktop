using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Windows;
using System.Net;

namespace desktop
{
    enum Opcodes
    {
        OP_NULL = 0x00,
        OP_LIST_DELETE = 0x08,
        OP_LOGIN = 0x10,
        OP_LIST_ADD = 0x20,
        OP_LIST_GET = 0x40,
        OP_LIST_SET = 0x80
    };

    class PacketBuilder
    {
        private byte[] m_sign = new byte[3] { 0x05, 0x05, 0x03 };
        private Opcodes m_opcode = Opcodes.OP_NULL;
        private byte m_subcode = 0x00;
        private byte[] m_data = null;

        public PacketBuilder()
        {
        }

        public void setOpcode(Opcodes opcode)
        {
            m_opcode = opcode;
        }

        public void setSubcode(byte subcode)
        {
            m_subcode = subcode;
        }

        public void setData(byte[] data)
        {
            m_data = data;
        }

        public void setStringAsData(string str)
        {
            m_data = Encoding.BigEndianUnicode.GetBytes(str);
        }

        public byte[] build()
        {
            int len = 5;
            if (m_data != null)
            {
                len += m_data.Length + 1;
            }
            byte[] buffer = new byte[len];
            Array.Copy(m_sign, buffer, m_sign.Length);
            int pos = m_sign.Length;
            buffer[pos++] = Convert.ToByte(m_opcode);
            buffer[pos++] = m_subcode;
            if (m_data != null)
            {
                buffer[2] = 0xFF;
                buffer[pos++] = Convert.ToByte(m_data.Length);
                Array.Copy(m_data, 0, buffer, pos++, m_data.Length);
            }
            return buffer;
        }
    }

    class PacketParser
    {
        public const int maxPacketSize = 256;
        public const int minPacketSize = 5;

        private byte[] m_bytes;

        private byte[] m_sign = new byte[3] { 0x05, 0x05, 0x03 };
        private Opcodes m_opcode;
        private byte m_subcode;
        private byte[] m_data;

        public PacketParser()
        {
        }

        public void setBytes(byte[] data, int offset, int length)
        {
            m_bytes = new byte[length];
            Array.Copy(data, offset, m_bytes, 0, length);

            m_opcode = 0x00;
            m_subcode = 0x00;
            m_data = null;
        }

        public bool parse()
        {
            bool ret = true;
            if (m_bytes.Length < PacketParser.minPacketSize)
            {
                ret = false;
            }
            if (ret)
            {
                for (int i = 0; i < m_sign.Length; i += 2)
                {
                    if (m_bytes[i] != m_sign[i])
                    {
                        ret = false;
                        break;
                    }
                }
            }
            if (ret == true)
            {
                var pos = m_sign.Length;
                m_opcode = (Opcodes)m_bytes[pos++];
                m_subcode = m_bytes[pos++];

                var data_len = m_bytes.Length - pos - 1;
                if (data_len > 0)
                {
                    m_data = new byte[data_len];
                    Array.Copy(m_bytes, pos + 1, m_data, 0, m_data.Length);
                }
            }

            return ret;
        }

        public Opcodes getOpcode()
        {
            return m_opcode;
        }

        public byte getSubcode()
        {
            return m_subcode;
        }

        public byte[] getData()
        {
            return m_data;
        }

        public string getDataAsString()
        {
            if (m_data == null)
            {
                return null;
            }
            return Encoding.BigEndianUnicode.GetString(m_data);
        }
    }

    class IncorrectLoginPasswordException : Exception
    {
        public IncorrectLoginPasswordException()
            : base("Incorrect login or password")
        {
        }
    };

    class ServerConnector
    {
        private TcpClient mServerSocket = null;
        private string mServerUrl = String.Empty;
        private int mPort = 0;

        public bool IsAvailable
        {
            get
            {
                return mServerSocket != null && mServerSocket.Connected;
            }
        }

        private PacketParser exchangePacket(PacketBuilder builder)
        {
            var packet = builder.build();
            mServerSocket.GetStream().Write(packet, 0, packet.Length);
            var length = readPacket(packet);
            PacketParser parser = new PacketParser();
            parser.setBytes(packet, 0, length);
            if (!parser.parse())
            {
                throw new Exception();
            }
            return parser;
        }

        public ServerConnector(string serverUrl, int port)
        {
            mServerUrl = serverUrl;
            mPort = port;
        }

        public void connect()
        {
            if (mServerSocket == null)
            {
                mServerSocket = new TcpClient();
            }
            if (!mServerSocket.Connected)
            {
                mServerSocket.Connect(mServerUrl, mPort);
            }
        }

        public void disconnect()
        {
            mServerSocket.Close();
            mServerSocket = null;
        }

        public int readPacket(byte[] array)
        {
            var stream = mServerSocket.GetStream();
            int total = 0;
            total += stream.Read(array, 0, PacketParser.minPacketSize);
            if (array[2] == 0xFF)
            {
                //read data section
                array[2] = 0x03;
                total++;
                array[PacketParser.minPacketSize] = (byte)stream.ReadByte();
                var length = array[PacketParser.minPacketSize];
                total += stream.Read(array, PacketParser.minPacketSize + 1, length);
            }

            return total;
        }

        public void login(string login, string password)
        {
            if (mServerSocket.Connected)
            {
                PacketBuilder builder = new PacketBuilder();
                builder.setOpcode(Opcodes.OP_LOGIN);
                builder.setSubcode(0x00);
                var dataString = String.Format("{0}:{1}", login, password);
                builder.setStringAsData(dataString);
                var parser = exchangePacket(builder);
                if (parser.getOpcode() != Opcodes.OP_LOGIN || parser.getSubcode() == 0x00)
                {
                    throw new IncorrectLoginPasswordException();
                }
            }
        }

        //In case if listID equals to zero function will return list of lists
        public List<string> getList(byte listID)
        {
            List<string> list = new List<string>();
            if (!mServerSocket.Connected)
            {
                return list;
            }

            PacketBuilder builder = new PacketBuilder();
            builder.setOpcode(Opcodes.OP_LIST_GET);
            builder.setSubcode(listID);
            var packet = builder.build();
            var stream = mServerSocket.GetStream();
            stream.Write(packet, 0, packet.Length);
            PacketParser parser = new PacketParser();
            packet = new byte[PacketParser.maxPacketSize];
            do
            {
                var length = readPacket(packet);
                parser.setBytes(packet, 0, length);
                if (!parser.parse())
                {
                    break;
                }
                string item = parser.getDataAsString();
                if (item != null)
                {
                    list.Add(item);
                }
            }
            while (parser.getSubcode() > 1);
            list.Reverse();

            return list;
        }

        public bool addList(string listName)
        {
            PacketBuilder builder = new PacketBuilder();
            builder.setOpcode(Opcodes.OP_LIST_ADD);
            builder.setSubcode(0x00);
            builder.setStringAsData(listName);
            var parser = exchangePacket(builder);
            bool ret = false;
            if (parser.getSubcode() != 0)
            {
                ret = true;
            }
            return ret;
        }

        public bool addListItem(byte listID, string itemName)
        {
            PacketBuilder builder = new PacketBuilder();
            builder.setOpcode(Opcodes.OP_LIST_ADD);
            builder.setSubcode(listID);
            builder.setStringAsData(itemName);
            var parser = exchangePacket(builder);
            bool ret = false;
            if (parser.getSubcode() == listID)
            {
                ret = true;
            }
            return ret;
        }

        public bool removeList(string listName)
        {
            PacketBuilder builder = new PacketBuilder();
            builder.setOpcode(Opcodes.OP_LIST_DELETE);
            builder.setSubcode(0x00);
            builder.setStringAsData(listName);
            var parser = exchangePacket(builder);
            bool ret = false;
            if (parser.getSubcode() != 0x00)
            {
                ret = true;
            }
            return ret;
        }

        public bool removeListItem(byte listID, string itemName)
        {
            PacketBuilder builder = new PacketBuilder();
            builder.setOpcode(Opcodes.OP_LIST_DELETE);
            builder.setSubcode(listID);
            builder.setStringAsData(itemName);
            var parser = exchangePacket(builder);
            bool ret = false;
            if (parser.getSubcode() != 0x00)
            {
                ret = true;
            }
            return ret;
        }
    }
}
