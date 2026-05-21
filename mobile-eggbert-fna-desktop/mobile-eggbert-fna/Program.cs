using WindowsPhoneSpeedyBlupi;

static class Program
{
    static void Main()
    {

        //Microsoft.Xna.Framework.FNADllMap.Init();
        // See https://aka.ms/new-console-template for more information
        //Console.WriteLine("Hello, World!");
        Env.init(EnvClasses.Impl.FNA, EnvClasses.Platform.Desktop);
        var game = new WindowsPhoneSpeedyBlupi.Game1();
        game.Run();
    }
}

