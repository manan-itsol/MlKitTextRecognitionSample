using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MlKitTextRecognitionSample.Services
{
    public interface IOcrExtractor
    {
        Task<string> ProcessImageAsync(byte[] imageData);
    }
}
