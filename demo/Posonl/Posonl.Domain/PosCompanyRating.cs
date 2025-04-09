namespace Posonl.Domain
{
    public class PosCompanyRating : BaseEntity
    {
        public long PosCompanyId { get; set; }
        public virtual PosCompany PosCompany { get; set; }

        public long RatingCategoryId { get; set; }
        public virtual RatingCategory? RatingCategory { get; set; }

        public decimal Score { get; set; }  // Örn: 1-5 arası veya 0-10 ölçeğinde puan
    }

}