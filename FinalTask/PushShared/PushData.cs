using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PushShared.Push.Data
{
    public class PushNotification
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
        [Required, EnumerablePhones]
        public IEnumerable<string> SendToNumbers { get; set; }
        [JsonIgnore]
        public IEnumerable<string> SendToGuids { get; set; } = Enumerable.Empty<string>();
    }

    public class EnumerablePhonesAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            if (value is not IEnumerable<string> phones) return false;
            var phoneAttribute = new PhoneAttribute();
            foreach (var phone in phones)
            {
                if (!phoneAttribute.IsValid(phone)) return false;
            }
            return true;
        }
    }
}
