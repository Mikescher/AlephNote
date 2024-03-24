using AlephNote.Common.Settings;

namespace AlephNote.GitBackupService
{
    internal class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("[@] Starting up...");

            var svc = new Service();

            try
            {
                var ok = svc.Init(args);
                if (!ok) return 1;

                ok = svc.Run();
                if (!ok) return 1;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("[@] AlephNote.GitBackupService encountered a fatal error");
                Console.Error.WriteLine("");
                Console.Error.WriteLine("============================================================");
                Console.Error.WriteLine("");
                Console.Error.WriteLine(e);
                Console.Error.WriteLine("");
                Console.Error.WriteLine("============================================================");
                Console.Error.WriteLine("");
                Console.Error.WriteLine("");
                return 1;
            }

            Console.WriteLine("[@] Service finished");
            return 0;
        }
    }
}