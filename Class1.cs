using Supabase;
using System;

public static class Class1
{
    public static Client SupabaseClient;

    static Class1()
    {
        try
        {
            var url = "https://sddxssylqgmxlrqxaghd.supabase.co/";
            var key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InNkZHhzc3lscWdteGxycXhhZ2hkIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTczNzQwMTg4NCwiZXhwIjoyMDUyOTc3ODg0fQ.4PXE6Z1PTqVLGERG4OJdV1dBEkakgKOi46qYI_vUTGc";

            Console.WriteLine("URL: " + url);
            Console.WriteLine("KEY: " + key);

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
            {
                throw new Exception("SUPABASE_URL или SUPABASE_KEY не установлены.");
            }

            SupabaseClient = new Client(url, key, new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = true
            });

            Console.WriteLine("Клиент Supabase создан. Инициализация...");
            SupabaseClient.InitializeAsync();
            Console.WriteLine("Инициализация Supabase завершена.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка инициализации Class1: {ex.Message}");
            throw;
        }
    }
}