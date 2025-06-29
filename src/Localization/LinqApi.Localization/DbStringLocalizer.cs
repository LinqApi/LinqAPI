using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace LinqApi.Localization
{

    public class DbStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DbStringLocalizerFactory(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            return CreateScopedLocalizer();
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            return CreateScopedLocalizer();
        }

        private IStringLocalizer CreateScopedLocalizer()
        {
            var scope = _scopeFactory.CreateScope();
            var localizer = scope.ServiceProvider.GetRequiredService<DbStringLocalizer>();

            // ❗ Uyarı: Bu scope dispose edilmiyor
            // Çözüm: ScopedLocalizer wrapper'ı kullan
            return new ScopedStringLocalizer(localizer, scope);
        }

        private class ScopedStringLocalizer : IStringLocalizer
        {
            private readonly IStringLocalizer _inner;
            private readonly IServiceScope _scope;

            public ScopedStringLocalizer(IStringLocalizer inner, IServiceScope scope)
            {
                _inner = inner;
                _scope = scope;
            }

            public LocalizedString this[string name] => _inner[name];

            public LocalizedString this[string name, params object[] arguments] => _inner[name, arguments];

            public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
                _inner.GetAllStrings(includeParentCultures);

            public void Dispose()
            {
                _scope.Dispose();
            }

            public IStringLocalizer WithCulture(CultureInfo culture)
            {
                return this;
            }
        }
    }

    public class DbStringLocalizer : IStringLocalizer
    {
        private readonly ILinqLocalizationDbContextAdapter _dbContext;

        public DbStringLocalizer(ILinqLocalizationDbContextAdapter dbContext)
        {
            _dbContext = dbContext;
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            // ⚠️ Hızlı ve basit çözüm — thread-safe değildir.
            CultureInfo.CurrentUICulture = culture;
            return this;
        }

        public LocalizedString this[string name]
        {
            get
            {
                var value = _dbContext.Localizations
                    .FirstOrDefault(l => l.Key == name && l.Culture == CultureInfo.CurrentUICulture.Name)?.Value;

                return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments] =>
            new LocalizedString(name, string.Format(this[name].Value, arguments), false);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return _dbContext.Localizations
                .Where(l => l.Culture == CultureInfo.CurrentUICulture.Name)
                .Select(l => new LocalizedString(l.Key, l.Value, false));
        }
    }
}
