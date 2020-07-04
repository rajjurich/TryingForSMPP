using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageCore
{
    public class PduHandler
    {
        private static ILog iLog = Log.GetInstance;
        public byte[] GetResponse(byte[] pdu)
        {
            byte[] NormalResponse = new byte[29];
            if (pduIsBindTrx(pdu))
            {
                iLog.Logger("- (PDU type was BindTRX)", Enumerations.LogType.ListenerAudit);

                NormalResponse[0] = Convert.ToByte(0x00);
                NormalResponse[1] = Convert.ToByte(0x00);
                NormalResponse[2] = Convert.ToByte(0x00);
                NormalResponse[3] = Convert.ToByte(0x15);

                NormalResponse[4] = Convert.ToByte(0x80);
                NormalResponse[5] = Convert.ToByte(0x00);
                NormalResponse[6] = Convert.ToByte(0x00);
                NormalResponse[7] = Convert.ToByte(0x09);

                NormalResponse[8] = Convert.ToByte(0x00);
                NormalResponse[9] = Convert.ToByte(0x00);
                NormalResponse[10] = Convert.ToByte(0x00);
                NormalResponse[11] = Convert.ToByte(0x00);

                NormalResponse[12] = Convert.ToByte(pdu[12]);
                NormalResponse[13] = Convert.ToByte(pdu[13]);
                NormalResponse[14] = Convert.ToByte(pdu[14]);
                NormalResponse[15] = Convert.ToByte(pdu[15]);

                NormalResponse[16] = Convert.ToByte(0x61);
                NormalResponse[17] = Convert.ToByte(0x62);
                NormalResponse[18] = Convert.ToByte(0x63);
                NormalResponse[19] = Convert.ToByte(0x64);

                NormalResponse[20] = Convert.ToByte(0x00);
            }
            else if (pduIsEnquireLink(pdu))
            {
                iLog.Logger("- (PDU type was Enquire_Link)", Enumerations.LogType.ListenerAudit);

                NormalResponse[0] = Convert.ToByte(0x00);
                NormalResponse[1] = Convert.ToByte(0x00);
                NormalResponse[2] = Convert.ToByte(0x00);
                NormalResponse[3] = Convert.ToByte(0x10);

                NormalResponse[4] = Convert.ToByte(0x80);
                NormalResponse[5] = Convert.ToByte(0x00);
                NormalResponse[6] = Convert.ToByte(0x00);
                NormalResponse[7] = Convert.ToByte(0x15);

                NormalResponse[8] = Convert.ToByte(0x00);
                NormalResponse[9] = Convert.ToByte(0x00);
                NormalResponse[10] = Convert.ToByte(0x00);
                NormalResponse[11] = Convert.ToByte(0x00);

                NormalResponse[12] = Convert.ToByte(pdu[12]);
                NormalResponse[13] = Convert.ToByte(pdu[13]);
                NormalResponse[14] = Convert.ToByte(pdu[14]);
                NormalResponse[15] = Convert.ToByte(pdu[15]);


                NormalResponse[16] = Convert.ToByte(0x00);
            }

            else if (pduIsUnbind(pdu))
            {
                iLog.Logger("- (PDU type was Unbind)", Enumerations.LogType.ListenerAudit);

                NormalResponse[0] = Convert.ToByte(0x00);
                NormalResponse[1] = Convert.ToByte(0x00);
                NormalResponse[2] = Convert.ToByte(0x00);
                NormalResponse[3] = Convert.ToByte(0x11);

                NormalResponse[4] = Convert.ToByte(0x80);
                NormalResponse[5] = Convert.ToByte(0x00);
                NormalResponse[6] = Convert.ToByte(0x00);
                NormalResponse[7] = Convert.ToByte(0x06);

                NormalResponse[8] = Convert.ToByte(pdu[8]);
                NormalResponse[9] = Convert.ToByte(pdu[9]);
                NormalResponse[10] = Convert.ToByte(pdu[10]);
                NormalResponse[11] = Convert.ToByte(pdu[11]);

                NormalResponse[12] = Convert.ToByte(pdu[12]);
                NormalResponse[13] = Convert.ToByte(pdu[13]);
                NormalResponse[14] = Convert.ToByte(pdu[14]);
                NormalResponse[15] = Convert.ToByte(pdu[15]);
            }


            return NormalResponse;
        }

        private bool pduIsUnbind(byte[] pdu)
        {
            return (pdu[7] == (byte)0x6);
        }

        private bool pduIsEnquireLink(byte[] pdu)
        {
            return (pdu[7] == (byte)0x15);
        }

        private bool pduIsBindTrx(byte[] pdu)
        {
            return (pdu[7] == (byte)9);
        }
    }
}
