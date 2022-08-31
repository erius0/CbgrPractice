using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PushShared.Push.Data;

namespace PushShared.Mobile.Data
{
    [Table("app_users")]
    public class MobileAppUser
    {
        [Key, Column("id"), JsonIgnore]
        public long Id { get; set; }
        [Required, GuidAttribue, Column("app_guid")]
        public string AppGuid { get; set; }
        [Required, Phone, Column("phone")]
        public string Phone { get; set; }
        [Required, Column("version")]
        public string Version { get; set; }
    }

    [Table("messages")]
    public class Message
    {
        [Key, Column("id"), JsonIgnore]
        public long Id { get; set; }
        [Required, Column("title")]
        public string Title { get; set; }
        [Required, Column("contents")]
        public string Contents { get; set; }
        [Required, Column("phone"), JsonIgnore]
        public string Phone { get; set; }
    }

    public class GuidAttribue : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            if (value is not string guid) return false;
            return Guid.TryParse(guid, out _);
        }
    }
}
