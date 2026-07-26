namespace CMS.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            string textFile = @"./Assets/TextFile.txt";
            
            string text = File.ReadAllText(textFile);
        }
    }
}