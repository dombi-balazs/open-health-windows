using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using open_health_windows.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace open_health_windows.Services
{
    internal class EvaluationService : IEvaluationService
    {
        private InferenceSession? _sessionFP32;
        private InferenceSession? _sessionINT8;
        private bool _useQuantized = false;

        private readonly string[] _classNames = { "Melanoma", "Nevus", "Seborrheic Keratosis" };

        public void SetModelMode(bool useQuantized)
        {
            _useQuantized = useQuantized;
        }

        public void InitializeModels(ChosenHardwareEntity.HardwareChoice hardwareChoice, string model1Path, string model2Path)
        {
            var sessionOptions = new SessionOptions();

            switch (hardwareChoice)
            {
                case ChosenHardwareEntity.HardwareChoice.iGPU:
                    sessionOptions.AppendExecutionProvider_DML(0);
                    break;
                case ChosenHardwareEntity.HardwareChoice.NPU:
                    sessionOptions.AppendExecutionProvider_CPU();
                    break;
                default:
                    sessionOptions.AppendExecutionProvider_CPU();
                    break;
            }

            if (_sessionFP32 == null) _sessionFP32 = new InferenceSession(model1Path, sessionOptions);
            if (_sessionINT8 == null) _sessionINT8 = new InferenceSession(model2Path, sessionOptions);
        }

        public async Task<string> AnalyzeImageAsync(byte[] imageBytes)
        {
            return await Task.Run(() =>
            {
                var currentSession = _useQuantized ? _sessionINT8 : _sessionFP32;

                if (currentSession == null)
                    throw new InvalidOperationException("Modellek nincsenek betöltve.");

                var inputTensor = ConvertImageToTensor(imageBytes);

                string inputName = currentSession.InputMetadata.Keys.First();
                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputName, inputTensor) };

                using var results = currentSession.Run(inputs);

                var outputName = currentSession.OutputMetadata.Keys.First();
                float[] outputData = results.First(r => r.Name == outputName).AsEnumerable<float>().ToArray();

                int maxIndex = 0;
                float maxScore = outputData[0];
                for (int i = 1; i < outputData.Length; i++)
                {
                    if (outputData[i] > maxScore)
                    {
                        maxScore = outputData[i];
                        maxIndex = i;
                    }
                }

                string label = maxIndex < _classNames.Length ? _classNames[maxIndex] : "Ismeretlen";
                return $"{label} (Bizonyosság: {maxScore * 100:F3}%)";
            });
        }

        private DenseTensor<float> ConvertImageToTensor(byte[] imageBytes)
        {
            using var image = Image.Load<Rgb24>(imageBytes);

            image.Mutate(x => x.Resize(new ResizeOptions { Size = new Size(224, 224), Mode = ResizeMode.Stretch }));

            var tensor = new DenseTensor<float>(new[] { 1, 3, 224, 224 });

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++)
                    {
                        tensor[0, 0, y, x] = row[x].R / 255f;
                        tensor[0, 1, y, x] = row[x].G / 255f;
                        tensor[0, 2, y, x] = row[x].B / 255f;
                    }
                }
            });
            return tensor;
        }

        public void Dispose()
        {
            _sessionFP32?.Dispose();
            _sessionINT8?.Dispose();
        }
    }
}