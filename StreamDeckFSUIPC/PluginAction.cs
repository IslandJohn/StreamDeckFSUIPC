using BarRaider.SdTools;
using FSUIPC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeckFSUIPC
{
    [PluginActionId("net.islandjohn.streamdeckfsuipc.virtualbutton")]
    public class PluginAction : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings();
                instance.VirtualJoystick = String.Empty;
                instance.ButtonNumber = String.Empty;
                return instance;
            }

            [JsonProperty(PropertyName = "virtualJoystick")]
            public string VirtualJoystick { get; set; }

            [JsonProperty(PropertyName = "buttonNumber")]
            public string ButtonNumber { get; set; }
        }

        #region Private Members

        private PluginSettings settings;

        #endregion
        public PluginAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                this.settings = PluginSettings.CreateDefaultSettings();
                SaveSettings();
            }
            else
            {
                this.settings = payload.Settings.ToObject<PluginSettings>();
            }
        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Destructor called");
        }

        public override void KeyPressed(KeyPayload payload)
        {
            try
            {
#if DEBUG
                Logger.Instance.LogMessage(
                    TracingLevel.INFO,
                    "Press virtual joystick " + settings.VirtualJoystick + ", button number " + settings.ButtonNumber);
#endif
                int vj = Convert.ToInt32(settings.VirtualJoystick);
                int bn = Convert.ToInt32(settings.ButtonNumber);

                if (!FSUIPCConnection.IsOpen)
                {
#if DEBUG
                    Logger.Instance.LogMessage(TracingLevel.INFO, "Opening FSUIPC connection...");
#endif
                    FSUIPCConnection.Open();
#if DEBUG
                    Logger.Instance.LogMessage(TracingLevel.INFO, "Done.");
#endif
                }


                FSUIPCConnection.Process("Buttons");
                Buttons.Value.Set((vj - 64) * 32 + bn, true);
                FSUIPCConnection.Process("Buttons");
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, ex.ToString());
            }
        }

        public override void KeyReleased(KeyPayload payload)
        {
            try
            {
#if DEBUG
                Logger.Instance.LogMessage(
                    TracingLevel.INFO,
                    "Release virtual joystick " + settings.VirtualJoystick + ", button number " + settings.ButtonNumber);
#endif
                int vj = Convert.ToInt32(settings.VirtualJoystick);
                int bn = Convert.ToInt32(settings.ButtonNumber);

                if (!FSUIPCConnection.IsOpen)
                {
#if DEBUG
                    Logger.Instance.LogMessage(TracingLevel.INFO, "Opening FSUIPC connection...");
#endif
                    FSUIPCConnection.Open();
#if DEBUG
                    Logger.Instance.LogMessage(TracingLevel.INFO, "Done.");
#endif
                }

                FSUIPCConnection.Process("Buttons");
                Buttons.Value.Set((vj - 64) * 32 + bn, false);
                FSUIPCConnection.Process("Buttons");
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, ex.ToString());
            }
        }

        private readonly Offset<BitArray> Buttons = new Offset<BitArray>("Buttons", 0x3340, 36);

        public override void OnTick() { }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            SaveSettings();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #region Private Methods

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        #endregion
    }
}