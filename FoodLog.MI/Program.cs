using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FoodLog.Common;
using Microsoft.Data.DataView;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;

namespace FoodLog.MI
{
    class Program
    {
        static readonly string _trainDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "Entries.csv");
        static readonly string _modelPath = Path.Combine(Environment.CurrentDirectory, "Data", "Model.zip");        
        private static List<Entry> entries = new List<Entry>();
        private static IDataView dataView;
        static void Main(string[] args)
        {
            GetTrainingData().GetAwaiter().GetResult();

            MLContext mlContext = new MLContext(seed:0);
            
            var model = Train(mlContext, _trainDataPath);

            Evaluate(mlContext, model);

            TestSinglePrediction(mlContext);
        }


        private static async Task GetTrainingData()
        {
            var api = new ApiWrapper();

            var entryViewModels = await api.GetEntries();

            foreach (var entry in entryViewModels)
            {
                entries.Add(new Entry()
                {
                    Dairy = entry.Dairy,
                    Gluten = entry.Gluten,
                    Alcohol =  entry.Alcohol,
                    Caffeine = entry.Caffeine,
                    FattyFood = entry.FattyFood,
                    Spice = entry.Spice,
                    OnionsPulses = entry.OnionsPulses,
                    Exercise = entry.Exercise,
                    Rating = entry.Rating
                });
            }
                        
        }


        public static ITransformer Train(MLContext mlContext, string dataPath)
        {
            dataView = mlContext.Data.ReadFromEnumerable(entries);

            var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("Dairy"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("Gluten"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("Alcohol"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("Caffeine"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("FattyFood"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("Spice"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("OnionsPulses"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("Exercise"))
                .Append(mlContext.Transforms.Concatenate("Features", "Dairy", "Gluten", "Alcohol", "Caffeine",
                    "FattyFood", "Spice", "OnionsPulses", "Exercise"))
                //.Append(mlContext.Regression.Trainers.FastTree());
                .Append(mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent(DefaultColumnNames.Label, DefaultColumnNames.Features))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));



            var model = pipeline.Fit(dataView);          

            SaveModelAsFile(mlContext, model);

            return model;
        }

        private static void Evaluate(MLContext mlContext, ITransformer model)
        {
            //this should really point to a test dataset which is different from the train data            
            var predictions = model.Transform(dataView);

            var metrics = mlContext.MulticlassClassification.Evaluate(predictions);

            Console.WriteLine($"*************************************************************************************************************");
            Console.WriteLine($"*       Metrics for Multi-class Classification model - Test Data     ");
            Console.WriteLine($"*------------------------------------------------------------------------------------------------------------");
            Console.WriteLine($"*       MicroAccuracy:    {metrics.AccuracyMicro:0.###}");
            Console.WriteLine($"*       MacroAccuracy:    {metrics.AccuracyMacro:0.###}");
            Console.WriteLine($"*       LogLoss:          {metrics.LogLoss:#.###}");
            Console.WriteLine($"*       LogLossReduction: {metrics.LogLossReduction:#.###}");
            Console.WriteLine($"*************************************************************************************************************");
        }

        private static void TestSinglePrediction(MLContext mlContext)
        {
            ITransformer loadedModel;
            using (var stream = new FileStream(_modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = mlContext.Model.Load(stream);
            }

            var predictionFunction = loadedModel.CreatePredictionEngine<Entry, EntryRatingPrediction>(mlContext);

            //var entrySample = new Entry()
            //{
            //    Dairy = true,
            //    Gluten = true,
            //    FattyFood = false,
            //    Spice = false,
            //    OnionsPulses = false,
            //    Exercise = true,
            //    Alcohol = false
            //};
            //    ,
            //    Rating = 0 // To predict. Actual/Observed = 2
            //};

            var entrySample = new Entry()
            {
                Dairy = false,
                Gluten = false,
                FattyFood = false,
                Spice = false,
                OnionsPulses = false,
                Exercise = false,
                Alcohol = false                
            };

            var prediction = predictionFunction.Predict(entrySample);
            int ratingVal = 1;

            foreach (var predictionProbFloat in prediction.ProbFloats)
            {
                Console.WriteLine($"Predicted Score For {ratingVal}: {predictionProbFloat.ToString()}");
                ratingVal++;
            }

            Console.WriteLine($"**********************************************************************");
            Console.WriteLine($"Predicted rating: {prediction.Rating}, actual rating: 2");
            Console.WriteLine($"**********************************************************************");
        }

        private static void SaveModelAsFile(MLContext mlContext, ITransformer model)
        {
            using (var fileStream = new FileStream(_modelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(model, fileStream);

            Console.WriteLine("The model is saved to {0}", _modelPath);
        }

    }


}
