using LiteDB;
using Noctus.Domain;
using System.IO;

namespace Noctus.Application.LocalPersistance
{
    public class Database
    {
        public static LiteDatabase Connection = new($"Filename={Path.Combine(ResourcesHelper.DefaultGenwaveFolderPath, "Genwave.db")};Password=667");
    }
}
