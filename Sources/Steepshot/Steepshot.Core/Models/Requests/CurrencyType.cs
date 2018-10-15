using System.ComponentModel.DataAnnotations;

namespace Steepshot.Core.Models.Requests
{
    public enum CurrencyType
    {
        None,
        Steem,
        Sbd,
        Golos,
        Gbg,
        Vim,
        Eos,
        [Display(Name = "Steem Power")]
        SteemPower
    }
}