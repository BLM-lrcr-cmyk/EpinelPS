using EpinelPS.Utils;
using Newtonsoft.Json;
using Paseto;
using Paseto.Builder;

namespace EpinelPS.Database;

internal class JsonDb
{
    public static CoreInfo Instance { get; internal set; }
    private static readonly object SaveLock = new();
    private static string DatabasePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db.json");

    // Note: change this in sodium
    public static byte[] ServerPrivateKey = Convert.FromBase64String("FSUY8Ohd942n5LWAfxn6slK3YGwc8OqmyJoJup9nNos=");
    public static byte[] ServerPublicKey = Convert.FromBase64String("04hFDd1e/BOEF2h4b0MdkX2h6W5REeqyW+0r9+eSeh0=");

    static JsonDb()
    {
        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/db.json"))
        {
            Console.WriteLine("users: warning: configuration not found, writing default data");
            Instance = new CoreInfo();
            Save();
        }       
       
        var j = LoadDatabase();
        if (j != null)
        {
            Instance = j;

            if (Instance.DbVersion != 5)
            {
                Logging.Warn("!!!WARNING!!!");
                Logging.Warn("Database version is extremely out of date.");
                Logging.Warn("It is recommended to delete db.json to avoid issues.");
            }

            if (Instance.LauncherTokenKey.Length == 0)
            {
                Console.WriteLine("Launcher token key is null, generating new key");

                var pasetoKey = new PasetoBuilder().Use(ProtocolVersion.V4, Purpose.Local)
                             .GenerateSymmetricKey();
                Instance.LauncherTokenKey = pasetoKey.Key.ToArray();
            }
            if (Instance.EncryptionTokenKey.Length == 0)
            {
                Console.WriteLine("EncryptionTokenKey is null, generating new key");

                var pasetoKey = new PasetoBuilder().Use(ProtocolVersion.V4, Purpose.Local)
                             .GenerateSymmetricKey();
                Instance.EncryptionTokenKey = pasetoKey.Key.ToArray();
            }

            Logging.SetOutputLevel(Instance.LogLevel);

            ValidateDb();
            Save();
            Console.WriteLine("JsonDb: Loaded");
        }
        else
        {
            throw new Exception("Failed to read configuration json file");
        }

    }

    public static void Reload()
    {
        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/db.json"))
        {
            Console.WriteLine("users: warning: configuration not found, writing default data");
            Instance = new CoreInfo();
            Save();
        }

        var j = LoadDatabase();
        if (j != null)
        {
            Instance = j;
            ValidateDb();
            Save();
            Console.WriteLine("Database reload complete.");
        }
    }

    private static CoreInfo? LoadDatabase()
    {
        return TryLoadDatabase(DatabasePath) ?? TryLoadDatabase(DatabasePath + ".bak");
    }

    private static CoreInfo? TryLoadDatabase(string path)
    {
        if (!File.Exists(path))
            return null;

        var text = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(text))
            return null;

        if (text.Contains("Char_Premium_Ticket"))
        {
            text = text.Replace("Char_Premium_Ticket", "CharPremiumTicket");
            text = text.Replace("Char_Customize_Ticket", "CharCustomizeTicket");
            text = text.Replace("Char_Select_01_Ticket", "CharSelect01Ticket");
            text = text.Replace("Char_Select_02_Ticket", "CharSelect02Ticket");
        }

        try
        {
            var db = JsonConvert.DeserializeObject<CoreInfo>(text);
            if (path != DatabasePath && db != null)
                Console.WriteLine($"Recovered database from {Path.GetFileName(path)}");
            return db;
        }
        catch (Exception)
        {
            Console.WriteLine($"Failed to read {Path.GetFileName(path)}");
            return null;
        }
    }

    private static void ValidateDb()
    {
        // check if character level is valid
        foreach (var user in Instance.Users)
        {
            foreach (var c in user.Characters)
            {
                if (c.Level < Utils.GameLimits.MinCharacterLevel)
                {
                    c.Level = Utils.GameLimits.MinCharacterLevel;
                }
                if (c.Level > Utils.GameLimits.MaxCharacterLevel)
                {
                    Console.WriteLine($"Warning: Character level for character {c.Tid} cannot be above {Utils.GameLimits.MaxCharacterLevel}, setting to {Utils.GameLimits.MaxCharacterLevel}");
                    c.Level = Utils.GameLimits.MaxCharacterLevel;
                }
            }

            if (user.SynchroDeviceLevel < Utils.GameLimits.SynchroDeviceBaseLevel)
                user.SynchroDeviceLevel = Utils.GameLimits.SynchroDeviceBaseLevel;
            else if (user.SynchroDeviceLevel > Utils.GameLimits.MaxCharacterLevel)
                user.SynchroDeviceLevel = Utils.GameLimits.MaxCharacterLevel;
        }
    }

    public static User? GetUser(ulong id)
    {
        return Instance.Users.Where(x => x.ID == id).FirstOrDefault();
    }

    public static RankData GetRank()
    {
        return Instance.RankDatas;
    }

    public static void Save()
    {
        if (Instance == null)
            return;

        lock (SaveLock)
        {
            string json = JsonConvert.SerializeObject(Instance, Formatting.Indented);
            string tempPath = DatabasePath + ".tmp";
            string backupPath = DatabasePath + ".bak";

            // Write and flush a complete file before replacing db.json. This prevents
            // a crash or power loss during a save from leaving an empty database.
            using (FileStream stream = new(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new(stream, new System.Text.UTF8Encoding(false)))
            {
                writer.Write(json);
                writer.Flush();
                stream.Flush(true);
            }

            if (File.Exists(DatabasePath))
            {
                File.Replace(tempPath, DatabasePath, backupPath, ignoreMetadataErrors: true);
            }
            else
            {
                File.Move(tempPath, DatabasePath);
            }
        }
    }
    public static int CurrentJukeboxBgm(int position)
    {
        var activeJukeboxBgm = new List<int>();
        //important first position holds lobby bgm id and second commanders room bgm id
        foreach (var user in Instance.Users)
        {
            if (user.JukeboxBgm == null || user.JukeboxBgm.Count == 0)
            {
                // this if statemet only exists becaus some weird black magic copies default value over and over
                //in the file when its set in public List<int> JukeboxBgm = new List<int>(); 
                //delete when or if it gets fixed

                user.JukeboxBgm = [2, 5];
            }

            activeJukeboxBgm.AddRange(user.JukeboxBgm);
        }

        if (activeJukeboxBgm.Count == 0)
        {
            return 8995001;
        }

        position = (position == 2 && activeJukeboxBgm.Count > 1) ? 2 : 1;
        return activeJukeboxBgm[position - 1];
    }

    public static bool IsSickPulls(User selectedUser)
    {
        if (selectedUser != null)
        {
            return selectedUser.sickpulls;
        }
        else
        {
            throw new Exception($"User not found");
        }
    }
}
