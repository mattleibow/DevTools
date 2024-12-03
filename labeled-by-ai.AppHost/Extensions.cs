
static class Extensions
{
    public static IResourceBuilder<TDestination> WithOptionalReference<TDestination>(
        this IResourceBuilder<TDestination> builder,
        IResourceBuilder<IResourceWithConnectionString>? source,
        string? connectionName = null,
        bool optional = false)
        where TDestination : IResourceWithEnvironment
    {
        if (source is null)
        {
            return builder;
        }

        return builder.WithReference(source, connectionName, optional);
    }
}
