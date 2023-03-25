using SocialConnect.Shared;

namespace SocialConnect.Domain.Entities
{
    public class Group : IEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int UserCount => Users?.Count() ?? 0;
        // Relations
        public virtual IList<GroupUser> Users { get; set; } = null!;
        public virtual IList<News> News { get; set; } = null!;
    }
}
