namespace IsoBoiler.Logging
{
    public static class LoggingExtensions
    {
        public static async Task LogExceptionsWith(this Task task, ILogBoiler logger)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                logger.Log(ex);
                throw;
            }
        }

        public static async Task<T> LogExceptionsWith<T>(this Task<T> task, ILogBoiler logger)
        {
            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                logger.Log(ex);
                throw;
            }
        }
    }
}
