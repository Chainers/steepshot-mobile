using System.ComponentModel;

namespace Steepshot.Core.Models.Enums
{
    public enum PowerAction
    {
        [Description("power up")]
        PowerUp,
        [Description("power down")]
        PowerDown,
        CancelPowerDown,
        None,
    }
}
