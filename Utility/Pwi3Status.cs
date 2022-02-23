#region "copyright"

/*
    Copyright Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace DaleGhent.NINA.PlaneWaveTools.Utility {

    public class Pwi3Status {

        public Pwi3Status() {
        }

        public Pwi3Status.System DeserializeStatus(string xml) {
            XmlSerializer serializer = new XmlSerializer(typeof(System));

            using (TextReader textReader = new StringReader(xml)) {
                var xmlReader = XmlReader.Create(textReader);
                return (Pwi3Status.System)serializer.Deserialize(xmlReader);
            }
        }

        [XmlRoot(ElementName = "focuser")]
        public class FocuserConfig {

            [XmlElement(ElementName = "max_position")]
            public int MaxPosition { get; set; }

            [XmlElement(ElementName = "max_rate")]
            public int MaxRate { get; set; }

            [XmlElement(ElementName = "unit")]
            public string Unit { get; set; }
        }

        [XmlRoot(ElementName = "focuser")]
        public class FocuserStatus {

            [XmlIgnore]
            public bool Connected { get; set; }

            [XmlElement(ElementName = "connected")]
            public string ConnectedBacking {
                get => Connected.ToString();
                set => Connected = bool.Parse(value.ToLower());
            }

            [XmlIgnore]
            public bool Connecting { get; set; }

            [XmlElement(ElementName = "connecting")]
            public string ConnectingBacking {
                get => Connecting.ToString();
                set => Connecting = bool.Parse(value.ToLower());
            }

            [XmlElement(ElementName = "position")]
            public int Position { get; set; }

            [XmlIgnore]
            public bool Moving { get; set; }

            [XmlElement(ElementName = "moving")]
            public string MovingBacking {
                get => Moving.ToString();
                set => Moving = bool.Parse(value.ToLower());
            }

            [XmlIgnore]
            public bool AutoFocusBusy { get; set; }

            [XmlElement(ElementName = "auto_focus_busy")]
            public string AutoFocusBusyBacking {
                get => AutoFocusBusy.ToString();
                set => AutoFocusBusy = bool.Parse(value.ToLower());
            }

            [XmlIgnore]
            public bool AutoFocusPermitted { get; set; }

            [XmlElement(ElementName = "auto_focus_permitted")]
            public string AutoFocusPermittedBacking {
                get => AutoFocusPermitted.ToString();
                set => AutoFocusPermitted = bool.Parse(value.ToLower());
            }

            [XmlIgnore]
            public bool AutoFocusLastResultSuccess { get; set; }

            [XmlElement(ElementName = "auto_focus_last_result_success")]
            public string AutoFocusLastResultSuccessBacking {
                get => AutoFocusLastResultSuccess.ToString();
                set => AutoFocusLastResultSuccess = bool.Parse(value.ToLower());
            }

            [XmlIgnore]
            public DateTime AutoFocusLastResultDatetime { get; set; }

            [XmlElement(ElementName = "auto_focus_last_result_datetime")]
            public string AutoFocusLastResultDatetimeBacking {
                get => AutoFocusLastResultDatetime.ToString("yyyy-MM-dd HH:mm:ss");
                set => AutoFocusLastResultDatetime = DateTime.Parse(value);
            }

            [XmlElement(ElementName = "auto_focus_last_result_diameter_px")]
            public int AutoFocusLastResultDiameterPx { get; set; }

            [XmlElement(ElementName = "auto_focus_last_result_tolerance_microns")]
            public int AutoFocusLastResultToleranceMicrons { get; set; }

            [XmlElement(ElementName = "auto_focus_last_result_position_microns")]
            public int AutoFocusLastResultPositionMicrons { get; set; }

            [XmlElement(ElementName = "auto_focus_last_result_status_text")]
            public string AutoFocusLastResultStatusText { get; set; }
        }

        [XmlRoot(ElementName = "rotator")]
        public class RotatorConfig {

            [XmlElement(ElementName = "max_position")]
            public int MaxPosition { get; set; }

            [XmlElement(ElementName = "max_rate")]
            public int MaxRate { get; set; }

            [XmlElement(ElementName = "unit")]
            public string Unit { get; set; }
        }

        [XmlRoot(ElementName = "rotator")]
        public class RotatorStatus {

            [XmlIgnore]
            public bool Connected { get; set; }

            [XmlElement(ElementName = "connected")]
            public string ConnectedBacking {
                get => Connected.ToString();
                set => Connected = bool.Parse(value.ToLower());
            }

            [XmlElement(ElementName = "position")]
            public double Position { get; set; }

            [XmlIgnore]
            public bool Moving { get; set; }

            [XmlElement(ElementName = "moving")]
            public string MovingBacking {
                get => Moving.ToString();
                set => Moving = bool.Parse(value.ToLower());
            }
        }

        [XmlRoot(ElementName = "mount")]
        public class MountStatus {

            [XmlIgnore]
            public bool Connected { get; set; }

            [XmlElement(ElementName = "connected")]
            public string ConnectedBacking {
                get => Connected.ToString();
                set => Connected = bool.Parse(value.ToLower());
            }

            [XmlIgnore]
            public bool Tracking { get; set; }

            [XmlElement(ElementName = "tracking")]
            public string TrackingBacking {
                get => Tracking.ToString();
                set => Tracking = bool.Parse(value.ToLower());
            }

            [XmlElement(ElementName = "ra")]
            public string Ra { get; set; }

            [XmlElement(ElementName = "dec")]
            public string Dec { get; set; }

            [XmlIgnore]
            public bool Moving { get; set; }

            [XmlElement(ElementName = "moving")]
            public string MovingBacking {
                get => Moving.ToString();
                set => Moving = bool.Parse(value.ToLower());
            }

            [XmlElement(ElementName = "meridian_side")]
            public string MeridianSide { get; set; }

            [XmlElement(ElementName = "ra_target")]
            public string RaTarget { get; set; }

            [XmlElement(ElementName = "dec_target")]
            public string DecTarget { get; set; }

            [XmlElement(ElementName = "ra_2000")]
            public string Ra2000 { get; set; }

            [XmlElement(ElementName = "dec_2000")]
            public string Dec2000 { get; set; }

            [XmlElement(ElementName = "ra_radian")]
            public string RaRadian { get; set; }

            [XmlElement(ElementName = "dec_radian")]
            public string DecRadian { get; set; }

            [XmlElement(ElementName = "alt_radian")]
            public string AltRadian { get; set; }

            [XmlElement(ElementName = "azm_radian")]
            public string AzmRadian { get; set; }

            [XmlElement(ElementName = "ra_target_radians")]
            public string RaTargetRadians { get; set; }

            [XmlElement(ElementName = "dec_target_radians")]
            public string DecTargetRadians { get; set; }

            [XmlElement(ElementName = "lst_radian")]
            public string LstRadians { get; set; }

            [XmlElement(ElementName = "tsign")]
            public short Tsign { get; set; }

            [XmlElement(ElementName = "type")]
            public string Type { get; set; }
        }

        [XmlRoot(ElementName = "temperature")]
        public class Temperature {

            [XmlElement(ElementName = "primary")]
            public double Primary { get; set; } = double.NaN;

            [XmlElement(ElementName = "ambient")]
            public double Ambient { get; set; } = double.NaN;

            [XmlElement(ElementName = "secondary")]
            public double Secondary { get; set; } = double.NaN;

            [XmlElement(ElementName = "primary_backplate")]
            public double PrimaryBackplate { get; set; } = double.NaN;

            [XmlElement(ElementName = "ambient_delta_t")]
            public double AmbientDeltaT { get; set; } = double.NaN;

            [XmlElement(ElementName = "ambient_efa")]
            public double AmbientEfa { get; set; } = double.NaN;

            [XmlElement(ElementName = "ascom")]
            public double Ascom { get; set; } = double.NaN;
        }

        [XmlRoot(ElementName = "config")]
        public class Config {

            [XmlElement(ElementName = "focuser")]
            public FocuserConfig Focuser { get; set; }

            [XmlElement(ElementName = "rotator")]
            public RotatorConfig Rotator { get; set; }
        }

        [XmlRoot(ElementName = "status")]
        public class Status {

            [XmlElement(ElementName = "focuser")]
            public FocuserStatus FocuserStatus { get; set; }

            [XmlElement(ElementName = "rotator")]
            public RotatorStatus RotatorStatus { get; set; }

            [XmlElement(ElementName = "mount")]
            public MountStatus MountStatus { get; set; }

            [XmlElement(ElementName = "temperature")]
            public Temperature Temperature { get; set; }

            [XmlElement(ElementName = "fans")]
            public string Fans { get; set; }
        }

        [XmlRoot(ElementName = "system")]
        public class System {

            [XmlIgnore]
            public DateTime Utc { get; set; }

            [XmlElement(ElementName = "utc")]
            public string UtcBacking {
                get => Utc.ToString("HH:mm:ss");
                set => Utc = DateTime.Parse(value);
            }

            [XmlIgnore]
            public DateTime Date { get; set; }

            [XmlElement(ElementName = "date")]
            public string DateBacking {
                get => Date.ToString("yyyy-MM-dd");
                set => Date = DateTime.Parse(value);
            }

            [XmlElement(ElementName = "application")]
            public string Application { get; set; }

            [XmlElement(ElementName = "version")]
            public Version Version { get; set; }

            [XmlIgnore]
            public DateTime DateConfig { get; set; }

            [XmlElement(ElementName = "date_config")]
            public string DateConfigBacking {
                get => DateConfig.ToString("yyyy-MM-dd");
                set => DateConfig = DateTime.Parse(value);
            }

            [XmlIgnore]
            public DateTime TimeConfig { get; set; }

            [XmlElement(ElementName = "time_config")]
            public string TimeConfigBacking {
                get => TimeConfig.ToString("HH:mm:ss");
                set => TimeConfig = DateTime.Parse(value);
            }

            [XmlElement(ElementName = "config")]
            public Config Config { get; set; }

            [XmlElement(ElementName = "status")]
            public Status Status { get; set; }
        }
    }
}