namespace Listserver;

public class ServerSettings
{
    /// <summary>
    /// Gets or sets the list server port (default = 21555).
    /// </summary>
    public int Port { get; set; } = 21555;

    /// <summary>
    ///     <para>
    ///         Gets or sets a Value indicating whether login is disabled.
    ///     </para>
    ///     <para>
    ///         When disabled account credentials are not verified.
    ///     </para>
    /// </summary>
    public bool DisableLogin { get; set; }

    /// <summary>
    ///     <para>
    ///         Gets or sets the message of the day.
    ///     </para>
    ///     <para>
    ///          This message is displayed beneath the server list.
    ///     </para>
    /// </summary>
    public string Motd { get; set; } = "Welcome %{AccountName} - List Server 2.1.5 Emulator By Seipheroth";

    /// <summary>
    /// Gets or sets a value indicating whether to show the 'Pay By Credit Card' button.
    /// </summary>
    public bool PayByCreditCard { get; set; }

    /// <summary>
    ///     <para>
    ///         Gets or sets the URL The 'Pay By Credit Card' button redirects the player to.
    ///     </para>
    ///     <para>
    ///         Do not include http:// or https:// in the URL, this is added automatically by the client.
    ///     </para>
    /// </summary>
    public string PayByCreditCardUrl { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets a value indicating whether to show the 'Pay By Phone' button.
    /// </summary>
    public bool PayByPhone { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show the 'Show More' button.
    /// </summary>
    public bool ShowMore { get; set; } = true;

    /// <summary>
    ///     <para>
    ///         Gets or sets the URL The 'Show More' button redirects the player to.
    ///     </para>
    ///     <para>
    ///         Do not include http:// or https:// in the URL, this is added automatically by the client.
    ///     </para>
    /// </summary>
    public string ShowMoreUrl { get; set; } = "github.com/Guthius/graal-listserver";
}