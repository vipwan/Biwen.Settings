namespace Biwen.Settings.Caching.Garnet
{
    /// <summary>
    /// GarnetClientOptions
    /// </summary>
    public class GarnetClientOptions
    {
        public string? Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 3278;

        public string? UserName { get; set; } = null;

        public string? Password { get; set; } = null;
    }
}