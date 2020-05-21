using Adex.Model;
using System.IO;
using System.Text;

namespace Adex.App
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new AdexContext())
            {
                db.Companies.Add(new Company() { Designation = "test" });
                db.SaveChanges();
            }
        }

        private static void GenerateSmallDataSample()
        {
            var files = Directory.GetFiles(@"E:\Git\ImmobilisCommander\ADEX\exports-etalab", "*.csv");
            var sb = new StringBuilder();

            foreach (var f in files)
            {
                using (var r = new StreamReader(f, true))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        sb.Append($"{r.ReadLine()}\n");
                    }
                }

                var newFile = Path.Combine(@"E:\Git\ImmobilisCommander\ADEX\Data", Path.GetFileName(f));
                if (File.Exists(newFile))
                {
                    File.Delete(newFile);
                }
                File.WriteAllText(newFile, sb.ToString(), Encoding.UTF8);
                sb.Clear();
            }
        }
    }
}
