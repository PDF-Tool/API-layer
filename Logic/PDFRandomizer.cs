using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("Hello, World!");

        RandomizerController controller = new RandomizerController();

        do
        {
            Console.WriteLine("Can you give me the following numbers: The number of the minimum data per page");
            string dataMinString = Console.ReadLine();
            Console.WriteLine("The number of the maximum data per page");
            string dataMaxString = Console.ReadLine();
            Console.WriteLine("The number of the minimum pages");
            string pageMinString = Console.ReadLine();
            Console.WriteLine("The number of the maximum pages");
            string pageMaxString = Console.ReadLine();

            if (controller.SetInput(dataMinString, dataMaxString, pageMinString, pageMaxString))
            {
                Console.WriteLine("The random numbers are:");
                Console.WriteLine("Data per page: " + controller.DataCount + " and the number of pages are: " + controller.PageCount);
            }
            else
            {
                Console.WriteLine("Invalid input. Please try again.");
            }

            
            Console.WriteLine("Do you want to run again? (yes/no)");
        } while (Console.ReadLine().Trim().ToLower() == "yes");
    }
}

class RandomizerModel
{
    private Random _random;

    public RandomizerModel()
    {
        _random = new Random();
    }

    public int GenerateRandomNumber(int min, int max)
    {
        return _random.Next(min, max + 1);//de plus 1 is zodat de maximale waarde ook mee telt 
    }
}

class RandomizerController
{
    private RandomizerModel _model;

    public int DataCount { get; private set; }
    public int PageCount { get; private set; }

    public RandomizerController()
    {
        _model = new RandomizerModel();
    }

    public bool SetInput(string dataMinString, string dataMaxString, string pageMinString, string pageMaxString)
    {
   
        if (int.TryParse(dataMinString, out int dataMin) &&
            int.TryParse(dataMaxString, out int dataMax) &&
            int.TryParse(pageMinString, out int pageMin) &&
            int.TryParse(pageMaxString, out int pageMax))
        {

            if (dataMin > 0 && dataMin < dataMax && pageMin > 0 && pageMin < pageMax)
            {
                DataCount = _model.GenerateRandomNumber(dataMin, dataMax);
                PageCount = _model.GenerateRandomNumber(pageMin, pageMax);
                return true; 
            }
        }
        return false;
    }
}