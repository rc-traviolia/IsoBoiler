using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.Globalization;

namespace IsoBoiler.Csv
{
    public static class CsvUtility
    {
        public static string MakePlatformNeutralPath(params string[] pathParts)
        {
            if (pathParts.Where(pp => pp.Contains('/') || pp.Contains('\\')).Any())
            {
                var fullySplitPaths = new List<string>();
                foreach (var part in pathParts)
                {
                    if (part.Contains('/') || part.Contains('\\'))
                    {
                        var splitParts = part.Split(new char[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries);
                        fullySplitPaths.AddRange(splitParts);
                    }
                    else
                    {
                        fullySplitPaths.Add(part);
                    }
                }

                pathParts = fullySplitPaths.ToArray();
            }

            var finalPath = string.Empty;
            foreach (var part in pathParts)
            {
                finalPath = Path.Combine(finalPath, part);
            }
            return finalPath;
        }
        public static List<TClass> Read<TClass>(string filePath, bool treatEmptyStringsAsNulls = true)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Could not find Resource file at {filePath}");

            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                IgnoreBlankLines = true,
                Delimiter = ",",
                BadDataFound = null,
                MissingFieldFound = null
            };

            using var streamReader = new StreamReader(filePath);
            using var csvReader = new CsvReader(streamReader, csvConfiguration);

            if (treatEmptyStringsAsNulls)
                csvReader.Context.TypeConverterCache.AddConverter<string>(new EmptyToNullStringConverter());

            // IMPORTANT: materialize before disposing csv/reader
            return csvReader.GetRecords<TClass>().ToList();
        }
        public sealed class EmptyToNullStringConverter : StringConverter
        {
            public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
                => string.IsNullOrWhiteSpace(text) ? null : base.ConvertFromString(text, row, memberMapData);
        }
    }
}
