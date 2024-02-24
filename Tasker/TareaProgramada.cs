namespace Tasker
{
    public static class TareaProgramada
    {
        public static void Ejecutar()
        {
            var rnd = new Random();
            var result = rnd.Next(1, 3);

            if (result == 2)
            {
                throw new InvalidOperationException("Error al ejecutar la tarea");
            }

            Console.WriteLine($"Tarea programada a las {DateTime.Now} | Valor del random: {result} ");
        }
    }
}
