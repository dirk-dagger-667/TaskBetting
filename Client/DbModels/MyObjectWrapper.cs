using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Client.DbModels
{
    public class MyObjectWrapper
    {
        public MyObjectWrapper()
        {
            this.Items = new HashSet<MyCollectionItem>();
        }

        [Key]
        public int ObjectWrapperId { get; set; }

        public byte[] ObjectAsByte { get; set; }

        public string Name { get; set; }

        public virtual ICollection<MyCollectionItem> Items { get; set; }
    }
}
