using System;

class RandomizerModel
{
    private Random _random;

    public RandomizerModel()
    {
        _random = new Random();
    }

    public int GenerateRandomNumber(int min, int max)
    {
        if (min > max)
        {
             return _random.Next(max, min + 1); 
        }
        return _random.Next(min, max + 1);
    }
}

public class PDFRandomizer
{
    private RandomizerModel _model;

    public PDFRandomizer()
    {
        _model = new RandomizerModel();
    }

    public (int DataCount, int PageCount) GenerateRandomValues(int dataMin, int dataMax, int pageMin, int pageMax)
    {
        if (dataMin <= 0 || dataMin >= dataMax || pageMin <= 0 || pageMin >= pageMax)
        {
            throw new ArgumentException("Invalid input ranges provided. Ensure min > 0 and min < max for both data and pages.");
        }

        int generatedDataCount = _model.GenerateRandomNumber(dataMin, dataMax);
        int generatedPageCount = _model.GenerateRandomNumber(pageMin, pageMax);

        return (generatedDataCount, generatedPageCount);
    }
}