using DaleGhent.NINA.PlaneWaveTools.Utility;
using System.ComponentModel;

namespace DaleGhent.NINA.PlaneWaveTools.Enum {

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum HeaterType {

        [Description("M1")]
        M1Heater = 1,

        [Description("M2")]
        M2Heater,

        [Description("M3")]
        M3Heater,

        [Description("Cable Tray")]
        CableTrayHeater,

        [Description("Azimuth Read Head")]
        AzmReadHeadHeater,

        [Description("Altitude Read Head")]
        AltReadHeadHeater,

        [Description("Rotator 1 Read Head")]
        Rot1ReadHeadHeater,

        [Description("Rotator 2 Read Head")]
        Rot2ReadHeadHeater,

        [Description("Shutter Actuator")]
        ShutterActuatorHeater,
    }
}