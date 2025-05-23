using VRPMS.BusinessLogic.Helpers;
using VRPMS.BusinessLogic.Interfaces.Services;
using VRPMS.Common.Exceptions;

namespace VRPMS.BusinessLogic.Services;

internal class DataService : IDataService
{
    public async Task ImportData(Stream fileStream)
    {
        if (fileStream.Length == 0)
        {
            throw new BusinessException(BusinessErrorMessages.FileIsEmpty);
        }

        if (!IsExcel(fileStream))
        {
            throw new BusinessException(BusinessErrorMessages.FileIsNotExcel);
        }
    }

    private bool IsExcel(Stream stream)
    {
        if (stream == null)
            return false;

        if (!stream.CanSeek)
        {
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;
            stream = ms;
        }

        long originalPosition = stream.Position;

        byte[] header = new byte[8];
        int bytesRead = stream.Read(header, 0, header.Length);

        stream.Position = originalPosition;

        if (bytesRead < 4)
            return false;

        // Сигнатура старого .xls (BIFF Compound File)
        byte[] xlsHeader = { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };
        if (bytesRead >= 8 && header.Take(8).SequenceEqual(xlsHeader))
            return true;

        // Сигнатура ZIP-контейнера для .xlsx
        byte[] zipHeader = { 0x50, 0x4B, 0x03, 0x04 };
        if (header.Take(4).SequenceEqual(zipHeader))
            return true;

        return false;
    }
}
