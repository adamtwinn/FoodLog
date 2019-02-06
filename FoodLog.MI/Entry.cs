using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML.Data;

namespace FoodLog.MI
{
    public class Entry
    {   
        [ColumnName("Label")]
        public int Rating;        

        public bool Dairy;
        
        public bool Gluten;              
                
        public bool Alcohol;
        
        public bool Caffeine;
        
        public bool FattyFood;
        
        public bool Spice;
        
        public bool OnionsPulses;
        
        public bool Exercise;
    }

    public class EntryRatingPrediction
    {
        [ColumnName("PredictedLabel")]
        public int Rating;
        [ColumnName("Score")]
        public float[] ProbFloats;
    }
}
