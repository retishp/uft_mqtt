/* MQTTPublish is sample code to add the MQTT Publish activity (widget) within HPE UFT. Please see this article on how to use this:
 * http://community.hpe.com/t5/All-About-the-Apps/Achieving-functional-test-automation-of-IoT-apps-with-HPE-UFT/ba-p/6892852#.V8LiqPkrJD9
 * Author: Retish Perumpilavil
*/

using System;
using System.Text;
using System.Threading;
using HP.ST.Fwk.RunTimeFWK;
using HP.ST.Fwk.RunTimeFWK.Utilities;
using log4net;
using uPLibrary.Networking.M2Mqtt;

namespace MQTTPublishProject
{
    [Serializable]
     public partial class MQTTPublish : STActivityBase
    {
        /// <summary>
        /// The log4net Log
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(MQTTPublish));

        /// <summary>
        /// Initializes a new instance of the Activity class.
        /// </summary>
        /// <param name="ctx"> activitie's Context </param>
        /// <param name="name"> The activity name. </param>
        public MQTTPublish(ISTRunTimeContext ctx, string name)
            : base(ctx, name)
        {
            this._mqttbroker = String.Empty;
            this._mqttmessage = String.Empty;
            this._mqtttopic = String.Empty;
            this._mqttusername = String.Empty;
            this._mqttpassword = String.Empty;
            
        }

        /// <summary>
        /// Execue and set results
        /// </summary>
        /// <returns></returns>
        /// // main function
       protected override STExecutionResult ExecuteStep()
       {
            try
            {
                byte bConn ;
                //Broker URL is a required field and cannot be null
                if (String.IsNullOrEmpty(this._mqttbroker))
                {
                    LogInfo("MQTT broker value cannot be null or empty");
                    this.Report("MQTT Error", "MQTT broker value cannot be null or empty");
                    return new STExecutionResult(STExecutionStatus.ActivityFailure); 
                }

                MqttClient client = new MqttClient(this._mqttbroker);
                string clientId = Guid.NewGuid().ToString();

                bConn = client.Connect(clientId); 

                if (String.IsNullOrEmpty(this._mqttusername) && String.IsNullOrEmpty(this._mqttpassword))
                {
                    bConn = client.Connect(clientId); 
                }
                else
                {
                    bConn = client.Connect(clientId, this._mqttusername, this._mqttpassword);
                }      
              
                LogInfo(this._mqtttopic);
                LogInfo(this._mqttmessage);
                if (!client.IsConnected)
                {
                    LogInfo("Cannot connect to broker. Please check input parameters and if the broker is running");
                    this.Report("MQTT Connect Error", "Cannot connect to broker. Please check input parameters and if the broker is running");
                    return new STExecutionResult(STExecutionStatus.ActivityFailure); 
                }
                //Publish message to topic. Message flag is set to true 
                // so that the broker stores the last published message
                ushort msgId = client.Publish(this._mqtttopic, 
                                              Encoding.UTF8.GetBytes(this._mqttmessage), 
                                             uPLibrary.Networking.M2Mqtt.Messages.MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, 
                                              true); 
                //Sleep for 2 seconds
                Thread.Sleep(2000);
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
