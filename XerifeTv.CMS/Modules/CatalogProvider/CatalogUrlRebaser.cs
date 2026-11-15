namespace XerifeTv.CMS.Modules.CatalogProvider;

// Troca o prefixo (host + token) antes de "/stream/" pelo BaseUrl atual do provedor
public static class CatalogUrlRebaser
{
    private const string StreamSegment = "/stream/";

    public static string? GetHost(string? baseUrl)
        => Uri.TryCreate(baseUrl?.Trim(), UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? uri.Host
            : null;

    public static string? Rebase(string? url, string newBaseUrl)
    {
        var newHost = GetHost(newBaseUrl);
        if (newHost is null || string.IsNullOrWhiteSpace(url))
            return url;

        var trimmedUrl = url.Trim();
        if (!string.Equals(GetHost(trimmedUrl), newHost, StringComparison.OrdinalIgnoreCase))
            return url;

        var streamIndex = trimmedUrl.IndexOf(StreamSegment, StringComparison.OrdinalIgnoreCase);
        if (streamIndex < 0)
            return url;

        return $"{newBaseUrl.Trim().TrimEnd('/')}{trimmedUrl[streamIndex..]}";
    }
}
