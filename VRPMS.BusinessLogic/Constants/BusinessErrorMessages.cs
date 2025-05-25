namespace VRPMS.BusinessLogic.Constants;

internal class BusinessErrorMessages
{
    public const string FileIsEmpty = "File is empty.";
    public const string FileIsNotExcel = "File must be *.xls OR *.xlsx format.";
    public const string ExcelTableNotFound = "The specified excel sheet '{0}' is empty or does not exist.";
    public const string InvalidDataInExcel = "Invalid data in the Excel sheet '{0}'.";
}
