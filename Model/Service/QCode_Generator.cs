using QRCoder;

namespace WebTrackED_CHED_MIMAROPA.Model.Service
{
    public class QRCode_Generator
    {
        public byte[] GenerateCode(string data)
        {
            byte[] QRCode = null;
            if (!string.IsNullOrEmpty(data))
            {
                QRCodeGenerator qrcodeGen = new QRCodeGenerator();
                QRCodeData datas = qrcodeGen.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                BitmapByteQRCode bitmap = new BitmapByteQRCode(datas);
                QRCode = bitmap.GetGraphic(20);
            }
            return QRCode;
        }
    }
}
