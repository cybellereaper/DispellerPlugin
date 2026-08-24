using Dispeller.Models;
using Lumina.Excel.Sheets;

namespace Dispeller.Services;

public static class ModelDetectionService
{
    public static ModelSignature ExtractModelInfo(ulong raw) =>
        ModelSignature.FromRaw(raw);

    public static bool ShareModel(Item item1, Item item2) =>
        ExtractModelInfo(item1.ModelMain) == ExtractModelInfo(item2.ModelMain);
}
