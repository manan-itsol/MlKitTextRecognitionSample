using Android.App;
using Android.Content;
using Android.Gms.Vision;
using Android.Gms.Vision.Texts;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using MlKitTextRecognitionSample.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MlKitTextRecognitionSample.Droid.OCR
{
    public class OcrExtractor : IOcrExtractor
    {
        #region service methods
        public async Task<string> ProcessImageAsync(byte[] imageData)
        {
            try
            {
                Bitmap bitmap = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
                var textRecognizer = new TextRecognizer.Builder(Application.Context).Build();
                Frame imageFrame = new Frame.Builder().SetBitmap(bitmap).Build();
                SparseArray textBlocks = textRecognizer.Detect(imageFrame);

                var textResult = ProcessText(textBlocks);
                textRecognizer.Release();
                return await System.Threading.Tasks.Task.FromResult(textResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion service methods

        #region helpers methods
        private string ProcessText(SparseArray textBlocks)
        {
            List<TextExtractionModel> textExtractions = new List<TextExtractionModel>();
            for (int index = 0; index < textBlocks.Size(); index++)
            {
                TextBlock tBlock = (TextBlock)textBlocks.ValueAt(index);
                foreach (var line in tBlock.Components)
                {
                    var x = line.BoundingBox.CenterX();
                    var y = line.BoundingBox.CenterY();
                    if (textExtractions.Count == 0)
                    {
                        textExtractions.Add(new TextExtractionModel
                        {
                            LinesList = new List<LineWithXY>
                            {
                                new LineWithXY(x,y,line.Value)
                            }
                        });
                    }
                    else
                    {
                        var nearest = GetNearest(textExtractions, y);
                        if (y >= nearest.CenterY - 15 && y <= nearest.CenterY + 15)
                        {
                            textExtractions.FirstOrDefault(x => x.CenterY == nearest.CenterY).LinesList.Add(new LineWithXY(x, y, line.Value));
                        }
                        else
                        {
                            if (textExtractions.Any(x => x.CenterY == y))
                            {
                                textExtractions.FirstOrDefault(x => x.CenterY == y).LinesList.Add(new LineWithXY(x, y, line.Value));
                            }
                            else
                            {
                                textExtractions.Add(new TextExtractionModel
                                {
                                    LinesList = new List<LineWithXY>
                                    {
                                        new LineWithXY(x,y,line.Value)
                                    }
                                });
                            }
                        }
                    }
                }
            }
            textExtractions = MergeNearestRows(textExtractions);
            string finalLines = string.Empty;
            foreach (var item in textExtractions.OrderBy(x => x.CenterY))
            {
                finalLines = $"{finalLines}{string.Join(' ', item.LinesList.OrderBy(x => x.X).Select(x => x.Text).ToList())}\n";
            }
            return finalLines;
        }

        private TextExtractionModel GetNearest(List<TextExtractionModel> textExtractions, int currentKey)
        {
            var sorted = textExtractions.OrderBy(x => x.CenterY).ToList();
            TextExtractionModel last = null;
            foreach (var item in sorted)
            {
                var less = currentKey < item.CenterY;
                if (less)
                {
                    last = item;
                }
                else
                {
                    if (last == null)
                        return item;
                    var lessDiff = currentKey - last.CenterY;
                    var greaterDiff = item.CenterY - currentKey;
                    if (lessDiff < greaterDiff)
                        return last;
                    else
                        return item;
                }
            }
            return last;
        }

        private List<TextExtractionModel> MergeNearestRows(List<TextExtractionModel> textExtractions)
        {
            textExtractions = textExtractions.OrderBy(x => x.CenterY).ToList();
            List<TextExtractionModel> toRemove = new List<TextExtractionModel>();
            TextExtractionModel last = null;
            foreach (var current in textExtractions)
            {
                if (last == null)
                {
                    last = current;
                    continue;
                }
                var diff = current.CenterY - last.CenterY;
                if (diff <= 20)
                {
                    current.LinesList.AddRange(last.LinesList);
                    toRemove.Add(last);
                }
                last = current;
            }
            foreach (var item in toRemove)
            {
                textExtractions.Remove(item);
            }
            return textExtractions;
        }
        #endregion helpers methods
    }

    public class TextExtractionModel
    {
        public TextExtractionModel()
        {
            LinesList = new List<LineWithXY>();
        }

        public float CenterY
        {
            get
            {
                float avg = 0;
                if (LinesList != null && LinesList.Count > 0)
                {
                    avg = LinesList.Sum(x => x.Y) / LinesList.Count;
                }
                return avg;
            }
        }
        public List<LineWithXY> LinesList { get; set; }
    }

    public class LineWithXY
    {
        public LineWithXY(int x, int y, string text)
        {
            X = x;
            Y = y;
            Text = text;
        }
        public int X { get; set; }
        public int Y { get; set; }
        public string Text { get; set; }
    }
}