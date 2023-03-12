namespace AyBorg.Plugins.ZXing.Models
{
    [Flags]
    public enum CodeFormats
    {
        // Barcodes:
        CODABAR = 1 << 0,
        CODE_39 = 1 << 2,
        CODE_93 = 1 << 3,
        CODE_128 = 1 << 4,
        EAN_8 = 1 << 5,
        EAN_13 = 1 << 6,
        ITF = 1 << 7,
        RSS_14 = 1 << 8,
        RSS_EXPANDED = 1 << 9,
        UPC_A = 1 << 10,
        UPC_E = 1 << 11,
        UPC_EAN_EXTENSION = 1 << 12,
        MSI = 1 << 13,
        PLESSEY = 1 << 14,
        IMB = 1 << 15,
        PHARMA_CODE = 1 << 16,
        // Matrix barcodes:
        AZTEC = 1 << 17,
        DATA_MATRIX = 1 << 18,
        MAXICODE = 1 << 19,
        PDF_417 = 1 << 20,
        QR_CODE = 1 << 21,
        // General
        All_Barcodes = CODABAR | CODE_39 | CODE_93 | CODE_128 | EAN_8 | EAN_13 | ITF | RSS_14 | RSS_EXPANDED | UPC_A | UPC_E | UPC_EAN_EXTENSION  | MSI | PLESSEY | IMB | PHARMA_CODE, 
        All_MatrixBarcodes = AZTEC | DATA_MATRIX | MAXICODE | PDF_417 | QR_CODE,
        All = All_Barcodes | All_MatrixBarcodes,
    }
}