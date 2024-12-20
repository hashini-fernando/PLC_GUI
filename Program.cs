namespace PLC_GUI;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>988u;l;\;;;;;
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1()); 
    }    
}