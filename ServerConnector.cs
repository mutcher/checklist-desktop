using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Windows;
using System.Net;
using System.Security;

namespace desktop
{
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

        /// <summary>
        /// Function converts NetworkPacket to byte array and
        /// send it to server
        /// </summary>
        /// <param name="packet">NetworkPacket which will be send</param>
        private void sendPacket(NetworkPacket packet)
        {
            List<byte> bytes = new List<byte>();
            // signature
            bytes.Add(0x05);
            bytes.Add(0x05);
            if (!String.IsNullOrEmpty(packet.data))
            {   // data section is not empty
                // in this case need to set data section flag
                bytes.Add(0xFF);
            }
            else
            {   // data section is empty
                // so data section flas should not be set
                bytes.Add(0x03);
            }
            // opcode
            bytes.Add((byte)packet.opcode);
            // subcode
            bytes.Add(packet.subcode);
            // data section
            if (!String.IsNullOrEmpty(packet.data))
            {
                byte[] data = Encoding.BigEndianUnicode.GetBytes(packet.data);
                bytes.Add(Convert.ToByte(data.Length));
                bytes.AddRange(data);
            }
            // sending bytes to server
            mServerSocket.GetStream().Write(bytes.ToArray(), 0, bytes.Count);
        }

        private bool receivePacket(NetworkPacket packet)
        {
            // need to clean up the packet
            packet.data = String.Empty;
            packet.subcode = 0x00;
            
            NetworkStream stream = mServerSocket.GetStream();
            byte[] buffer = new byte[NetworkPacket.minPacketSize];
            
            // reading data into buffer
            int read_count = stream.Read(buffer, 0, buffer.Length);
            if (read_count == NetworkPacket.minPacketSize)
            {   // check is data section exist
                bool read_data = buffer[2] == 0xFF;
                buffer[2] = 0x03;
                bool is_packet_valid = true;
                // need to check is signature is ok
                for (int i = 0; i < 3; i++)
                {
                    is_packet_valid = buffer[i] == ((i != 2) ? 0x05 : 0x03);
                    if (!is_packet_valid)
                    {
                        break;
                    }
                }

                if (is_packet_valid)
                {   // continue parsing only for valid package
                    // opcode
                    packet.opcode = (PacketOpcodes)buffer[3];
                    // subcode
                    packet.subcode = buffer[4];
                    if (read_data)
                    {   // data section
                        int data_length = stream.ReadByte();
                        buffer = new byte[data_length];
                        stream.Read(buffer, 0, buffer.Length);
                        // decoding data section
                        packet.data = Encoding.BigEndianUnicode.GetString(buffer);
                    }
                }
            }
            return true;
        }

        private void exchangePacket(NetworkPacket packet)
        {
            sendPacket(packet);
            receivePacket(packet);
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

        public void login(string login, String password)
        {
            if (mServerSocket.Connected)
            {
                NetworkPacket packet = new NetworkPacket();
                packet.opcode = PacketOpcodes.OP_LOGIN;
                packet.subcode = 0x00;
                packet.data = String.Format("{0}:{1}", login, password);
                exchangePacket(packet);
                if (packet.opcode != PacketOpcodes.OP_LOGIN ||
                    packet.subcode == 0x00)
                {
                    throw new IncorrectLoginPasswordException();
                }
            }
        }

        /// <summary>
        /// Function get all list record from server.
        /// In case if listID is 0(zero) function return list of the lists
        /// </summary>
        /// <param name="listID">List ID</param>
        /// <returns>Function returns all records of the list</returns>
        public List<string> getList(byte listID)
        {
            List<string> list = new List<string>();
            if (mServerSocket.Connected)
            {
                NetworkPacket packet = new NetworkPacket();
                packet.opcode = PacketOpcodes.OP_LIST_GET;
                packet.subcode = listID;
                sendPacket(packet);

                do
                {
                    if (!receivePacket(packet))
                    {
                        break;
                    }
                 
                    list.Add(packet.data);
                }
                while (packet.subcode > 1);
                // Server send list items in reverse order
                // so list should be reversed
                list.Reverse();
            }
            return list;
        }

        /// <summary>
        /// Function creates new list and save it on server
        /// </summary>
        /// <param name="listName">Name of the list</param>
        /// <returns>true in case if list has been created, otherwise false</returns>
        public bool addList(string listName)
        {
            NetworkPacket packet = new NetworkPacket();
            packet.opcode = PacketOpcodes.OP_LIST_ADD;
            packet.subcode = 0x00;
            packet.data = listName;

            exchangePacket(packet);
            return packet.subcode != 0x00;
        }

        public bool addListItem(byte listID, string itemName)
        {
            NetworkPacket packet = new NetworkPacket();
            packet.opcode = PacketOpcodes.OP_LIST_ADD;
            packet.subcode = listID;
            packet.data = itemName;
            exchangePacket(packet);
            return packet.subcode == listID;
        }

        public bool removeList(string listName)
        {
            NetworkPacket packet = new NetworkPacket();
            packet.opcode = PacketOpcodes.OP_LIST_DELETE;
            packet.subcode = 0x00;
            packet.data = listName;
            exchangePacket(packet);

            return packet.subcode != 0x00;
        }

        public bool removeListItem(byte listID, string itemName)
        {
            NetworkPacket packet = new NetworkPacket();
            packet.opcode = PacketOpcodes.OP_LIST_DELETE;
            packet.subcode = listID;
            packet.data = itemName;
            exchangePacket(packet);

            return packet.subcode != 0x00;
        }
    }
}
