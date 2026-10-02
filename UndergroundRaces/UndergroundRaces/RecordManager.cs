using System;
using System.Globalization;
using System.IO;

namespace UndergroundRaces
{
    public static class RecordManager
    {
        private static readonly string RutaArchivo = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "UndergroundRaces",
            "record.txt");

        public static TimeSpan RecordActual { get; private set; } = CargarRecord();

        public static bool Registrar(TimeSpan tiempo)
        {
            if (tiempo <= TimeSpan.Zero ||
                (RecordActual > TimeSpan.Zero && tiempo >= RecordActual))
                return false;

            RecordActual = tiempo;
            GuardarRecord();
            return true;
        }

        public static string TextoRecordActual()
        {
            return RecordActual == TimeSpan.Zero
                ? "Record actual: 0"
                : $"Record actual: {RecordActual.TotalSeconds:0.0} s";
        }

        private static TimeSpan CargarRecord()
        {
            try
            {
                if (long.TryParse(File.ReadAllText(RutaArchivo), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out long ticks) && ticks > 0)
                    return TimeSpan.FromTicks(ticks);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }

            return TimeSpan.Zero;
        }

        private static void GuardarRecord()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(RutaArchivo));
                File.WriteAllText(RutaArchivo, RecordActual.Ticks.ToString(CultureInfo.InvariantCulture));
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}