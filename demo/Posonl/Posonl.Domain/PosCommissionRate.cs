namespace Posonl.Domain
{
    public class PosCommissionRate : BaseEntity
    {
        public long PosCompanyId { get; set; }
        public virtual PosCompany? PosCompany { get; set; }
        public long CountryId { get; set; }
        public virtual Country? Country { get; set; }
        public decimal CommissionPercentage { get; set; } // Örn: %1.5, %2.3
    }

}