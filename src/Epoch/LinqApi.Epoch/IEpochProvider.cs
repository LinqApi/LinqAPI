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
}