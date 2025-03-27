using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqApi.Epoch
{
    /// <summary>
    /// bu projede kalacak
    /// </summary>
    public interface IEpochProvider
    {
        /// <summary>
        /// Verilen oluşturulma tarihine göre epoch değerini hesaplar.
        /// </summary>
        /// <param name="createdAt">Kayıt oluşturulma tarihi.</param>
        /// <returns>Hesaplanan epoch değeri.</returns>
        long GetEpoch(DateTime createdAt);
    }


    /// <summary>
    /// epoch projesine gidecek =>  o da LinqApi.Epoch diye bir nuget olacak.
    /// </summary>
    public class LinqDefaultEpochProvider : IEpochProvider
    {
        // Temel epoch tarihi: 1 Ocak 2024 (UTC)
        private readonly DateTime _baseEpoch = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        // Her epoch birimi 3 dakika (180 saniye)
        private const int SecondsPerEpoch = 180;

        public long GetEpoch(DateTime createdAt)
        {
            // Oluşturulma tarihini UTC’ye çeviriyoruz.
            DateTime utcCreatedAt = createdAt.ToUniversalTime();
            double totalSeconds = (utcCreatedAt - _baseEpoch).TotalSeconds;
            return (long)(totalSeconds / SecondsPerEpoch);
        }
    }

    public static class LinqEpochServiceCollectionExtensions
    {
        public static IServiceCollection AddLinqEpochProvider(this IServiceCollection services)
        {
            services.AddSingleton<IEpochProvider, LinqDefaultEpochProvider>();
            return services;
        }
    }
}