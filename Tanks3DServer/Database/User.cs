using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.Database
{
    internal class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }
        [Required]
        public string AuthCode { get; set; }
        [Required]
        public string WalletAddres { get; set; }
        [Required]
        public string Dumpprivkey { get; set; }
        [Required]
        public string InventoryJSon { get; set; }
        [Required]
        [JsonIgnore]
        public string ProccesedTransactionsJSon { get; set; }
        [Required]
        public decimal Balance { get; set; } = 0;

        [NotMapped]
        public string ParticipantID { get; set; }
    }
}
