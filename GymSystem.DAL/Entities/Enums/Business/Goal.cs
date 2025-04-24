using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities.Enums.Business
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Goal
    {
        WeightLoss,
        MuscleGainAndGainWeight,
        MuscleGainAndLossWeight,
        BetterBodyShape
    }
}
