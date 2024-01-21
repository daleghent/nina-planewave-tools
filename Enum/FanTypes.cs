using DaleGhent.NINA.PlaneWaveTools.Utility;
using System.ComponentModel;

namespace DaleGhent.NINA.PlaneWaveTools.Enum {

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum FanTypes {

        [Description("M1 Fans")]
        M1Fans = 1,

        [Description("M1 Rear Fans")]
        M1RearFans,

        [Description("M1 Side Fans")]
        M1SideFans,

        [Description("M2 Fans")]
        M2Fans,

        [Description("M3 Fans")]
        M3Fans,

        [Description("M1 Heater Fans")]
        M1HeaterFans,

        [Description("M2 Heater Fans")]
        M2HeaterFans,

        [Description("M3 Heater Fans")]
        M3HeaterFans,

        [Description("Cabinet")]
        Cabinet,
    }
}