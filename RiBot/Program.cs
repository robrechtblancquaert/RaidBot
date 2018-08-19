using System;
using System.Threading.Tasks;

namespace RiBot
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Writer.Log("started RiBot");

            Bot bot = new Bot();
            try
            {
                await bot.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

        }
    }
}
