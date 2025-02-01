namespace Idler.Helpers
{
    using Microsoft.ML;
    using Microsoft.ML.Data;
    using System.Collections.Generic;
    using Idler.Models;

    public class NlpModelManager
    {
        private MLContext MLContext { get; }
        private ITransformer Model { get; set; }

        public NlpModelManager()
        {
            this.MLContext = new MLContext();
        }

        public void TrainModel(IEnumerable<TrainData> trainData)
        {
            IDataView trainingDataView = this.MLContext.Data.LoadFromEnumerable(trainData);

            var pipeline = this.MLContext.Transforms.Conversion.MapValueToKey(inputColumnName: "CategoryId", outputColumnName: "Label")
                .Append(this.MLContext.Transforms.Text.FeaturizeText(inputColumnName: "Description", outputColumnName: "Features"))
                .AppendCacheCheckpoint(this.MLContext)
                .Append(this.MLContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
                .Append(this.MLContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            this.Model = pipeline.Fit(trainingDataView);
        }

        public int PredictCategoryId(string description)
        {
            var predictionEngine = this.MLContext.Model.CreatePredictionEngine<TrainData, PredictionResult>(this.Model);
            var prediction = predictionEngine.Predict(new TrainData { Description = description });
            return prediction.PredictedCategoryId;
        }
    }

    public class PredictionResult
    {
        [ColumnName("PredictedLabel")]
        public int PredictedCategoryId { get; set; }
    }

}
