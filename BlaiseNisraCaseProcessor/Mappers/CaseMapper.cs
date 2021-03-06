using System.Collections.Generic;
using Blaise.Nuget.Api.Contracts.Enums;
using Blaise.Nuget.Api.Contracts.Extensions;
using BlaiseNisraCaseProcessor.Enums;
using BlaiseNisraCaseProcessor.Interfaces.Mappers;
using BlaiseNisraCaseProcessor.Models;
using Newtonsoft.Json;
using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Mappers
{
    public class CaseMapper : ICaseMapper
    {
        public string MapToSerializedJson(IDataRecord recordData, string surveyName, string serverPark, CaseStatusType caseStatusType)
        {
            var jsonData = new Dictionary<string, string>();

            if (recordData != null)
            {
                foreach (var qidField in recordData.GetField("QID").Fields)
                {
                    jsonData[qidField.LocalName] = qidField.DataValue.ValueAsText.ToLower();
                }
            }

            jsonData["instrument_name"] = surveyName;
            jsonData["server_park"] = serverPark;
            jsonData["status"] = caseStatusType.FullName();

            return JsonConvert.SerializeObject(jsonData);
        }

        public Dictionary<string, string> MapFieldDictionaryFromRecordFields(IDataRecord2 recordData)
        {
            var fieldDictionary = new Dictionary<string, string>();
            var dataFields = recordData.GetDataFields();

            foreach (var dataField in dataFields)
            {
                fieldDictionary[dataField.FullName] = dataField.DataValue.ValueAsText;
            }

            return fieldDictionary;
        }

        public NisraCaseActionModel MapToNisraCaseActionModel(string message)
        {
            try
            {
                return JsonConvert.DeserializeObject<NisraCaseActionModel>(message);
            }
            catch 
            {
                // This is horrible I know but we currently don't really care about the message as it is only a trigger
                // and we need to ensure a message incorrectly put on this topic does not trigger it
            }

            return new NisraCaseActionModel { Action = ActionType.NotSupported };
        }
    }
}
