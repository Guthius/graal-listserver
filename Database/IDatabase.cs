namespace Listserver
{
    public interface IDatabase
    {
        /// <summary>
        /// Gets the list of servers as a single string.
        /// </summary>
        /// <returns></returns>
        string GetServers();

        /// <summary>
        /// Checks whether a account with the specified credentials exists.
        /// </summary>
        /// <param name="accountName">The name of the account.</param>
        /// <param name="password">The password.</param>
        /// <returns>True if the account exists; otherwise, false.</returns>
        bool AccountExists(string accountName, string password);
    }
}