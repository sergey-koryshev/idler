namespace Idler.Managers
{
    using Idler.Helpers.DB;
    using Idler.Models;
    using Idler.Properties;
    using Microsoft.ML;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Manages the lifecycle and operations of the NLP (Natural Language Processing) model used for automatic note categorization.
    /// Handles model training, loading, saving, prediction, and status tracking.
    /// Implements a singleton pattern to ensure a single instance throughout the application.
    /// </summary>
    public class NlpModelManager
    {
        private readonly string NlpModelFileName = "IdlerNlpModel.zip";
        private MLContext MLContext { get; }
        private ITransformer Model { get; set; }
        private PredictionEngine<TrainData, PredictionResult> PredictionEngine { get; set; }
        private static NlpModelManager instance;
        private NlpModelStatus nlpModelStatus;

        /// <summary>
        /// Gets the current status of the NLP model.
        /// </summary>
        public NlpModelStatus NlpModelStatus
        { 
            get => this.nlpModelStatus;
            private set
            {
                this.nlpModelStatus = value;
                this.ModelStatusChanged?.Invoke(this, value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the NLP model is ready for predictions.
        /// </summary>
        public bool IsReady => this.NlpModelStatus != NlpModelStatus.None && NlpModelStatus.Completed.HasFlag(this.NlpModelStatus);

        /// <summary>
        /// Occurs when the model status changes.
        /// </summary>
        public event EventHandler<NlpModelStatus> ModelStatusChanged;

        /// <summary>
        /// Gets the singleton instance of the <see cref="NlpModelManager"/>.
        /// </summary>
        /// <returns>The singleton instance.</returns>
        public static NlpModelManager GetInstance()
        {
            if (instance == null)
            {
                instance = new NlpModelManager();
            }

            return instance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NlpModelManager"/> class.
        /// Private to enforce singleton pattern.
        /// </summary>
        private NlpModelManager()
        {
            this.MLContext = new MLContext();
        }

        /// <summary>
        /// Initializes the NLP model manager by loading or training the model as needed.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeAsync()
        {
            if (!Settings.Default.IsAutoCategorizationEnabled || this.IsReady)
            {
                return;
            }

            this.LoadModelFromDisk();

            if (this.Model == null)
            {
                await this.RetrainModelAsync();
            }

            this.PredictionEngine = this.MLContext.Model.CreatePredictionEngine<TrainData, PredictionResult>(this.Model);
        }

        /// <summary>
        /// Predicts the category ID for the given note description.
        /// </summary>
        /// <param name="description">The note description to categorize.</param>
        /// <returns>The predicted category ID.</returns>
        /// <exception cref="Exception">Thrown if the model is not ready.</exception>
        public int PredictCategoryId(string description)
        {
            if (!this.IsReady)
            {
                throw new Exception("NlpModelManager should be initialized first to predict category id.");
            }

            var prediction = this.PredictionEngine.Predict(new TrainData { Description = description });
            return prediction.PredictedCategoryId;
        }

        /// <summary>
        /// Trains the NLP model using data from the database and saves it to disk.
        /// Updates the model status accordingly.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task RetrainModelAsync()
        {
            this.NlpModelStatus = NlpModelStatus.Training;
            var trainData = await DataBaseFunctions.GetTrainData();
            this.TrainModel(trainData);
            this.SaveModelToDisk();
            this.NlpModelStatus = NlpModelStatus.Trained;
        }

        /// <summary>
        /// Trains the ML.NET model using the provided training data.
        /// </summary>
        /// <param name="trainData">The training data.</param>
        private void TrainModel(IEnumerable<TrainData> trainData)
        {
            IDataView trainingDataView = this.MLContext.Data.LoadFromEnumerable(trainData);

            var pipeline = this.MLContext.Transforms.Conversion.MapValueToKey(inputColumnName: "CategoryId", outputColumnName: "Label")
                .Append(this.MLContext.Transforms.Text.FeaturizeText(inputColumnName: "Description", outputColumnName: "Features"))
                .AppendCacheCheckpoint(this.MLContext)
                .Append(this.MLContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
                .Append(this.MLContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            this.Model = pipeline.Fit(trainingDataView);
        }

        /// <summary>
        /// Loads the trained model from disk if it exists and updates the model status.
        /// </summary>
        private void LoadModelFromDisk()
        {
            if (File.Exists(NlpModelFileName))
            {
                this.NlpModelStatus = NlpModelStatus.Loading;
                using (var stream = new FileStream(NlpModelFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    this.Model = this.MLContext.Model.Load(stream, out var _);
                    this.NlpModelStatus = NlpModelStatus.Loaded;
                }
            }
        }

        /// <summary>
        /// Saves the trained model to disk.
        /// </summary>
        private void SaveModelToDisk()
        {
            using (var stream = new FileStream(NlpModelFileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                this.MLContext.Model.Save(this.Model, null, stream);
            }
        }
    }
}
