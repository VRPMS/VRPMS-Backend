using System.ComponentModel;
using System.Data;
using VRPMS.BusinessLogic.Constants;
using VRPMS.Common.Exceptions;

namespace VRPMS.BusinessLogic.Validators.BusinessValidators;

internal class ExcelValidator
{
    public async Task ValidateExcelFile(Stream fileStream)
    {
        if (fileStream.Length == 0)
        {
            throw new BusinessException(BusinessErrorMessages.FileIsEmpty);
        }

        if (!await IsExcel(fileStream).ConfigureAwait(false))
        {
            throw new BusinessException(BusinessErrorMessages.FileIsNotExcel);
        }
    }

    public DataTable GetTableAndCheckExists(DataSet ds, string tableName, Predicate<DataTable>? predicate = null)
    {
        var table = ds.Tables[tableName];

        if (table == null || table.Rows.Count == 0 || table.Columns.Count == 0)
        {
            throw new BusinessException(string.Format(BusinessErrorMessages.ExcelTableNotFound, tableName));
        }

        if (predicate != null && !predicate(table))
        {
            throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, tableName));
        }

        return table;
    }

    public T GetValueAndCheckType<T>(object? value, string tableName, Predicate<T>? predicate = null)
    {
        if (value == null)
        {
            throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, tableName));
        }

        var text = value.ToString()!;
        T result;

        try
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));

            if (!converter.CanConvertFrom(typeof(string)))
            {
                throw new NotSupportedException();
            }

            result = (T)converter.ConvertFromInvariantString(text)!;
        }
        catch
        {
            throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, tableName));
        }

        if (predicate != null && !predicate(result))
        {
            throw new BusinessException(string.Format(BusinessErrorMessages.InvalidDataInExcel, tableName));
        }

        return result;
    }

    public async Task<bool> IsExcel(Stream stream)
    {
        if (stream == null) return false;

        if (!stream.CanSeek)
        {
            var ms = new MemoryStream();
            await stream.CopyToAsync(ms).ConfigureAwait(false);
            ms.Position = 0;
            stream = ms;
        }

        var originalPos = stream.Position;
        var header = new byte[8];
        var bytesRead = await stream.ReadAsync(header, 0, header.Length).ConfigureAwait(false);
        stream.Position = originalPos;

        if (bytesRead < 4) return false;

        var xlsHeader = new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };
        if (bytesRead >= 8 && header.AsSpan(0, 8).SequenceEqual(xlsHeader))
        {
            return true;
        }

        var zipHeader = new byte[] { 0x50, 0x4B, 0x03, 0x04 };
        return header.AsSpan(0, 4).SequenceEqual(zipHeader);
    }
}
