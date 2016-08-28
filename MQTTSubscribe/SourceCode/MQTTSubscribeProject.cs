/* MQTTSubscribe is sample code to add the MQTT Subscribe activity (widget) within HPE UFT. Please see this article on how to use this:
 * http://community.hpe.com/t5/All-About-the-Apps/Achieving-functional-test-automation-of-IoT-apps-with-HPE-UFT/ba-p/6892852#.V8LiqPkrJD9
 * Author: Retish Perumpilavil
*/


using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HP.ST.Fwk.RunTimeFWK;
using HP.ST.Fwk.RunTimeFWK.Utilities;
using log4net;
using uPLibrary.Networking.M2Mqtt;

namespace MQTTSubscribeProject
{
    [Serializable]
     public partial class MQTTSubscribe : STActivityBase
    {
        /// <summary>
        /// The log4net Log
        /// </summary>
        private Boolean bFlag;
        private static readonly ILog Log = LogManager.GetLogger(typeof(MQTTSubscribe));

        /// <summary>
        /// Initializes a new instance of the Activity class.
        /// </summary>
        /// <param name="ctx"> activitie's Context </param>
        /// <param name="name"> The activity name. </param>
        public MQTTSubscribe(ISTRunTimeContext ctx, string name)
            : base(ctx, name)
        {
            bFlag = false;
        }

        /// <summary>
        /// Execue and set results
        /// </summary>
        /// <returns></returns>
        // callback function invoked when a subscribed message is received
        void client_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            LogInfo("Received = " + Encoding.UTF8.GetString(e.Message) + " on topic " + e.Topic);
            this._mqttmessage = Encoding.UTF8.GetString(e.Message);
            bFlag = true;

        }

        // main function       
        protected override STExecutionResult ExecuteStep()
       {
            try
            {
                byte bConn;
                int i = 0;
                //Broker URL is a required field and cannot be null
                if (String.IsNullOrEmpty(this._mqttbroker))
                {
                    LogInfo("MQTT broker value cannot be null or empty");
                    this.Report("MQTT Error", "MQTT broker value cannot be null or empty");
                    return new STExecutionResult(STExecutionStatus.ActivityFailure);
                }

                MqttClient client = new MqttClient(this._mqttbroker);
                client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
                string clientId = Guid.NewGuid().ToString();

                if (String.IsNullOrEmpty(this._mqttusername) && String.IsNullOrEmpty(this._mqttpassword))
                {
                    bConn = client.Connect(clientId);
                }
                else
                {
                    bConn = client.Connect(clientId, this._mqttusername, this._mqttpassword);
                }

                if (!client.IsConnected)
                {
                    LogInfo("Cannot connect to broker. Please check input parameters and if the broker is running");
                    this.Report("MQTT Connect Error", "Cannot connect to broker. Please check input parameters and if the broker is running");
                    return new STExecutionResult(STExecutionStatus.ActivityFailure);
                }
                             
                string[] strTopic = { this._mqtttopic };
                byte[] strByte = { uPLibrary.Networking.M2Mqtt.Messages.MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE };
                // subscribe to topic
                ushort msgId = client.Subscribe(strTopic,strByte);

                // loop to suspend main thread for upto 10 seconds 
                //while waiting for callback to complete                                
                i = 0;
                while (bFlag == false)
                {
                    Thread.Sleep(1000);
                    i++;
                    if (i > 10) break;
                }
                bFlag = false;
           
                client.Disconnect();

                return new STExecutionResult(STExecutionStatus.Success);
            }
            catch (Exception e)
            {
                return new STExecutionResult(STExecutionStatus.ActivityFailure);
            }
        }

       
    }
}
