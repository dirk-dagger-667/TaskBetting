using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Client.DbModels
{
    public class MyCollectionItem
    {
        [Key]
        public int MyCollectionItemId { get; set; }

        [ForeignKey(nameof(MyObjectWrapper))]
        public int? MyObjectWrapperId { get; set; }

        public virtual MyObjectWrapper MyObjectWrapper { get; set; }
    }
}
