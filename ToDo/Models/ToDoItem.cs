using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Resources;
using System.Text.Json.Serialization;
using ToDoAPI.Models.Validation;

namespace ToDoAPI.Models
{
    /// <summary>
    /// Class describes single todo item.
    /// </summary>
    public class ToDoItem
    {
        private static ResourceManager ResourceManager = new ResourceManager(typeof(Resources.ValidationMessages));

        public int Id { get; set; }

        // required modifier is applied to avoid compiler warning
        // "Non-nullable property must contain a non-null value when exiting constructor"

        [Required(ErrorMessageResourceName = "TitleRequired", ErrorMessageResourceType = typeof(Resources.ValidationMessages))]
        [MaxLength(100, ErrorMessageResourceName = "TitleMaxLength", ErrorMessageResourceType = typeof(Resources.ValidationMessages))]
        public required string Title { get; set; }

        [Required(ErrorMessageResourceName = "DescriptionRequired", ErrorMessageResourceType = typeof(Resources.ValidationMessages))]
        [MaxLength(500, ErrorMessageResourceName = "DescriptionMaxLength", ErrorMessageResourceType = typeof(Resources.ValidationMessages))]
        public required string Description { get; set; }

        [JsonConverter(typeof(DateTimeFormatConverter))] // specify our custom json converter for proper data type deserialization        
        [Required(ErrorMessageResourceName = "ExpiryDateRequired", ErrorMessageResourceType = typeof(Resources.ValidationMessages))]
        [FutureDate(ErrorMessageResourceName = "ExpiryDateFuture", ErrorMessageResourceType = typeof(Resources.ValidationMessages))]
        public DateTime ExpiryDate { get; set; }

        [Range(0, 100, ErrorMessageResourceName = "PercentCompleteRange", ErrorMessageResourceType = typeof(Resources.ValidationMessages))]
        public double PercentComplete { get; set; }

        public bool IsDone { get; set; }
    }
}
