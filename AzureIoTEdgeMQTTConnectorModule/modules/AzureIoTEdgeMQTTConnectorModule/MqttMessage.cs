namespace AzureIoTEdgeMQTTConnectorModule
{
    public class MqttMessage{

        public string sensor {get;set;}
        public string payload {get;set;}

        public MqttMessageType MqttMessageType{
            get{
                switch(this.sensor){
                    case "temperature": return MqttMessageType.Temperature;
                    case "humidity": return MqttMessageType.Humidity;
                    case "peoplecount": return MqttMessageType.PeopleCount;
                    default: return MqttMessageType.Unknwon;
                }
            }
        }
    }
}