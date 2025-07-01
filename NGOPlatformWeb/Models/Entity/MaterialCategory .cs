namespace NGOPlatformWeb.Models.Entity
{
    public class MaterialCategory
    {
        public int MaterialCategoryId { get; set; }

        public string? MaterialCategoryName { get; set; }

        public virtual ICollection<Supply> Supplies { get; set; }
    }
}
